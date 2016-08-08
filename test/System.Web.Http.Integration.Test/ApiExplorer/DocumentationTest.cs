// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Http.Description;
using System.Web.Http.Dispatcher;
using Microsoft.TestCommon;

namespace System.Web.Http.ApiExplorer
{
    public class DocumentationTest
    {
        [Fact]
        public void VerifyDefaultDocumentationMessage()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "{controller}/{id}", new { id = RouteParameter.Optional });
            ItemFormatter customFormatter = new ItemFormatter();
            config.Formatters.Add(customFormatter);

            DefaultHttpControllerSelector controllerSelector = ApiExplorerHelper.GetStrictControllerSelector(config, typeof(ItemController));
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);

            IApiExplorer explorer = config.Services.GetApiExplorer();
            foreach (ApiDescription description in explorer.ApiDescriptions)
            {
                Assert.Null(description.Documentation);
                foreach (ApiParameterDescription param in description.ParameterDescriptions)
                {
                    Assert.Null(param.Documentation);
                }
            }
        }

        [Fact]
        public void VerifyCustomDocumentationProviderMessage()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "{controller}/{id}", new { id = RouteParameter.Optional });
            ItemFormatter customFormatter = new ItemFormatter();
            config.Formatters.Add(customFormatter);

            DefaultHttpControllerSelector controllerSelector = ApiExplorerHelper.GetStrictControllerSelector(config, typeof(DocumentationController));
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);

            AttributeDocumentationProvider documentationProvider = new AttributeDocumentationProvider();
            config.Services.Replace(typeof(IDocumentationProvider), documentationProvider);

            IApiExplorer explorer = config.Services.GetApiExplorer();
            foreach (ApiDescription description in explorer.ApiDescriptions)
            {
                Assert.Equal(
                    "Documentation controller",
                    documentationProvider.GetDocumentation(description.ActionDescriptor.ControllerDescriptor));
                Assert.Equal(
                    String.Format("{0} action", description.ActionDescriptor.ActionName),
                    description.Documentation);
                foreach (ApiParameterDescription param in description.ParameterDescriptions)
                {
                    Assert.Equal(
                        String.Format("{0} parameter", param.Name),
                        param.Documentation);
                }
                if (description.ResponseDescription.DeclaredType != null)
                {
                    Assert.Equal(
                        String.Format("{0} response", description.ActionDescriptor.ActionName),
                        description.ResponseDescription.Documentation);
                }
            }
        }
    }
}