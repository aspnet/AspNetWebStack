// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Facebook.Models;
using Microsoft.AspNet.Facebook.Providers;
using Microsoft.AspNet.Facebook.Realtime;
using Microsoft.TestCommon;

namespace Microsoft.AspNet.Facebook.Test
{
    public class FacebookRealtimeControllerTest
    {
        private const string AppSignatureHeader1 = "sha1=dbb5c0bfaac69ffee7b633e88d85e107fba7ecca";
        private const string AppSecret1 = "f8ad79c0081a80bb885e2b280c3f8442";
        private const string AppSignatureHeader2 = "sha1=3ee3a233ca0c872cae6b40d38a99dff26bf8eb27";
        private const string AppSecret2 = "134cfa3691d1f51c64e70700f397ed20";
        private const string ContentString = "{\"object\":\"user\",\"entry\":[{\"uid\":\"17825901\",\"id\":\"17825901\",\"time\":1352251746,\"changed_fields\":[\"likes\"]}]}";

        [Fact]
        public void Overriding_VerificationToken()
        {
            var userRealTimeController = new UserRealtimeCallbackController();
            Assert.Null(userRealTimeController.VerifyToken);

            userRealTimeController = new UserRealtimeCallbackController(null, "foo");
            Assert.Equal("foo", userRealTimeController.VerifyToken);

            userRealTimeController = new UserRealtimeCallbackController(null, String.Empty);
            Assert.Equal(String.Empty, userRealTimeController.VerifyToken);
        }

        [Theory]
        [InlineData("123456", "foo")]
        [InlineData("654321", "bar")]
        public async Task Get_ReturnsOk_WithValidParameters(string challenge, string verifyToken)
        {
            var userRealTimeController = new UserRealtimeCallbackController(null, verifyToken);
            userRealTimeController.Request = new HttpRequestMessage();
            var subscriptionVerification = new SubscriptionVerification
            {
                Challenge = challenge,
                Mode = "subscribe",
                Verify_Token = verifyToken
            };
            Assert.Equal(challenge, await userRealTimeController.Get(subscriptionVerification).Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData(null, "foo", HttpStatusCode.BadRequest)]
        [InlineData("654321", null, HttpStatusCode.BadRequest)]
        [InlineData("654321", "", HttpStatusCode.BadRequest)]
        [InlineData("", "bar", HttpStatusCode.BadRequest)]
        [InlineData("654321", "bar", HttpStatusCode.OK)]
        public void Get_ReturnsExpectedStatusCode(string challenge, string verifyToken, HttpStatusCode expectedStatusCode)
        {
            var userRealTimeController = new UserRealtimeCallbackController(null, verifyToken);
            userRealTimeController.Request = new HttpRequestMessage();
            var subscriptionVerification = new SubscriptionVerification
            {
                Challenge = challenge,
                Mode = "subscribe",
                Verify_Token = "bar"
            };
            Assert.Equal(expectedStatusCode, userRealTimeController.Get(subscriptionVerification).StatusCode);
        }

        [Theory]
        [InlineData(ContentString, AppSignatureHeader1, AppSecret1)]
        [InlineData(ContentString, AppSignatureHeader2, AppSecret2)]
        public async Task Post_ReturnsOk_WithValidParameters(string contentString, string headerValue, string appSecret)
        {
            var userRealTimeController = new UserRealtimeCallbackController(appSecret, null);
            userRealTimeController.Request = new HttpRequestMessage
            {
                Content = new StringContent(contentString, Encoding.UTF8, "text/json")
            };
            var request = userRealTimeController.Request;
            request.Headers.Add("X-Hub-Signature", headerValue);
            Assert.Equal(HttpStatusCode.OK, (await userRealTimeController.Post()).StatusCode);
        }

        [Theory]
        [InlineData(ContentString, AppSignatureHeader1, AppSecret2)]
        [InlineData(ContentString, AppSignatureHeader2, AppSecret1)]
        [InlineData(ContentString, null, AppSecret2)]
        [InlineData(ContentString, AppSignatureHeader1, null)]
        public async Task Post_ReturnsBadRequest_WithInValidParameters(string contentString, string headerValue, string AppSecret)
        {
            var userRealTimeController = new UserRealtimeCallbackController(AppSecret, null);
            userRealTimeController.Request = new HttpRequestMessage
            {
                Content = new StringContent(contentString, Encoding.UTF8, "text/json")
            };
            var request = userRealTimeController.Request;
            if (headerValue != null)
            {
                request.Headers.Add("X-Hub-Signature", headerValue);
            }
            Assert.Equal(HttpStatusCode.BadRequest, (await userRealTimeController.Post()).StatusCode);
        }

        private sealed class UserRealtimeCallbackController : FacebookRealtimeUpdateController
        {
            private string _verifyToken;

            public UserRealtimeCallbackController() : this(null, null) { }

            public UserRealtimeCallbackController(string appSecret, string verifyToken)
            {
                // Avoid base class's fallback to static GlobalFacebookConfiguration.
                var config = new FacebookConfiguration();
                config.ClientProvider = new DefaultFacebookClientProvider(config);
                config.PermissionService = new DefaultFacebookPermissionService(config);
                config.AppSecret = appSecret;

                FacebookConfiguration = config;
                _verifyToken = verifyToken;
            }

            public override string VerifyToken
            {
                get
                {
                    return _verifyToken;
                }
            }

            public override Task HandleUpdateAsync(ChangeNotification notification)
            {
                return Task.FromResult(0);
            }
        }
    }
}
