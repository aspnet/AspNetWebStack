// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.TestCommon;
using Moq;

namespace Microsoft.Web.Helpers.Test
{
    public class VideoTest
    {
        private VirtualPathUtilityWrapper _pathUtility = new VirtualPathUtilityWrapper();

        [Fact]
        public void FlashCannotOverrideHtmlAttributes()
        {
            Assert.ThrowsArgument(() => { Video.Flash(GetContext(), _pathUtility, "http://foo.bar.com/foo.swf", htmlAttributes: new { cLASSid = "CanNotOverride" }); }, "htmlAttributes", "Property \"cLASSid\" cannot be set through this argument.");
        }

        [Fact]
        public void FlashDefaults()
        {
            string html = Video.Flash(GetContext(), _pathUtility, "http://foo.bar.com/foo.swf").ToString().Replace("\r\n", "");
            Assert.True(html.StartsWith(
                "<object classid=\"clsid:d27cdb6e-ae6d-11cf-96b8-444553540000\" " +
                "codebase=\"http://download.macromedia.com/pub/shockwave/cabs/flash/swflash.cab\" type=\"application/x-oleobject\" >"
                            ));
            Assert.True(html.Contains("<param name=\"movie\" value=\"http://foo.bar.com/foo.swf\" />"));
            Assert.True(html.Contains("<embed src=\"http://foo.bar.com/foo.swf\" type=\"application/x-shockwave-flash\" />"));
            Assert.True(html.EndsWith("</object>"));
        }

        [Fact]
        public void FlashThrowsWhenPathIsEmpty()
        {
            Assert.ThrowsArgumentNullOrEmptyString(() => { Video.Flash(GetContext(), _pathUtility, String.Empty); }, "path");
        }

        [Fact]
        public void FlashThrowsWhenPathIsNull()
        {
            Assert.ThrowsArgumentNullOrEmptyString(() => { Video.Flash(GetContext(), _pathUtility, null); }, "path");
        }

        [Fact]
        public void FlashWithExposedOptions()
        {
            string html = Video.Flash(GetContext(), _pathUtility, "http://foo.bar.com/foo.swf", width: "100px", height: "100px",
                                      play: false, loop: false, menu: false, backgroundColor: "#000", quality: "Q", scale: "S", windowMode: "WM",
                                      baseUrl: "http://foo.bar.com/", version: "1.0.0.0", htmlAttributes: new { id = "fl" }, embedName: "efl").ToString().Replace("\r\n", "");

            Assert.True(html.StartsWith(
                "<object classid=\"clsid:d27cdb6e-ae6d-11cf-96b8-444553540000\" " +
                "codebase=\"http://download.macromedia.com/pub/shockwave/cabs/flash/swflash.cab#version=1,0,0,0\" " +
                "height=\"100px\" id=\"fl\" type=\"application/x-oleobject\" width=\"100px\" >"
                            ));
            Assert.True(html.Contains("<param name=\"play\" value=\"False\" />"));
            Assert.True(html.Contains("<param name=\"loop\" value=\"False\" />"));
            Assert.True(html.Contains("<param name=\"menu\" value=\"False\" />"));
            Assert.True(html.Contains("<param name=\"bgColor\" value=\"#000\" />"));
            Assert.True(html.Contains("<param name=\"quality\" value=\"Q\" />"));
            Assert.True(html.Contains("<param name=\"scale\" value=\"S\" />"));
            Assert.True(html.Contains("<param name=\"wmode\" value=\"WM\" />"));
            Assert.True(html.Contains("<param name=\"base\" value=\"http://foo.bar.com/\" />"));

            var embed = new Regex("<embed.*/>").Match(html);
            Assert.True(embed.Success);
            Assert.True(embed.Value.StartsWith("<embed src=\"http://foo.bar.com/foo.swf\" width=\"100px\" height=\"100px\" name=\"efl\" type=\"application/x-shockwave-flash\" "));
            Assert.True(embed.Value.Contains("play=\"False\""));
            Assert.True(embed.Value.Contains("loop=\"False\""));
            Assert.True(embed.Value.Contains("menu=\"False\""));
            Assert.True(embed.Value.Contains("bgColor=\"#000\""));
            Assert.True(embed.Value.Contains("quality=\"Q\""));
            Assert.True(embed.Value.Contains("scale=\"S\""));
            Assert.True(embed.Value.Contains("wmode=\"WM\""));
            Assert.True(embed.Value.Contains("base=\"http://foo.bar.com/\""));
        }

        [Fact]
        public void FlashWithUnexposedOptions()
        {
            string html = Video.Flash(GetContext(), _pathUtility, "http://foo.bar.com/foo.swf", options: new { X = "Y", Z = 123 }).ToString().Replace("\r\n", "");
            Assert.True(html.Contains("<param name=\"X\" value=\"Y\" />"));
            Assert.True(html.Contains("<param name=\"Z\" value=\"123\" />"));
            // note - can't guarantee order of optional params:
            Assert.True(
                html.Contains("<embed src=\"http://foo.bar.com/foo.swf\" type=\"application/x-shockwave-flash\" X=\"Y\" Z=\"123\" />") ||
                html.Contains("<embed src=\"http://foo.bar.com/foo.swf\" type=\"application/x-shockwave-flash\" Z=\"123\" X=\"Y\" />")
                );
        }

        [Fact]
        public void MediaPlayerCannotOverrideHtmlAttributes()
        {
            Assert.ThrowsArgument(() => { Video.MediaPlayer(GetContext(), _pathUtility, "http://foo.bar.com/foo.wmv", htmlAttributes: new { cODEbase = "CanNotOverride" }); }, "htmlAttributes", "Property \"cODEbase\" cannot be set through this argument.");
        }

        [Fact]
        public void MediaPlayerDefaults()
        {
            string html = Video.MediaPlayer(GetContext(), _pathUtility, "http://foo.bar.com/foo.wmv").ToString().Replace("\r\n", "");
            Assert.True(html.StartsWith(
                "<object classid=\"clsid:6BF52A52-394A-11D3-B153-00C04F79FAA6\" >"
                            ));
            Assert.True(html.Contains("<param name=\"URL\" value=\"http://foo.bar.com/foo.wmv\" />"));
            Assert.True(html.Contains("<embed src=\"http://foo.bar.com/foo.wmv\" type=\"application/x-mplayer2\" />"));
            Assert.True(html.EndsWith("</object>"));
        }

        [Fact]
        public void MediaPlayerThrowsWhenPathIsEmpty()
        {
            Assert.ThrowsArgumentNullOrEmptyString(() => { Video.MediaPlayer(GetContext(), _pathUtility, String.Empty); }, "path");
        }

        [Fact]
        public void MediaPlayerThrowsWhenPathIsNull()
        {
            Assert.ThrowsArgumentNullOrEmptyString(() => { Video.MediaPlayer(GetContext(), _pathUtility, null); }, "path");
        }

        [Fact]
        public void MediaPlayerWithExposedOptions()
        {
            string html = Video.MediaPlayer(GetContext(), _pathUtility, "http://foo.bar.com/foo.wmv", width: "100px", height: "100px",
                                            autoStart: false, playCount: 2, uiMode: "UIMODE", stretchToFit: true, enableContextMenu: false, mute: true,
                                            volume: 1, baseUrl: "http://foo.bar.com/", htmlAttributes: new { id = "mp" }, embedName: "emp").ToString().Replace("\r\n", "");
            Assert.True(html.StartsWith(
                "<object classid=\"clsid:6BF52A52-394A-11D3-B153-00C04F79FAA6\" height=\"100px\" id=\"mp\" width=\"100px\" >"
                            ));
            Assert.True(html.Contains("<param name=\"URL\" value=\"http://foo.bar.com/foo.wmv\" />"));
            Assert.True(html.Contains("<param name=\"autoStart\" value=\"False\" />"));
            Assert.True(html.Contains("<param name=\"playCount\" value=\"2\" />"));
            Assert.True(html.Contains("<param name=\"uiMode\" value=\"UIMODE\" />"));
            Assert.True(html.Contains("<param name=\"stretchToFit\" value=\"True\" />"));
            Assert.True(html.Contains("<param name=\"enableContextMenu\" value=\"False\" />"));
            Assert.True(html.Contains("<param name=\"mute\" value=\"True\" />"));
            Assert.True(html.Contains("<param name=\"volume\" value=\"1\" />"));
            Assert.True(html.Contains("<param name=\"baseURL\" value=\"http://foo.bar.com/\" />"));

            var embed = new Regex("<embed.*/>").Match(html);
            Assert.True(embed.Success);
            Assert.True(embed.Value.StartsWith("<embed src=\"http://foo.bar.com/foo.wmv\" width=\"100px\" height=\"100px\" name=\"emp\" type=\"application/x-mplayer2\" "));
            Assert.True(embed.Value.Contains("autoStart=\"False\""));
            Assert.True(embed.Value.Contains("playCount=\"2\""));
            Assert.True(embed.Value.Contains("uiMode=\"UIMODE\""));
            Assert.True(embed.Value.Contains("stretchToFit=\"True\""));
            Assert.True(embed.Value.Contains("enableContextMenu=\"False\""));
            Assert.True(embed.Value.Contains("mute=\"True\""));
            Assert.True(embed.Value.Contains("volume=\"1\""));
            Assert.True(embed.Value.Contains("baseURL=\"http://foo.bar.com/\""));
        }

        [Fact]
        public void MediaPlayerWithUnexposedOptions()
        {
            string html = Video.MediaPlayer(GetContext(), _pathUtility, "http://foo.bar.com/foo.wmv", options: new { X = "Y", Z = 123 }).ToString().Replace("\r\n", "");
            Assert.True(html.Contains("<param name=\"X\" value=\"Y\" />"));
            Assert.True(html.Contains("<param name=\"Z\" value=\"123\" />"));
            Assert.True(
                html.Contains("<embed src=\"http://foo.bar.com/foo.wmv\" type=\"application/x-mplayer2\" X=\"Y\" Z=\"123\" />") ||
                html.Contains("<embed src=\"http://foo.bar.com/foo.wmv\" type=\"application/x-mplayer2\" Z=\"123\" X=\"Y\" />")
                );
        }

        [Fact]
        public void SilverlightCannotOverrideHtmlAttributes()
        {
            Assert.ThrowsArgument(() =>
            {
                Video.Silverlight(GetContext(), _pathUtility, "http://foo.bar.com/foo.xap", "100px", "100px",
                                  htmlAttributes: new { WIDTH = "CanNotOverride" });
            }, "htmlAttributes", "Property \"WIDTH\" cannot be set through this argument.");
        }

        [Fact]
        public void SilverlightDefaults()
        {
            string html = Video.Silverlight(GetContext(), _pathUtility, "http://foo.bar.com/foo.xap", "100px", "100px").ToString().Replace("\r\n", "");
            Assert.True(html.StartsWith(
                "<object data=\"data:application/x-silverlight-2,\" height=\"100px\" type=\"application/x-silverlight-2\" " +
                "width=\"100px\" >"
                            ));
            Assert.True(html.Contains("<param name=\"source\" value=\"http://foo.bar.com/foo.xap\" />"));
            Assert.True(html.Contains(
                "<a href=\"http://go.microsoft.com/fwlink/?LinkID=149156\" style=\"text-decoration:none\">" +
                "<img src=\"http://go.microsoft.com/fwlink?LinkId=108181\" alt=\"Get Microsoft Silverlight\" " +
                "style=\"border-style:none\"/></a>"));
            Assert.True(html.EndsWith("</object>"));
        }

        [Fact]
        public void SilverlightThrowsWhenPathIsEmpty()
        {
            Assert.ThrowsArgumentNullOrEmptyString(() => { Video.Silverlight(GetContext(), _pathUtility, String.Empty, "100px", "100px"); }, "path");
        }

        [Fact]
        public void SilverlightThrowsWhenPathIsNull()
        {
            Assert.ThrowsArgumentNullOrEmptyString(() => { Video.Silverlight(GetContext(), _pathUtility, null, "100px", "100px"); }, "path");
        }

        [Fact]
        public void SilverlightThrowsWhenHeightIsEmpty()
        {
            Assert.ThrowsArgumentNullOrEmptyString(() => { Video.Silverlight(GetContext(), _pathUtility, "http://foo.bar.com/foo.xap", "100px", String.Empty); }, "height");
        }

        [Fact]
        public void SilverlightThrowsWhenHeightIsNull()
        {
            Assert.ThrowsArgumentNullOrEmptyString(() => { Video.Silverlight(GetContext(), _pathUtility, "http://foo.bar.com/foo.xap", "100px", null); }, "height");
        }

        [Fact]
        public void SilverlightThrowsWhenWidthIsEmpty()
        {
            Assert.ThrowsArgumentNullOrEmptyString(() => { Video.Silverlight(GetContext(), _pathUtility, "http://foo.bar.com/foo.xap", String.Empty, "100px"); }, "width");
        }

        [Fact]
        public void SilverlightThrowsWhenWidthIsNull()
        {
            Assert.ThrowsArgumentNullOrEmptyString(() => { Video.Silverlight(GetContext(), _pathUtility, "http://foo.bar.com/foo.xap", null, "100px"); }, "width");
        }

        [Fact]
        public void SilverlightWithExposedOptions()
        {
            string html = Video.Silverlight(GetContext(), _pathUtility, "http://foo.bar.com/foo.xap", width: "85%", height: "85%",
                                            backgroundColor: "red", initParameters: "X=Y", minimumVersion: "1.0.0.0", autoUpgrade: false,
                                            htmlAttributes: new { id = "sl" }).ToString().Replace("\r\n", "");
            Assert.True(html.StartsWith(
                "<object data=\"data:application/x-silverlight-2,\" height=\"85%\" id=\"sl\" " +
                "type=\"application/x-silverlight-2\" width=\"85%\" >"
                            ));
            Assert.True(html.Contains("<param name=\"background\" value=\"red\" />"));
            Assert.True(html.Contains("<param name=\"initparams\" value=\"X=Y\" />"));
            Assert.True(html.Contains("<param name=\"minruntimeversion\" value=\"1.0.0.0\" />"));
            Assert.True(html.Contains("<param name=\"autoUpgrade\" value=\"False\" />"));

            var embed = new Regex("<embed.*/>").Match(html);
            Assert.False(embed.Success);
        }

        [Fact]
        public void SilverlightWithUnexposedOptions()
        {
            string html = Video.Silverlight(GetContext(), _pathUtility, "http://foo.bar.com/foo.xap", width: "50px", height: "50px",
                                            options: new { X = "Y", Z = 123 }).ToString().Replace("\r\n", "");
            Assert.True(html.Contains("<param name=\"X\" value=\"Y\" />"));
            Assert.True(html.Contains("<param name=\"Z\" value=\"123\" />"));
        }

        [Fact]
        public void ValidatePathResolvesExistingLocalPath()
        {
            string path = Assembly.GetExecutingAssembly().Location;
            Mock<VirtualPathUtilityBase> pathUtility = new Mock<VirtualPathUtilityBase>();
            pathUtility.Setup(p => p.Combine(It.IsAny<string>(), It.IsAny<string>())).Returns(path);
            pathUtility.Setup(p => p.ToAbsolute(It.IsAny<string>())).Returns(path);

            Mock<HttpServerUtilityBase> serverMock = new Mock<HttpServerUtilityBase>();
            serverMock.Setup(s => s.MapPath(It.IsAny<string>())).Returns(path);
            HttpContextBase context = GetContext(serverMock.Object);

            string html = Video.Flash(context, pathUtility.Object, "foo.bar").ToString();
            Assert.True(html.StartsWith("<object"));
            Assert.True(html.Contains(HttpUtility.HtmlAttributeEncode(HttpUtility.UrlPathEncode(path))));
        }

        [Fact]
        public void ValidatePathThrowsForNonExistingLocalPath()
        {
            string path = "c:\\does\\not\\exist.swf";
            Mock<VirtualPathUtilityBase> pathUtility = new Mock<VirtualPathUtilityBase>();
            pathUtility.Setup(p => p.Combine(It.IsAny<string>(), It.IsAny<string>())).Returns(path);
            pathUtility.Setup(p => p.ToAbsolute(It.IsAny<string>())).Returns(path);

            Mock<HttpServerUtilityBase> serverMock = new Mock<HttpServerUtilityBase>();
            serverMock.Setup(s => s.MapPath(It.IsAny<string>())).Returns(path);
            HttpContextBase context = GetContext(serverMock.Object);

            Assert.Throws<InvalidOperationException>(() => { Video.Flash(context, pathUtility.Object, "exist.swf"); }, "The media file \"exist.swf\" does not exist.");
        }

        private static HttpContextBase GetContext(HttpServerUtilityBase serverUtility = null)
        {
            // simple mocked context - won't reference as long as path starts with 'http'
            Mock<HttpRequestBase> requestMock = new Mock<HttpRequestBase>();
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>();
            contextMock.Setup(context => context.Request).Returns(requestMock.Object);
            contextMock.Setup(context => context.Server).Returns(serverUtility);
            return contextMock.Object;
        }
    }
}
