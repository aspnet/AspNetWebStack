// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.ModelBinding;
using System.Web.Http.ValueProviders;

namespace System.Web.Http
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
    public sealed class FromUriAttribute : ModelBinderAttribute
    {
        public override IEnumerable<ValueProviderFactory> GetValueProviderFactories(HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            foreach (ValueProviderFactory f in base.GetValueProviderFactories(configuration))
            {
                if (f is IUriValueProviderFactory)
                {
                    yield return f;
                }
            }
        }
    }
}
