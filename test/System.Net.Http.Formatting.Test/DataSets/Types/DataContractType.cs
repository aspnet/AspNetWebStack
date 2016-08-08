// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Microsoft.TestCommon.Types;

namespace System.Net.Http.Formatting.DataSets.Types
{
    [DataContract]
    [KnownType(typeof(DerivedDataContractType))]
    [XmlInclude(typeof(DerivedDataContractType))]
    public class DataContractType : INameAndIdContainer
    {
        private int id;
        private string name;

        public DataContractType()
        {
        }

        public DataContractType(int id, string name)
        {
            this.id = id;
            this.name = name;
        }

        [DataMember]
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

        [DataMember]
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

        public static IEnumerable<DataContractType> GetTestData()
        {
            return new DataContractType[] { new DataContractType(), new DataContractType(1, "SomeName") };
        }

        public static IEnumerable<DerivedDataContractType> GetDerivedTypeTestData()
        {
            return new DerivedDataContractType[] { 
                new DerivedDataContractType(), 
                new DerivedDataContractType(1, "SomeName", null), 
                new DerivedDataContractType(1, "SomeName", new WcfPocoType(2, "SomeOtherName"))};
        }
    }
}
