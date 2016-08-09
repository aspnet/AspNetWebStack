// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Http.Dispatcher;
using System.Web.Http.WebHost.Properties;
using System.Xml;

namespace System.Web.Http.WebHost
{
    // Processes files with this format:
    //
    // <typeCache lastModified=... mvcVersionId=...>
    //   <assembly name=...>
    //     <module versionId=...>
    //       <type>...</type>
    //     </module>
    //   </assembly>
    // </typeCache>
    //
    // This is used to store caches of files between AppDomain resets, leading to improved cold boot time
    // and more efficient use of memory.

    /// <summary>
    /// Manages serializing and deserializing the cache managed by <see cref="HttpControllerTypeCache"/>.
    /// </summary>
    internal sealed class HttpControllerTypeCacheSerializer
    {
        private static readonly Guid _mvcVersionId = typeof(HttpControllerTypeCacheSerializer).Module.ModuleVersionId;

        // used for unit testing

        private DateTime CurrentDate
        {
            get { return CurrentDateOverride ?? DateTime.Now; }
        }

        internal DateTime? CurrentDateOverride { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This is an instance method for consistency with the SerializeTypes() method.")]
        public ICollection<Type> DeserializeTypes(TextReader input)
        {
            // DevDiv: 314059: TypeCacheSerializer should use regular serialization instead of DOM
            XmlDocument doc = new XmlDocument();
            doc.Load(input);
            XmlElement rootElement = doc.DocumentElement;

            Guid readMvcVersionId = new Guid(rootElement.Attributes["mvcVersionId"].Value);
            if (readMvcVersionId != _mvcVersionId)
            {
                // The cache is outdated because the cache file was produced by a different version
                // of MVC.
                return null;
            }

            List<Type> deserializedTypes = new List<Type>();
            foreach (XmlNode assemblyNode in rootElement.ChildNodes)
            {
                string assemblyName = assemblyNode.Attributes["name"].Value;
                Assembly assembly = Assembly.Load(assemblyName);

                foreach (XmlNode moduleNode in assemblyNode.ChildNodes)
                {
                    Guid moduleVersionId = new Guid(moduleNode.Attributes["versionId"].Value);

                    foreach (XmlNode typeNode in moduleNode.ChildNodes)
                    {
                        string typeName = typeNode.InnerText;
                        Type type = assembly.GetType(typeName);
                        if (type == null || type.Module.ModuleVersionId != moduleVersionId)
                        {
                            // The cache is outdated because we couldn't find a previously recorded
                            // type or the type's containing module was modified.
                            return null;
                        }
                        else
                        {
                            deserializedTypes.Add(type);
                        }
                    }
                }
            }

            return deserializedTypes;
        }

        public void SerializeTypes(IEnumerable<Type> types, TextWriter output)
        {
            var groupedByAssembly = from type in types
                                    group type by type.Module
                                    into groupedByModule
                                    group groupedByModule by groupedByModule.Key.Assembly;

            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateComment(SRResources.TypeCache_DoNotModify));

            XmlElement typeCacheElement = doc.CreateElement("typeCache");
            doc.AppendChild(typeCacheElement);
            typeCacheElement.SetAttribute("lastModified", CurrentDate.ToString(CultureInfo.InvariantCulture));
            typeCacheElement.SetAttribute("mvcVersionId", _mvcVersionId.ToString());

            foreach (var assemblyGroup in groupedByAssembly)
            {
                XmlElement assemblyElement = doc.CreateElement("assembly");
                typeCacheElement.AppendChild(assemblyElement);
                assemblyElement.SetAttribute("name", assemblyGroup.Key.FullName);

                foreach (var moduleGroup in assemblyGroup)
                {
                    XmlElement moduleElement = doc.CreateElement("module");
                    assemblyElement.AppendChild(moduleElement);
                    moduleElement.SetAttribute("versionId", moduleGroup.Key.ModuleVersionId.ToString());

                    foreach (Type type in moduleGroup)
                    {
                        XmlElement typeElement = doc.CreateElement("type");
                        moduleElement.AppendChild(typeElement);
                        typeElement.AppendChild(doc.CreateTextNode(type.FullName));
                    }
                }
            }

            doc.Save(output);
        }
    }
}
