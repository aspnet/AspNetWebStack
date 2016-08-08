// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Configuration;

namespace System.Web.WebPages.Razor.Configuration
{
    public class RazorWebSectionGroup : ConfigurationSectionGroup
    {
        public static readonly string GroupName = "system.web.webPages.razor";

        // Use flags instead of null values since tests may want to set the property to null
        private bool _hostSet = false;
        private bool _pagesSet = false;

        private HostSection _host;
        private RazorPagesSection _pages;

        [ConfigurationProperty("host", IsRequired = false)]
        public HostSection Host
        {
            get { return _hostSet ? _host : (HostSection)Sections["host"]; }
            set
            {
                _host = value;
                _hostSet = true;
            }
        }

        [ConfigurationProperty("pages", IsRequired = false)]
        public RazorPagesSection Pages
        {
            get { return _pagesSet ? _pages : (RazorPagesSection)Sections["pages"]; }
            set
            {
                _pages = value;
                _pagesSet = true;
            }
        }
    }
}
