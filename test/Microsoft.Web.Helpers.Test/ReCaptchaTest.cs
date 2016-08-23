// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Web.Helpers.Test;
using System.Web.TestUtil;
using System.Web.WebPages.Scope;
using Microsoft.TestCommon;
using Moq;

namespace Microsoft.Web.Helpers.Test
{
    [Xunit.Collection("Uses ScopeStorage or ViewEngines.Engines")]
    public class ReCaptchaTest : IDisposable
    {
        [Fact]
        public void ReCaptchaOptionsMissingWhenNoOptionsAndDefaultRendering()
        {
            var html = ReCaptcha.GetHtml(GetContext(), "PUBLIC_KEY");
            UnitTestHelper.AssertEqualsIgnoreWhitespace(
                @"<script src=""http://www.google.com/recaptcha/api/challenge?k=PUBLIC_KEY"" type=""text/javascript""></script>" +
                @"<noscript>" +
                @"<iframe frameborder=""0"" height=""300px"" src=""http://www.google.com/recaptcha/api/noscript?k=PUBLIC_KEY"" width=""500px""></iframe><br/><br/>" +
                @"<textarea cols=""40"" name=""recaptcha_challenge_field"" rows=""3""></textarea>" +
                @"<input name=""recaptcha_response_field"" type=""hidden"" value=""manual_challenge""/>" +
                @"</noscript>",
                html.ToString());
            XhtmlAssert.Validate1_0(html, addRoot: true);
        }

        [Fact]
        public void ReCaptchaOptionsWhenOneOptionAndDefaultRendering()
        {
            var html = ReCaptcha.GetHtml(GetContext(), "PUBLIC_KEY", options: new { theme = "white" });
            UnitTestHelper.AssertEqualsIgnoreWhitespace(
                @"<script type=""text/javascript"">var RecaptchaOptions={""theme"":""white""};</script>" +
                @"<script src=""http://www.google.com/recaptcha/api/challenge?k=PUBLIC_KEY"" type=""text/javascript""></script>" +
                @"<noscript>" +
                @"<iframe frameborder=""0"" height=""300px"" src=""http://www.google.com/recaptcha/api/noscript?k=PUBLIC_KEY"" width=""500px""></iframe><br/><br/>" +
                @"<textarea cols=""40"" name=""recaptcha_challenge_field"" rows=""3""></textarea>" +
                @"<input name=""recaptcha_response_field"" type=""hidden"" value=""manual_challenge""/>" +
                @"</noscript>",
                html.ToString());
            XhtmlAssert.Validate1_0(html, addRoot: true);
        }

        [Fact]
        public void ReCaptchaOptionsWhenMultipleOptionsAndDefaultRendering()
        {
            var html = ReCaptcha.GetHtml(GetContext(), "PUBLIC_KEY", options: new { theme = "white", tabindex = 5 });
            UnitTestHelper.AssertEqualsIgnoreWhitespace(
                @"<script type=""text/javascript"">var RecaptchaOptions={""theme"":""white"",""tabindex"":5};</script>" +
                @"<script src=""http://www.google.com/recaptcha/api/challenge?k=PUBLIC_KEY"" type=""text/javascript""></script>" +
                @"<noscript>" +
                @"<iframe frameborder=""0"" height=""300px"" src=""http://www.google.com/recaptcha/api/noscript?k=PUBLIC_KEY"" width=""500px""></iframe><br/><br/>" +
                @"<textarea cols=""40"" name=""recaptcha_challenge_field"" rows=""3""></textarea>" +
                @"<input name=""recaptcha_response_field"" type=""hidden"" value=""manual_challenge""/>" +
                @"</noscript>",
                html.ToString());
            XhtmlAssert.Validate1_0(html, addRoot: true);
        }

        [Fact]
        public void ReCaptchaOptionsWhenMultipleOptionsFromDictionaryAndDefaultRendering()
        {
            // verifies that a dictionary will serialize the same as a projection
            var options = new Dictionary<string, object> { { "theme", "white" }, { "tabindex", 5 } };
            var html = ReCaptcha.GetHtml(GetContext(), "PUBLIC_KEY", options: options);
            UnitTestHelper.AssertEqualsIgnoreWhitespace(
                @"<script type=""text/javascript"">var RecaptchaOptions={""theme"":""white"",""tabindex"":5};</script>" +
                @"<script src=""http://www.google.com/recaptcha/api/challenge?k=PUBLIC_KEY"" type=""text/javascript""></script>" +
                @"<noscript>" +
                @"<iframe frameborder=""0"" height=""300px"" src=""http://www.google.com/recaptcha/api/noscript?k=PUBLIC_KEY"" width=""500px""></iframe><br/><br/>" +
                @"<textarea cols=""40"" name=""recaptcha_challenge_field"" rows=""3""></textarea>" +
                @"<input name=""recaptcha_response_field"" type=""hidden"" value=""manual_challenge""/>" +
                @"</noscript>",
                html.ToString());
            XhtmlAssert.Validate1_0(html, addRoot: true);
        }

        [Fact]
        public void RenderUsesLastError()
        {
            HttpContextBase context = GetContext();
            ReCaptcha.HandleValidateResponse(context, "false\nincorrect-captcha-sol");
            var html = ReCaptcha.GetHtml(context, "PUBLIC_KEY");
            UnitTestHelper.AssertEqualsIgnoreWhitespace(
                @"<script src=""http://www.google.com/recaptcha/api/challenge?k=PUBLIC_KEY&amp;error=incorrect-captcha-sol"" type=""text/javascript""></script>" +
                @"<noscript>" +
                @"<iframe frameborder=""0"" height=""300px"" src=""http://www.google.com/recaptcha/api/noscript?k=PUBLIC_KEY"" width=""500px""></iframe><br/><br/>" +
                @"<textarea cols=""40"" name=""recaptcha_challenge_field"" rows=""3""></textarea>" +
                @"<input name=""recaptcha_response_field"" type=""hidden"" value=""manual_challenge""/>" +
                @"</noscript>",
                html.ToString());
            XhtmlAssert.Validate1_0(html, addRoot: true);
        }

        [Fact]
        public void RenderWhenConnectionIsSecure()
        {
            var html = ReCaptcha.GetHtml(GetContext(isSecure: true), "PUBLIC_KEY");
            UnitTestHelper.AssertEqualsIgnoreWhitespace(
                @"<script src=""https://www.google.com/recaptcha/api/challenge?k=PUBLIC_KEY"" type=""text/javascript""></script>" +
                @"<noscript>" +
                @"<iframe frameborder=""0"" height=""300px"" src=""https://www.google.com/recaptcha/api/noscript?k=PUBLIC_KEY"" width=""500px""></iframe><br/><br/>" +
                @"<textarea cols=""40"" name=""recaptcha_challenge_field"" rows=""3""></textarea>" +
                @"<input name=""recaptcha_response_field"" type=""hidden"" value=""manual_challenge""/>" +
                @"</noscript>",
                html.ToString());
            XhtmlAssert.Validate1_0(html, addRoot: true);
        }

        [Fact]
        public void ValidateThrowsWhenRemoteAddressNotAvailable()
        {
            HttpContextBase context = GetContext();
            VirtualPathUtilityBase virtualPathUtility = GetVirtualPathUtility();
            context.Request.Form["recaptcha_challenge_field"] = "CHALLENGE";
            context.Request.Form["recaptcha_response_field"] = "RESPONSE";

            Assert.Throws<InvalidOperationException>(() => { ReCaptcha.Validate(context, privateKey: "PRIVATE_KEY", virtualPathUtility: virtualPathUtility).ToString(); }, "The captcha cannot be validated because the remote address was not found in the request.");
        }

        [Fact]
        public void ValidateReturnsFalseWhenChallengeNotPosted()
        {
            HttpContextBase context = GetContext();
            VirtualPathUtilityBase virtualPathUtility = GetVirtualPathUtility();
            context.Request.ServerVariables["REMOTE_ADDR"] = "127.0.0.1";

            Assert.False(ReCaptcha.Validate(context, privateKey: "PRIVATE_KEY", virtualPathUtility: virtualPathUtility));
        }

        [Fact]
        public void ValidatePostData()
        {
            HttpContextBase context = GetContext();
            VirtualPathUtilityBase virtualPathUtility = GetVirtualPathUtility();
            context.Request.ServerVariables["REMOTE_ADDR"] = "127.0.0.1";
            context.Request.Form["recaptcha_challenge_field"] = "CHALLENGE";
            context.Request.Form["recaptcha_response_field"] = "RESPONSE";

            Assert.Equal("privatekey=PRIVATE_KEY&remoteip=127.0.0.1&challenge=CHALLENGE&response=RESPONSE",
                         ReCaptcha.GetValidatePostData(context, "PRIVATE_KEY", virtualPathUtility));
        }

        [Fact]
        public void ValidatePostDataWhenNoResponse()
        {
            // Arrange
            HttpContextBase context = GetContext();
            VirtualPathUtilityBase virtualPathUtility = GetVirtualPathUtility();
            context.Request.ServerVariables["REMOTE_ADDR"] = "127.0.0.1";
            context.Request.Form["recaptcha_challenge_field"] = "CHALLENGE";

            // Act
            var validatePostData = ReCaptcha.GetValidatePostData(context, "PRIVATE_KEY", virtualPathUtility);

            // Assert
            Assert.Equal("privatekey=PRIVATE_KEY&remoteip=127.0.0.1&challenge=CHALLENGE&response=", validatePostData);
        }

        [Fact]
        public void ValidateResponseReturnsFalseOnEmptyReCaptchaResponse()
        {
            HttpContextBase context = GetContext();
            Assert.False(ReCaptcha.HandleValidateResponse(context, ""));
            Assert.Equal(String.Empty, ReCaptcha.GetLastError(context));
        }

        [Fact]
        public void ValidateResponseReturnsTrueOnSuccess()
        {
            HttpContextBase context = GetContext();
            Assert.True(ReCaptcha.HandleValidateResponse(context, "true\nsuccess"));
            Assert.Equal(String.Empty, ReCaptcha.GetLastError(context));
        }

        [Fact]
        public void ValidateResponseReturnsFalseOnError()
        {
            HttpContextBase context = GetContext();
            Assert.False(ReCaptcha.HandleValidateResponse(context, "false\nincorrect-captcha-sol"));
            Assert.Equal("incorrect-captcha-sol", ReCaptcha.GetLastError(context));
        }

        [Fact]
        public void ReCaptchaPrivateKeyThowsWhenSetToNull()
        {
            Assert.ThrowsArgumentNull(() => ReCaptcha.PrivateKey = null, "value");
        }

        [Fact]
        public void ReCaptchaPrivateKeyUsesScopeStorage()
        {
            // Arrange
            var value = "value";

            // Act
            ReCaptcha.PrivateKey = value;

            // Assert
            Assert.Equal(ReCaptcha.PrivateKey, value);
            Assert.Equal(ScopeStorage.CurrentScope[ReCaptcha._privateKey], value);
        }

        [Fact]
        public void PublicKeyThowsWhenSetToNull()
        {
            Assert.ThrowsArgumentNull(() => ReCaptcha.PublicKey = null, "value");
        }

        [Fact]
        public void ReCaptchaPublicKeyUsesScopeStorage()
        {
            // Arrange
            var value = "value";

            // Act
            ReCaptcha.PublicKey = value;

            // Assert
            Assert.Equal(ReCaptcha.PublicKey, value);
            Assert.Equal(ScopeStorage.CurrentScope[ReCaptcha._publicKey], value);
        }

        private HttpContextBase GetContext(bool isSecure = false)
        {
            // mock HttpRequest
            Mock<HttpRequestBase> requestMock = new Mock<HttpRequestBase>();
            requestMock.Setup(request => request.IsSecureConnection).Returns(isSecure);
            requestMock.Setup(request => request.Form).Returns(new NameValueCollection());
            requestMock.Setup(request => request.ServerVariables).Returns(new NameValueCollection());

            // mock HttpContext
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>();
            contextMock.Setup(context => context.Items).Returns(new Hashtable());
            contextMock.Setup(context => context.Request).Returns(requestMock.Object);
            return contextMock.Object;
        }

        private static VirtualPathUtilityBase GetVirtualPathUtility()
        {
            var virtualPathUtility = new Mock<VirtualPathUtilityBase>();
            virtualPathUtility.Setup(c => c.ToAbsolute(It.IsAny<string>())).Returns<string>(_ => _);

            return virtualPathUtility.Object;
        }

        public void Dispose()
        {
            // Reset ScopeStorage (written via e.g. ReCaptcha.PrivateKey) between tests to avoid unexpected interactions.
            ScopeStorage.CurrentProvider = new StaticScopeStorageProvider();
            ScopeStorage.GlobalScope.Clear();
        }
    }
}
