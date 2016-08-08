// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Microsoft.TestCommon.Types;

namespace System.Net.Http.Formatting.DataSets.Types
{
    [KnownType(typeof(DerivedWcfPocoType))]
    [XmlInclude(typeof(DerivedWcfPocoType))]
    public class WcfPocoType : INameAndIdContainer
    {
        private int id;
        private string name;

        public WcfPocoType()
        {
        }

        public WcfPocoType(int id, string name)
        {
            this.id = id;
            this.name = name;
        }

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

        [IgnoreDataMember]
        [XmlIgnore]
        public bool IdSet { get; private set; }

        [IgnoreDataMember]
        [XmlIgnore]
        public bool NameSet { get; private set; }

        public static IEnumerable<WcfPocoType> GetTestData()
        {
            return new WcfPocoType[] { new WcfPocoType(), new WcfPocoType(1, "SomeName") };
        }

        public static IEnumerable<WcfPocoType> GetTestDataWithNull()
        {
            return GetTestData().Concat(new WcfPocoType[] { null });
        }

        public static IEnumerable<DerivedWcfPocoType> GetDerivedTypeTestData()
        {
            return new DerivedWcfPocoType[] { 
                new DerivedWcfPocoType(), 
                new DerivedWcfPocoType(1, "SomeName", null), 
                new DerivedWcfPocoType(1, "SomeName", new WcfPocoType(2, "SomeOtherName"))};
        }

        public static IEnumerable<DerivedWcfPocoType> GetDerivedTypeTestDataWithNull()
        {
            return GetDerivedTypeTestData().Concat(new DerivedWcfPocoType[] { null });
        }
    }
}
