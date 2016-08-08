// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.ModelBinding;
using System.Web.Http.ValueProviders;

namespace System.Web.Http.ApiExplorer
{
    public class ParameterSourceController : ApiController
    {
        public void GetCompleTypeFromUri([FromUri]ComplexType value, string name)
        {
        }

        public void PostSimpleTypeFromBody([FromBody] string name)
        {
        }

        public void GetCustomFromUriAttribute([MyFromUriAttribute] ComplexType value, ComplexType bodyValue)
        {
        }

        public void GetFromHeaderAttribute([FromHeaderAttribute] string value)
        {
        }

        public class ComplexType
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class MyFromUriAttribute : ModelBinderAttribute, IUriValueProviderFactory
        {
            public override IEnumerable<ValueProviderFactory> GetValueProviderFactories(HttpConfiguration configuration)
            {
                var factories = from f in base.GetValueProviderFactories(configuration) where f is IUriValueProviderFactory select f;
                return factories;
            }
        }

        private class FromHeaderAttribute : ModelBinderAttribute
        {
            public override IEnumerable<ValueProviderFactory> GetValueProviderFactories(HttpConfiguration configuration)
            {
                var factories = new ValueProviderFactory[] { new HeaderValueProvider() };
                return factories;
            }
        }

        private class HeaderValueProvider : ValueProviderFactory
        {
            public override IValueProvider GetValueProvider(Controllers.HttpActionContext actionContext)
            {
                throw new NotImplementedException();
            }
        }
    }
}
