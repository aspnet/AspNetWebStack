// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Web.UI;

namespace System.Web.Mvc
{
    // This class is referenced dynamically by the web.config built by project templates.
    // Do not delete this class based on it not being statically referenced by product code.

    internal class ViewTypeParserFilter : PageParserFilter
    {
        private static Dictionary<string, Type> _directiveBaseTypeMappings = new Dictionary<string, Type>
        {
            { "page", typeof(ViewPage) },
            { "control", typeof(ViewUserControl) },
            { "master", typeof(ViewMasterPage) },
        };

        private string _inherits;

        public ViewTypeParserFilter()
        {
        }

        public override bool AllowCode
        {
            get { return true; }
        }

        public override int NumberOfControlsAllowed
        {
            get { return -1; }
        }

        public override int NumberOfDirectDependenciesAllowed
        {
            get { return -1; }
        }

        public override int TotalNumberOfDependenciesAllowed
        {
            get { return -1; }
        }

        public override void PreprocessDirective(string directiveName, IDictionary attributes)
        {
            base.PreprocessDirective(directiveName, attributes);

            Type baseType;
            if (_directiveBaseTypeMappings.TryGetValue(directiveName, out baseType))
            {
                string inheritsAttribute = attributes["inherits"] as string;

                // Since the ASP.NET page parser doesn't understand native generic syntax, we
                // need to swap out whatever the user provided with the default base type for
                // the given directive (page vs. control vs. master). We stash the old value
                // and swap it back in inside the control builder. Our "is this generic?"
                // check here really only works for C# and VB.NET, since we're checking for
                // < or ( in the type name.
                //
                // We only change generic directives, because doing so breaks back-compat
                // for property setters on @Page, @Control, and @Master directives. The user
                // can work around this breaking behavior by using a non-generic inherits
                // directive, or by using the CLR syntax for generic type names.

                if (inheritsAttribute != null && inheritsAttribute.IndexOfAny(new[] { '<', '(' }) > 0)
                {
                    attributes["inherits"] = baseType.FullName;
                    _inherits = inheritsAttribute;
                }
            }
        }

        public override void ParseComplete(ControlBuilder rootBuilder)
        {
            base.ParseComplete(rootBuilder);

            IMvcControlBuilder builder = rootBuilder as IMvcControlBuilder;
            if (builder != null)
            {
                builder.Inherits = _inherits;
            }
        }

        // Everything else in this class is unrelated to our 'inherits' handling.
        // Since PageParserFilter blocks everything by default, we need to unblock it

        public override bool AllowBaseType(Type baseType)
        {
            return true;
        }

        public override bool AllowControl(Type controlType, ControlBuilder builder)
        {
            return true;
        }

        public override bool AllowVirtualReference(string referenceVirtualPath, VirtualReferenceType referenceType)
        {
            return true;
        }

        public override bool AllowServerSideInclude(string includeVirtualPath)
        {
            return true;
        }
    }
}
