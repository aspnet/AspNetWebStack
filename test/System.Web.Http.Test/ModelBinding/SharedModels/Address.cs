// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.Http.ModelBinding
{
    public class Address
    {
        public List<StreetAddress> AddressLines { get; set; }

        public string ZipCode { get; set; }
    }
}
