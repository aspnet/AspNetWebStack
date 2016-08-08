// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http.Headers;
using System.Web.Http.Description;
using Microsoft.TestCommon;
using ROOT_PROJECT_NAMESPACE.Areas.HelpPage;
using ROOT_PROJECT_NAMESPACE.Areas.HelpPage.Models;

namespace WebApiHelpPageWebHost.UnitTest
{
    public class HelpPageApiModelTest
    {
        [Fact]
        public void Default_Constructor()
        {
            HelpPageApiModel model = new HelpPageApiModel();
            Assert.NotNull(model.SampleRequests);
            Assert.NotNull(model.SampleResponses);
            Assert.NotNull(model.ErrorMessages);
            Assert.Null(model.ApiDescription);
        }

        [Fact]
        public void ApiDescription_Property()
        {
            HelpPageApiModel model = new HelpPageApiModel();
            ApiDescription description = new ApiDescription();
            model.ApiDescription = description;
            Assert.NotNull(model.ApiDescription);
            Assert.Same(description, model.ApiDescription);
        }

        [Fact]
        public void ErrorMessages_Property()
        {
            HelpPageApiModel model = new HelpPageApiModel();
            string error = "an error";
            model.ErrorMessages.Add(error);
            Assert.NotEmpty(model.ErrorMessages);
            Assert.Same(error, model.ErrorMessages[0]);
        }

        [Fact]
        public void SampleRequests_Property()
        {
            HelpPageApiModel model = new HelpPageApiModel();
            ImageSample sample = new ImageSample("http://host/image.png");
            model.SampleRequests.Add(new MediaTypeHeaderValue("text/plain"), sample);
            object sampleRequest;
            model.SampleRequests.TryGetValue(new MediaTypeHeaderValue("text/plain"), out sampleRequest);
            Assert.NotEmpty(model.SampleRequests);
            Assert.Same(sample, sampleRequest);
        }

        [Fact]
        public void SampleResponses_Property()
        {
            HelpPageApiModel model = new HelpPageApiModel();
            InvalidSample sample = new InvalidSample("invalid");
            model.SampleResponses.Add(new MediaTypeHeaderValue("text/xml"), sample);
            object sampleResponse;
            model.SampleResponses.TryGetValue(new MediaTypeHeaderValue("text/xml"), out sampleResponse);
            Assert.NotEmpty(model.SampleResponses);
            Assert.Same(sample, sampleResponse);
        }
    }
}
