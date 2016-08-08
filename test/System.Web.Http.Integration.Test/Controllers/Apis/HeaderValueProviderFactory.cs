// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Http.Controllers;
using System.Web.Http.ValueProviders;
namespace System.Web.Http
{
    public class HeaderValueProviderFactory : ValueProviderFactory
    {
        public override IValueProvider GetValueProvider(HttpActionContext actionContext)
        {
            return new HeaderValueProvider();
        }
    }

    public class HeaderValueProvider : IValueProvider
    {
        public bool ContainsPrefix(string prefix)
        {
            return false;
        }

        public ValueProviderResult GetValue(string key)
        {
            return null;
        }
    }
}
