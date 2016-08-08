// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Web.WebPages.Instrumentation;
using System.Web.WebPages.Resources;

/*
WebPage class hierarchy

WebPageExecutingBase                        The base class for all Plan9 files (_pagestart, _appstart, and regular pages)
    ApplicationStartPage                    Used for _appstart.cshtml
    WebPageRenderingBase
        StartPage                           Used for _pagestart.cshtml
        WebPageBase
            WebPage                         Plan9Pages
            ViewWebPage?                    MVC Views
HelperPage                                  Base class for Web Pages in App_Code.
*/

namespace System.Web.WebPages
{
    // The base class for all CSHTML files (_pagestart, _appstart, and regular pages)
    public abstract class WebPageExecutingBase
    {
        private IVirtualPathFactory _virtualPathFactory;
        private DynamicHttpApplicationState _dynamicAppState;
        private InstrumentationService _instrumentationService = null;

        internal InstrumentationService InstrumentationService
        {
            get
            {
                if (_instrumentationService == null)
                {
                    _instrumentationService = new InstrumentationService();
                }
                return _instrumentationService;
            }
            set { _instrumentationService = value; }
        }

        public virtual HttpApplicationStateBase AppState
        {
            get
            {
                if (Context != null)
                {
                    return Context.Application;
                }
                return null;
            }
        }

        public virtual dynamic App
        {
            get
            {
                if (_dynamicAppState == null && AppState != null)
                {
                    _dynamicAppState = new DynamicHttpApplicationState(AppState);
                }
                return _dynamicAppState;
            }
        }

        public virtual HttpContextBase Context { get; set; }

        public virtual string VirtualPath { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual IVirtualPathFactory VirtualPathFactory
        {
            get { return _virtualPathFactory ?? VirtualPathFactoryManager.Instance; }
            set { _virtualPathFactory = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public abstract void Execute();

        public virtual string Href(string path, params object[] pathParts)
        {
            return UrlUtil.GenerateClientUrl(Context, VirtualPath, path, pathParts);
        }

        protected internal void BeginContext(int startPosition, int length, bool isLiteral)
        {
            BeginContext(GetOutputWriter(), VirtualPath, startPosition, length, isLiteral);
        }

        protected internal void BeginContext(string virtualPath, int startPosition, int length, bool isLiteral)
        {
            BeginContext(GetOutputWriter(), virtualPath, startPosition, length, isLiteral);
        }

        protected internal void BeginContext(TextWriter writer, int startPosition, int length, bool isLiteral)
        {
            BeginContext(writer, VirtualPath, startPosition, length, isLiteral);
        }

        protected internal void BeginContext(TextWriter writer, string virtualPath, int startPosition, int length, bool isLiteral)
        {
            // Double check that the instrumentation service is active because WriteAttribute always calls this
            if (InstrumentationService.IsAvailable)
            {
                InstrumentationService.BeginContext(Context,
                                                    virtualPath,
                                                    writer,
                                                    startPosition,
                                                    length,
                                                    isLiteral);
            }
        }

        protected internal void EndContext(int startPosition, int length, bool isLiteral)
        {
            EndContext(GetOutputWriter(), VirtualPath, startPosition, length, isLiteral);
        }

        protected internal void EndContext(string virtualPath, int startPosition, int length, bool isLiteral)
        {
            EndContext(GetOutputWriter(), virtualPath, startPosition, length, isLiteral);
        }

        protected internal void EndContext(TextWriter writer, int startPosition, int length, bool isLiteral)
        {
            EndContext(writer, VirtualPath, startPosition, length, isLiteral);
        }

        protected internal void EndContext(TextWriter writer, string virtualPath, int startPosition, int length, bool isLiteral)
        {
            // Double check that the instrumentation service is active because WriteAttribute always calls this
            if (InstrumentationService.IsAvailable)
            {
                InstrumentationService.EndContext(Context,
                                                  virtualPath,
                                                  writer,
                                                  startPosition,
                                                  length,
                                                  isLiteral);
            }
        }

        internal virtual string GetDirectory(string virtualPath)
        {
            return VirtualPathUtility.GetDirectory(virtualPath);
        }

        /// <summary>
        /// Normalizes path relative to the current virtual path and throws if a file does not exist at the location.
        /// </summary>
        protected internal virtual string NormalizeLayoutPagePath(string layoutPagePath)
        {
            var virtualPath = NormalizePath(layoutPagePath);
            // Look for it as specified, either absolute, relative or same folder
            if (VirtualPathFactory.Exists(virtualPath))
            {
                return virtualPath;
            }
            throw new HttpException(String.Format(CultureInfo.CurrentCulture, WebPageResources.WebPage_LayoutPageNotFound, layoutPagePath, virtualPath));
        }

        public virtual string NormalizePath(string path)
        {
            // If it's relative, resolve it
            return VirtualPathUtility.Combine(VirtualPath, path);
        }

        public abstract void Write(HelperResult result);

        public abstract void Write(object value);

        public abstract void WriteLiteral(object value);

        public virtual void WriteAttribute(string name, PositionTagged<string> prefix, PositionTagged<string> suffix, params AttributeValue[] values)
        {
            WriteAttributeTo(GetOutputWriter(), name, prefix, suffix, values);
        }

        public virtual void WriteAttributeTo(TextWriter writer, string name, PositionTagged<string> prefix, PositionTagged<string> suffix, params AttributeValue[] values)
        {
            WriteAttributeTo(VirtualPath, writer, name, prefix, suffix, values);
        }

        protected internal virtual void WriteAttributeTo(string pageVirtualPath, TextWriter writer, string name, PositionTagged<string> prefix, PositionTagged<string> suffix, params AttributeValue[] values)
        {
            bool first = true;
            bool wroteSomething = false;
            if (values.Length == 0)
            {
                // Explicitly empty attribute, so write the prefix and suffix
                WritePositionTaggedLiteral(writer, pageVirtualPath, prefix);
                WritePositionTaggedLiteral(writer, pageVirtualPath, suffix);
            }
            else
            {
                for (int i = 0; i < values.Length; i++)
                {
                    AttributeValue attrVal = values[i];
                    PositionTagged<object> val = attrVal.Value;
                    PositionTagged<string> next = i == values.Length - 1 ?
                        suffix : // End of the list, grab the suffix
                        values[i + 1].Prefix; // Still in the list, grab the next prefix

                    if (val.Value == null)
                    {
                        // Nothing to write
                        continue;
                    }

                    // The special cases here are that the value we're writing might already be a string, or that the 
                    // value might be a bool. If the value is the bool 'true' we want to write the attribute name instead
                    // of the string 'true'. If the value is the bool 'false' we don't want to write anything.
                    //
                    // Otherwise the value is another object (perhaps an IHtmlString), and we'll ask it to format itself.
                    string stringValue;

                    // Intentionally using is+cast here for performance reasons. This is more performant than as+bool? 
                    // because of boxing.
                    if (val.Value is bool)
                    {
                        if ((bool)val.Value)
                        {
                            stringValue = name;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        stringValue = val.Value as string;
                    }

                    if (first)
                    {
                        WritePositionTaggedLiteral(writer, pageVirtualPath, prefix);
                        first = false;
                    }
                    else
                    {
                        WritePositionTaggedLiteral(writer, pageVirtualPath, attrVal.Prefix);
                    }

                    // Calculate length of the source span by the position of the next value (or suffix)
                    int sourceLength = next.Position - attrVal.Value.Position;

                    BeginContext(writer, pageVirtualPath, attrVal.Value.Position, sourceLength, isLiteral: attrVal.Literal);

                    // The extra branching here is to ensure that we call the Write*To(string) overload when
                    // possible.
                    if (attrVal.Literal && stringValue != null)
                    {
                        WriteLiteralTo(writer, stringValue);
                    }
                    else if (attrVal.Literal)
                    {
                        WriteLiteralTo(writer, val.Value);
                    }
                    else if (stringValue != null)
                    {
                        WriteTo(writer, stringValue);
                    }
                    else
                    {
                        WriteTo(writer, val.Value);
                    }

                    EndContext(writer, pageVirtualPath, attrVal.Value.Position, sourceLength, isLiteral: attrVal.Literal);
                    wroteSomething = true;
                }
                if (wroteSomething)
                {
                    WritePositionTaggedLiteral(writer, pageVirtualPath, suffix);
                }
            }
        }

        private void WritePositionTaggedLiteral(TextWriter writer, string pageVirtualPath, string value, int position)
        {
            BeginContext(writer, pageVirtualPath, position, value.Length, isLiteral: true);
            WriteLiteralTo(writer, value);
            EndContext(writer, pageVirtualPath, position, value.Length, isLiteral: true);
        }

        private void WritePositionTaggedLiteral(TextWriter writer, string pageVirtualPath, PositionTagged<string> value)
        {
            WritePositionTaggedLiteral(writer, pageVirtualPath, value.Value, value.Position);
        }

        // This method is called by generated code and needs to stay in sync with the parser
        public static void WriteTo(TextWriter writer, HelperResult content)
        {
            if (content != null)
            {
                content.WriteTo(writer);
            }
        }

        // This method is called by generated code and needs to stay in sync with the parser
        public static void WriteTo(TextWriter writer, object content)
        {
            writer.Write(HttpUtility.HtmlEncode(content));
        }

        // Perf optimization to avoid calling string.ToString when we already know the type is a string.
        private static void WriteTo(TextWriter writer, string content)
        {
            writer.Write(HttpUtility.HtmlEncode(content));
        }

        // This method is called by generated code and needs to stay in sync with the parser
        public static void WriteLiteralTo(TextWriter writer, object content)
        {
            writer.Write(content);
        }

        // Perf optimization to avoid calling string.ToString when we already know the type is a string.
        private static void WriteLiteralTo(TextWriter writer, string content)
        {
            writer.Write(content);
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "A method is more appropriate in this case since a property likely already exists to hold this value")]
        protected internal virtual TextWriter GetOutputWriter()
        {
            return TextWriter.Null;
        }
    }
}
