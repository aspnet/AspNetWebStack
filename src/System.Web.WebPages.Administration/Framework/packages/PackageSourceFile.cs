// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using System.Xml.Linq;

namespace System.Web.WebPages.Administration.PackageManager
{
    /// <summary>
    /// Provides an abstraction for reading and writing feeds to disk.
    /// Calls to this file must be externally guarded against multiple threads.
    /// </summary>
    internal class PackageSourceFile : IPackagesSourceFile
    {
        private const string UrlAttribute = "url";
        private const string NameAttribute = "displayname";
        private const string FilterPreferredAttribute = "filterpreferred";
        private readonly string _fileName;

        public PackageSourceFile(string fileName)
        {
            _fileName = fileName;
        }

        public void WriteSources(IEnumerable<WebPackageSource> sources)
        {
            WriteFeeds(sources, () => GetStreamForWrite());
        }

        public IEnumerable<WebPackageSource> ReadSources()
        {
            return ReadFeeds(() => GetStreamForRead());
        }

        public bool Exists()
        {
            return HostingEnvironment.VirtualPathProvider.FileExists(_fileName);
        }

        internal static IEnumerable<WebPackageSource> ReadFeeds(Func<Stream> getStream)
        {
            using (var stream = getStream())
            {
                var document = XDocument.Load(stream);
                var root = document.Root;

                return (from element in root.Elements()
                        select ParsePackageSource(element)).ToList();
            }
        }

        internal static void WriteFeeds(IEnumerable<WebPackageSource> sources, Func<Stream> getStream)
        {
            var xmlTree = from item in sources
                          select new XElement("source",
                                              new XAttribute(UrlAttribute, item.Source),
                                              new XAttribute(NameAttribute, item.Name),
                                              new XAttribute(FilterPreferredAttribute, item.FilterPreferredPackages));

            using (Stream stream = getStream())
            {
                new XDocument(new XElement("sources", xmlTree)).Save(stream);
            }
        }

        internal static WebPackageSource ParsePackageSource(XElement element)
        {
            var urlAttribute = element.Attribute(UrlAttribute);
            var nameAttribute = element.Attribute(NameAttribute);
            var filterPreferredAttribute = element.Attribute(FilterPreferredAttribute);

            // Throw if the file was tampered externally
            if (urlAttribute == null || nameAttribute == null)
            {
                throw new FormatException();
            }
            Uri feedUrl;
            if (!Uri.TryCreate(urlAttribute.Value, UriKind.Absolute, out feedUrl))
            {
                throw new FormatException();
            }

            return new WebPackageSource(feedUrl.OriginalString, nameAttribute.Value) { FilterPreferredPackages = filterPreferredAttribute != null && filterPreferredAttribute.Value.AsBool(false) };
        }

        private Stream GetStreamForRead()
        {
            var vpp = HostingEnvironment.VirtualPathProvider;
            return vpp.GetFile(_fileName).Open();
        }

        private Stream GetStreamForWrite()
        {
            string mappedPath = HostingEnvironment.MapPath(_fileName);
            if (!File.Exists(_fileName))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(mappedPath));
                return File.Create(mappedPath);
            }
            return File.Open(mappedPath, FileMode.Truncate);
        }
    }
}
