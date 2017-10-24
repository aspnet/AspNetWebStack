// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public abstract class DataAnnotationsModelMetadataProviderTestBase
    {
        protected abstract AssociatedMetadataProvider MakeProvider();

        [Fact]
        public void GetMetadataForPropertiesSetTypesAndPropertyNames()
        {
            // Arrange
            var provider = MakeProvider();

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
            var provider = MakeProvider();

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
            var provider = MakeProvider();

            // Act
            ModelMetadata result = provider.GetMetadataForType(null, typeof(string));

            // Assert
            Assert.Equal(typeof(string), result.ModelType);
            Assert.Null(result.PropertyName);
        }

        // [HiddenInput] tests

        class HiddenModel
        {
            public int NoAttribute { get; set; }

            [HiddenInput]
            public int DefaultHidden { get; set; }

            [HiddenInput(DisplayValue = false)]
            public int HiddenWithDisplayValueFalse { get; set; }

            [HiddenInput]
            [UIHint("CustomUIHint")]
            public int HiddenAndUIHint { get; set; }
        }

        [Fact]
        public void HiddenAttributeSetsTemplateHintAndHideSurroundingHtml()
        {
            // Arrange
            var provider = MakeProvider();

            // Act & Assert
            ModelMetadata noAttributeMetadata = provider.GetMetadataForProperty(null, typeof(HiddenModel), "NoAttribute");
            Assert.Null(noAttributeMetadata.TemplateHint);
            Assert.False(noAttributeMetadata.HideSurroundingHtml);

            ModelMetadata defaultHiddenMetadata = provider.GetMetadataForProperty(null, typeof(HiddenModel), "DefaultHidden");
            Assert.Equal("HiddenInput", defaultHiddenMetadata.TemplateHint);
            Assert.False(defaultHiddenMetadata.HideSurroundingHtml);

            ModelMetadata hiddenWithDisplayValueFalseMetadata = provider.GetMetadataForProperty(null, typeof(HiddenModel), "HiddenWithDisplayValueFalse");
            Assert.Equal("HiddenInput", hiddenWithDisplayValueFalseMetadata.TemplateHint);
            Assert.True(hiddenWithDisplayValueFalseMetadata.HideSurroundingHtml);

            // [UIHint] overrides the template hint from [Hidden]
            Assert.Equal("CustomUIHint", provider.GetMetadataForProperty(null, typeof(HiddenModel), "HiddenAndUIHint").TemplateHint);
        }

        // [UIHint] tests

        class UIHintModel
        {
            public int NoAttribute { get; set; }

            [UIHint("MyCustomTemplate")]
            public int DefaultUIHint { get; set; }

            [UIHint("MyMvcTemplate", "MVC")]
            public int MvcUIHint { get; set; }

            [UIHint("MyWebFormsTemplate", "WebForms")]
            public int NoMvcUIHint { get; set; }

            [UIHint("MyDefaultTemplate")]
            [UIHint("MyWebFormsTemplate", "WebForms")]
            [UIHint("MyMvcTemplate", "MVC")]
            public int MultipleUIHint { get; set; }
        }

        [Fact]
        public void UIHintAttributeSetsTemplateHint()
        {
            // Arrange
            var provider = MakeProvider();

            // Act & Assert
            Assert.Null(provider.GetMetadataForProperty(null, typeof(UIHintModel), "NoAttribute").TemplateHint);
            Assert.Equal("MyCustomTemplate", provider.GetMetadataForProperty(null, typeof(UIHintModel), "DefaultUIHint").TemplateHint);
            Assert.Equal("MyMvcTemplate", provider.GetMetadataForProperty(null, typeof(UIHintModel), "MvcUIHint").TemplateHint);
            Assert.Null(provider.GetMetadataForProperty(null, typeof(UIHintModel), "NoMvcUIHint").TemplateHint);

            Assert.Equal("MyMvcTemplate", provider.GetMetadataForProperty(null, typeof(UIHintModel), "MultipleUIHint").TemplateHint);
        }

        // [DataType] tests

        const string DerivedDataTypeAttributeFormatString = "Time is {0:HH:mm}";

        class DerivedDataTypeAttribute : DataTypeAttribute
        {
            public DerivedDataTypeAttribute()
                : base(DataType.Time)
            {
                DisplayFormat = new DisplayFormatAttribute
                {
                    ApplyFormatInEditMode = true,
                    DataFormatString = DerivedDataTypeAttributeFormatString,
                };
            }
        }

        class DataTypeModel
        {
            public int NoAttribute { get; set; }

            [DataType(DataType.EmailAddress)]
            public int EmailAddressProperty { get; set; }

            [DataType("CustomDataType")]
            public int CustomDataTypeProperty { get; set; }

            [DataType(DataType.Date)]
            public DateTime DateProperty { get; set; }

            [DerivedDataType]
            public DateTime TimeProperty { get; set; }
        }

        [Fact]
        public void DataTypeAttributeSetsDataTypeName()
        {
            // Arrange
            var provider = MakeProvider();

            // Act & Assert
            Assert.Null(provider.GetMetadataForProperty(null, typeof(DataTypeModel), "NoAttribute").DataTypeName);
            Assert.Equal("EmailAddress", provider.GetMetadataForProperty(null, typeof(DataTypeModel), "EmailAddressProperty").DataTypeName);
            Assert.Equal("CustomDataType", provider.GetMetadataForProperty(null, typeof(DataTypeModel), "CustomDataTypeProperty").DataTypeName);
            Assert.Equal("Date", provider.GetMetadataForProperty(null, typeof(DataTypeModel), "DateProperty").DataTypeName);
            Assert.Equal("Time", provider.GetMetadataForProperty(null, typeof(DataTypeModel), "TimeProperty").DataTypeName);
        }

        [Theory]
        [InlineData("NoAttribute", null)]
        [InlineData("EmailAddressProperty", null)]
        [InlineData("CustomDataTypeProperty", null)]
        [InlineData("DateProperty", "{0:d}")] // DataType.Date default format
        [InlineData("TimeProperty", DerivedDataTypeAttributeFormatString)]
        public void DataTypeAttributeSetsDisplayFormats(string propertyName, string formatString)
        {
            // Arrange
            var provider = MakeProvider();

            // Act
            string displayFormat = provider.GetMetadataForProperty(null, typeof(DataTypeModel), propertyName).DisplayFormatString;
            string editFormat = provider.GetMetadataForProperty(null, typeof(DataTypeModel), propertyName).EditFormatString;

            // Assert
            Assert.Equal(formatString, displayFormat);
            Assert.Equal(formatString, editFormat);
        }

        [Theory]
        [InlineData("NoAttribute", false)]
        [InlineData("EmailAddressProperty", false)]
        [InlineData("CustomDataTypeProperty", false)]
        [InlineData("DateProperty", false)] // Uses DataType.Date default format
        [InlineData("TimeProperty", true)]
        public void DataTypeAttributeSetsHasNonDefaultEditFormat(string propertyName, bool expectedNonDefaultEditFormat)
        {
            // Arrange
            var provider = MakeProvider();

            // Act
            bool hasNonDefaultEditFormat = provider.GetMetadataForProperty(null, typeof(DataTypeModel), propertyName).HasNonDefaultEditFormat;

            // Assert
            Assert.Equal(expectedNonDefaultEditFormat, hasNonDefaultEditFormat);
        }

        // [ReadOnly] & [Editable] tests

        class ReadOnlyModel
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

        [Fact]
        public void ReadOnlyTests()
        {
            // Arrange
            var provider = MakeProvider();

            // Act & Assert
            Assert.False(provider.GetMetadataForProperty(null, typeof(ReadOnlyModel), "NoAttributes").IsReadOnly);
            Assert.True(provider.GetMetadataForProperty(null, typeof(ReadOnlyModel), "ReadOnlyAttribute").IsReadOnly);
            Assert.True(provider.GetMetadataForProperty(null, typeof(ReadOnlyModel), "EditableAttribute").IsReadOnly);
            Assert.False(provider.GetMetadataForProperty(null, typeof(ReadOnlyModel), "BothAttributes").IsReadOnly);
        }

        // [DisplayFormat] tests

        class DisplayFormatModel
        {
            public int NoAttribute { get; set; }

            [DisplayFormat(NullDisplayText = "(null value)")]
            public int NullDisplayText { get; set; }

            [DisplayFormat(DataFormatString = "Data {0} format")]
            public int DisplayFormatString { get; set; }

            [DisplayFormat(DataFormatString = "Data {0} format", ApplyFormatInEditMode = true)]
            public int DisplayAndEditFormatString { get; set; }

            [DisplayFormat(ConvertEmptyStringToNull = true)]
            public int ConvertEmptyStringToNullTrue { get; set; }

            [DisplayFormat(ConvertEmptyStringToNull = false)]
            public int ConvertEmptyStringToNullFalse { get; set; }

            [DataType(DataType.Currency)]
            public int DataTypeWithoutDisplayFormatOverride { get; set; }

            [DataType(DataType.Currency)]
            [DisplayFormat(DataFormatString = "format override", ApplyFormatInEditMode = true)]
            public int DataTypeWithDisplayFormatOverride { get; set; }

            [DisplayFormat(HtmlEncode = true)]
            public int HtmlEncodeTrue { get; set; }

            [DisplayFormat(HtmlEncode = false)]
            public int HtmlEncodeFalse { get; set; }

            [DataType(DataType.Currency)]
            [DisplayFormat(HtmlEncode = false)]
            public int HtmlEncodeFalseWithDataType { get; set; }

            // DataType trumps DisplayFormat.HtmlEncode
        }

        [Fact]
        public void DisplayFormatAttributetSetsNullDisplayText()
        {
            // Arrange
            var provider = MakeProvider();

            // Act & Assert
            Assert.Null(provider.GetMetadataForProperty(null, typeof(DisplayFormatModel), "NoAttribute").NullDisplayText);
            Assert.Equal("(null value)", provider.GetMetadataForProperty(null, typeof(DisplayFormatModel), "NullDisplayText").NullDisplayText);
        }

        [Fact]
        public void DisplayFormatAttributeSetsDisplayFormatString()
        {
            // Arrange
            var provider = MakeProvider();

            // Act & Assert
            Assert.Null(provider.GetMetadataForProperty(null, typeof(DisplayFormatModel), "NoAttribute").DisplayFormatString);
            Assert.Equal("Data {0} format", provider.GetMetadataForProperty(null, typeof(DisplayFormatModel), "DisplayFormatString").DisplayFormatString);
            Assert.Equal("Data {0} format", provider.GetMetadataForProperty(null, typeof(DisplayFormatModel), "DisplayAndEditFormatString").DisplayFormatString);
        }

        [Fact]
        public void DisplayFormatAttributeSetEditFormatString()
        {
            // Arrange
            var provider = MakeProvider();

            // Act & Assert
            Assert.Null(provider.GetMetadataForProperty(null, typeof(DisplayFormatModel), "NoAttribute").EditFormatString);
            Assert.Null(provider.GetMetadataForProperty(null, typeof(DisplayFormatModel), "DisplayFormatString").EditFormatString);
            Assert.Equal("Data {0} format", provider.GetMetadataForProperty(null, typeof(DisplayFormatModel), "DisplayAndEditFormatString").EditFormatString);
        }

        [Fact]
        public void DisplayFormatAttributeSetsConvertEmptyStringToNull()
        {
            // Arrange
            var provider = MakeProvider();

            // Act & Assert
            Assert.True(provider.GetMetadataForProperty(null, typeof(DisplayFormatModel), "NoAttribute").ConvertEmptyStringToNull);
            Assert.True(provider.GetMetadataForProperty(null, typeof(DisplayFormatModel), "ConvertEmptyStringToNullTrue").ConvertEmptyStringToNull);
            Assert.False(provider.GetMetadataForProperty(null, typeof(DisplayFormatModel), "ConvertEmptyStringToNullFalse").ConvertEmptyStringToNull);
        }

        [Fact]
        public void DataTypeWithoutDisplayFormatOverrideUsesDataTypesDisplayFormat()
        {
            // Arrange
            var provider = MakeProvider();

            // Act
            string result = provider.GetMetadataForProperty(null, typeof(DisplayFormatModel), "DataTypeWithoutDisplayFormatOverride").DisplayFormatString;

            // Assert
            Assert.Equal("{0:C}", result); // Currency's default format string
        }

        [Fact]
        public void DataTypeWithDisplayFormatOverrideUsesDisplayFormatOverride()
        {
            // Arrange
            var provider = MakeProvider();

            // Act
            string result = provider.GetMetadataForProperty(null, typeof(DisplayFormatModel), "DataTypeWithDisplayFormatOverride").DisplayFormatString;

            // Assert
            Assert.Equal("format override", result);
        }

        [Fact]
        public void DataTypeInfluencedByDisplayFormatAttributeHtmlEncode()
        {
            // Arrange
            var provider = MakeProvider();

            // Act & Assert
            Assert.Null(provider.GetMetadataForProperty(null, typeof(DisplayFormatModel), "NoAttribute").DataTypeName);
            Assert.Null(provider.GetMetadataForProperty(null, typeof(DisplayFormatModel), "HtmlEncodeTrue").DataTypeName);
            Assert.Equal("Html", provider.GetMetadataForProperty(null, typeof(DisplayFormatModel), "HtmlEncodeFalse").DataTypeName);
            Assert.Equal("Currency", provider.GetMetadataForProperty(null, typeof(DisplayFormatModel), "HtmlEncodeFalseWithDataType").DataTypeName);
        }

        [Theory]
        [InlineData("NoAttribute", false)]
        [InlineData("NullDisplayText", false)]
        [InlineData("DisplayFormatString", false)]
        [InlineData("DisplayAndEditFormatString", true)]
        [InlineData("ConvertEmptyStringToNullTrue", false)]
        [InlineData("ConvertEmptyStringToNullFalse", false)]
        [InlineData("DataTypeWithoutDisplayFormatOverride", false)]
        [InlineData("DataTypeWithDisplayFormatOverride", true)]
        [InlineData("HtmlEncodeTrue", false)]
        [InlineData("HtmlEncodeFalse", false)]
        [InlineData("HtmlEncodeFalseWithDataType", false)]
        public void DisplayFormatSetsHasNonDefaultEditFormat(string propertyName, bool expectedHasNonDefaultEditFormat)
        {
            // Arrange
            AssociatedMetadataProvider provider = MakeProvider();

            // Act
            bool hasNonDefaultEditFormat = provider.GetMetadataForProperty(null, typeof(DisplayFormatModel), propertyName).HasNonDefaultEditFormat;

            // Assert
            Assert.Equal(expectedHasNonDefaultEditFormat, hasNonDefaultEditFormat);
        }

        [Theory]
        [InlineData("NoAttribute", true)]
        [InlineData("HtmlEncodeTrue", true)]
        [InlineData("HtmlEncodeFalse", false)]
        [InlineData("HtmlEncodeFalseWithDataType", false)]
        public void DisplayFormatSetsHtmlEncode(string propertyName, bool expectedHtmlEncode)
        {
            // Arrange
            AssociatedMetadataProvider provider = MakeProvider();

            // Act
            bool htmlEncode = provider.GetMetadataForProperty(null, typeof(DisplayFormatModel), propertyName).HtmlEncode;

            // Assert
            Assert.Equal(expectedHtmlEncode, htmlEncode);
        }

        // [ScaffoldColumn] tests

        class ScaffoldColumnModel
        {
            public int NoAttribute { get; set; }

            [ScaffoldColumn(true)]
            public int ScaffoldColumnTrue { get; set; }

            [ScaffoldColumn(false)]
            public int ScaffoldColumnFalse { get; set; }
        }

        [Fact]
        public void ScaffoldColumnAttributeSetsShowForDisplay()
        {
            // Arrange
            var provider = MakeProvider();

            // Act & Assert
            Assert.True(provider.GetMetadataForProperty(null, typeof(ScaffoldColumnModel), "NoAttribute").ShowForDisplay);
            Assert.True(provider.GetMetadataForProperty(null, typeof(ScaffoldColumnModel), "ScaffoldColumnTrue").ShowForDisplay);
            Assert.False(provider.GetMetadataForProperty(null, typeof(ScaffoldColumnModel), "ScaffoldColumnFalse").ShowForDisplay);
        }

        [Fact]
        public void ScaffoldColumnAttributeSetsShowForEdit()
        {
            // Arrange
            var provider = MakeProvider();

            // Act & Assert
            Assert.True(provider.GetMetadataForProperty(null, typeof(ScaffoldColumnModel), "NoAttribute").ShowForEdit);
            Assert.True(provider.GetMetadataForProperty(null, typeof(ScaffoldColumnModel), "ScaffoldColumnTrue").ShowForEdit);
            Assert.False(provider.GetMetadataForProperty(null, typeof(ScaffoldColumnModel), "ScaffoldColumnFalse").ShowForEdit);
        }

        // [DisplayColumn] tests

        [DisplayColumn("NoPropertyWithThisName")]
        class UnknownDisplayColumnModel
        {
        }

        [Fact]
        public void SimpleDisplayNameWithUnknownDisplayColumnThrows()
        {
            // Arrange
            var provider = MakeProvider();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => provider.GetMetadataForType(() => new UnknownDisplayColumnModel(), typeof(UnknownDisplayColumnModel)).SimpleDisplayText,
                typeof(UnknownDisplayColumnModel).FullName + " has a DisplayColumn attribute for NoPropertyWithThisName, but property NoPropertyWithThisName does not exist.");
        }

        [DisplayColumn("WriteOnlyProperty")]
        class WriteOnlyDisplayColumnModel
        {
            public int WriteOnlyProperty
            {
                set { }
            }
        }

        [DisplayColumn("PrivateReadPublicWriteProperty")]
        class PrivateReadPublicWriteDisplayColumnModel
        {
            public int PrivateReadPublicWriteProperty { private get; set; }
        }

        [Fact]
        public void SimpleDisplayTextForTypeWithWriteOnlyDisplayColumnThrows()
        {
            // Arrange
            var provider = MakeProvider();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => provider.GetMetadataForType(() => new WriteOnlyDisplayColumnModel(), typeof(WriteOnlyDisplayColumnModel)).SimpleDisplayText,
                typeof(WriteOnlyDisplayColumnModel).FullName + " has a DisplayColumn attribute for WriteOnlyProperty, but property WriteOnlyProperty does not have a public getter.");

            Assert.Throws<InvalidOperationException>(
                () => provider.GetMetadataForType(() => new PrivateReadPublicWriteDisplayColumnModel(), typeof(PrivateReadPublicWriteDisplayColumnModel)).SimpleDisplayText,
                typeof(PrivateReadPublicWriteDisplayColumnModel).FullName + " has a DisplayColumn attribute for PrivateReadPublicWriteProperty, but property PrivateReadPublicWriteProperty does not have a public getter.");
        }

        [DisplayColumn("DisplayColumnProperty")]
        class SimpleDisplayTextAttributeModel
        {
            public int FirstProperty
            {
                get { return 42; }
            }

            [ScaffoldColumn(false)]
            public string DisplayColumnProperty { get; set; }
        }

        class SimpleDisplayTextAttributeModelContainer
        {
            [DisplayFormat(NullDisplayText = "This is the null display text")]
            public SimpleDisplayTextAttributeModel Inner { get; set; }
        }

        [Fact]
        public void SimpleDisplayTextForNonNullClassWithNonNullDisplayColumnValue()
        {
            // Arrange
            string expected = "Custom property display value";
            var provider = MakeProvider();
            var model = new SimpleDisplayTextAttributeModel { DisplayColumnProperty = expected };
            var metadata = provider.GetMetadataForType(() => model, typeof(SimpleDisplayTextAttributeModel));

            // Act
            string result = metadata.SimpleDisplayText;

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void SimpleDisplayTextForNullClassRevertsToDefaultBehavior()
        {
            // Arrange
            var provider = MakeProvider();
            var metadata = provider.GetMetadataForProperty(null, typeof(SimpleDisplayTextAttributeModelContainer), "Inner");

            // Act
            string result = metadata.SimpleDisplayText;

            // Assert
            Assert.Equal("This is the null display text", result);
        }

        [Fact]
        public void SimpleDisplayTextForNonNullClassWithNullDisplayColumnValueRevertsToDefaultBehavior()
        {
            // Arrange
            var provider = MakeProvider();
            var model = new SimpleDisplayTextAttributeModel();
            var metadata = provider.GetMetadataForType(() => model, typeof(SimpleDisplayTextAttributeModel));

            // Act
            string result = metadata.SimpleDisplayText;

            // Assert
            Assert.Equal("42", result); // Falls back to the default logic of first property value
        }

        // [Required] tests

        class IsRequiredModel
        {
            public int NonNullableWithout { get; set; }

            public string NullableWithout { get; set; }

            [Required]
            public string NullableWith { get; set; }
        }

        [Fact]
        public void IsRequiredTests()
        {
            // Arrange
            var provider = MakeProvider();

            // Act & Assert
            Assert.True(provider.GetMetadataForProperty(null, typeof(IsRequiredModel), "NonNullableWithout").IsRequired);
            Assert.False(provider.GetMetadataForProperty(null, typeof(IsRequiredModel), "NullableWithout").IsRequired);
            Assert.True(provider.GetMetadataForProperty(null, typeof(IsRequiredModel), "NullableWith").IsRequired);
        }

        // [Display] & [DisplayName] tests

        class DisplayModel
        {
            public int NoAttribute { get; set; }

            // Description

            [Display]
            public int DescriptionNotSet { get; set; }

            [Display(Description = "Description text")]
            public int DescriptionSet { get; set; }

            // DisplayName

            [DisplayName("Value from DisplayName")]
            public int DisplayNameAttributeNoDisplayAttribute { get; set; }

            [Display]
            public int DisplayAttributeNameNotSet { get; set; }

            [Display(Name = "Non empty name")]
            public int DisplayAttributeNonEmptyName { get; set; }

            [Display]
            [DisplayName("Value from DisplayName")]
            public int BothAttributesNameNotSet { get; set; }

            [Display(Name = "Value from Display")]
            [DisplayName("Value from DisplayName")]
            public int BothAttributes { get; set; }

            // Display trumps DisplayName

            // Order

            [Display]
            public int OrderNotSet { get; set; }

            [Display(Order = 2112)]
            public int OrderSet { get; set; }

            // ShortDisplayName

            [Display]
            public int ShortNameNotSet { get; set; }

            [Display(ShortName = "Short name")]
            public int ShortNameSet { get; set; }

            // Watermark

            [Display]
            public int PromptNotSet { get; set; }

            [Display(Prompt = "Enter stuff here")]
            public int PromptSet { get; set; }
        }

        [Fact]
        public void DescriptionTests()
        {
            // Arrange
            var provider = MakeProvider();

            // Act & Assert
            Assert.Null(provider.GetMetadataForProperty(null, typeof(DisplayModel), "NoAttribute").Description);
            Assert.Null(provider.GetMetadataForProperty(null, typeof(DisplayModel), "DescriptionNotSet").Description);
            Assert.Equal("Description text", provider.GetMetadataForProperty(null, typeof(DisplayModel), "DescriptionSet").Description);
        }

        [Fact]
        public void DisplayNameTests()
        {
            // Arrange
            var provider = MakeProvider();

            // Act & Assert
            Assert.Null(provider.GetMetadataForProperty(null, typeof(DisplayModel), "NoAttribute").DisplayName);
            Assert.Equal("Value from DisplayName", provider.GetMetadataForProperty(null, typeof(DisplayModel), "DisplayNameAttributeNoDisplayAttribute").DisplayName);
            Assert.Null(provider.GetMetadataForProperty(null, typeof(DisplayModel), "DisplayAttributeNameNotSet").DisplayName);
            Assert.Equal("Non empty name", provider.GetMetadataForProperty(null, typeof(DisplayModel), "DisplayAttributeNonEmptyName").DisplayName);
            Assert.Equal("Value from DisplayName", provider.GetMetadataForProperty(null, typeof(DisplayModel), "BothAttributesNameNotSet").DisplayName);
            Assert.Equal("Value from Display", provider.GetMetadataForProperty(null, typeof(DisplayModel), "BothAttributes").DisplayName);
        }

        [Fact]
        public void OrderTests()
        {
            // Arrange
            var provider = MakeProvider();

            // Act & Assert
            Assert.Equal(10000, provider.GetMetadataForProperty(null, typeof(DisplayModel), "NoAttribute").Order);
            Assert.Equal(10000, provider.GetMetadataForProperty(null, typeof(DisplayModel), "OrderNotSet").Order);
            Assert.Equal(2112, provider.GetMetadataForProperty(null, typeof(DisplayModel), "OrderSet").Order);
        }

        [Fact]
        public void ShortDisplayNameTests()
        {
            // Arrange
            var provider = MakeProvider();

            // Act & Assert
            Assert.Null(provider.GetMetadataForProperty(null, typeof(DisplayModel), "NoAttribute").ShortDisplayName);
            Assert.Null(provider.GetMetadataForProperty(null, typeof(DisplayModel), "ShortNameNotSet").ShortDisplayName);
            Assert.Equal("Short name", provider.GetMetadataForProperty(null, typeof(DisplayModel), "ShortNameSet").ShortDisplayName);
        }

        [Fact]
        public void WatermarkTests()
        {
            // Arrange
            var provider = MakeProvider();

            // Act & Assert
            Assert.Null(provider.GetMetadataForProperty(null, typeof(DisplayModel), "NoAttribute").Watermark);
            Assert.Null(provider.GetMetadataForProperty(null, typeof(DisplayModel), "PromptNotSet").Watermark);
            Assert.Equal("Enter stuff here", provider.GetMetadataForProperty(null, typeof(DisplayModel), "PromptSet").Watermark);
        }
    }
}
