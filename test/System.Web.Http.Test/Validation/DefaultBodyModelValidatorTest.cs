// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.Metadata.Providers;
using System.Web.Http.ModelBinding;
using System.Xml.Linq;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Validation
{
    public class DefaultBodyModelValidatorTest
    {
        private static Person LonelyPerson;

        static DefaultBodyModelValidatorTest()
        {
            LonelyPerson = new Person() { Name = "Reallllllllly Long Name" };
            LonelyPerson.Friend = LonelyPerson;
        }

        public static IEnumerable<object[]> ValidationErrors
        {
            get
            {
                return new TheoryDataSet<object, Type, Dictionary<string, string>>()
                {
                    // Primitives
                    { null, typeof(Person), new Dictionary<string, string>() },
                    { 14, typeof(int), new Dictionary<string, string>() },
                    { "foo", typeof(string), new Dictionary<string, string>() },

                    // Object Traversal : make sure we can traverse the object graph without throwing
                    { new ValueType() { Reference = "ref", Value = 256}, typeof(ValueType), new Dictionary<string, string>()},
                    { new ReferenceType() { Reference = "ref", Value = 256}, typeof(ReferenceType), new Dictionary<string, string>()},

                    // Classes
                    { new Person() { Name = "Rick", Profession = "Astronaut" }, typeof(Person), new Dictionary<string, string>() },
                    { new Person(), typeof(Person), new Dictionary<string, string>()
                        {
                            { "Name", "The Name field is required." },
                            { "Profession", "The Profession field is required." }
                        }
                    },

                    { new Person() { Name = "Rick", Friend = new Person() }, typeof(Person), new Dictionary<string, string>()
                        {
                            { "Profession", "The Profession field is required." },
                            { "Friend.Name", "The Name field is required." },
                            { "Friend.Profession", "The Profession field is required." }
                        }
                    },

                    // Collections
                    { new Person[] { new Person(), new Person() }, typeof(Person[]), new Dictionary<string, string>()
                        {
                            { "[0].Name", "The Name field is required." },
                            { "[0].Profession", "The Profession field is required." },
                            { "[1].Name", "The Name field is required." },
                            { "[1].Profession", "The Profession field is required." }
                        }
                    },

                    { new List<Person> { new Person(), new Person() }, typeof(Person[]), new Dictionary<string, string>()
                        {
                            { "[0].Name", "The Name field is required." },
                            { "[0].Profession", "The Profession field is required." },
                            { "[1].Name", "The Name field is required." },
                            { "[1].Profession", "The Profession field is required." }
                        }
                    },

                    { new Dictionary<string, Person> { { "Joe", new Person() } , { "Mark", new Person() } }, typeof(Dictionary<string, Person>), new Dictionary<string, string>()
                        {
                            { "[0].Value.Name", "The Name field is required." },
                            { "[0].Value.Profession", "The Profession field is required." },
                            { "[1].Value.Name", "The Name field is required." },
                            { "[1].Value.Profession", "The Profession field is required." }
                        }
                    },

                    // IValidatableObject's
                    { new ValidatableModel(), typeof(ValidatableModel), new Dictionary<string, string>()
                        {
                            { "", "Error1" },
                            { "Property1", "Error2" },
                            { "Property2", "Error3" },
                            { "Property3", "Error3" }
                        }
                    },
                    { new[] { new ValidatableModel() }, typeof(ValidatableModel[]), new Dictionary<string, string>()
                        {
                            { "[0]", "Error1" },
                            { "[0].Property1", "Error2" },
                            { "[0].Property2", "Error3" },
                            { "[0].Property3", "Error3" }
                        }
                    },

                    // Testing we don't blow up on cycles
                    { LonelyPerson, typeof(Person), new Dictionary<string, string>()
                        {
                            { "Name", "The field Name must be a string with a maximum length of 10." },
                            { "Profession", "The Profession field is required." }
                        }
                    },

                    // Testing that we don't bubble up exceptions when property getters throw
                    { new Uri("/api/values", UriKind.Relative), typeof(Uri), new Dictionary<string, string>() },

                    // Testing that excluded types don't result in any errors
                    { typeof(string), typeof(Type), new Dictionary<string, string>() },
                    { new byte[] { (byte)'a', (byte)'b' }, typeof(byte[]), new Dictionary<string, string>() },
                    { XElement.Parse("<xml>abc</xml>"), typeof(XElement), new Dictionary<string, string>() }
                };
            }
        }

        [Theory]
        [ReplaceCulture]
        [PropertyData("ValidationErrors")]
        public void ExpectedValidationErrorsRaised(object model, Type type, Dictionary<string, string> expectedErrors)
        {
            // Arrange
            ModelMetadataProvider metadataProvider = new DataAnnotationsModelMetadataProvider();
            HttpActionContext actionContext = ContextUtil.CreateActionContext();

            // Act
            Assert.DoesNotThrow(() =>
                new DefaultBodyModelValidator().Validate(model, type, metadataProvider, actionContext, string.Empty)
            );

            // Assert
            Dictionary<string, string> actualErrors = new Dictionary<string, string>();
            foreach (KeyValuePair<string, ModelState> keyStatePair in actionContext.ModelState)
            {
                foreach (ModelError error in keyStatePair.Value.Errors)
                {
                    actualErrors.Add(keyStatePair.Key, error.ErrorMessage);
                }
            }

            Assert.Equal(expectedErrors.Count, actualErrors.Count);
            foreach (KeyValuePair<string, string> keyErrorPair in expectedErrors)
            {
                Assert.Contains(keyErrorPair.Key, actualErrors.Keys);
                Assert.Equal(keyErrorPair.Value, actualErrors[keyErrorPair.Key]);
            }
        }

        [Fact]
        public void MultipleValidationErrorsOnSameMemberReported()
        {
            // Arrange
            ModelMetadataProvider metadataProvider = new DataAnnotationsModelMetadataProvider();
            HttpActionContext actionContext = ContextUtil.CreateActionContext();
            object model = new Address() { Street = "Microsoft Way" };

            // Act
            Assert.DoesNotThrow(() =>
                new DefaultBodyModelValidator().Validate(model, typeof(Address), metadataProvider, actionContext, string.Empty)
            );

            // Assert
            Assert.Contains("Street", actionContext.ModelState.Keys);
            ModelState streetState = actionContext.ModelState["Street"];
            Assert.Equal(2, streetState.Errors.Count);
        }

        [Fact]
        public void ExcludedTypes_AreNotValidated()
        {
            // Arrange
            ModelMetadataProvider metadataProvider = new DataAnnotationsModelMetadataProvider();
            HttpActionContext actionContext = ContextUtil.CreateActionContext();
            Mock<DefaultBodyModelValidator> mockValidator = new Mock<DefaultBodyModelValidator>();
            mockValidator.CallBase = true;
            mockValidator.Setup(validator => validator.ShouldValidateType(typeof(Person))).Returns(false);

            // Act
            mockValidator.Object.Validate(new Person(), typeof(Person), metadataProvider, actionContext, string.Empty);

            // Assert
            Assert.True(actionContext.ModelState.IsValid);
        }

        [Fact]
        public void ExcludedPropertyTypes_AreShallowValidated()
        {
            // Arrange
            ModelMetadataProvider metadataProvider = new DataAnnotationsModelMetadataProvider();
            HttpActionContext actionContext = ContextUtil.CreateActionContext();
            Mock<DefaultBodyModelValidator> mockValidator = new Mock<DefaultBodyModelValidator>();
            mockValidator.CallBase = true;
            mockValidator.Setup(validator => validator.ShouldValidateType(typeof(Person))).Returns(false);

            // Act
            mockValidator.Object.Validate(new Pet(), typeof(Pet), metadataProvider, actionContext, string.Empty);

            // Assert
            Assert.False(actionContext.ModelState.IsValid);
            ModelState modelState = actionContext.ModelState["Owner"];
            Assert.Single(modelState.Errors);
        }

        [Fact]
        public void Validate_DoesNotUseOverridden_GetHashCodeOrEquals()
        {
            // Arrange
            ModelMetadataProvider metadataProvider = new DataAnnotationsModelMetadataProvider();
            HttpActionContext actionContext = ContextUtil.CreateActionContext();
            DefaultBodyModelValidator validator = new DefaultBodyModelValidator();
            object instance = new[] { new TypeThatOverridesEquals { Funny = "hehe" }, new TypeThatOverridesEquals { Funny = "hehe" } };

            // Act & Assert
            Assert.DoesNotThrow(
                () => validator.Validate(instance, typeof(TypeThatOverridesEquals[]), metadataProvider, actionContext, String.Empty));
        }

        public class Person
        {
            [Required]
            [StringLength(10)]
            public string Name { get; set; }

            [Required]
            public string Profession { get; set; }

            public Person Friend { get; set; }
        }

        public class Address
        {
            [StringLength(5)]
            [RegularExpression("hehehe")]
            public string Street { get; set; }
        }

        public struct ValueType
        {
            public int Value;
            public string Reference;
        }

        public class ReferenceType
        {
            public static string StaticProperty { get { return "static"; } }
            public int Value;
            public string Reference;
        }

        public class Pet
        {
            [Required]
            public Person Owner { get; set; }
        }

        public class ValidatableModel : IValidatableObject
        {
            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                yield return new ValidationResult("Error1", new string[] { });
                yield return new ValidationResult("Error2", new[] { "Property1" });
                yield return new ValidationResult("Error3", new[] { "Property2", "Property3" });
            }
        }

        public class TypeThatOverridesEquals
        {
            [StringLength(2)]
            public string Funny { get; set; }

            public override bool Equals(object obj)
            {
                throw new InvalidOperationException();
            }

            public override int GetHashCode()
            {
                throw new InvalidOperationException();
            }
        }
    }
}
