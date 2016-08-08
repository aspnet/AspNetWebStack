// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.WebPages;
using System.Xml;
using System.Xml.Resolvers;
using Microsoft.TestCommon;

namespace System.Web.Helpers.Test
{
    // see: http://msdn.microsoft.com/en-us/library/hdf992b8(v=VS.100).aspx
    // see: http://blogs.msdn.com/xmlteam/archive/2008/08/14/introducing-the-xmlpreloadedresolver.aspx
    public class XhtmlAssert
    {
        const string Xhtml10Wrapper = "<html xmlns=\"http://www.w3.org/1999/xhtml\"><head></head><body>{0}</body></html>";
        const string DOCTYPE_XHTML1_1 = "<!DOCTYPE {0} PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"xhtml11-flat.dtd\">\r\n";

        public static void Validate1_0(object result, bool addRoot = false)
        {
            string html = null;
            if (addRoot)
            {
                html = String.Format(Xhtml10Wrapper, GetHtml(result));
            }
            else
            {
                html = GetHtml(result);
            }

            Validate1_0(html);
        }

        public static void Validate1_1(object result, string wrapper = null)
        {
            string root;
            string html = GetHtml(result);
            if (String.IsNullOrEmpty(wrapper))
            {
                root = GetRoot(html);
            }
            else
            {
                root = wrapper;
                html = String.Format("<{0}>{1}</{0}>", wrapper, html);
            }
            Validate1_1(root, html);
        }

        private static string GetHtml(object result)
        {
            Assert.True((result is IHtmlString) || (result is HelperResult), "Helpers should return IHTMLString or HelperResult");
            return result.ToString();
        }

        private static string GetRoot(string html)
        {
            Regex regex = new Regex(@"<(\w+)[\s>]");
            Match match = regex.Match(html);
            Assert.True(match.Success, "Could not determine root element");
            Assert.True(match.Groups.Count > 1, "Could not determine root element");
            return match.Groups[1].Value;
        }

        private static void Validate1_0(string html)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Parse;
            settings.XmlResolver = new XmlPreloadedResolver(XmlKnownDtds.Xhtml10);

            Validate(settings, html);
        }

        private static void Validate1_1(string root, string html)
        {
            var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Parse, ValidationType = ValidationType.DTD, XmlResolver = new AssemblyResourceXmlResolver() };

            string docType = String.Format(DOCTYPE_XHTML1_1, root);
            Validate(settings, docType + html);
        }

        private static void Validate(XmlReaderSettings settings, string html)
        {
            using (StringReader sr = new StringReader(html))
            {
                using (XmlReader reader = XmlReader.Create(sr, settings))
                {
                    while (reader.Read())
                    {
                        // XHTML element and attribute names must be lowercase, since XML is case sensitive.
                        // The W3C validator detects this, but we must manually check since the XmlReader does not.
                        // See: http://www.w3.org/TR/xhtml1/#h-4.2
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            string element = reader.Name;
                            Assert.True(element == element.ToLowerInvariant());
                            if (reader.HasAttributes)
                            {
                                for (int i = 0; i < reader.AttributeCount; i++)
                                {
                                    reader.MoveToAttribute(i);
                                    string attribute = reader.Name;
                                    Assert.True(attribute == attribute.ToLowerInvariant());
                                }
                                // move back to element node
                                reader.MoveToElement();
                            }
                        }
                    }
                }
            }
        }

        private class AssemblyResourceXmlResolver : XmlResolver
        {
            public override ICredentials Credentials
            {
                set { throw new NotSupportedException(); }
            }

            public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
            {
                Assembly assembly = typeof(XhtmlAssert).Assembly;
                return assembly.GetManifestResourceStream("System.Web.Helpers.Test.TestFiles.xhtml11-flat.dtd");
            }
        }
    }
}
