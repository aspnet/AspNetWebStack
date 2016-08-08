// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.Filters;
using System.Web.Http.Tracing;

namespace System.Web.Http
{
    public class EnumParameterOverloadsController : ApiController
    {
        public IEnumerable<string> Get()
        {
            return new string[] { "get" };
        }

        public FilterScope GetWithEnumParameter(FilterScope scope)
        {
            return scope;
        }

        public string GetWithTwoEnumParameters([FromUri]TraceLevel level, TraceKind kind)
        {
            return level.ToString() + kind.ToString();
        }

        public string GetWithNullableEnumParameter(TraceLevel? level)
        {
            return level.ToString();
        }
    }
}