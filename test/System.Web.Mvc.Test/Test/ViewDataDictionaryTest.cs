// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.TestUtil;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    [Xunit.Collection("Uses ScopeStorage or ViewEngines.Engines")] // Uses ModelMetadataProviders.Current
    public class ViewDataDictionaryTest
    {
        [Fact]
        public void ConstructorThrowsIfDictionaryIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { new ViewDataDictionary((ViewDataDictionary)null); }, "dictionary");
        }

        [Fact]
        public void ConstructorWithViewDataDictionaryCopiesModelAndModelState()
        {
            // Arrange
            ViewDataDictionary originalVdd = new ViewDataDictionary();
            object model = new object();
            originalVdd.Model = model;
            originalVdd["foo"] = "bar";
            originalVdd.ModelState.AddModelError("key", "error");

            // Act
            ViewDataDictionary newVdd = new ViewDataDictionary(originalVdd);

            // Assert
            Assert.Equal(model, newVdd.Model);
            Assert.True(newVdd.ModelState.ContainsKey("key"));
            Assert.Equal("error", newVdd.ModelState["key"].Errors[0].ErrorMessage);
            Assert.Equal("bar", newVdd["foo"]);
        }

        [Fact]
        public void ConstructorWithViewDataDictionaryUsesCopyOnWriteDictionary()
        {
            // Arrange
            ViewDataDictionary originalVdd = new ViewDataDictionary();

            // Act
            ViewDataDictionary newVdd = new ViewDataDictionary(originalVdd);

            // Assert
            Assert.IsType<Dictionary<string, object>>(originalVdd.InnerDictionary);
            Assert.IsType<CopyOnWriteDictionary<string, object>>(newVdd.InnerDictionary);
        }

        [Fact]
        public void DictionaryInterface()
        {
            // Arrange
            DictionaryHelper<string, object> helper = new DictionaryHelper<string, object>()
            {
                Creator = () => new ViewDataDictionary(),
                Comparer = StringComparer.OrdinalIgnoreCase,
                SampleKeys = new string[] { "foo", "bar", "baz", "quux", "QUUX" },
                SampleValues = new object[] { 42, "string value", new DateTime(2001, 1, 1), new object(), 32m },
                ThrowOnKeyNotFound = false
            };

            // Act & assert
            helper.Execute();
        }

        [Fact]
        public void EvalReturnsSimplePropertyValue()
        {
            var obj = new { Foo = "Bar" };
            ViewDataDictionary vdd = new ViewDataDictionary(obj);

            Assert.Equal("Bar", vdd.Eval("Foo"));
        }

        [Fact]
        public void EvalWithModelAndDictionaryPropertyEvaluatesDictionaryValue()
        {
            var obj = new { Foo = new Dictionary<string, object> { { "Bar", "Baz" } } };
            ViewDataDictionary vdd = new ViewDataDictionary(obj);

            Assert.Equal("Baz", vdd.Eval("Foo.Bar"));
        }

        [Fact]
        public void EvalEvaluatesDictionaryThenModel()
        {
            var obj = new { Foo = "NotBar" };
            ViewDataDictionary vdd = new ViewDataDictionary(obj);
            vdd.Add("Foo", "Bar");

            Assert.Equal("Bar", vdd.Eval("Foo"));
        }

        [Fact]
        public void EvalReturnsValueOfCompoundExpressionByFollowingObjectPath()
        {
            var obj = new { Foo = new { Bar = "Baz" } };
            ViewDataDictionary vdd = new ViewDataDictionary(obj);

            Assert.Equal("Baz", vdd.Eval("Foo.Bar"));
        }

        [Fact]
        public void EvalReturnsNullIfExpressionDoesNotMatch()
        {
            var obj = new { Foo = new { Biz = "Baz" } };
            ViewDataDictionary vdd = new ViewDataDictionary(obj);

            Assert.Null(vdd.Eval("Foo.Bar"));
        }

        [Fact]
        public void EvalReturnsValueJustAdded()
        {
            ViewDataDictionary vdd = new ViewDataDictionary();
            vdd.Add("Foo", "Blah");

            Assert.Equal("Blah", vdd.Eval("Foo"));
        }

        [Fact]
        public void EvalWithCompoundExpressionReturnsIndexedValue()
        {
            ViewDataDictionary vdd = new ViewDataDictionary();
            vdd.Add("Foo.Bar", "Baz");

            Assert.Equal("Baz", vdd.Eval("Foo.Bar"));
        }

        [Fact]
        public void EvalWithCompoundExpressionReturnsPropertyOfAddedObject()
        {
            ViewDataDictionary vdd = new ViewDataDictionary();
            vdd.Add("Foo", new { Bar = "Baz" });

            Assert.Equal("Baz", vdd.Eval("Foo.Bar"));
        }

        [Fact]
        public void EvalWithCompoundIndexExpressionReturnsEval()
        {
            ViewDataDictionary vdd = new ViewDataDictionary();
            vdd.Add("Foo.Bar", new { Baz = "Quux" });

            Assert.Equal("Quux", vdd.Eval("Foo.Bar.Baz"));
        }

        [Fact]
        public void EvalWithCompoundIndexAndCompoundExpressionReturnsValue()
        {
            ViewDataDictionary vdd = new ViewDataDictionary();
            vdd.Add("Foo.Bar", new { Baz = new { Blah = "Quux" } });

            Assert.Equal("Quux", vdd.Eval("Foo.Bar.Baz.Blah"));
        }

        /// <summary>
        /// Make sure that dict["foo.bar"] gets chosen before dict["foo"]["bar"]
        /// </summary>
        [Fact]
        public void EvalChoosesValueInDictionaryOverOtherValue()
        {
            ViewDataDictionary vdd = new ViewDataDictionary();
            vdd.Add("Foo", new { Bar = "Not Baz" });
            vdd.Add("Foo.Bar", "Baz");

            Assert.Equal("Baz", vdd.Eval("Foo.Bar"));
        }

        /// <summary>
        /// Make sure that dict["foo.bar"]["baz"] gets chosen before dict["foo"]["bar"]["baz"]
        /// </summary>
        [Fact]
        public void EvalChoosesCompoundValueInDictionaryOverOtherValues()
        {
            ViewDataDictionary vdd = new ViewDataDictionary();
            vdd.Add("Foo", new { Bar = new { Baz = "Not Quux" } });
            vdd.Add("Foo.Bar", new { Baz = "Quux" });

            Assert.Equal("Quux", vdd.Eval("Foo.Bar.Baz"));
        }

        /// <summary>
        /// Make sure that dict["foo.bar"]["baz"] gets chosen before dict["foo"]["bar.baz"]
        /// </summary>
        [Fact]
        public void EvalChoosesCompoundValueInDictionaryOverOtherValuesWithCompoundProperty()
        {
            ViewDataDictionary vdd = new ViewDataDictionary();
            vdd.Add("Foo", new Person());
            vdd.Add("Foo.Bar", new { Baz = "Quux" });

            Assert.Equal("Quux", vdd.Eval("Foo.Bar.Baz"));
        }

        [Fact]
        public void EvalThrowsIfExpressionIsEmpty()
        {
            // Arrange
            ViewDataDictionary vdd = new ViewDataDictionary();

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { vdd.Eval(String.Empty); }, "expression");
        }

        [Fact]
        public void EvalThrowsIfExpressionIsNull()
        {
            // Arrange
            ViewDataDictionary vdd = new ViewDataDictionary();

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { vdd.Eval(null); }, "expression");
        }

        [Fact]
        public void EvalWithCompoundExpressionAndDictionarySubExpressionChoosesDictionaryValue()
        {
            ViewDataDictionary vdd = new ViewDataDictionary();
            vdd.Add("Foo", new Dictionary<string, object> { { "Bar", "Baz" } });

            Assert.Equal("Baz", vdd.Eval("Foo.Bar"));
        }

        [Fact]
        public void EvalWithDictionaryAndNoMatchReturnsNull()
        {
            ViewDataDictionary vdd = new ViewDataDictionary();
            vdd.Add("Foo", new Dictionary<string, object> { { "NotBar", "Baz" } });

            object result = vdd.Eval("Foo.Bar");
            Assert.Null(result);
        }

        [Fact]
        public void EvalWithNestedDictionariesEvalCorrectly()
        {
            ViewDataDictionary vdd = new ViewDataDictionary();
            vdd.Add("Foo", new Dictionary<string, object> { { "Bar", new Hashtable { { "Baz", "Quux" } } } });

            Assert.Equal("Quux", vdd.Eval("Foo.Bar.Baz"));
        }

        [Fact]
        public void EvalFormatWithNullValueReturnsEmptyString()
        {
            // Arrange
            ViewDataDictionary vdd = new ViewDataDictionary();

            // Act
            string formattedValue = vdd.Eval("foo", "for{0}mat");

            // Assert
            Assert.Equal(String.Empty, formattedValue);
        }

        [Fact]
        public void EvalFormatWithEmptyFormatReturnsViewData()
        {
            // Arrange
            ViewDataDictionary vdd = new ViewDataDictionary();
            vdd["foo"] = "value";

            // Act
            string formattedValue = vdd.Eval("foo", "");

            // Assert
            Assert.Equal("value", formattedValue);
        }

        [Fact]
        public void EvalFormatWithFormatReturnsFormattedViewData()
        {
            // Arrange
            ViewDataDictionary vdd = new ViewDataDictionary();
            vdd["foo"] = "value";

            // Act
            string formattedValue = vdd.Eval("foo", "for{0}mat");

            // Assert
            Assert.Equal("forvaluemat", formattedValue);
        }

        [Fact]
        public void EvalPropertyNamedModel()
        {
            // Arrange
            ViewDataDictionary vdd = new ViewDataDictionary();
            vdd["Title"] = "Home Page";
            vdd["Message"] = "Welcome to ASP.NET MVC!";
            vdd.Model = new TheQueryStringParam
            {
                Name = "The Name",
                Value = "The Value",
                Model = "The Model",
            };

            // Act
            object o = vdd.Eval("Model");

            // Assert
            Assert.Equal("The Model", o);
        }

        [Fact]
        public void EvalSubPropertyNamedValueInModel()
        {
            // Arrange
            ViewDataDictionary vdd = new ViewDataDictionary();
            vdd["Title"] = "Home Page";
            vdd["Message"] = "Welcome to ASP.NET MVC!";
            vdd.Model = new TheQueryStringParam
            {
                Name = "The Name",
                Value = "The Value",
                Model = "The Model",
            };

            // Act
            object o = vdd.Eval("Value");

            // Assert
            Assert.Equal("The Value", o);
        }

        [Fact]
        public void GetViewDataInfoFromDictionary()
        {
            // Arrange
            ViewDataDictionary fooVdd = new ViewDataDictionary()
            {
                { "Bar", "barValue" }
            };
            ViewDataDictionary vdd = new ViewDataDictionary()
            {
                { "Foo", fooVdd }
            };

            // Act
            ViewDataInfo info = vdd.GetViewDataInfo("foo.bar");

            // Assert
            Assert.NotNull(info);
            Assert.Equal(fooVdd, info.Container);
            Assert.Equal("barValue", info.Value);
        }

        [Fact]
        public void GetViewDataInfoFromDictionaryWithMissingEntry()
        {
            // Arrange
            ViewDataDictionary vdd = new ViewDataDictionary();

            // Act
            ViewDataInfo info = vdd.GetViewDataInfo("foo");

            // Assert
            Assert.Null(info);
        }

        [Fact]
        public void GetViewDataInfoFromDictionaryWithNullEntry()
        {
            // Arrange
            ViewDataDictionary vdd = new ViewDataDictionary()
            {
                { "Foo", null }
            };

            // Act
            ViewDataInfo info = vdd.GetViewDataInfo("foo");

            // Assert
            Assert.NotNull(info);
            Assert.Equal(vdd, info.Container);
            Assert.Null(info.Value);
        }

        [Fact]
        public void GetViewDataInfoFromModel()
        {
            // Arrange
            object model = new { foo = "fooValue" };
            ViewDataDictionary vdd = new ViewDataDictionary(model);

            PropertyDescriptor propDesc = TypeDescriptor.GetProperties(model).Find("foo", true /* ignoreCase */);

            // Act
            ViewDataInfo info = vdd.GetViewDataInfo("foo");

            // Assert
            Assert.NotNull(info);
            Assert.Equal(model, info.Container);
            Assert.Equal(propDesc, info.PropertyDescriptor);
            Assert.Equal("fooValue", info.Value);
        }

        [Fact]
        public void FabricatesModelMetadataFromModelWhenModelMetadataHasNotBeenSet()
        {
            // Arrange
            object model = new { foo = "fooValue", bar = "barValue" };
            ViewDataDictionary vdd = new ViewDataDictionary(model);

            // Act
            ModelMetadata metadata = vdd.ModelMetadata;

            // Assert
            Assert.NotNull(metadata);
            Assert.Equal(model.GetType(), metadata.ModelType);
        }

        [Fact]
        public void ReturnsExistingModelMetadata()
        {
            // Arrange
            object model = new { foo = "fooValue", bar = "barValue" };
            ModelMetadata originalMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, model.GetType());
            ViewDataDictionary vdd = new ViewDataDictionary(model) { ModelMetadata = originalMetadata };

            // Act
            ModelMetadata metadata = vdd.ModelMetadata;

            // Assert
            Assert.Same(originalMetadata, metadata);
        }

        [Fact]
        public void ModelMetadataIsNullIfModelMetadataHasNotBeenSetAndModelIsNull()
        {
            // Arrange
            ViewDataDictionary vdd = new ViewDataDictionary();

            // Act
            ModelMetadata metadata = vdd.ModelMetadata;

            // Assert
            Assert.Null(metadata);
        }

        [Fact]
        public void ModelMetadataCanBeFabricatedWithNullModelAndGenericViewDataDictionary()
        {
            // Arrange
            ViewDataDictionary vdd = new ViewDataDictionary<Exception>();

            // Act
            ModelMetadata metadata = vdd.ModelMetadata;

            // Assert
            Assert.NotNull(metadata);
            Assert.Equal(typeof(Exception), metadata.ModelType);
        }

        [Fact]
        public void ModelSetterThrowsIfValueIsNullAndModelTypeIsNonNullable()
        {
            // Arrange
            ViewDataDictionary vdd = new ViewDataDictionary<int>();

            // Act & assert
            Assert.Throws<InvalidOperationException>(
                delegate { vdd.Model = null; },
                @"The model item passed into the dictionary is null, but this dictionary requires a non-null model item of type 'System.Int32'.");
        }

        [Fact]
        public void ChangingModelReplacesModelMetadata()
        {
            // Arrange
            ViewDataDictionary vdd = new ViewDataDictionary(new Object());
            ModelMetadata originalMetadata = vdd.ModelMetadata;

            // Act
            vdd.Model = "New Model";

            // Assert
            Assert.NotSame(originalMetadata, vdd.ModelMetadata);
        }

        public class TheQueryStringParam
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public string Model { get; set; }
        }

        public class Person : CustomTypeDescriptor
        {
            public override PropertyDescriptorCollection GetProperties()
            {
                return new PropertyDescriptorCollection(new PersonPropertyDescriptor[] { new PersonPropertyDescriptor() });
            }
        }

        public class PersonPropertyDescriptor : PropertyDescriptor
        {
            public PersonPropertyDescriptor()
                : base("Bar.Baz", null)
            {
            }

            public override object GetValue(object component)
            {
                return "Quux";
            }

            public override bool CanResetValue(object component)
            {
                return false;
            }

            public override Type ComponentType
            {
                get { return typeof(Person); }
            }

            public override bool IsReadOnly
            {
                get { return false; }
            }

            public override Type PropertyType
            {
                get { return typeof(string); }
            }

            public override void ResetValue(object component)
            {
            }

            public override void SetValue(object component, object value)
            {
            }

            public override bool ShouldSerializeValue(object component)
            {
                return true;
            }
        }
    }
}
