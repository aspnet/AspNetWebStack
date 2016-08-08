// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Specialized;

namespace System.Web.Mvc.Test
{
    public abstract class MockableUnvalidatedRequestValues : IUnvalidatedRequestValues
    {
        public abstract NameValueCollection Form { get; }
        public abstract NameValueCollection QueryString { get; }
        public abstract string this[string key] { get; }
    }
}
