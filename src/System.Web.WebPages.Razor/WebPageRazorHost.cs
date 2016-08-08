// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.CodeDom;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Compilation;
using System.Web.Hosting;
using System.Web.Razor;
using System.Web.Razor.Generator;
using System.Web.Razor.Parser;
using System.Web.WebPages.Instrumentation;
using System.Web.WebPages.Razor.Resources;
using Microsoft.Internal.Web.Utils;

namespace System.Web.WebPages.Razor
{
    public class WebPageRazorHost : RazorEngineHost
    {
        // DevDiv Bug 941404 - Add a prefix and folder name to class names
        internal const string PageClassNamePrefix = "_Page_";
        internal const string ApplicationInstancePropertyName = "ApplicationInstance";
        internal const string ContextPropertyName = "Context";
        internal const string DefineSectionMethodName = "DefineSection";
        internal const string WebDefaultNamespace = "ASP";
        internal const string WriteToMethodName = "WriteTo";
        internal const string WriteLiteralToMethodName = "WriteLiteralTo";
        internal const string BeginContextMethodName = "BeginContext";
        internal const string EndContextMethodName = "EndContext";
        internal const string ResolveUrlMethodName = "Href";

        private const string ApplicationStartFileName = "_AppStart";
        private const string PageStartFileName = "_PageStart";

        internal static readonly string FallbackApplicationTypeName = typeof(HttpApplication).FullName;
        internal static readonly string PageBaseClass = typeof(WebPage).FullName;
        internal static readonly string TemplateTypeName = typeof(HelperResult).FullName;

        private static ConcurrentDictionary<string, object> _importedNamespaces = new ConcurrentDictionary<string, object>();
        private readonly Dictionary<string, string> _specialFileBaseTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private string _className;
        private RazorCodeLanguage _codeLanguage;
        private string _globalAsaxTypeName;
        private bool? _isSpecialPage;
        private string _physicalPath = null;
        private string _specialFileBaseClass;

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "The code path is safe, it is a property setter and not dependent on other state")]
        private WebPageRazorHost()
        {
            NamespaceImports.Add("System");
            NamespaceImports.Add("System.Collections.Generic");
            NamespaceImports.Add("System.IO");
            NamespaceImports.Add("System.Linq");
            NamespaceImports.Add("System.Net");
            NamespaceImports.Add("System.Web");
            NamespaceImports.Add("System.Web.Helpers");
            NamespaceImports.Add("System.Web.Security");
            NamespaceImports.Add("System.Web.UI");
            NamespaceImports.Add("System.Web.WebPages");
            NamespaceImports.Add("System.Web.WebPages.Html");

            RegisterSpecialFile(ApplicationStartFileName, typeof(ApplicationStartPage));
            RegisterSpecialFile(PageStartFileName, typeof(StartPage));
            DefaultNamespace = WebDefaultNamespace;
            GeneratedClassContext = new GeneratedClassContext(GeneratedClassContext.DefaultExecuteMethodName,
                                                              GeneratedClassContext.DefaultWriteMethodName,
                                                              GeneratedClassContext.DefaultWriteLiteralMethodName,
                                                              WriteToMethodName,
                                                              WriteLiteralToMethodName,
                                                              TemplateTypeName,
                                                              DefineSectionMethodName,
                                                              BeginContextMethodName,
                                                              EndContextMethodName)
            {
                ResolveUrlMethodName = ResolveUrlMethodName
            };
            DefaultPageBaseClass = PageBaseClass;
            DefaultDebugCompilation = true;
            EnableInstrumentation = false;
        }

        public WebPageRazorHost(string virtualPath)
            : this(virtualPath, null)
        {
        }

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "The code path is safe, it is a property setter and not dependent on other state")]
        public WebPageRazorHost(string virtualPath, string physicalPath)
            : this()
        {
            if (String.IsNullOrEmpty(virtualPath))
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, CommonResources.Argument_Cannot_Be_Null_Or_Empty, "virtualPath"), "virtualPath");
            }

            VirtualPath = virtualPath;

            PhysicalPath = physicalPath;
            DefaultClassName = GetClassName(VirtualPath);
            CodeLanguage = GetCodeLanguage();
            EnableInstrumentation = new InstrumentationService().IsAvailable;
        }

        public override RazorCodeLanguage CodeLanguage
        {
            get
            {
                if (_codeLanguage == null)
                {
                    _codeLanguage = GetCodeLanguage();
                }
                return _codeLanguage;
            }
            protected set { _codeLanguage = value; }
        }

        public override string DefaultBaseClass
        {
            get
            {
                if (base.DefaultBaseClass != null)
                {
                    return base.DefaultBaseClass;
                }
                if (IsSpecialPage)
                {
                    return SpecialPageBaseClass;
                }
                else
                {
                    return DefaultPageBaseClass;
                }
            }
            set { base.DefaultBaseClass = value; }
        }

        public override string DefaultClassName
        {
            get
            {
                if (_className == null)
                {
                    _className = GetClassName(VirtualPath);
                }
                return _className;
            }
            set { _className = value; }
        }

        public bool DefaultDebugCompilation { get; set; }

        public string DefaultPageBaseClass { get; set; }

        internal string GlobalAsaxTypeName
        {
            get { return _globalAsaxTypeName ?? (HostingEnvironment.IsHosted ? BuildManager.GetGlobalAsaxType().FullName : FallbackApplicationTypeName); }
            set { _globalAsaxTypeName = value; }
        }

        public bool IsSpecialPage
        {
            get
            {
                CheckForSpecialPage();
                return _isSpecialPage.Value;
            }
        }

        public string PhysicalPath
        {
            get
            {
                MapPhysicalPath();
                return _physicalPath;
            }
            set { _physicalPath = value; }
        }

        public override string InstrumentedSourceFilePath
        {
            get { return VirtualPath; }
            set { VirtualPath = value; }
        }

        private string SpecialPageBaseClass
        {
            get
            {
                CheckForSpecialPage();
                return _specialFileBaseClass;
            }
        }

        public string VirtualPath { get; private set; }

        /// <summary>
        /// Adds a namespace to the global imports list for this AppDomain
        /// </summary>
        /// <remarks>
        /// IMPORTANT: ALL uses of WebPageRazorHost (and derived classes) within the same AppDomain will share this list.
        /// Therefore this method should only be used in runtime scenarios, not in design-time scenarios where the Razor
        /// data structures for multiple files and projects may be shared within a single AppDomain.
        /// </remarks>
        /// <param name="ns">The namespace to add to the global imports list.</param>
        public static void AddGlobalImport(string ns)
        {
            if (String.IsNullOrEmpty(ns))
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, CommonResources.Argument_Cannot_Be_Null_Or_Empty, "ns"), "ns");
            }

            _importedNamespaces.TryAdd(ns, null);
        }

        private void CheckForSpecialPage()
        {
            if (!_isSpecialPage.HasValue)
            {
                string fileName = Path.GetFileNameWithoutExtension(VirtualPath);
                string baseType;
                if (_specialFileBaseTypes.TryGetValue(fileName, out baseType))
                {
                    _isSpecialPage = true;
                    _specialFileBaseClass = baseType;
                }
                else
                {
                    _isSpecialPage = false;
                }
            }
        }

        public override ParserBase CreateMarkupParser()
        {
            return new HtmlMarkupParser();
        }

        private static RazorCodeLanguage DetermineCodeLanguage(string fileName)
        {
            string extension = Path.GetExtension(fileName);

            // Use an if rather than else-if just in case Path.GetExtension returns null for some reason
            if (String.IsNullOrEmpty(extension))
            {
                return null;
            }
            if (extension[0] == '.')
            {
                extension = extension.Substring(1); // Trim off the dot
            }

            // Look up the language
            // At the moment this only deals with code languages: cs, vb, etc., but in theory we could have MarkupLanguageServices which allow for
            // interesting combinations like: vbcss, csxml, etc.
            RazorCodeLanguage language = GetLanguageByExtension(extension);
            return language;
        }

        protected virtual string GetClassName(string virtualPath)
        {
            // Remove "~/" and run through our santizer
            // For example, for ~/Foo/Bar/Baz.cshtml, the class name is _Page_Foo_Bar_Baz_cshtml
            return ParserHelpers.SanitizeClassName(PageClassNamePrefix + virtualPath.TrimStart('~', '/'));
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Method involves significant processing and should not be a property")]
        protected virtual RazorCodeLanguage GetCodeLanguage()
        {
            RazorCodeLanguage language = DetermineCodeLanguage(VirtualPath);
            if (language == null && !String.IsNullOrEmpty(PhysicalPath))
            {
                language = DetermineCodeLanguage(PhysicalPath);
            }

            if (language == null)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, RazorWebResources.BuildProvider_No_CodeLanguageService_For_Path, VirtualPath));
            }

            return language;
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method involves copying memory, so a property is not appropriate")]
        public static IEnumerable<string> GetGlobalImports()
        {
            return _importedNamespaces.ToArray().Select(pair => pair.Key);
        }

        private static RazorCodeLanguage GetLanguageByExtension(string extension)
        {
            return RazorCodeLanguage.GetLanguageByExtension(extension);
        }

        private void MapPhysicalPath()
        {
            if (_physicalPath == null && HostingEnvironment.IsHosted)
            {
                string path = HostingEnvironment.MapPath(VirtualPath);
                if (!String.IsNullOrEmpty(path) && File.Exists(path))
                {
                    _physicalPath = path;
                }
            }
        }

        public override void PostProcessGeneratedCode(CodeGeneratorContext context)
        {
            base.PostProcessGeneratedCode(context);

            // Add additional global imports
            context.Namespace.Imports.AddRange(GetGlobalImports().Select(s => new CodeNamespaceImport(s)).ToArray());

            // Create ApplicationInstance property
            CodeMemberProperty prop = new CodeMemberProperty()
            {
                Name = ApplicationInstancePropertyName,
                Type = new CodeTypeReference(GlobalAsaxTypeName),
                HasGet = true,
                HasSet = false,
                Attributes = MemberAttributes.Family | MemberAttributes.Final
            };
            prop.GetStatements.Add(
                new CodeMethodReturnStatement(
                    new CodeCastExpression(
                        new CodeTypeReference(GlobalAsaxTypeName),
                        new CodePropertyReferenceExpression(
                            new CodePropertyReferenceExpression(
                                null,
                                ContextPropertyName),
                            ApplicationInstancePropertyName))));
            context.GeneratedClass.Members.Insert(0, prop);
        }

        protected void RegisterSpecialFile(string fileName, Type baseType)
        {
            if (baseType == null)
            {
                throw new ArgumentNullException("baseType");
            }
            RegisterSpecialFile(fileName, baseType.FullName);
        }

        protected void RegisterSpecialFile(string fileName, string baseTypeName)
        {
            if (String.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, CommonResources.Argument_Cannot_Be_Null_Or_Empty, "fileName"), "fileName");
            }
            if (String.IsNullOrEmpty(baseTypeName))
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, CommonResources.Argument_Cannot_Be_Null_Or_Empty, "baseTypeName"), "baseTypeName");
            }

            _specialFileBaseTypes[fileName] = baseTypeName;
        }
    }
}
