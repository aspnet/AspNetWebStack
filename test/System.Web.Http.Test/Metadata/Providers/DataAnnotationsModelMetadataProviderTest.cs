// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.TestCommon;

namespace System.Web.Http.Metadata.Providers
{
    public class DataAnnotationsModelMetadataProviderTest
    {
        [Fact]
        public void GetMetadataForPropertiesSetTypesAndPropertyNames()
        {
            // Arrange
            var provider = new DataAnnotationsModelMetadataProvider();

            // Act
            IEnumerable<ModelMetadata> result = provider.GetMetadataForProperties("foo", typeof(string));

            // Assert
            Assert.Contains(result, m => m.ModelType == typeof(int)
                                        && m.PropertyName == "Length"
                                        && (int)m.Model == 3);
        }

        [Fact]
        public void GetMetadataForPropertySetsTypeAndPropertyName()
        {
            // Arrange
            var provider = new DataAnnotationsModelMetadataProvider();

            // Act
            ModelMetadata result = provider.GetMetadataForProperty(null, typeof(string), "Length");

            // Assert
            Assert.Equal(typeof(int), result.ModelType);
            Assert.Equal("Length", result.PropertyName);
        }

        [Fact]
        public void GetMetadataForTypeSetsTypeWithNullPropertyName()
        {
            // Arrange
            var provider = new DataAnnotationsModelMetadataProvider();

            // Act
            ModelMetadata result = provider.GetMetadataForType(null, typeof(string));

            // Assert
            Assert.Equal(typeof(string), result.ModelType);
            Assert.Null(result.PropertyName);
        }

        [Theory]
        [InlineData("NoAttributes", false)]
        [InlineData("ReadOnlyAttribute", true)]
        [InlineData("EditableAttribute", true)]
        [InlineData("BothAttributes", false)]
        public void ReadOnlyTests(string propertyName, bool expected)
        {
            // Arrange
            var provider = new DataAnnotationsModelMetadataProvider();

            // Act
            var actual = provider.GetMetadataForProperty(null, typeof(ReadOnlyModel), propertyName).IsReadOnly;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("NoAttribute", "NoAttribute")]
        [InlineData("NothingSet", "NothingSet")]
        [InlineData("EmptyDisplayName", "")]
        [InlineData("DescriptionSet", "DescriptionSet")]
        [InlineData("NameSet", "Name text1")]
        [InlineData("DisplayNameSet", "Just DisplayName")]
        [InlineData("BothSet", "Name text2")]
        [InlineData("FallbackToDisplayName", "Fallback")]
        [InlineData("FallbackToProperty", "FallbackToProperty")]
        [InlineData("FallbackToPropertyFromDisplayName", "FallbackToPropertyFromDisplayName")]
        [InlineData("DisplayNameDefault", "")] // The default for DisplayName is the empty string, we don't have special handling for it, and nither does MVC.
        public void DataAnnotationsNameTests(string propertyName, string expected)
        {
            // Arrange
            var provider = new DataAnnotationsModelMetadataProvider();

            // Act
            var actual = provider.GetMetadataForProperty(null, typeof(DisplayModel), propertyName).GetDisplayName();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void DisplayAttribute_WithLocalizedName()
        {
            // Guard
            var expected = Resources.String1;
            Assert.NotEqual("String1", expected);

            // Arrange
            var provider = new DataAnnotationsModelMetadataProvider();

            // Act
            var actual = provider.GetMetadataForProperty(null, typeof(DisplayModel), "Localized").GetDisplayName();
            
            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("NoAttribute", null)]
        [InlineData("NothingSet", null)]
        [InlineData("NameSet", null)]
        [InlineData("DescriptionSet", "Description text1")]
        [InlineData("BothSet", "Description text2")]
        public void DataAnnotationsDescriptionTests(string propertyName, string expected)
        {
            // Arrange
            var provider = new DataAnnotationsModelMetadataProvider();

            // Act
            var actual = provider.GetMetadataForProperty(null, typeof(DisplayModel), propertyName).Description;

            // Assert
            Assert.Equal(expected, actual);
        }

        // [Display] & [DisplayName] tests
        private class DisplayModel
        {
            public int NoAttribute { get; set; }

            // Description + Name combination.

            [Display]
            public int NothingSet { get; set; }

            [Display(Name = "")]
            public int EmptyDisplayName { get; set; }

            [Display(Description = "Description text1")]
            public int DescriptionSet { get; set; }

            [Display(Name = "Name text1")]
            public int NameSet { get; set; }

            [DisplayName("Just DisplayName")]
            public int DisplayNameSet { get; set; }

            [Display(Description = "Description text2", Name = "Name text2")]
            [DisplayName("This won't be used")]
            public int BothSet { get; set; }

            [Display(Name = "String1", ResourceType = typeof(Resources))]
            public int Localized { get; set; }

            [Display]
            [DisplayName("Fallback")]
            public int FallbackToDisplayName { get; set; }

            [Display]
            public int FallbackToProperty { get; set; }

            [DisplayName(null)]
            public int FallbackToPropertyFromDisplayName { get; set; }

            [DisplayName]
            public int DisplayNameDefault { get; set; }
        }

        // [ReadOnly] & [Editable] tests
        private class ReadOnlyModel
        {
            public int NoAttributes { get; set; }

            [ReadOnly(true)]
            public int ReadOnlyAttribute { get; set; }

            [Editable(false)]
            public int EditableAttribute { get; set; }

            [ReadOnly(true)]
            [Editable(true)]
            public int BothAttributes { get; set; }

            // Editable trumps ReadOnly
        }
    }
}
