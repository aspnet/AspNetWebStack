// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using System.Text;
using System.Web.WebPages.Administration.PackageManager;
using System.Xml.Linq;
using Microsoft.TestCommon;

namespace System.Web.WebPages.Administration.Test
{
    public class PackagesSourceFileTest
    {
        [Fact]
        public void PackagesSourceFileThrowsIfTheXmlElementDoesNotContainNameAndUrl()
        {
            // Arrange
            var element = new XElement("source");

            // Act and Assert
            Assert.Throws<FormatException>(() => PackageSourceFile.ParsePackageSource(element));
        }

        [Fact]
        public void PackagesSourceFileThrowsIfTheXmlElementDoesNotContainUrl()
        {
            // Arrange
            var element = new XElement("source", new XAttribute("displayname", "foo"), new XAttribute("filterpreferred", false));

            // Act and Assert
            Assert.Throws<FormatException>(() => PackageSourceFile.ParsePackageSource(element));
        }

        [Fact]
        public void PackagesSourceFileThrowsIfTheXmlElementDoesNotContainName()
        {
            // Arrange
            var element = new XElement("source", new XAttribute("url", "http://microsoft.com"), new XAttribute("filterpreferred", false));

            // Act and Assert
            Assert.Throws<FormatException>(() => PackageSourceFile.ParsePackageSource(element));
        }

        [Fact]
        public void PackagesSourceFileDoesNotThrowIfXmlElementDoesNotContainPreferred()
        {
            // Arrange
            var element = new XElement("source", new XAttribute("displayname", "foo"), new XAttribute("url", "http://microsoft.com"));

            // Act 
            var item = PackageSourceFile.ParsePackageSource(element);

            // Assert
            Assert.NotNull(item);
        }

        [Fact]
        public void PackagesSourceFileThrowsIfTheFeedUrlIsMalformed()
        {
            // Arrange
            var element = new XElement("source",
                                       new XAttribute("displayname", "foo"),
                                       new XAttribute("url", "bad-url.com"),
                                       new XAttribute("filterpreferred", false)
                );

            // Act and Assert
            Assert.Throws<FormatException>(() => PackageSourceFile.ParsePackageSource(element));
        }

        [Fact]
        public void PackagesSourceFileParsesXElement()
        {
            // Arrange
            var element = new XElement("source",
                                       new XAttribute("displayname", "foo"),
                                       new XAttribute("url", "http://www.microsoft.com"),
                                       new XAttribute("filterpreferred", true)
                );

            // Act 
            var WebPackageSource = PackageSourceFile.ParsePackageSource(element);

            // Assert
            Assert.Equal("foo", WebPackageSource.Name);
            Assert.Equal("http://www.microsoft.com", WebPackageSource.Source);
            Assert.True(WebPackageSource.FilterPreferredPackages);
        }

        [Fact]
        public void PackagesSourceFileReadsAllFeedsFromStream()
        {
            // Arrange
            var document = new XDocument(
                new XElement("sources",
                             new XElement("source", new XAttribute("displayname", "Feed1"), new XAttribute("url", "http://www.microsoft.com/feed1"), new XAttribute("filterpreferred", true)),
                             new XElement("source", new XAttribute("displayname", "Feed2"), new XAttribute("url", "http://www.microsoft.com/feed2"), new XAttribute("filterpreferred", true))
                    ));
            var stream = new MemoryStream();
            document.Save(stream);
            stream = new MemoryStream(stream.ToArray());
            string xml = new StreamReader(stream).ReadToEnd().TrimEnd('\0');

            // Act 
            var result = PackageSourceFile.ReadFeeds(() => new MemoryStream(Encoding.Default.GetBytes(xml)));

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Equal("Feed1", result.First().Name);
            Assert.Equal("Feed2", result.Last().Name);
        }

        [Fact]
        public void PackagesSourceFileWritesAllFeedsToStream()
        {
            // Arrange
            var packagesSources = new[]
            {
                new WebPackageSource(name: "Feed1", source: "http://www.microsoft.com/Feed1"),
                new WebPackageSource(name: "Feed2", source: "http://www.microsoft.com/Feed2") { FilterPreferredPackages = true }
            };
            var stream = new MemoryStream();

            // Act
            PackageSourceFile.WriteFeeds(packagesSources, () => stream);
            stream = new MemoryStream(stream.ToArray());
            string result = new StreamReader(stream).ReadToEnd().TrimEnd('\0');

            // Assert
            var document = XDocument.Parse(result);
            Assert.Equal(document.Root.Name, "sources");
            Assert.Equal(document.Root.Elements().Count(), 2);

            var firstFeed = document.Root.Elements().First();
            Assert.Equal(firstFeed.Name, "source");
            Assert.Equal(firstFeed.Attribute("displayname").Value, "Feed1");
            Assert.Equal(firstFeed.Attribute("url").Value, "http://www.microsoft.com/Feed1");
            Assert.Equal(firstFeed.Attribute("filterpreferred").Value, "false");

            var secondFeed = document.Root.Elements().Last();
            Assert.Equal(secondFeed.Name, "source");
            Assert.Equal(secondFeed.Attribute("displayname").Value, "Feed2");
            Assert.Equal(secondFeed.Attribute("url").Value, "http://www.microsoft.com/Feed2");
            Assert.Equal(secondFeed.Attribute("filterpreferred").Value, "true");
        }
    }
}
