// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace System.Net.Http.Formatting.DataSets.Types
{
    [KnownType(typeof(DerivedWcfPocoType))]
    [XmlInclude(typeof(DerivedWcfPocoType))]
    public class DerivedXmlSerializableType : XmlSerializableType, INotJsonSerializable
    {
        private WcfPocoType reference;

        public DerivedXmlSerializableType()
        {
        }

        public DerivedXmlSerializableType(int id, string name, WcfPocoType reference)
            : base(id, name)
        {
            this.reference = reference;
        }

        [XmlElement]
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

        [XmlIgnore]
        public bool ReferenceSet { get; private set; }

        public static new IEnumerable<DerivedXmlSerializableType> GetTestData()
        {
            return new DerivedXmlSerializableType[] { 
                new DerivedXmlSerializableType(), 
                new DerivedXmlSerializableType(1, "SomeName", new WcfPocoType(2, "SomeOtherName")) };
        }

        public static IEnumerable<DerivedXmlSerializableType> GetKnownTypeTestData()
        {
            return new DerivedXmlSerializableType[] { 
                new DerivedXmlSerializableType(), 
                new DerivedXmlSerializableType(1, "SomeName", null), 
                new DerivedXmlSerializableType(1, "SomeName", new DerivedWcfPocoType(2, "SomeOtherName", null))};
        }
    }
}
