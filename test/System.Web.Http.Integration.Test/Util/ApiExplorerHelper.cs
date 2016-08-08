// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Web.Http.Dispatcher;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.ApiExplorer
{
    public static class ApiExplorerHelper
    {
        public static void VerifyApiDescriptions(Collection<ApiDescription> apiDescriptions, List<object> expectedResults)
        {
            Assert.Equal(expectedResults.Count, apiDescriptions.Count);
            ApiDescription[] sortedDescriptions = apiDescriptions.OrderBy(description => description.ID).ToArray();
            object[] sortedExpectedResults = expectedResults.OrderBy(r =>
            {
                dynamic expectedResult = r;
                HttpMethod expectedHttpMethod = expectedResult.HttpMethod;
                string expectedRelativePath = expectedResult.RelativePath;
                return expectedHttpMethod + expectedRelativePath;
            }).ToArray();

            for (int i = 0; i < sortedDescriptions.Length; i++)
            {
                dynamic expectedResult = sortedExpectedResults[i];
                ApiDescription matchingDescription = sortedDescriptions[i];
                Assert.Equal(expectedResult.HttpMethod, matchingDescription.HttpMethod);
                Assert.Equal(expectedResult.RelativePath, matchingDescription.RelativePath, ignoreCase: true);
                Assert.Equal(expectedResult.HasRequestFormatters, matchingDescription.SupportedRequestBodyFormatters.Count > 0);
                Assert.Equal(expectedResult.HasResponseFormatters, matchingDescription.SupportedResponseFormatters.Count > 0);
                Assert.Equal(expectedResult.NumberOfParameters, matchingDescription.ParameterDescriptions.Count);
            }
        }

        public static DefaultHttpControllerSelector GetStrictControllerSelector(HttpConfiguration config, params Type[] controllerTypes)
        {
            Dictionary<string, HttpControllerDescriptor> controllerMapping = new Dictionary<string, HttpControllerDescriptor>();
            foreach (Type controllerType in controllerTypes)
            {
                string controllerName = controllerType.Name.Substring(0, controllerType.Name.Length - DefaultHttpControllerSelector.ControllerSuffix.Length);
                var controllerDescriptor = new HttpControllerDescriptor(config, controllerName, controllerType);
                controllerMapping.Add(controllerDescriptor.ControllerName, controllerDescriptor);
            }

            Mock<DefaultHttpControllerSelector> factory = new Mock<DefaultHttpControllerSelector>(config);
            factory.Setup(f => f.GetControllerMapping()).Returns(controllerMapping);
            return factory.Object;
        }
    }
}
