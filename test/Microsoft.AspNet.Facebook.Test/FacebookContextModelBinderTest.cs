// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Specialized;
using System.Web.Mvc;
using Microsoft.AspNet.Facebook.ModelBinders;
using Microsoft.AspNet.Facebook.Providers;
using Microsoft.AspNet.Facebook.Test.Helpers;
using Microsoft.TestCommon;

namespace Microsoft.AspNet.Facebook.Test
{
    public class FacebookContextModelBinderTest
    {
        [Fact]
        public void Constructor_ThrowsArgumentNullException()
        {
            Assert.ThrowsArgumentNull(() => new FacebookContextModelBinder(null), "config");
        }

        [Fact]
        public void BindModel_ReturnsExpectedFacebookContext_WhenSignedRequestComesFromForm()
        {
            FacebookConfiguration config = new FacebookConfiguration();
            config.AppSecret = "3e29b24f825e737d97aed5eb62df5076";
            config.ClientProvider = new DefaultFacebookClientProvider(config);
            FacebookContextModelBinder contextBinder = new FacebookContextModelBinder(config);
            ControllerContext controllerContext = MockHelpers.CreateControllerContext(new NameValueCollection
            {
                {"signed_request", "x1yDEgacN3N5iu23Ji8NLYp9LGO1-cUXKHTJQrMqzVQ.eyJhbGdvcml0aG0iOiJITUFDLVNIQTI1NiIsImV4cGlyZXMiOjEzNTM5MTMyMDAsImlzc3VlZF9hdCI6MTM1MzkwNzQ5Miwib2F1dGhfdG9rZW4iOiJBQUFGUlJPcWtwZ01CQURBSjNQZk5vNldXMlJ5WkFSQ1hjU0daQlhpNTBLTG9wRzFwYmFwc2M2aThKY3h6WkFQN1pDSnlpcXVHYlc3WXlCam1aQjh0UWpyelZ2VTNrYm44b3N3WXR5czkzTWdaRFpEIiwidXNlciI6eyJjb3VudHJ5IjoidXMiLCJsb2NhbGUiOiJlbl9VUyIsImFnZSI6eyJtaW4iOjIxfX0sInVzZXJfaWQiOiIxNzgyNTkwMSJ9"}
            });
            ModelBindingContext modelBindingContext = new ModelBindingContext();

            FacebookContext context = Assert.IsType<FacebookContext>(contextBinder.BindModel(controllerContext, modelBindingContext));

            Assert.NotNull((object)context.SignedRequest);
            Assert.NotNull(context.AccessToken);
            Assert.Equal("17825901", context.UserId);
        }

        [Fact]
        public void BindModel_ReturnsExpectedFacebookContext_WhenSignedRequestComesFromQuery()
        {
            FacebookConfiguration config = new FacebookConfiguration();
            config.AppSecret = "3e29b24f825e737d97aed5eb62df5076";
            config.ClientProvider = new DefaultFacebookClientProvider(config);
            FacebookContextModelBinder contextBinder = new FacebookContextModelBinder(config);
            ControllerContext controllerContext = MockHelpers.CreateControllerContext(
                null,
                new NameValueCollection
                {
                    {"signed_request", "x1yDEgacN3N5iu23Ji8NLYp9LGO1-cUXKHTJQrMqzVQ.eyJhbGdvcml0aG0iOiJITUFDLVNIQTI1NiIsImV4cGlyZXMiOjEzNTM5MTMyMDAsImlzc3VlZF9hdCI6MTM1MzkwNzQ5Miwib2F1dGhfdG9rZW4iOiJBQUFGUlJPcWtwZ01CQURBSjNQZk5vNldXMlJ5WkFSQ1hjU0daQlhpNTBLTG9wRzFwYmFwc2M2aThKY3h6WkFQN1pDSnlpcXVHYlc3WXlCam1aQjh0UWpyelZ2VTNrYm44b3N3WXR5czkzTWdaRFpEIiwidXNlciI6eyJjb3VudHJ5IjoidXMiLCJsb2NhbGUiOiJlbl9VUyIsImFnZSI6eyJtaW4iOjIxfX0sInVzZXJfaWQiOiIxNzgyNTkwMSJ9"}
                });

            FacebookContext context = Assert.IsType<FacebookContext>(contextBinder.BindModel(controllerContext, new ModelBindingContext()));

            Assert.NotNull((object)context.SignedRequest);
            Assert.NotNull(context.AccessToken);
            Assert.Equal("17825901", context.UserId);
        }

        [Fact]
        public void BindModel_ReturnsInvalidModelState_WhenSignedRequestIsNull()
        {
            FacebookConfiguration config = new FacebookConfiguration();
            config.AppSecret = "abcdef";
            config.ClientProvider = new DefaultFacebookClientProvider(config);
            FacebookContextModelBinder contextBinder = new FacebookContextModelBinder(config);
            ControllerContext controllerContext = MockHelpers.CreateControllerContext();
            ModelBindingContext modelBindingContext = new ModelBindingContext();

            object context = contextBinder.BindModel(controllerContext, modelBindingContext);

            Assert.Null(context);
            Assert.False(modelBindingContext.ModelState.IsValid);
        }
    }
}