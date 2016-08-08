// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Microsoft.TestCommon.Types;

namespace System.Net.Http.Formatting.DataSets.Types
{
    [KnownType(typeof(DerivedXmlSerializableType))]
    [XmlInclude(typeof(DerivedXmlSerializableType))]
    public class XmlSerializableType : INameAndIdContainer
    {
        private int id;
        private string name;

        public XmlSerializableType()
        {
        }

        public XmlSerializableType(int id, string name)
        {
            this.id = id;
            this.name = name;
        }

        [XmlAttribute]
        public int Id
        {
            get
            {
                return this.id;
            }

            set
            {
                this.IdSet = true;
                this.id = value;
            }
        }

        [XmlElement]
        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                this.NameSet = true;
                this.name = value;
            }

        }

        [XmlIgnore]
        public bool IdSet { get; private set; }

        [XmlIgnore]
        public bool NameSet { get; private set; }

        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(this, obj))
            {
                return true;
            }

            XmlSerializableType other = obj as XmlSerializableType;
            if (Object.ReferenceEquals(other, null))
            {
                return false;
            }

            return String.Equals(this.Name, other.Name, StringComparison.Ordinal) && this.Id == other.Id;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static IEnumerable<XmlSerializableType> GetTestData()
        {
            return new XmlSerializableType[] { new XmlSerializableType(), new XmlSerializableType(1, "SomeName") };
        }

        public static IEnumerable<DerivedXmlSerializableType> GetDerivedTypeTestData()
        {
            return new DerivedXmlSerializableType[] { 
                new DerivedXmlSerializableType(), 
                new DerivedXmlSerializableType(1, "SomeName", null), 
                new DerivedXmlSerializableType(1, "SomeName", new WcfPocoType(2, "SomeOtherName"))};
        }
    }
}
