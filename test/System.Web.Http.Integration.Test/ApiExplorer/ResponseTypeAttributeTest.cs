// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Description;
using System.Web.Http.Dispatcher;
using Microsoft.TestCommon;

namespace System.Web.Http.ApiExplorer
{
    public class ResponseTypeAttributeTest
    {
        [Theory]
        [InlineData("Get", typeof(IHttpActionResult), typeof(User), 2)]
        [InlineData("Post", typeof(HttpResponseMessage), typeof(User), 2)]
        [InlineData("Delete", typeof(string), null, 2)]
        [InlineData("Head", typeof(IHttpActionResult), typeof(void), 0)]
        public void DeclaredResponseType_AppearsOnApiDescription(string actionName, Type declaredType, Type responseType, int responseFormattersCount)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "{controller}/{id}", new { id = RouteParameter.Optional });
            Type controllerToTest = typeof(ResponseTypeController);
            DefaultHttpControllerSelector controllerSelector = ApiExplorerHelper.GetStrictControllerSelector(config, controllerToTest);
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);

            Collection<ApiDescription> apis = config.Services.GetApiExplorer().ApiDescriptions;
            ApiDescription expectedApi = apis.FirstOrDefault(api => api.ActionDescriptor.ActionName == actionName &&
                api.ResponseDescription.DeclaredType == declaredType &&
                api.ResponseDescription.ResponseType == responseType);

            Assert.NotNull(expectedApi);
            Assert.Equal(responseFormattersCount, expectedApi.SupportedResponseFormatters.Count);
        }
    }
}