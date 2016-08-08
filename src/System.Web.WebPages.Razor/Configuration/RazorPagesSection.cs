// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Web.Configuration;

namespace System.Web.WebPages.Razor.Configuration
{
    public class RazorPagesSection : ConfigurationSection
    {
        public static readonly string SectionName = RazorWebSectionGroup.GroupName + "/pages";

        private static readonly ConfigurationProperty _pageBaseTypeProperty =
            new ConfigurationProperty("pageBaseType",
                                      typeof(string),
                                      null,
                                      ConfigurationPropertyOptions.IsRequired);

        private static readonly ConfigurationProperty _namespacesProperty =
            new ConfigurationProperty("namespaces",
                                      typeof(NamespaceCollection),
                                      null,
                                      ConfigurationPropertyOptions.IsRequired);

        private bool _pageBaseTypeSet = false;
        private bool _namespacesSet = false;

        private string _pageBaseType;
        private NamespaceCollection _namespaces;

        [ConfigurationProperty("pageBaseType", IsRequired = true)]
        public string PageBaseType
        {
            get { return _pageBaseTypeSet ? _pageBaseType : (string)this[_pageBaseTypeProperty]; }
            set
            {
                _pageBaseType = value;
                _pageBaseTypeSet = true;
            }
        }

        [ConfigurationProperty("namespaces", IsRequired = true)]
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Being able to set this property is extremely useful for third-parties who are testing components which interact with the Razor configuration system")]
        public NamespaceCollection Namespaces
        {
            get { return _namespacesSet ? _namespaces : (NamespaceCollection)this[_namespacesProperty]; }
            set
            {
                _namespaces = value;
                _namespacesSet = true;
            }
        }
    }
}
