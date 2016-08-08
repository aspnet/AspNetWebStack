// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Configuration;

namespace System.Web.WebPages.Razor.Configuration
{
    public class HostSection : ConfigurationSection
    {
        public static readonly string SectionName = RazorWebSectionGroup.GroupName + "/host";

        private static readonly ConfigurationProperty _typeProperty =
            new ConfigurationProperty("factoryType",
                                      typeof(string),
                                      null,
                                      ConfigurationPropertyOptions.IsRequired);

        private bool _factoryTypeSet = false;
        private string _factoryType;

        [ConfigurationProperty("factoryType", IsRequired = true, DefaultValue = null)]
        public string FactoryType
        {
            get { return _factoryTypeSet ? _factoryType : (string)this[_typeProperty]; }
            set
            {
                _factoryType = value;
                _factoryTypeSet = true;
            }
        }
    }
}
