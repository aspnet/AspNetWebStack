// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web.TestUtil;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class ModelStateDictionaryTest
    {
        [Fact]
        public void AddModelErrorCreatesModelStateIfNotPresent()
        {
            // Arrange
            ModelStateDictionary dictionary = new ModelStateDictionary();

            // Act
            dictionary.AddModelError("some key", "some error");

            // Assert
            KeyValuePair<string, ModelState> kvp = Assert.Single(dictionary);
            Assert.Equal("some key", kvp.Key);
            ModelError error = Assert.Single(kvp.Value.Errors);
            Assert.Equal("some error", error.ErrorMessage);
        }

        [Fact]
        public void AddModelErrorThrowsIfKeyIsNull()
        {
            // Arrange
            ModelStateDictionary dictionary = new ModelStateDictionary();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { dictionary.AddModelError(null, (string)null); }, "key");
        }

        [Fact]
        public void AddModelErrorUsesExistingModelStateIfPresent()
        {
            // Arrange
            ModelStateDictionary dictionary = new ModelStateDictionary();
            dictionary.AddModelError("some key", "some error");
            Exception ex = new Exception();

            // Act
            dictionary.AddModelError("some key", ex);

            // Assert
            KeyValuePair<string, ModelState> kvp = Assert.Single(dictionary);
            Assert.Equal("some key", kvp.Key);

            Assert.Equal(2, kvp.Value.Errors.Count);
            Assert.Equal("some error", kvp.Value.Errors[0].ErrorMessage);
            Assert.Same(ex, kvp.Value.Errors[1].Exception);
        }

        [Fact]
        public void ConstructorThrowsIfDictionaryIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { new ModelStateDictionary((ModelStateDictionary)null); }, "dictionary");
        }

        [Fact]
        public void ConstructorWithDictionaryParameter()
        {
            // Arrange
            ModelStateDictionary oldDictionary = new ModelStateDictionary()
            {
                { "foo", new ModelState() { Value = HtmlHelperTest.GetValueProviderResult("bar", "bar") } }
            };

            // Act
            ModelStateDictionary newDictionary = new ModelStateDictionary(oldDictionary);

            // Assert
            Assert.Single(newDictionary);
            Assert.Equal("bar", newDictionary["foo"].Value.ConvertTo(typeof(string)));
        }

        [Fact]
        public void ConstructorWithDictionaryParameterUsesCopyOnWriteDictionary()
        {
            // Arrange
            ModelStateDictionary oldDictionary = new ModelStateDictionary();

            // Act
            ModelStateDictionary newDictionary = new ModelStateDictionary(oldDictionary);

            // Assert
            Assert.IsType<Dictionary<string, ModelState>>(oldDictionary.InnerDictionary);
            Assert.IsType<CopyOnWriteDictionary<string, ModelState>>(newDictionary.InnerDictionary);
        }

        [Fact]
        public void DictionaryInterface()
        {
            // Arrange
            DictionaryHelper<string, ModelState> helper = new DictionaryHelper<string, ModelState>()
            {
                Creator = () => new ModelStateDictionary(),
                Comparer = StringComparer.OrdinalIgnoreCase,
                SampleKeys = new string[] { "foo", "bar", "baz", "quux", "QUUX" },
                SampleValues = new ModelState[] { new ModelState(), new ModelState(), new ModelState(), new ModelState(), new ModelState() },
                ThrowOnKeyNotFound = false
            };

            // Act & assert
            helper.Execute();
        }

        [Fact]
        public void DictionaryIsSerializable()
        {
            // Arrange
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();

            ModelStateDictionary originalDict = new ModelStateDictionary();
            originalDict.AddModelError("foo", new InvalidOperationException("Some invalid operation."));
            originalDict.AddModelError("foo", new InvalidOperationException("Some other invalid operation."));
            originalDict.AddModelError("bar", "Some exception text.");
            originalDict.SetModelValue("baz", new ValueProviderResult("rawValue", "attemptedValue", CultureInfo.GetCultureInfo("fr-FR")));

            // Act
            formatter.Serialize(stream, originalDict);
            stream.Position = 0;
            ModelStateDictionary deserializedDict = formatter.Deserialize(stream) as ModelStateDictionary;

            // Assert
            Assert.NotNull(deserializedDict);
            Assert.Equal(3, deserializedDict.Count);

            ModelState foo = deserializedDict["FOO"];
            InvalidOperationException exception0 = Assert.IsType<InvalidOperationException>(foo.Errors[0].Exception);
            Assert.Equal("Some invalid operation.", exception0.Message);
            InvalidOperationException exception1 = Assert.IsType<InvalidOperationException>(foo.Errors[1].Exception);
            Assert.Equal("Some other invalid operation.", exception1.Message);

            ModelState bar = deserializedDict["BAR"];
            Assert.Equal("Some exception text.", bar.Errors[0].ErrorMessage);

            ModelState baz = deserializedDict["BAZ"];
            Assert.Equal("rawValue", baz.Value.RawValue);
            Assert.Equal("attemptedValue", baz.Value.AttemptedValue);
            Assert.Equal(CultureInfo.GetCultureInfo("fr-FR"), baz.Value.Culture);
        }

        [Fact]
        public void IsValidFieldReturnsFalseIfDictionaryDoesNotContainKey()
        {
            // Arrange
            ModelStateDictionary msd = new ModelStateDictionary();

            // Act
            bool isValid = msd.IsValidField("foo");

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void IsValidFieldReturnsFalseIfKeyChildContainsErrors()
        {
            // Arrange
            ModelStateDictionary msd = new ModelStateDictionary();
            msd.AddModelError("foo.bar", "error text");

            // Act
            bool isValid = msd.IsValidField("foo");

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void IsValidFieldReturnsFalseIfKeyContainsErrors()
        {
            // Arrange
            ModelStateDictionary msd = new ModelStateDictionary();
            msd.AddModelError("foo", "error text");

            // Act
            bool isValid = msd.IsValidField("foo");

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void IsValidFieldReturnsTrueIfModelStateDoesNotContainErrors()
        {
            // Arrange
            ModelStateDictionary msd = new ModelStateDictionary()
            {
                { "foo", new ModelState() { Value = new ValueProviderResult(null, null, null) } }
            };

            // Act
            bool isValid = msd.IsValidField("foo");

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void IsValidFieldThrowsIfKeyIsNull()
        {
            // Arrange
            ModelStateDictionary msd = new ModelStateDictionary();

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { msd.IsValidField(null); }, "key");
        }

        [Fact]
        public void IsValidPropertyReturnsFalseIfErrors()
        {
            // Arrange
            ModelState errorState = new ModelState() { Value = HtmlHelperTest.GetValueProviderResult("quux", "quux") };
            errorState.Errors.Add("some error");
            ModelStateDictionary dictionary = new ModelStateDictionary()
            {
                { "foo", new ModelState() { Value = HtmlHelperTest.GetValueProviderResult("bar", "bar") } },
                { "baz", errorState }
            };

            // Act
            bool isValid = dictionary.IsValid;

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void IsValidPropertyReturnsTrueIfNoErrors()
        {
            // Arrange
            ModelStateDictionary dictionary = new ModelStateDictionary()
            {
                { "foo", new ModelState() { Value = HtmlHelperTest.GetValueProviderResult("bar", "bar") } },
                { "baz", new ModelState() { Value = HtmlHelperTest.GetValueProviderResult("quux", "bar") } }
            };

            // Act
            bool isValid = dictionary.IsValid;

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void MergeCopiesDictionaryEntries()
        {
            // Arrange
            ModelStateDictionary fooDict = new ModelStateDictionary() { { "foo", new ModelState() } };
            ModelStateDictionary barDict = new ModelStateDictionary() { { "bar", new ModelState() } };

            // Act
            fooDict.Merge(barDict);

            // Assert
            Assert.Equal(2, fooDict.Count);
            Assert.Equal(barDict["bar"], fooDict["bar"]);
        }

        [Fact]
        public void MergeDoesNothingIfParameterIsNull()
        {
            // Arrange
            ModelStateDictionary fooDict = new ModelStateDictionary() { { "foo", new ModelState() } };

            // Act
            fooDict.Merge(null);

            // Assert
            Assert.Single(fooDict);
            Assert.True(fooDict.ContainsKey("foo"));
        }

        [Fact]
        public void SetAttemptedValueCreatesModelStateIfNotPresent()
        {
            // Arrange
            ModelStateDictionary dictionary = new ModelStateDictionary();

            // Act
            dictionary.SetModelValue("some key", HtmlHelperTest.GetValueProviderResult("some value", "some value"));

            // Assert
            Assert.Single(dictionary);
            ModelState modelState = dictionary["some key"];

            Assert.Empty(modelState.Errors);
            Assert.Equal("some value", modelState.Value.ConvertTo(typeof(string)));
        }

        [Fact]
        public void SetAttemptedValueUsesExistingModelStateIfPresent()
        {
            // Arrange
            ModelStateDictionary dictionary = new ModelStateDictionary();
            dictionary.AddModelError("some key", "some error");
            Exception ex = new Exception();

            // Act
            dictionary.SetModelValue("some key", HtmlHelperTest.GetValueProviderResult("some value", "some value"));

            // Assert
            Assert.Single(dictionary);
            ModelState modelState = dictionary["some key"];

            ModelError error = Assert.Single(modelState.Errors);
            Assert.Equal("some error", error.ErrorMessage);
            Assert.Equal("some value", modelState.Value.ConvertTo(typeof(string)));
        }
    }
}
