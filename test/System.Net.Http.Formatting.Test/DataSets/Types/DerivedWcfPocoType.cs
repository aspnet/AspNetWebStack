// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace System.Net.Http.Formatting.DataSets.Types
{
    public class DerivedWcfPocoType : WcfPocoType
    {
        private WcfPocoType reference;

        public DerivedWcfPocoType()
        {
        }

        public DerivedWcfPocoType(int id, string name, WcfPocoType reference)
            : base(id, name)
        {
            this.reference = reference;
        }

        public WcfPocoType Reference
        {
            get
            {
                return this.reference;
            }

            set
            {
                this.ReferenceSet = true;
                this.reference = value;
            }
        }

        [IgnoreDataMember]
        [XmlIgnore]
        public bool ReferenceSet { get; private set; }
    }
}
