// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Web.Http.Description;
using System.Xml.Serialization;
using Newtonsoft.Json;
using ROOT_PROJECT_NAMESPACE.Areas.HelpPage.ModelDescriptions;

namespace WebApiHelpPageWebHost.UnitTest
{
    public class MyGenericType<T, T2>
    {
        public T MyProperty { get; set; }

        public T2 MyProperty2 { get; set; }
    }

    public class Customer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public List<Order> Orders { get; set; }
    }

    public class Order
    {
        public Guid Id { get; set; }

        public Dictionary<Item, string> Items { get; set; }

        public DateTime ShipDate { get; set; }
    }

    public class Item
    {
        public Customer Buyer { get; set; }

        public string Name { get; set; }

        public double Price { get; set; }
    }

    public class ComplexTypeWithPublicFields
    {
        public string Name;
        public Guid Id;
        public Item item;
    }

    public struct ComplexStruct
    {
        public string Name;
        public Guid Id;

        public DateTime Time { get; set; }

        public DateTimeKind Kind { get; set; }
    }

    public class TypeWithNoDefaultConstructor
    {
        public TypeWithNoDefaultConstructor(int id, string name)
        {
        }
    }

    internal class NonPublicType { }

    public enum EmptyEnum { }

    [ModelName("CustomUser")]
    public class User
    {
        /// <summary>
        /// User name.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Age of the user.
        /// </summary>
        [Required]
        [Range(1, 200)]
        public int Age { get; set; }

        /// <summary>
        /// User comment.
        /// </summary>
        [StringLength(100)]
        public string Comment { get; set; }

        /// <summary>
        /// U.S. phone number.
        /// </summary>
        public int PhoneNumber { get; set; }
    }

    [ModelName("MyAddress")]
    public class Address
    {
        [DataType(DataType.PostalCode)]
        public string ZipCode { get; set; }

        /// <summary>
        /// Street name.
        /// </summary>
        [RegularExpression("[a-z]")]
        public string Street { get; set; }

        /// <summary>
        /// Geo-coordinate of the address.
        /// </summary>
        [MinLength(2)]
        [MaxLength(3)]
        public int[] Coordinates { get; set; }
    }

    [DataContract]
    public struct StructDataContractType
    {
        public int NoMember { get; set; }

        [DataMember]
        public int Member { get; set; }

        public int NoField { get; set; }

        [DataMember]
        public int Field { get; set; }
    }

    [DataContract]
    public enum EnumDataContractType
    {
        [EnumMember]
        Value,
        NoValue
    }

    [DataContract]
    public class DataContractType
    {
        public int NoMember { get; set; }

        [DataMember]
        public int Member { get; set; }

        public int NoField { get; set; }

        [DataMember]
        public int Field { get; set; }
    }

    public class TypeWithIgnores
    {
        [XmlIgnore]
        public int NoXmlMember { get; set; }

        [JsonIgnore]
        public int NoJsonMember { get; set; }

        [ApiExplorerSettings(IgnoreApi = true)]
        public int NoMember { get; set; }

        [ApiExplorerSettings(IgnoreApi = false)]
        public int Member { get; set; }

        [IgnoreDataMember]
        public int NoDataMember{ get; set; }

        [DataMember]
        public int DataMember { get; set; }

        public int RegularMember { get; set; }

        [XmlIgnore]
        public int NoXmlField;

        [JsonIgnore]
        public int NoJsonField;

        [IgnoreDataMember]
        public int NoDataField;

        [NonSerialized]
        public int NoSerializedField;

        [ApiExplorerSettings(IgnoreApi = true)]
        public int NoField;

        [ApiExplorerSettings(IgnoreApi = false)]
        public int Field;

        [DataMember]
        public int DataField;

        public int RegularField;
    }

    public class DerivedType : TypeWithIgnores
    {
        public string DerivedField;

        public string DerivedMember { get; set; }
    }

    public class PropertyAliasType
    {
        [DataMember(Name = "RenamedField")]
        public string Bar;

        [JsonProperty("RenamedMember")]
        public string Foo { get; set; }

        [JsonProperty("JsonField")]
        [DataMember(Name = "XmlField")]
        public string FooBar;

        [JsonProperty("JsonProperty")]
        [DataMember(Name = "XmlProperty")]
        public string Property4 { get; set; }
    }

    [DataContract]
    public class DataContractPropertyAliasType
    {
        [DataMember(Name = "RenamedField")]
        public string Bar;

        [JsonProperty("RenamedMember")]
        public string Foo { get; set; }

        [JsonProperty("JsonField")]
        [DataMember(Name = "XmlField")]
        public string FooBar;

        [JsonProperty("JsonProperty")]
        [DataMember(Name = "XmlProperty")]
        public string Property4 { get; set; }
    }

    public class MultipleDataAnnotations
    {
        [Range(1, 200)]
        [StringLength(100)]
        [DataType(DataType.PostalCode)]
        [RegularExpression("[a-z]")]
        [Required]
        [MinLength(2)]
        [MaxLength(3)]
        public string Property { get; set; }

        [MinLength(2)]
        [MaxLength(3)]
        [Range(1, 200)]
        [StringLength(100)]
        [DataType(DataType.PostalCode)]
        [RegularExpression("[a-z]")]
        public string OptionalProperty { get; set; }
    }
}

namespace WebApiHelpPageWebHost.UnitTest2
{
    public class Customer
    {
        public string Name { get; set; }
    }
}