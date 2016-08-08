// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Mvc;
using Microsoft.TestCommon;
using ROOT_PROJECT_NAMESPACE.Areas.HelpPage;
using ROOT_PROJECT_NAMESPACE.Areas.HelpPage.ModelDescriptions;

namespace WebApiHelpPageWebHost.UnitTest
{
    public class ModelDescriptionGeneratorTest
    {
        [Theory]
        [InlineData(typeof(bool), "Boolean", "boolean")]
        [InlineData(typeof(byte), "Byte", "byte")]
        [InlineData(typeof(char), "Char", "character")]
        [InlineData(typeof(Decimal), "Decimal", "decimal number")]
        [InlineData(typeof(double), "Double", "decimal number")]
        [InlineData(typeof(Guid), "Guid", "globally unique identifier")]
        [InlineData(typeof(int), "Int32", "integer")]
        [InlineData(typeof(string), "String", "string")]
        [InlineData(typeof(float), "Single", "decimal number")]
        [InlineData(typeof(long), "Int64", "integer")]
        [InlineData(typeof(uint), "UInt32", "unsigned integer")]
        [InlineData(typeof(ulong), "UInt64", "unsigned integer")]
        [InlineData(typeof(sbyte), "SByte", "signed byte")]
        [InlineData(typeof(DateTime), "DateTime", "date")]
        [InlineData(typeof(DateTimeOffset), "DateTimeOffset", "date")]
        [InlineData(typeof(TimeSpan), "TimeSpan", "time interval")]
        [InlineData(typeof(UInt16), "UInt16", "unsigned integer")]
        [InlineData(typeof(Int16), "Int16", "integer")]
        [InlineData(typeof(Uri), "Uri", "URI")]
        public void CreateModelDescription_SimpleTypes(Type type, string expectedModelName, string expectedDocumentation)
        {
            HttpConfiguration config = new HttpConfiguration();
            ModelDescriptionGenerator modelGenerator = new ModelDescriptionGenerator(config);

            SimpleTypeModelDescription simpleModel = Assert.IsType<SimpleTypeModelDescription>(modelGenerator.GetOrCreateModelDescription(type));

            Assert.NotNull(simpleModel);
            Assert.Equal(expectedModelName, simpleModel.Name);
            Assert.Equal(type, simpleModel.ModelType);
            Assert.Equal(expectedDocumentation, simpleModel.Documentation);
        }

        [Theory]
        [InlineData(typeof(IEnumerable<string>), typeof(string))]
        [InlineData(typeof(IEnumerable<List<string>>), typeof(List<string>))]
        [InlineData(typeof(IEnumerable<int>), typeof(int))]
        [InlineData(typeof(IEnumerable<Nullable<double>>), typeof(double))]
        [InlineData(typeof(IEnumerable<DateTime>), typeof(DateTime))]
        [InlineData(typeof(ICollection<string>), typeof(string))]
        [InlineData(typeof(ICollection<int>), typeof(int))]
        [InlineData(typeof(ICollection<Nullable<double>>), typeof(double))]
        [InlineData(typeof(ICollection<DateTime>), typeof(DateTime))]
        [InlineData(typeof(IList<string>), typeof(string))]
        [InlineData(typeof(IList<int>), typeof(int))]
        [InlineData(typeof(IList<Nullable<double>>), typeof(double))]
        [InlineData(typeof(IList<DateTime>), typeof(DateTime))]
        [InlineData(typeof(List<string>), typeof(string))]
        [InlineData(typeof(List<int>), typeof(int))]
        [InlineData(typeof(List<Nullable<double>>), typeof(double))]
        [InlineData(typeof(List<DateTime>), typeof(DateTime))]
        [InlineData(typeof(IEnumerable<KeyValuePair<string, int>>), typeof(KeyValuePair<string, int>))]
        [InlineData(typeof(IEnumerable<KeyValuePair<DateTime, int?>>), typeof(KeyValuePair<DateTime, int?>))]
        [InlineData(typeof(IEnumerable), typeof(object))]
        [InlineData(typeof(ICollection), typeof(object))]
        [InlineData(typeof(IList), typeof(object))]
        [InlineData(typeof(ArrayList), typeof(object))]
        [InlineData(typeof(HashSet<string>), typeof(string))]
        [InlineData(typeof(IQueryable<string>), typeof(string))]
        [InlineData(typeof(IQueryable<Customer>), typeof(Customer))]
        [InlineData(typeof(IQueryable<HttpVerbs[]>), typeof(HttpVerbs[]))]
        [InlineData(typeof(IQueryable<List<DateTime>>), typeof(List<DateTime>))]
        [InlineData(typeof(IQueryable), typeof(object))]
        [InlineData(typeof(string[]), typeof(string))]
        [InlineData(typeof(int[]), typeof(int))]
        [InlineData(typeof(double?[]), typeof(double))]
        [InlineData(typeof(DateTime[]), typeof(DateTime))]
        [InlineData(typeof(Customer[]), typeof(Customer))]
        [InlineData(typeof(HttpVerbs[]), typeof(HttpVerbs))]
        public void CreateModelDescription_Collection(Type collectionType, Type itemType)
        {
            HttpConfiguration config = new HttpConfiguration();
            ModelDescriptionGenerator modelGenerator = new ModelDescriptionGenerator(config);

            CollectionModelDescription collectionModel = Assert.IsType<CollectionModelDescription>(modelGenerator.GetOrCreateModelDescription(collectionType));

            Assert.NotNull(collectionModel);
            Assert.Equal(collectionType, collectionModel.ModelType);
            Assert.Equal(itemType, collectionModel.ElementDescription.ModelType);
        }

        [Theory]
        [InlineData(typeof(Dictionary<string, int>), typeof(string), typeof(int))]
        [InlineData(typeof(Dictionary<string, Dictionary<Customer, Order>>), typeof(string), typeof(Dictionary<Customer, Order>))]
        [InlineData(typeof(Hashtable), typeof(object), typeof(object))]
        [InlineData(typeof(IDictionary), typeof(object), typeof(object))]
        [InlineData(typeof(IDictionary<string, DateTime>), typeof(string), typeof(DateTime))]
        [InlineData(typeof(SortedDictionary<string, Guid>), typeof(string), typeof(Guid))]
        [InlineData(typeof(OrderedDictionary), typeof(object), typeof(object))]
        [InlineData(typeof(ConcurrentDictionary<Guid, DateTime>), typeof(Guid), typeof(DateTime))]
        public void CreateModelDescription_Dictionary(Type dictionaryType, Type keyType, Type valueType)
        {
            HttpConfiguration config = new HttpConfiguration();
            ModelDescriptionGenerator modelGenerator = new ModelDescriptionGenerator(config);

            DictionaryModelDescription dictionaryModel = Assert.IsType<DictionaryModelDescription>(modelGenerator.GetOrCreateModelDescription(dictionaryType));

            Assert.NotNull(dictionaryModel);
            Assert.Equal(dictionaryType, dictionaryModel.ModelType);
            Assert.Equal(keyType, dictionaryModel.KeyModelDescription.ModelType);
            Assert.Equal(valueType, dictionaryModel.ValueModelDescription.ModelType);
        }

        [Theory]
        [InlineData(typeof(KeyValuePair<string, int>), typeof(string), typeof(int))]
        [InlineData(typeof(KeyValuePair<Dictionary<int, string>, int>), typeof(Dictionary<int, string>), typeof(int))]
        [InlineData(typeof(KeyValuePair<string, User>), typeof(string), typeof(User))]
        public void CreateModelDescription_KeyValuePair(Type modelType, Type keyType, Type valueType)
        {
            HttpConfiguration config = new HttpConfiguration();
            ModelDescriptionGenerator modelGenerator = new ModelDescriptionGenerator(config);

            KeyValuePairModelDescription keyValuePairModel = Assert.IsType<KeyValuePairModelDescription>(modelGenerator.GetOrCreateModelDescription(modelType));

            Assert.NotNull(keyValuePairModel);
            Assert.Equal(modelType, keyValuePairModel.ModelType);
            Assert.Equal(keyType, keyValuePairModel.KeyModelDescription.ModelType);
            Assert.Equal(valueType, keyValuePairModel.ValueModelDescription.ModelType);
        }

        [Theory]
        [InlineData(typeof(HttpStatusCode), 47)]
        [InlineData(typeof(HttpVerbs), 7)]
        [InlineData(typeof(ApiParameterSource), 3)]
        [InlineData(typeof(SampleDirection), 2)]
        [InlineData(typeof(ConsoleColor), 16)]
        [InlineData(typeof(HttpCompletionOption), 2)]
        [InlineData(typeof(EnumDataContractType), 1)]
        public void CreateModelDescription_Enum(Type type, int enumValueCount)
        {
            HttpConfiguration config = new HttpConfiguration();
            ModelDescriptionGenerator modelGenerator = new ModelDescriptionGenerator(config);

            EnumTypeModelDescription enumModel = Assert.IsType<EnumTypeModelDescription>(modelGenerator.GetOrCreateModelDescription(type));

            Assert.NotNull(enumModel);
            Assert.Equal(type, enumModel.ModelType);
            Assert.Equal(enumValueCount, enumModel.Values.Count);
        }

        [Theory]
        [InlineData(typeof(ComplexTypeWithPublicFields), new[] { "Name", "Id", "item" })]
        [InlineData(typeof(ComplexStruct), new[] { "Name", "Id", "Time", "Kind" })]
        [InlineData(typeof(ApiDescription), new[] { "HttpMethod", "RelativePath", "ActionDescriptor", "Route", "Documentation", "SupportedResponseFormatters", "SupportedRequestBodyFormatters", "ParameterDescriptions", "ResponseDescription", "ID" })]
        [InlineData(typeof(ApiParameterDescription), new[] { "Name", "Documentation", "Source", "ParameterDescriptor" })]
        [InlineData(typeof(Customer), new[] { "Name", "Id", "Orders" })] // Circular reference
        [InlineData(typeof(Order), new[] { "Items", "Id", "ShipDate" })] // Circular reference
        [InlineData(typeof(Item), new[] { "Name", "Buyer", "Price" })] // Circular reference
        [InlineData(typeof(DataContractType), new[] { "Member", "Field" })]
        [InlineData(typeof(StructDataContractType), new[] { "Member", "Field" })]
        [InlineData(typeof(TypeWithIgnores), new[] { "Member", "DataMember", "RegularMember", "Field", "DataField", "RegularField" })]
        [InlineData(typeof(DerivedType), new[] { "Member", "DataMember", "RegularMember", "Field", "DataField", "RegularField", "DerivedField", "DerivedMember" })]
        [InlineData(typeof(PropertyAliasType), new[] { "RenamedMember", "Bar", "JsonField", "JsonProperty" })]
        [InlineData(typeof(DataContractPropertyAliasType), new[] { "RenamedField", "JsonField", "JsonProperty" })]
        public void CreateModelDescription_ComplexType(Type type, string[] expectedPropertyNames)
        {
            HttpConfiguration config = new HttpConfiguration();
            ModelDescriptionGenerator modelGenerator = new ModelDescriptionGenerator(config);

            ComplexTypeModelDescription complexModel = Assert.IsType<ComplexTypeModelDescription>(modelGenerator.GetOrCreateModelDescription(type));
            var propertyNames = complexModel.Properties.Select(m => m.Name).ToArray();

            Assert.NotNull(complexModel);
            Assert.Equal(type, complexModel.ModelType);
            Assert.Equal(expectedPropertyNames.Length, propertyNames.Length);
            foreach (var expectedPropertyName in expectedPropertyNames)
            {
                Assert.Contains(expectedPropertyName, propertyNames);
            }
        }

        [Theory]
        [InlineData(typeof(ComplexTypeWithPublicFields), "ComplexTypeWithPublicFields")]
        [InlineData(typeof(ComplexStruct), "ComplexStruct")]
        [InlineData(typeof(Customer), "Customer")]
        [InlineData(typeof(Order), "Order")]
        [InlineData(typeof(Item), "Item")]
        [InlineData(typeof(User), "CustomUser")]
        [InlineData(typeof(Address), "MyAddress")]
        public void CreateModelDescription_ComplexType_ReturnsExpectedModelName(Type type, string expectedModelName)
        {
            HttpConfiguration config = new HttpConfiguration();
            ModelDescriptionGenerator modelGenerator = new ModelDescriptionGenerator(config);

            ComplexTypeModelDescription complexModel = Assert.IsType<ComplexTypeModelDescription>(modelGenerator.GetOrCreateModelDescription(type));

            Assert.NotNull(complexModel);
            Assert.Equal(type, complexModel.ModelType);
            Assert.Equal(expectedModelName, complexModel.Name);
        }

        public static IEnumerable<object[]> CreateModelDescription_ComplexType_WithAnnotation_PropertyData
        {
            get
            {
                object type;
                object annotationMapping;

                type = typeof(User);
                annotationMapping = new Dictionary<string, string[]>
                {
                    { "Name", new[] { "Required" } },
                    { "Age", new[] { "Required", "Range: inclusive between 1 and 200" } },
                    { "Comment", new[] { "String length: inclusive between 0 and 100" } },
                    { "PhoneNumber", new string[0] },
                };

                yield return new[] { type, annotationMapping };

                type = typeof(Address);
                annotationMapping = new Dictionary<string, string[]>
                {
                    { "ZipCode", new[] { "Data type: PostalCode" } },
                    { "Street", new[] { "Matching regular expression pattern: [a-z]" } },
                    { "Coordinates", new[] { "Max length: 3", "Min length: 2" } },
                };

                yield return new[] { type, annotationMapping };

                type = typeof(MultipleDataAnnotations);
                annotationMapping = new Dictionary<string, string[]>
                {
                    {
                        "Property", new[] {
                            "Required",
                            "Data type: PostalCode",
                            "Matching regular expression pattern: [a-z]",
                            "Max length: 3",
                            "Min length: 2",
                            "Range: inclusive between 1 and 200",
                            "String length: inclusive between 0 and 100"}
                    },
                    {
                        "OptionalProperty", new[] {
                            "Data type: PostalCode",
                            "Matching regular expression pattern: [a-z]",
                            "Max length: 3",
                            "Min length: 2",
                            "Range: inclusive between 1 and 200",
                            "String length: inclusive between 0 and 100"}
                    }
                };

                yield return new[] { type, annotationMapping };
            }
        }

        [Theory]
        [PropertyData("CreateModelDescription_ComplexType_WithAnnotation_PropertyData")]
        public void CreateModelDescription_ComplexType_WithAnnotation(Type type, Dictionary<string, string[]> annotationMapping)
        {
            HttpConfiguration config = new HttpConfiguration();
            ModelDescriptionGenerator modelGenerator = new ModelDescriptionGenerator(config);

            ComplexTypeModelDescription complexModel = Assert.IsType<ComplexTypeModelDescription>(modelGenerator.GetOrCreateModelDescription(type));

            Assert.NotNull(complexModel);
            Assert.Equal(type, complexModel.ModelType);
            Assert.Equal(annotationMapping.Count, complexModel.Properties.Count);
            foreach (var property in complexModel.Properties)
            {
                string[] expectedAnnotations = annotationMapping[property.Name];
                Assert.Equal(expectedAnnotations.Length, property.Annotations.Count);
                for (int i = 0; i < expectedAnnotations.Length; i++)
                {
                    Assert.Equal(expectedAnnotations[i], property.Annotations[i].Documentation);
                }
            }
        }

        public static IEnumerable<object[]> CreateModelDescription_ComplexType_WithDocumentation_PropertyData
        {
            get
            {
                object type;
                object annotationMapping;

                type = typeof(User);
                annotationMapping = new Dictionary<string, string>
                {
                    { "Name", "User name."},
                    { "Age", "Age of the user." },
                    { "Comment", "User comment." },
                    { "PhoneNumber", "U.S. phone number." },
                };

                yield return new[] { type, annotationMapping };

                type = typeof(Address);
                annotationMapping = new Dictionary<string, string>
                {
                    { "ZipCode", null },
                    { "Street", "Street name." },
                    { "Coordinates", "Geo-coordinate of the address." },
                };

                yield return new[] { type, annotationMapping };
            }
        }

        [Theory]
        [PropertyData("CreateModelDescription_ComplexType_WithDocumentation_PropertyData")]
        public void CreateModelDescription_ComplexType_WithDocumentation(Type type, Dictionary<string, string> documentationMapping)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.SetDocumentationProvider(new XmlDocumentationProvider("WebApiHelpPage.Test.XML"));
            ModelDescriptionGenerator modelGenerator = new ModelDescriptionGenerator(config);

            ComplexTypeModelDescription complexModel = Assert.IsType<ComplexTypeModelDescription>(modelGenerator.GetOrCreateModelDescription(type));

            Assert.NotNull(complexModel);
            Assert.Equal(type, complexModel.ModelType);
            Assert.Equal(documentationMapping.Count, complexModel.Properties.Count);
            foreach (var property in complexModel.Properties)
            {
                string expectedDocumentation = documentationMapping[property.Name];
                Assert.Equal(expectedDocumentation, property.Documentation);
            }
        }

        [Fact]
        public void CreateModelDescription_ThrowsOnDuplicateModelName()
        {
            HttpConfiguration config = new HttpConfiguration();
            ModelDescriptionGenerator modelGenerator = new ModelDescriptionGenerator(config);

            Assert.Throws<InvalidOperationException>(() =>
            {
                modelGenerator.GetOrCreateModelDescription(typeof(Customer));
                modelGenerator.GetOrCreateModelDescription(typeof(WebApiHelpPageWebHost.UnitTest2.Customer));
            },
            "A model description could not be created. Duplicate model name 'Customer' was found for types 'WebApiHelpPageWebHost.UnitTest.Customer' and 'WebApiHelpPageWebHost.UnitTest2.Customer'. Use the [ModelName] attribute to change the model name for at least one of the types so that it has a unique name.");
        }
    }
}