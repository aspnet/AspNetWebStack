// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security;

namespace System.Web.Http.Metadata.Providers
{
    public class CachedDataAnnotationsMetadataAttributes
    {
        public CachedDataAnnotationsMetadataAttributes(IEnumerable<Attribute> attributes)
        {
            Display = attributes.OfType<DisplayAttribute>().FirstOrDefault();
            DisplayFormat = attributes.OfType<DisplayFormatAttribute>().FirstOrDefault();
            DisplayName = attributes.OfType<DisplayNameAttribute>().FirstOrDefault();
            Editable = attributes.OfType<EditableAttribute>().FirstOrDefault();
            ReadOnly = attributes.OfType<ReadOnlyAttribute>().FirstOrDefault();
        }

        public DisplayAttribute Display { get; protected set; }

        public DisplayNameAttribute DisplayName { get; protected set; }

        public DisplayFormatAttribute DisplayFormat { get; protected set; }

        public EditableAttribute Editable { get; protected set; }

        public ReadOnlyAttribute ReadOnly { get; protected set; }
    }
}
