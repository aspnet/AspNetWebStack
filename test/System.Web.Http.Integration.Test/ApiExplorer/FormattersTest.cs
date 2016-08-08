// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Description;
using System.Web.Http.Dispatcher;
using System.Web.Http.Tracing;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.ApiExplorer
{
    public class FormattersTest
    {
        [Fact]
        public void CustomRequestBodyFormatters_ShowUpOnDescription()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "{controller}/{id}", new { id = RouteParameter.Optional });
            ItemFormatter customFormatter = new ItemFormatter();
            config.Formatters.Add(customFormatter);

            DefaultHttpControllerSelector controllerSelector = ApiExplorerHelper.GetStrictControllerSelector(config, typeof(ItemController));
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);

            IApiExplorer explorer = config.Services.GetApiExplorer();
            ApiDescription description = explorer.ApiDescriptions.FirstOrDefault(desc => desc.ActionDescriptor.ActionName == "PostItem");
            Assert.True(description.SupportedRequestBodyFormatters.Any(formatter => formatter == customFormatter), "Did not find the custom formatter on the SupportedRequestBodyFormatters.");
        }

        [Fact]
        public void CustomResponseFormatters_ShowUpOnDescription()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "{controller}/{id}", new { id = RouteParameter.Optional });
            ItemFormatter customFormatter = new ItemFormatter();
            config.Formatters.Add(customFormatter);

            DefaultHttpControllerSelector controllerSelector = ApiExplorerHelper.GetStrictControllerSelector(config, typeof(ItemController));
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);

            IApiExplorer explorer = config.Services.GetApiExplorer();
            ApiDescription description = explorer.ApiDescriptions.FirstOrDefault(desc => desc.ActionDescriptor.ActionName == "PostItem");
            Assert.True(description.SupportedResponseFormatters.Any(formatter => formatter == customFormatter), "Did not find the custom formatter on the SupportedResponseFormatters.");
        }

        [Fact]
        public void SupportedRequestBodyFormatters_ReturnsFormattersWithoutTracers_WithNoTracing()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "{controller}/{id}", new { id = RouteParameter.Optional });
            ItemFormatter customFormatter = new ItemFormatter();
            config.Formatters.Add(customFormatter);
            config.Initializer.Invoke(config);
            int expectedFormatterCount = config.Formatters.Count - 1;
            DefaultHttpControllerSelector controllerSelector = ApiExplorerHelper.GetStrictControllerSelector(config, typeof(ItemController));
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);
            IApiExplorer explorer = config.Services.GetApiExplorer();

            // Act 
            ApiDescription description = explorer.ApiDescriptions.FirstOrDefault(desc => desc.ActionDescriptor.ActionName == "PostItem");
            Collection<MediaTypeFormatter> formatters = description.SupportedRequestBodyFormatters;

            // Assert
            Assert.False(formatters.Any(f => f is IFormatterTracer), "Tracers are present");
            Assert.Equal(expectedFormatterCount, formatters.Count);
        }

        [Fact]
        public void SupportedResponseFormatters_ReturnsFormattersWithoutTracers_WithNoTracing()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "{controller}/{id}", new { id = RouteParameter.Optional });
            ItemFormatter customFormatter = new ItemFormatter();
            config.Formatters.Add(customFormatter);
            config.Initializer.Invoke(config);
            int expectedFormatterCount = config.Formatters.Count - 2;
            DefaultHttpControllerSelector controllerSelector = ApiExplorerHelper.GetStrictControllerSelector(config, typeof(ItemController));
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);
            IApiExplorer explorer = config.Services.GetApiExplorer();

            // Act 
            ApiDescription description = explorer.ApiDescriptions.FirstOrDefault(desc => desc.ActionDescriptor.ActionName == "PostItem");
            Collection<MediaTypeFormatter> formatters = description.SupportedResponseFormatters;

            // Assert
            Assert.False(formatters.Any(f => f is IFormatterTracer), "Tracers are present");
            Assert.Equal(expectedFormatterCount, formatters.Count);
        }

        [Fact]
        public void SupportedRequestBodyFormatters_ReturnsFormattersWithoutTracers_WithTracing()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "{controller}/{id}", new { id = RouteParameter.Optional });
            ItemFormatter customFormatter = new ItemFormatter();
            config.Formatters.Add(customFormatter);
            ITraceWriter testTraceWriter = new Mock<ITraceWriter>().Object;
            config.Services.Replace(typeof(ITraceWriter), testTraceWriter);
            config.Initializer.Invoke(config);
            int expectedFormatterCount = config.Formatters.Count - 1;
            DefaultHttpControllerSelector controllerSelector = ApiExplorerHelper.GetStrictControllerSelector(config, typeof(ItemController));
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);
            IApiExplorer explorer = config.Services.GetApiExplorer();

            // Act 
            ApiDescription description = explorer.ApiDescriptions.FirstOrDefault(desc => desc.ActionDescriptor.ActionName == "PostItem");
            Collection<MediaTypeFormatter> formatters = description.SupportedRequestBodyFormatters;

            // Assert
            Assert.False(formatters.Any(f => f is IFormatterTracer), "Tracers are present");
            Assert.Equal(expectedFormatterCount, formatters.Count);
        }

        [Fact]
        public void SupportedResponseFormatters_ReturnsFormattersWithoutTracers_WithTracing()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "{controller}/{id}", new { id = RouteParameter.Optional });
            ItemFormatter customFormatter = new ItemFormatter();
            config.Formatters.Add(customFormatter);
            ITraceWriter testTraceWriter = new Mock<ITraceWriter>().Object;
            config.Services.Replace(typeof(ITraceWriter), testTraceWriter);
            config.Initializer.Invoke(config);
            int expectedFormatterCount = config.Formatters.Count - 2;
            DefaultHttpControllerSelector controllerSelector = ApiExplorerHelper.GetStrictControllerSelector(config, typeof(ItemController));
            config.Services.Replace(typeof(IHttpControllerSelector), controllerSelector);
            IApiExplorer explorer = config.Services.GetApiExplorer();

            // Act 
            ApiDescription description = explorer.ApiDescriptions.FirstOrDefault(desc => desc.ActionDescriptor.ActionName == "PostItem");
            Collection<MediaTypeFormatter> formatters = description.SupportedResponseFormatters;

            // Assert
            Assert.False(formatters.Any(f => f is IFormatterTracer), "Tracers are present");
            Assert.Equal(expectedFormatterCount, formatters.Count);
        }
    }
}
