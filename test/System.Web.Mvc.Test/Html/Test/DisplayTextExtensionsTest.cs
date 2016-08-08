// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;

namespace System.Web.Mvc.Html.Test
{
    public class DisplayTextExtensionsTest
    {
        [Fact]
        public void DisplayText_ThrowsArgumentNull_IfNameNull()
        {
            // Arrange
            var viewData = new ViewDataDictionary<OverriddenToStringModel>(model: null);
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act & Assert
            // Note ArgumentNullException uses incorrect parameter name.
            Assert.ThrowsArgumentNull(
                () => helper.DisplayText(name: null),
                "expression");
        }

        [Fact]
        public void DisplayTextFor_ThrowsArgumentNull_IfExpressionNull()
        {
            // Arrange
            var viewData = new ViewDataDictionary<OverriddenToStringModel>(model: null);
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => helper.DisplayTextFor<OverriddenToStringModel, string>(expression: null),
                "expression");
        }

        [Fact]
        public void DisplayText_ReturnsEmpty_IfValueNull()
        {
            // Arrange
            var viewData = new ViewDataDictionary<OverriddenToStringModel>(model: null);
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            MvcHtmlString result = helper.DisplayText("");

            // Assert
            Assert.Empty(result.ToHtmlString());
        }

        [Fact]
        public void DisplayTextFor_ReturnsEmpty_IfValueNull()
        {
            // Arrange
            var viewData = new ViewDataDictionary<OverriddenToStringModel>(model: null);
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            MvcHtmlString result = helper.DisplayTextFor(m => m);

            // Assert
            Assert.Empty(result.ToHtmlString());
        }

        [Fact]
        public void DisplayText_ReturnsNullDisplayText_IfSetAndValueNull()
        {
            // Arrange
            var viewData = new ViewDataDictionary<OverriddenToStringModel>(model: null);
            viewData.ModelMetadata.NullDisplayText = "Null display Text";
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            MvcHtmlString result = helper.DisplayText("");

            // Assert
            Assert.Equal("Null display Text", result.ToHtmlString());
        }

        [Fact]
        public void DisplayTextFor_ReturnsNullDisplayText_IfSetAndValueNull()
        {
            // Arrange
            var viewData = new ViewDataDictionary<OverriddenToStringModel>(model: null);
            viewData.ModelMetadata.NullDisplayText = "Null display Text";
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            MvcHtmlString result = helper.DisplayTextFor(m => m);

            // Assert
            Assert.Equal("Null display Text", result.ToHtmlString());
        }

        [Fact]
        public void DisplayText_ReturnsValue_IfNameEmpty()
        {
            // Arrange
            var model = new OverriddenToStringModel("Model value");
            var viewData = new ViewDataDictionary<OverriddenToStringModel>(model);
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            MvcHtmlString result = helper.DisplayText("");

            // Assert
            Assert.Equal("Model value", result.ToHtmlString());
        }

        [Fact]
        public void DisplayText_ReturnsEmpty_IfNameNotFound()
        {
            // Arrange
            var model = new OverriddenToStringModel("Model value");
            var viewData = new ViewDataDictionary<OverriddenToStringModel>(model);
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            MvcHtmlString result = helper.DisplayText("NonExistentProperty");

            // Assert
            Assert.Empty(result.ToHtmlString());
        }

        [Fact]
        public void DisplayTextFor_ReturnsValue_IfIdentityExpression()
        {
            // Arrange
            var model = new OverriddenToStringModel("Model value");
            var viewData = new ViewDataDictionary<OverriddenToStringModel>(model);
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            MvcHtmlString result = helper.DisplayTextFor(m => m);

            // Assert
            Assert.Equal("Model value", result.ToHtmlString());
        }

        [Theory]
        [PropertyData("ConditionallyHtmlEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void DisplayText_HonoursHtmlEncode_IfOverridden(string text, bool htmlEncode, string expectedResult)
        {
            // Arrange
            var model = new OverriddenToStringModel(text);
            var viewData = new ViewDataDictionary<OverriddenToStringModel>(model);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            MvcHtmlString result = helper.DisplayText("");

            // Assert
            Assert.Equal(expectedResult, result.ToHtmlString());
        }

        [Theory]
        [PropertyData("ConditionallyHtmlEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void DisplayTextFor_HonoursHtmlEncode_IfOverridden(string text, bool htmlEncode, string expectedResult)
        {
            // Arrange
            var model = new OverriddenToStringModel(text);
            var viewData = new ViewDataDictionary<OverriddenToStringModel>(model);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            MvcHtmlString result = helper.DisplayTextFor(m => m);

            // Assert
            Assert.Equal(expectedResult, result.ToHtmlString());
        }

        [Theory]
        [PropertyData("ConditionallyHtmlEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void DisplayText_HonoursHtmlEncode_ForProperty(string text, bool htmlEncode, string expectedResult)
        {
            // Arrange
            var model = new DontHtmlEncodeModel
            {
                Encoded = text,
                NotEncoded = text,
            };
            var viewData = new ViewDataDictionary<DontHtmlEncodeModel>(model);
            var helper = MvcHelper.GetHtmlHelper(viewData);
            var propertyName = htmlEncode ? "Encoded" : "NotEncoded";

            // Act
            MvcHtmlString result = helper.DisplayText(propertyName);

            // Assert
            Assert.Equal(expectedResult, result.ToHtmlString());
        }

        [Theory]
        [PropertyData("ConditionallyHtmlEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void DisplayTextFor_HonoursHtmlEncode_ForProperty(string text, bool htmlEncode, string expectedResult)
        {
            // Arrange
            var model = new DontHtmlEncodeModel
            {
                Encoded = text,
                NotEncoded = text,
            };
            var viewData = new ViewDataDictionary<DontHtmlEncodeModel>(model);
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            MvcHtmlString result;
            if (htmlEncode)
            {
                result = helper.DisplayTextFor(m => m.Encoded);
            }
            else
            {
                result = helper.DisplayTextFor(m => m.NotEncoded);
            }

            // Assert
            Assert.Equal(expectedResult, result.ToHtmlString());
        }

        [Fact]
        public void DisplayText_ReturnsSimpleDisplayText_IfSetAndValueNonNull()
        {
            // Arrange
            var model = new OverriddenToStringModel("ignored text");
            var viewData = new ViewDataDictionary<OverriddenToStringModel>(model);
            viewData.ModelMetadata.SimpleDisplayText = "Simple display text";
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            MvcHtmlString result = helper.DisplayText("");

            // Assert
            Assert.Equal("Simple display text", result.ToHtmlString());
        }

        [Fact]
        public void DisplayTextFor_ReturnsSimpleDisplayText_IfSetAndValueNonNull()
        {
            // Arrange
            var model = new OverriddenToStringModel("ignored text");
            var viewData = new ViewDataDictionary<OverriddenToStringModel>(model);
            viewData.ModelMetadata.SimpleDisplayText = "Simple display text";
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            MvcHtmlString result = helper.DisplayTextFor(m => m);

            // Assert
            Assert.Equal("Simple display text", result.ToHtmlString());
        }

        [Fact]
        public void DisplayText_ReturnsPropertyValue_IfNameFound()
        {
            // Arrange
            var model = new OverriddenToStringModel("ignored text")
            {
                Name = "Property value",
            };
            var viewData = new ViewDataDictionary<OverriddenToStringModel>(model);
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            MvcHtmlString result = helper.DisplayText("Name");

            // Assert
            Assert.Equal("Property value", result.ToHtmlString());
        }

        [Fact]
        public void DisplayTextFor_ReturnsPropertyValue_IfPropertyExpression()
        {
            // Arrange
            var model = new OverriddenToStringModel("ignored text")
            {
                Name = "Property value",
            };
            var viewData = new ViewDataDictionary<OverriddenToStringModel>(model);
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            MvcHtmlString result = helper.DisplayTextFor(m => m.Name);

            // Assert
            Assert.Equal("Property value", result.ToHtmlString());
        }

        [Fact]
        public void DisplayText_ReturnsViewDataEntry()
        {
            // Arrange
            var model = new OverriddenToStringModel("Model value")
            {
                Name = "Property value",
            };
            var viewData = new ViewDataDictionary<OverriddenToStringModel>(model)
            {
                { "Name", "View data dictionary value" },
            };
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            MvcHtmlString result = helper.DisplayText("Name");

            // Assert
            Assert.Equal("View data dictionary value", result.ToHtmlString());
        }

        [Fact]
        public void DisplayTextFor_IgnoresViewDataEntry()
        {
            // Arrange
            var model = new OverriddenToStringModel("Model value")
            {
                Name = "Property value",
            };
            var viewData = new ViewDataDictionary<OverriddenToStringModel>(model)
            {
                { "Name", "View data dictionary value" },
            };
            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            MvcHtmlString result = helper.DisplayTextFor(m => m.Name);

            // Assert
            Assert.Equal("Property value", result.ToHtmlString());
        }

        [Fact]
        public void DisplayText_ReturnsModelStateEntry()
        {
            // Arrange
            var model = new OverriddenToStringModel("Model value")
            {
                Name = "Property value",
            };
            var viewData = new ViewDataDictionary<OverriddenToStringModel>(model)
            {
                { "Name", "View data dictionary value" },
            };
            viewData.TemplateInfo.HtmlFieldPrefix = "FieldPrefix";

            var modelState = new ModelState();
            modelState.Value = new ValueProviderResult(
                rawValue: new string[] { "Attempted name value" },
                attemptedValue: "Attempted name value",
                culture: CultureInfo.InvariantCulture);
            viewData.ModelState["FieldPrefix.Name"] = modelState;

            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            MvcHtmlString result = helper.DisplayText("Name");

            // Assert
            Assert.Equal("View data dictionary value", result.ToHtmlString());
        }

        [Fact]
        public void DisplayTextFor_IgnoresModelStateEntry()
        {
            // Arrange
            var model = new OverriddenToStringModel("Model value")
            {
                Name = "Property value",
            };
            var viewData = new ViewDataDictionary<OverriddenToStringModel>(model)
            {
                { "Name", "View data dictionary value" },
            };
            viewData.TemplateInfo.HtmlFieldPrefix = "FieldPrefix";

            var modelState = new ModelState();
            modelState.Value = new ValueProviderResult(
                rawValue: new string[] { "Attempted name value" },
                attemptedValue: "Attempted name value",
                culture: CultureInfo.InvariantCulture);
            viewData.ModelState["FieldPrefix.Name"] = modelState;

            var helper = MvcHelper.GetHtmlHelper(viewData);

            // Act
            MvcHtmlString result = helper.DisplayTextFor(m => m.Name);

            // Assert
            Assert.Equal("Property value", result.ToHtmlString());
        }

        private sealed class DontHtmlEncodeModel
        {
            public string Encoded { get; set; }

            [DisplayFormat(HtmlEncode = false)]
            public string NotEncoded { get; set; }
        }

        // ModelMetadata.SimpleDisplayText returns ToString() displayForModelResult if that method has been overridden.
        private sealed class OverriddenToStringModel
        {
            private readonly string _simpleDisplayText;

            public OverriddenToStringModel(string simpleDisplayText)
            {
                _simpleDisplayText = simpleDisplayText;
            }

            public string Name { get; set; }

            public override string ToString()
            {
                return _simpleDisplayText;
            }
        }
    }
}
