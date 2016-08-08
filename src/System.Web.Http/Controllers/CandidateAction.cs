// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Web.Http.Controllers;

namespace System.Web.Http.Routing
{
    // This is static description of an action and can be shared across requests. 
    // Direct routes may cache a list of these. 
    [DebuggerDisplay("{DebuggerToString()}")]
    internal class CandidateAction
    {
        public HttpActionDescriptor ActionDescriptor { get; set; }
        public int Order { get; set; }
        public decimal Precedence { get; set; }

        public bool MatchName(string actionName)
        {
            return String.Equals(ActionDescriptor.ActionName, actionName, StringComparison.OrdinalIgnoreCase);
        }

        public bool MatchVerb(HttpMethod method)
        {
            return ActionDescriptor.SupportedHttpMethods.Contains(method);
        }

        internal string DebuggerToString()
        {
            return String.Format(CultureInfo.CurrentCulture, "{0}, Order={1}, Prec={2}", ActionDescriptor.ActionName, Order, Precedence);
        }
    }
}
