// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace System.Net.Http.Formatting.DataSets.Types
{
    [DataContract]
    [KnownType(typeof(DerivedWcfPocoType))]
    [KnownType(typeof(DerivedDataContractType))]
    [XmlInclude(typeof(DerivedWcfPocoType))]
    [XmlInclude(typeof(DerivedDataContractType))]
    public class DerivedDataContractType : DataContractType
    {
        private WcfPocoType reference;

        public DerivedDataContractType()
        {
        }

        public DerivedDataContractType(int id, string name, WcfPocoType reference)
            : base(id, name)
        {
            this.reference = reference;
        }

        [DataMember]
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

        public static new IEnumerable<DerivedDataContractType> GetTestData()
        {
            return new DerivedDataContractType[] { 
                new DerivedDataContractType(), 
                new DerivedDataContractType(1, "SomeName", new WcfPocoType(2, "SomeOtherName")) };
        }

        public static IEnumerable<DerivedDataContractType> GetKnownTypeTestData()
        {
            return new DerivedDataContractType[] { 
                new DerivedDataContractType(), 
                new DerivedDataContractType(1, "SomeName", null), 
                new DerivedDataContractType(1, "SomeName", new DerivedWcfPocoType(2, "SomeOtherName", null))};
        }
    }
}
