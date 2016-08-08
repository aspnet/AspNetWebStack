// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.TestUtil;
using Microsoft.TestCommon;

namespace Microsoft.Web.Helpers.Test
{
    public class AnalyticsTest
    {
        [Fact]
        public void GetYahooAnalyticsHtmlTest()
        {
            string account = "My_yahoo_account";
            string actual = Analytics.GetYahooHtml(account).ToString();
            Assert.True(actual.Contains(".yahoo.com") && actual.Contains("My_yahoo_account"));
        }

        [Fact]
        public void GetStatCounterAnalyticsHtmlTest()
        {
            int project = 31553;
            string security = "stat_security";
            string actual = Analytics.GetStatCounterHtml(project, security).ToString();
            Assert.True(actual.Contains("statcounter.com/counter/counter_xhtml.js") &&
                        actual.Contains(project.ToString()) && actual.Contains(security));
        }

        [Fact]
        public void GetGoogleAnalyticsHtmlTest()
        {
            string account = "My_google_account";
            string actual = Analytics.GetGoogleHtml(account).ToString();
            Assert.True(actual.Contains("google-analytics.com/ga.js") && actual.Contains("My_google_account"));
        }

        [Fact]
        public void GetGoogleAnalyticsEscapesJavascript()
        {
            string account = "My_\"google_account";
            string actual = Analytics.GetGoogleHtml(account).ToString();
            string expected = "<script type=\"text/javascript\">\n" +
                              "var gaJsHost = ((\"https:\" == document.location.protocol) ? \"https://ssl.\" : \"http://www.\");\n" +
                              "document.write(unescape(\"%3Cscript src='\" + gaJsHost + \"google-analytics.com/ga.js' type='text/javascript'%3E%3C/script%3E\"));\n" +
                              "</script>\n" +
                              "<script type=\"text/javascript\">\n" +
                              "try{\n" +
                              "var pageTracker = _gat._getTracker(\"My_\\\"google_account\");\n" +
                              "pageTracker._trackPageview();\n" +
                              "} catch(err) {}\n" +
                              "</script>\n";
            UnitTestHelper.AssertEqualsIgnoreWhitespace(expected, actual);
        }

        [Fact]
        public void GetGoogleAnalyticsAsyncHtmlTest()
        {
            string account = "My_google_account";
            string actual = Analytics.GetGoogleAsyncHtml(account).ToString();
            Assert.True(actual.Contains("google-analytics.com/ga.js") && actual.Contains("My_google_account"));
        }

        [Fact]
        public void GetGoogleAnalyticsAsyncHtmlEscapesJavaScript()
        {
            string account = "My_\"google_account";
            string actual = Analytics.GetGoogleAsyncHtml(account).ToString();
            string expected = "<script type=\"text/javascript\">\n" +
                              "var _gaq = _gaq || [];\n" +
                              "_gaq.push(['_setAccount', 'My_\\\"google_account']);\n" +
                              "_gaq.push(['_trackPageview']);\n" +
                              "(function() {\n" +
                              "var ga = document.createElement('script'); ga.type = 'text/javascript'; ga.async = true;\n" +
                              "ga.src = ('https:' == document.location.protocol ? 'https://ssl' : 'http://www') + '.google-analytics.com/ga.js';\n" +
                              "var s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(ga, s);\n" +
                              "})();\n" +
                              "</script>\n";
            UnitTestHelper.AssertEqualsIgnoreWhitespace(expected, actual);
        }

        [Fact]
        public void GetYahooAnalyticsEscapesJavascript()
        {
            string account = "My_\"yahoo_account";
            string actual = Analytics.GetYahooHtml(account).ToString();
            string expected = "<script type=\"text/javascript\">\n" +
                              "window.ysm_customData = new Object();\n" +
                              "window.ysm_customData.conversion = \"transId=,currency=,amount=\";\n" +
                              "var ysm_accountid = \"My_\\\"yahoo_account\";\n" +
                              "document.write(\"<SCR\" + \"IPT language='JavaScript' type='text/javascript' \"\n" +
                              "+ \"SRC=//\" + \"srv3.wa.marketingsolutions.yahoo.com\" + \"/script/ScriptServlet\" + \"?aid=\" + ysm_accountid\n" +
                              "+ \"></SCR\" + \"IPT>\");\n" +
                              "</script>\n";
            UnitTestHelper.AssertEqualsIgnoreWhitespace(expected, actual);
        }

        [Fact]
        public void GetStatCounterAnalyticsEscapesCorrectly()
        {
            string account = "My_\"stat_account";
            string actual = Analytics.GetStatCounterHtml(2, account).ToString();
            string expected = "<script type=\"text/javascript\">\n" +
                              "var sc_project=2;\n" +
                              "var sc_invisible=1;\n" +
                              "var sc_security=\"My_\\\"stat_account\";\n" +
                              "var sc_text=2;\n" +
                              "var sc_https=1;\n" +
                              "var scJsHost = ((\"https:\" == document.location.protocol) ? \"https://secure.\" : \"http://www.\");\n" +
                              "document.write(\"<sc\" + \"ript type='text/javascript' src='\" + " +
                              "scJsHost + \"statcounter.com/counter/counter_xhtml.js'></\" + \"script>\");\n" +
                              "</script>\n\n" +
                              "<noscript>" +
                              "<div class=\"statcounter\">" +
                              "<a title=\"tumblrstatistics\" class=\"statcounter\" href=\"http://www.statcounter.com/tumblr/\">" +
                              "<img class=\"statcounter\" src=\"https://c.statcounter.com/2/0/My_&quot;stat_account/1/\" alt=\"tumblr statistics\"/>" +
                              "</a>" +
                              "</div>" +
                              "</noscript>";

            UnitTestHelper.AssertEqualsIgnoreWhitespace(expected, actual);
        }
    }
}
