// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Routing;
using System.Web.WebPages.Scope;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;
using Moq;

namespace System.Web.Mvc.Html.Test
{
    [Xunit.Collection("Uses ScopeStorage or ViewEngines.Engines")]
    public class ValidationExtensionsTest : IDisposable
    {
        // Validate

        [Fact]
        public void Validate_AddsClientValidationMetadata()
        {
            var originalProviders = ModelValidatorProviders.Providers.ToArray();
            ModelValidatorProviders.Providers.Clear();

            try
            {
                // Arrange
                HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());
                FormContext formContext = new FormContext()
                {
                    FormId = "form_id"
                };
                htmlHelper.ViewContext.ClientValidationEnabled = true;
                htmlHelper.ViewContext.FormContext = formContext;

                ModelClientValidationRule[] expectedValidationRules = new ModelClientValidationRule[]
                {
                    new ModelClientValidationRule() { ValidationType = "ValidationRule1" },
                    new ModelClientValidationRule() { ValidationType = "ValidationRule2" }
                };

                Mock<ModelValidator> mockValidator = new Mock<ModelValidator>(ModelMetadata.FromStringExpression("", htmlHelper.ViewContext.ViewData), htmlHelper.ViewContext);
                mockValidator.Setup(v => v.GetClientValidationRules())
                    .Returns(expectedValidationRules);
                Mock<ModelValidatorProvider> mockValidatorProvider = new Mock<ModelValidatorProvider>();
                mockValidatorProvider.Setup(vp => vp.GetValidators(It.IsAny<ModelMetadata>(), It.IsAny<ControllerContext>()))
                    .Returns(new[] { mockValidator.Object });
                ModelValidatorProviders.Providers.Add(mockValidatorProvider.Object);

                // Act
                htmlHelper.Validate("baz");

                // Assert
                Assert.NotNull(formContext.GetValidationMetadataForField("baz"));
                Assert.Equal(expectedValidationRules, formContext.FieldValidators["baz"].ValidationRules.ToArray());
            }
            finally
            {
                ModelValidatorProviders.Providers.Clear();
                foreach (var provider in originalProviders)
                {
                    ModelValidatorProviders.Providers.Add(provider);
                }
            }
        }

        [Fact]
        public void Validate_DoesNothingIfClientValidationIsNotEnabled()
        {
            // Arrange
            HtmlHelper<ValidationModel> htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());
            htmlHelper.ViewContext.FormContext = new FormContext();
            htmlHelper.ViewContext.ClientValidationEnabled = false;

            // Act
            htmlHelper.Validate("foo");

            // Assert
            Assert.Empty(htmlHelper.ViewContext.FormContext.FieldValidators);
        }

        [Fact]
        public void Validate_DoesNothingIfUnobtrusiveJavaScriptIsEnabled()
        {
            // Arrange
            HtmlHelper<ValidationModel> htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());
            htmlHelper.ViewContext.FormContext = new FormContext();
            htmlHelper.ViewContext.ClientValidationEnabled = true;
            htmlHelper.ViewContext.UnobtrusiveJavaScriptEnabled = true;

            // Act
            htmlHelper.Validate("foo");

            // Assert
            Assert.Empty(htmlHelper.ViewContext.FormContext.FieldValidators);
        }

        [Fact]
        public void Validate_ThrowsIfModelNameIsNull()
        {
            // Arrange
            HtmlHelper<ValidationModel> htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { htmlHelper.Validate((string)null /* modelName */); }, "modelName");
        }

        [Fact]
        public void ValidateFor_AddsClientValidationMetadata()
        {
            var originalProviders = ModelValidatorProviders.Providers.ToArray();
            ModelValidatorProviders.Providers.Clear();

            try
            {
                // Arrange
                HtmlHelper<ValidationModel> htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());
                FormContext formContext = new FormContext()
                {
                    FormId = "form_id"
                };
                htmlHelper.ViewContext.ClientValidationEnabled = true;
                htmlHelper.ViewContext.FormContext = formContext;

                ModelClientValidationRule[] expectedValidationRules = new ModelClientValidationRule[]
                {
                    new ModelClientValidationRule() { ValidationType = "ValidationRule1" },
                    new ModelClientValidationRule() { ValidationType = "ValidationRule2" }
                };

                Mock<ModelValidator> mockValidator = new Mock<ModelValidator>(ModelMetadata.FromStringExpression("", htmlHelper.ViewContext.ViewData), htmlHelper.ViewContext);
                mockValidator.Setup(v => v.GetClientValidationRules())
                    .Returns(expectedValidationRules);
                Mock<ModelValidatorProvider> mockValidatorProvider = new Mock<ModelValidatorProvider>();
                mockValidatorProvider.Setup(vp => vp.GetValidators(It.IsAny<ModelMetadata>(), It.IsAny<ControllerContext>()))
                    .Returns(new[] { mockValidator.Object });
                ModelValidatorProviders.Providers.Add(mockValidatorProvider.Object);

                // Act
                htmlHelper.ValidateFor(m => m.baz);

                // Assert
                Assert.NotNull(formContext.GetValidationMetadataForField("baz"));
                Assert.Equal(expectedValidationRules, formContext.FieldValidators["baz"].ValidationRules.ToArray());
            }
            finally
            {
                ModelValidatorProviders.Providers.Clear();
                foreach (var provider in originalProviders)
                {
                    ModelValidatorProviders.Providers.Add(provider);
                }
            }
        }

        // ValidationMessage

        [Fact]
        public void ValidationMessageAllowsEmptyModelName()
        {
            // Arrange
            ViewDataDictionary vdd = new ViewDataDictionary();
            vdd.ModelState.AddModelError("", "some error text");
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(vdd);

            // Act
            MvcHtmlString html = htmlHelper.ValidationMessage("");

            // Assert
            Assert.Equal("<span class=\"field-validation-error\">some error text</span>", html.ToHtmlString());
        }

        [Fact]
        public void ValidationMessageReturnsNullForNullModelState()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithNullModelState());

            // Act
            MvcHtmlString html = htmlHelper.ValidationMessage("foo");

            // Assert
            Assert.Null(html);
        }

        [Fact]
        public void ValidationMessageReturnsFirstErrorWithMessage()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());

            // Act
            MvcHtmlString html = htmlHelper.ValidationMessage("foo");

            // Assert
            Assert.Equal("<span class=\"field-validation-error\">foo error &lt;1&gt;</span>", html.ToHtmlString());
        }

        [Fact]
        public void ValidationMessageReturnsGenericMessageInsteadOfExceptionText()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());

            // Act
            MvcHtmlString html = htmlHelper.ValidationMessage("quux");

            // Assert
            Assert.Equal("<span class=\"field-validation-error\">The value &#39;quuxValue&#39; is invalid.</span>", html.ToHtmlString());
        }

        [Fact]
        public void ValidationMessageReturnsNullForInvalidName()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());

            // Act
            MvcHtmlString html = htmlHelper.ValidationMessage("boo");

            // Assert
            Assert.Null(html);
        }

        [Fact]
        public void ValidationMessageReturnsWithObjectAttributes()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());

            // Act
            MvcHtmlString html = htmlHelper.ValidationMessage("foo", new { bar = "bar" });

            // Assert
            Assert.Equal("<span bar=\"bar\" class=\"field-validation-error\">foo error &lt;1&gt;</span>", html.ToHtmlString());
        }

        [Fact]
        public void ValidationMessageReturnsWithObjectAttributesWithUnderscores()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());

            // Act
            MvcHtmlString html = htmlHelper.ValidationMessage("foo", new { foo_bar = "bar" });

            // Assert
            Assert.Equal("<span class=\"field-validation-error\" foo-bar=\"bar\">foo error &lt;1&gt;</span>", html.ToHtmlString());
        }

        [Fact]
        public void ValidationMessageReturnsWithCustomMessage()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());

            // Act
            MvcHtmlString html = htmlHelper.ValidationMessage("foo", "bar error");

            // Assert
            Assert.Equal("<span class=\"field-validation-error\">bar error</span>", html.ToHtmlString());
        }

        [Fact]
        public void ValidationMessageReturnsWithCustomMessageAndObjectAttributes()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());

            // Act
            MvcHtmlString html = htmlHelper.ValidationMessage("foo", "bar error", new { baz = "baz" });

            // Assert
            Assert.Equal("<span baz=\"baz\" class=\"field-validation-error\">bar error</span>", html.ToHtmlString());
        }

        [Fact]
        public void ValidationMessageReturnsWithCustomMessageAndObjectAttributesWithUnderscores()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());

            // Act
            MvcHtmlString html = htmlHelper.ValidationMessage("foo", "bar error", new { foo_baz = "baz" });

            // Assert
            Assert.Equal("<span class=\"field-validation-error\" foo-baz=\"baz\">bar error</span>", html.ToHtmlString());
        }


        [Fact]
        public void ValidationMessageWithOverriddenTag_UsesGivenTag()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());
            htmlHelper.SetValidationMessageElement("label");

            // Act
            MvcHtmlString html = htmlHelper.ValidationMessage("foo", "bar error");

            // Assert
            Assert.Equal(
                "<label class=\"field-validation-error\">bar error</label>",
                html.ToHtmlString());
        }

        [Fact]
        public void ValidationMessageWithOverriddenTag_SetPropertyInValidationMessageElement_UsesGivenTag()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());
            htmlHelper.ViewContext.ValidationMessageElement = "label";

            // Act
            MvcHtmlString html = htmlHelper.ValidationMessage("foo", "bar error");

            // Assert
            Assert.Equal(
                "<label class=\"field-validation-error\">bar error</label>",
                html.ToHtmlString());
        }

        [Fact]
        public void ValidationMessageWithCustomTag()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());

            // Act
            MvcHtmlString html = htmlHelper.ValidationMessage("foo", "bar error", "label");

            // Assert
            Assert.Equal(
                "<label class=\"field-validation-error\">bar error</label>",
                html.ToHtmlString());
        }

        [Fact]
        public void ValidationMessageWithObjectAttributesWithCustomTag()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());

            // Act
            MvcHtmlString html = htmlHelper.ValidationMessage("foo", "bar error", new { baz = "baz"}, "label");

            // Assert
            Assert.Equal(
                "<label baz=\"baz\" class=\"field-validation-error\">bar error</label>",
                html.ToHtmlString());
        }

        [Fact]
        public void ValidationMessageWithAttributesDictionaryWithCustomTag()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());
            RouteValueDictionary htmlAttributes = new RouteValueDictionary();
            htmlAttributes["baz"] = "baz";

            // Act
            MvcHtmlString html = htmlHelper.ValidationMessage("foo", "bar error", htmlAttributes, "label");

            // Assert
            Assert.Equal(
                "<label baz=\"baz\" class=\"field-validation-error\">bar error</label>",
                html.ToHtmlString());
        }

        [Fact]
        public void ValidationMessageWithoutMessageWithAttributesDictionaryWithCustomTag()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());
            RouteValueDictionary htmlAttributes = new RouteValueDictionary();
            htmlAttributes["baz"] = "baz";

            // Act
            MvcHtmlString html = htmlHelper.ValidationMessage("foo", htmlAttributes, "label");

            // Assert
            Assert.Equal(
                "<label baz=\"baz\" class=\"field-validation-error\">foo error &lt;1&gt;</label>",
                html.ToHtmlString());
        }

        [Fact]
        public void ValidationMessageWithoutMessageWithObjectAttributesWithCustomTag()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());

            // Act
            MvcHtmlString html = htmlHelper.ValidationMessage("foo", new { bar = "bar", baz = "baz"}, "label");

            // Assert
            Assert.Equal(
                "<label bar=\"bar\" baz=\"baz\" class=\"field-validation-error\">foo error &lt;1&gt;</label>",
                html.ToHtmlString());
        }

        [Fact]
        public void ValidationMessageThrowsIfModelNameIsNull()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { htmlHelper.ValidationMessage(null); }, "modelName");
        }

        [Fact]
        public void ValidationMessageWithClientValidation_DefaultMessage_Valid()
        {
            var originalProviders = ModelValidatorProviders.Providers.ToArray();
            ModelValidatorProviders.Providers.Clear();

            try
            {
                // Arrange
                HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());
                FormContext formContext = new FormContext();
                htmlHelper.ViewContext.ClientValidationEnabled = true;
                htmlHelper.ViewContext.FormContext = formContext;

                ModelClientValidationRule[] expectedValidationRules = new ModelClientValidationRule[]
                {
                    new ModelClientValidationRule() { ValidationType = "ValidationRule1" },
                    new ModelClientValidationRule() { ValidationType = "ValidationRule2" }
                };

                Mock<ModelValidator> mockValidator = new Mock<ModelValidator>(ModelMetadata.FromStringExpression("", htmlHelper.ViewContext.ViewData), htmlHelper.ViewContext);
                mockValidator.Setup(v => v.GetClientValidationRules())
                    .Returns(expectedValidationRules);
                Mock<ModelValidatorProvider> mockValidatorProvider = new Mock<ModelValidatorProvider>();
                mockValidatorProvider.Setup(vp => vp.GetValidators(It.IsAny<ModelMetadata>(), It.IsAny<ControllerContext>()))
                    .Returns(new[] { mockValidator.Object });
                ModelValidatorProviders.Providers.Add(mockValidatorProvider.Object);

                // Act
                MvcHtmlString html = htmlHelper.ValidationMessage("baz"); // 'baz' is valid

                // Assert
                Assert.Equal("<span class=\"field-validation-valid\" id=\"baz_validationMessage\"></span>", html.ToHtmlString());
                Assert.NotNull(formContext.GetValidationMetadataForField("baz"));
                Assert.Equal("baz_validationMessage", formContext.FieldValidators["baz"].ValidationMessageId);
                Assert.True(formContext.FieldValidators["baz"].ReplaceValidationMessageContents);
                Assert.Equal(expectedValidationRules, formContext.FieldValidators["baz"].ValidationRules.ToArray());
            }
            finally
            {
                ModelValidatorProviders.Providers.Clear();
                foreach (var provider in originalProviders)
                {
                    ModelValidatorProviders.Providers.Add(provider);
                }
            }
        }

        [Fact]
        public void ValidationMessageWithClientValidation_DefaultMessage_Valid_Unobtrusive()
        {
            var originalProviders = ModelValidatorProviders.Providers.ToArray();
            ModelValidatorProviders.Providers.Clear();

            try
            {
                // Arrange
                HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());
                FormContext formContext = new FormContext();
                htmlHelper.ViewContext.ClientValidationEnabled = true;
                htmlHelper.ViewContext.UnobtrusiveJavaScriptEnabled = true;
                htmlHelper.ViewContext.FormContext = formContext;

                ModelClientValidationRule[] expectedValidationRules = new ModelClientValidationRule[]
                {
                    new ModelClientValidationRule() { ValidationType = "ValidationRule1" },
                    new ModelClientValidationRule() { ValidationType = "ValidationRule2" }
                };

                Mock<ModelValidator> mockValidator = new Mock<ModelValidator>(ModelMetadata.FromStringExpression("", htmlHelper.ViewContext.ViewData), htmlHelper.ViewContext);
                mockValidator.Setup(v => v.GetClientValidationRules())
                    .Returns(expectedValidationRules);
                Mock<ModelValidatorProvider> mockValidatorProvider = new Mock<ModelValidatorProvider>();
                mockValidatorProvider.Setup(vp => vp.GetValidators(It.IsAny<ModelMetadata>(), It.IsAny<ControllerContext>()))
                    .Returns(new[] { mockValidator.Object });
                ModelValidatorProviders.Providers.Add(mockValidatorProvider.Object);

                // Act
                MvcHtmlString html = htmlHelper.ValidationMessage("baz"); // 'baz' is valid

                // Assert
                Assert.Equal("<span class=\"field-validation-valid\" data-valmsg-for=\"baz\" data-valmsg-replace=\"true\"></span>", html.ToHtmlString());
            }
            finally
            {
                ModelValidatorProviders.Providers.Clear();
                foreach (var provider in originalProviders)
                {
                    ModelValidatorProviders.Providers.Add(provider);
                }
            }
        }

        [Fact]
        public void ValidationMessageWithClientValidation_ExplicitMessage_Valid()
        {
            var originalProviders = ModelValidatorProviders.Providers.ToArray();
            ModelValidatorProviders.Providers.Clear();

            try
            {
                // Arrange
                HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());
                FormContext formContext = new FormContext();
                htmlHelper.ViewContext.ClientValidationEnabled = true;
                htmlHelper.ViewContext.FormContext = formContext;

                ModelClientValidationRule[] expectedValidationRules = new ModelClientValidationRule[]
                {
                    new ModelClientValidationRule() { ValidationType = "ValidationRule1" },
                    new ModelClientValidationRule() { ValidationType = "ValidationRule2" }
                };

                Mock<ModelValidator> mockValidator = new Mock<ModelValidator>(ModelMetadata.FromStringExpression("", htmlHelper.ViewContext.ViewData), htmlHelper.ViewContext);
                mockValidator.Setup(v => v.GetClientValidationRules())
                    .Returns(expectedValidationRules);
                Mock<ModelValidatorProvider> mockValidatorProvider = new Mock<ModelValidatorProvider>();
                mockValidatorProvider.Setup(vp => vp.GetValidators(It.IsAny<ModelMetadata>(), It.IsAny<ControllerContext>()))
                    .Returns(new[] { mockValidator.Object });
                ModelValidatorProviders.Providers.Add(mockValidatorProvider.Object);

                // Act
                MvcHtmlString html = htmlHelper.ValidationMessage("baz", "some explicit message"); // 'baz' is valid

                // Assert
                Assert.Equal("<span class=\"field-validation-valid\" id=\"baz_validationMessage\">some explicit message</span>", html.ToHtmlString());
                Assert.NotNull(formContext.GetValidationMetadataForField("baz"));
                Assert.Equal("baz_validationMessage", formContext.FieldValidators["baz"].ValidationMessageId);
                Assert.False(formContext.FieldValidators["baz"].ReplaceValidationMessageContents);
                Assert.Equal(expectedValidationRules, formContext.FieldValidators["baz"].ValidationRules.ToArray());
            }
            finally
            {
                ModelValidatorProviders.Providers.Clear();
                foreach (var provider in originalProviders)
                {
                    ModelValidatorProviders.Providers.Add(provider);
                }
            }
        }

        [Fact]
        public void ValidationMessageWithClientValidation_ExplicitMessage_Valid_Unobtrusive()
        {
            var originalProviders = ModelValidatorProviders.Providers.ToArray();
            ModelValidatorProviders.Providers.Clear();

            try
            {
                // Arrange
                HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());
                FormContext formContext = new FormContext();
                htmlHelper.ViewContext.ClientValidationEnabled = true;
                htmlHelper.ViewContext.UnobtrusiveJavaScriptEnabled = true;
                htmlHelper.ViewContext.FormContext = formContext;

                ModelClientValidationRule[] expectedValidationRules = new ModelClientValidationRule[]
                {
                    new ModelClientValidationRule() { ValidationType = "ValidationRule1" },
                    new ModelClientValidationRule() { ValidationType = "ValidationRule2" }
                };

                Mock<ModelValidator> mockValidator = new Mock<ModelValidator>(ModelMetadata.FromStringExpression("", htmlHelper.ViewContext.ViewData), htmlHelper.ViewContext);
                mockValidator.Setup(v => v.GetClientValidationRules())
                    .Returns(expectedValidationRules);
                Mock<ModelValidatorProvider> mockValidatorProvider = new Mock<ModelValidatorProvider>();
                mockValidatorProvider.Setup(vp => vp.GetValidators(It.IsAny<ModelMetadata>(), It.IsAny<ControllerContext>()))
                    .Returns(new[] { mockValidator.Object });
                ModelValidatorProviders.Providers.Add(mockValidatorProvider.Object);

                // Act
                MvcHtmlString html = htmlHelper.ValidationMessage("baz", "some explicit message"); // 'baz' is valid

                // Assert
                Assert.Equal("<span class=\"field-validation-valid\" data-valmsg-for=\"baz\" data-valmsg-replace=\"false\">some explicit message</span>", html.ToHtmlString());
            }
            finally
            {
                ModelValidatorProviders.Providers.Clear();
                foreach (var provider in originalProviders)
                {
                    ModelValidatorProviders.Providers.Add(provider);
                }
            }
        }

        [Fact]
        public void ValidationMessageWithModelStateAndNoErrors()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());

            // Act
            MvcHtmlString html = htmlHelper.ValidationMessage("baz");

            // Assert
            Assert.Null(html);
        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void ValidationMessage_AttributeEncodes_AddedHtmlAttributes(
            string text,
            bool htmlEncode,
            string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;

            var helper = MvcHelper.GetHtmlHelper(viewData);
            helper.EnableClientValidation();
            helper.EnableUnobtrusiveJavaScript();

            // Act
            var result = helper.ValidationMessage(modelName: "name", htmlAttributes: new { attribute = text, })
                .ToHtmlString();

            // Assert
            Assert.Equal(
                "<span attribute=\"" +
                    encodedText +
                    "\" class=\"field-validation-valid\" data-valmsg-for=\"name\" data-valmsg-replace=\"true\">" +
                    "</span>",
                result);
        }

        [Theory]
        [PropertyData("HtmlEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void ValidationMessage_HtmlEncodes_Message(
            string text,
            bool htmlEncode,
            string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;

            var helper = MvcHelper.GetHtmlHelper(viewData);
            helper.EnableClientValidation();
            helper.EnableUnobtrusiveJavaScript();

            // Act
            var result = helper.ValidationMessage(modelName: "name", validationMessage: text).ToHtmlString();

            // Assert
            Assert.Equal(
                "<span class=\"field-validation-valid\" data-valmsg-for=\"name\" data-valmsg-replace=\"false\">" +
                    encodedText +
                    "</span>",
                result);
        }

        [Theory]
        [PropertyData("HtmlEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void ValidationMessage_HtmlEncodes_ModelStateAttemptedValue(
            string text,
            bool htmlEncode,
            string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            viewData.ModelState.AddModelError(key: "name", errorMessage: null);
            var valueProvider = new ValueProviderResult(rawValue: null, attemptedValue: text, culture: null);
            viewData.ModelState.SetModelValue(key: "name", value: valueProvider);

            var helper = MvcHelper.GetHtmlHelper(viewData);
            helper.EnableClientValidation();
            helper.EnableUnobtrusiveJavaScript();

            // Act
            var result = helper.ValidationMessage(modelName: "name").ToHtmlString();

            // Assert
            Assert.Equal(
                "<span class=\"field-validation-error\" data-valmsg-for=\"name\" data-valmsg-replace=\"true\">The value &#39;" +
                    encodedText +
                    "&#39; is invalid.</span>",
                result);
        }

        [Theory]
        [PropertyData("HtmlEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void ValidationMessage_HtmlEncodes_ModelStateError(
            string text,
            bool htmlEncode,
            string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            viewData.ModelState.AddModelError("name", text);

            var helper = MvcHelper.GetHtmlHelper(viewData);
            helper.EnableClientValidation();
            helper.EnableUnobtrusiveJavaScript();

            // Act
            var result = helper.ValidationMessage(modelName: "name").ToHtmlString();

            // Assert
            Assert.Equal(
                "<span class=\"field-validation-error\" data-valmsg-for=\"name\" data-valmsg-replace=\"true\">" +
                    encodedText +
                    "</span>",
                result);
        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void ValidationMessage_AttributeEncodes_Name(string text, bool htmlEncode, string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;

            var helper = MvcHelper.GetHtmlHelper(viewData);
            helper.EnableClientValidation();
            helper.EnableUnobtrusiveJavaScript();

            // Act
            var result = helper.ValidationMessage(modelName: text).ToHtmlString();

            // Assert
            Assert.Equal(
                "<span class=\"field-validation-valid\" data-valmsg-for=\"" +
                    encodedText +
                    "\" data-valmsg-replace=\"true\"></span>",
                result);
        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void ValidationMessage_AttributeEncodes_Prefix(string text, bool htmlEncode, string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            viewData.TemplateInfo.HtmlFieldPrefix = text;

            var helper = MvcHelper.GetHtmlHelper(viewData);
            helper.EnableClientValidation();
            helper.EnableUnobtrusiveJavaScript();

            // Act
            var result = helper.ValidationMessage(modelName: String.Empty).ToHtmlString();

            // Assert
            Assert.Equal(
                "<span class=\"field-validation-valid\" data-valmsg-for=\"" +
                    encodedText +
                    "\" data-valmsg-replace=\"true\"></span>",
                result);
        }

        [Theory]
        [PropertyData("AttributeEncodedData_NoEncodedText", PropertyType = typeof(EncodedDataSets))]
        public void ValidationMessage_DoesNotEncode_Tag(string text, bool htmlEncode)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;

            var helper = MvcHelper.GetHtmlHelper(viewData);
            helper.EnableClientValidation();
            helper.EnableUnobtrusiveJavaScript();

            // Act
            var result = helper.ValidationMessage(modelName: "name", validationMessage: null, tag: text).ToHtmlString();

            // Assert
            Assert.Equal(
                "<" +
                    text +
                    " class=\"field-validation-valid\" data-valmsg-for=\"name\" data-valmsg-replace=\"true\"></" +
                    text +
                    ">",
                result);
        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void ValidationMessageNonUnobtrusive_AttributeEncodes_AddedHtmlAttributes(
            string text,
            bool htmlEncode,
            string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;

            var helper = MvcHelper.GetHtmlHelper(viewData);
            helper.EnableClientValidation();
            helper.EnableUnobtrusiveJavaScript(enabled: false);

            // Act
            var result = helper.ValidationMessage(modelName: "name", htmlAttributes: new { attribute = text, })
                .ToHtmlString();

            // Assert
            Assert.Equal(
                "<span attribute=\"" +
                    encodedText +
                    "\" class=\"field-validation-valid\" id=\"name_validationMessage\">" +
                    "</span>",
                result);
        }

        [Theory]
        [PropertyData("HtmlEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void ValidationMessageNonUnobtrusive_HtmlEncodes_Message(
            string text,
            bool htmlEncode,
            string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;

            var helper = MvcHelper.GetHtmlHelper(viewData);
            helper.EnableClientValidation();
            helper.EnableUnobtrusiveJavaScript(enabled: false);

            // Act
            var result = helper.ValidationMessage(modelName: "name", validationMessage: text).ToHtmlString();

            // Assert
            Assert.Equal(
                "<span class=\"field-validation-valid\" id=\"name_validationMessage\">" + encodedText + "</span>",
                result);
        }

        [Theory]
        [PropertyData("HtmlEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void ValidationMessageNonUnobtrusive_HtmlEncodes_ModelStateAttemptedValue(
            string text,
            bool htmlEncode,
            string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            viewData.ModelState.AddModelError(key: "name", errorMessage: null);
            var valueProvider = new ValueProviderResult(rawValue: null, attemptedValue: text, culture: null);
            viewData.ModelState.SetModelValue(key: "name", value: valueProvider);

            var helper = MvcHelper.GetHtmlHelper(viewData);
            helper.EnableClientValidation();
            helper.EnableUnobtrusiveJavaScript(enabled: false);

            // Act
            var result = helper.ValidationMessage(modelName: "name").ToHtmlString();

            // Assert
            Assert.Equal(
                "<span class=\"field-validation-error\" id=\"name_validationMessage\">The value &#39;" +
                    encodedText +
                    "&#39; is invalid.</span>",
                result);
        }

        [Theory]
        [PropertyData("HtmlEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void ValidationMessageNonUnobtrusive_HtmlEncodes_ModelStateError(
            string text,
            bool htmlEncode,
            string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            viewData.ModelState.AddModelError("name", text);

            var helper = MvcHelper.GetHtmlHelper(viewData);
            helper.EnableClientValidation();
            helper.EnableUnobtrusiveJavaScript(enabled: false);

            // Act
            var result = helper.ValidationMessage(modelName: "name").ToHtmlString();

            // Assert
            Assert.Equal(
                "<span class=\"field-validation-error\" id=\"name_validationMessage\">" + encodedText + "</span>",
                result);
        }

        [Theory]
        [PropertyData("IdEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void ValidationMessageNonUnobtrusive_IdEncodes_Name(string text, bool htmlEncode, string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;

            var helper = MvcHelper.GetHtmlHelper(viewData);
            helper.EnableClientValidation();
            helper.EnableUnobtrusiveJavaScript(enabled: false);

            // Act
            var result = helper.ValidationMessage(modelName: text).ToHtmlString();

            // Assert
            Assert.Equal(
                "<span class=\"field-validation-valid\" id=\"" + encodedText + "_validationMessage\"></span>",
                result);
        }

        [Theory]
        [PropertyData("IdEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void ValidationMessageNonUnobtrusive_IdEncodes_Prefix(string text, bool htmlEncode, string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            viewData.TemplateInfo.HtmlFieldPrefix = text;

            var helper = MvcHelper.GetHtmlHelper(viewData);
            helper.EnableClientValidation();
            helper.EnableUnobtrusiveJavaScript(enabled: false);

            // Act
            var result = helper.ValidationMessage(modelName: String.Empty).ToHtmlString();

            // Assert
            Assert.Equal(
                "<span class=\"field-validation-valid\" id=\"" + encodedText + "_validationMessage\"></span>",
                result);
        }

        [Theory]
        [PropertyData("AttributeEncodedData_NoEncodedText", PropertyType = typeof(EncodedDataSets))]
        public void ValidationMessageNonUnobtrusive_DoesNotEncode_Tag(string text, bool htmlEncode)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;

            var helper = MvcHelper.GetHtmlHelper(viewData);
            helper.EnableClientValidation();
            helper.EnableUnobtrusiveJavaScript(enabled: false);

            // Act
            var result = helper.ValidationMessage(modelName: "name", validationMessage: null, tag: text).ToHtmlString();

            // Assert
            Assert.Equal(
                "<" + text + " class=\"field-validation-valid\" id=\"name_validationMessage\"></" + text + ">",
                result);
        }

        // ValidationMessageFor

        [Fact]
        public void ValidationMessageForThrowsIfExpressionIsNull()
        {
            // Arrange
            HtmlHelper<object> htmlHelper = MvcHelper.GetHtmlHelper();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => htmlHelper.ValidationMessageFor<object, object>(null),
                "expression"
                );
        }

        [Fact]
        public void ValidationMessageForReturnsNullIfModelStateIsNull()
        {
            // Arrange
            HtmlHelper<ValidationModel> htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithNullModelState());

            // Act
            MvcHtmlString html = htmlHelper.ValidationMessageFor(m => m.foo);

            // Assert
            Assert.Null(html);
        }

        [Fact]
        public void ValidationMessageForReturnsFirstErrorWithErrorMessage()
        {
            // Arrange
            HtmlHelper<ValidationModel> htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());

            // Act
            MvcHtmlString html = htmlHelper.ValidationMessageFor(m => m.foo);

            // Assert
            Assert.Equal("<span class=\"field-validation-error\">foo error &lt;1&gt;</span>", html.ToHtmlString());
        }

        [Fact]
        public void ValidationMessageForReturnsGenericMessageInsteadOfExceptionText()
        {
            // Arrange
            HtmlHelper<ValidationModel> htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());

            // Act
            MvcHtmlString html = htmlHelper.ValidationMessageFor(m => m.quux);

            // Assert
            Assert.Equal("<span class=\"field-validation-error\">The value &#39;quuxValue&#39; is invalid.</span>", html.ToHtmlString());
        }

        [Fact]
        public void ValidationMessageForReturnsWithObjectAttributes()
        {
            // Arrange
            HtmlHelper<ValidationModel> htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());

            // Act
            MvcHtmlString html = htmlHelper.ValidationMessageFor(m => m.foo, null /* validationMessage */, new { bar = "bar" });

            // Assert
            Assert.Equal("<span bar=\"bar\" class=\"field-validation-error\">foo error &lt;1&gt;</span>", html.ToHtmlString());
        }

        [Fact]
        public void ValidationMessageForReturnsWithObjectAttributesWithUnderscores()
        {
            // Arrange
            HtmlHelper<ValidationModel> htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());

            // Act
            MvcHtmlString html = htmlHelper.ValidationMessageFor(m => m.foo, null /* validationMessage */, new { foo_bar = "bar" });

            // Assert
            Assert.Equal("<span class=\"field-validation-error\" foo-bar=\"bar\">foo error &lt;1&gt;</span>", html.ToHtmlString());
        }

        [Fact]
        public void ValidationMessageForReturnsWithCustomMessage()
        {
            // Arrange
            HtmlHelper<ValidationModel> htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());

            // Act
            MvcHtmlString html = htmlHelper.ValidationMessageFor(m => m.foo, "bar error");

            // Assert
            Assert.Equal("<span class=\"field-validation-error\">bar error</span>", html.ToHtmlString());
        }

        [Fact]
        public void ValidationMessageForReturnsWithCustomMessageAndObjectAttributes()
        {
            // Arrange
            HtmlHelper<ValidationModel> htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());

            // Act
            MvcHtmlString html = htmlHelper.ValidationMessageFor(m => m.foo, "bar error", new { baz = "baz" });

            // Assert
            Assert.Equal("<span baz=\"baz\" class=\"field-validation-error\">bar error</span>", html.ToHtmlString());
        }

        [Fact]
        public void ValidationMessageForWithOverriddenTag_UsesGivenTag()
        {
            // Arrange
            HtmlHelper<ValidationModel> htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());
            htmlHelper.SetValidationMessageElement("label");

            // Act
            MvcHtmlString html = htmlHelper.ValidationMessageFor(m => m.foo, "bar error");

            // Assert
            Assert.Equal("<label class=\"field-validation-error\">bar error</label>", html.ToHtmlString());
        }

        [Fact]
        public void ValidationMessageForWithCustomTag()
        {
            // Arrange
            HtmlHelper<ValidationModel> htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());

            // Act
            MvcHtmlString html = htmlHelper.ValidationMessageFor(m => m.foo, "bar error", "label");

            // Assert
            Assert.Equal("<label class=\"field-validation-error\">bar error</label>", html.ToHtmlString());
        }

        [Fact]
        public void ValidationMessageForWithDictionaryAndCustomTag()
        {
            // Arrange
            HtmlHelper<ValidationModel> htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());
            RouteValueDictionary htmlAttributes = new RouteValueDictionary();
            htmlAttributes["class"] = "my-class";

            // Act
            MvcHtmlString html = htmlHelper.ValidationMessageFor(m => m.foo, "bar error", htmlAttributes, "label");

            // Assert
            Assert.Equal("<label class=\"field-validation-error my-class\">bar error</label>", html.ToHtmlString());
        }

        [Fact]
        public void ValidationMessageForWithObjectAttributesAndCustomTag()
        {
            // Arrange
            HtmlHelper<ValidationModel> htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());

            // Act
            MvcHtmlString html = htmlHelper.ValidationMessageFor(m => m.foo, "bar error", new { @class = "baz" }, "label");

            // Assert
            Assert.Equal("<label class=\"field-validation-error baz\">bar error</label>", html.ToHtmlString());
        }

        [Fact]
        public void ValidationMessageForWithClientValidation()
        {
            var originalProviders = ModelValidatorProviders.Providers.ToArray();
            ModelValidatorProviders.Providers.Clear();

            try
            {
                // Arrange
                HtmlHelper<ValidationModel> htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());
                FormContext formContext = new FormContext();
                htmlHelper.ViewContext.ClientValidationEnabled = true;
                htmlHelper.ViewContext.FormContext = formContext;

                ModelClientValidationRule[] expectedValidationRules = new ModelClientValidationRule[]
                {
                    new ModelClientValidationRule() { ValidationType = "ValidationRule1" },
                    new ModelClientValidationRule() { ValidationType = "ValidationRule2" }
                };

                Mock<ModelValidator> mockValidator = new Mock<ModelValidator>(ModelMetadata.FromStringExpression("", htmlHelper.ViewContext.ViewData), htmlHelper.ViewContext);
                mockValidator.Setup(v => v.GetClientValidationRules())
                    .Returns(expectedValidationRules);
                Mock<ModelValidatorProvider> mockValidatorProvider = new Mock<ModelValidatorProvider>();
                mockValidatorProvider.Setup(vp => vp.GetValidators(It.IsAny<ModelMetadata>(), It.IsAny<ControllerContext>()))
                    .Returns(new[] { mockValidator.Object });
                ModelValidatorProviders.Providers.Add(mockValidatorProvider.Object);

                // Act
                MvcHtmlString html = htmlHelper.ValidationMessageFor(m => m.baz);

                // Assert
                Assert.Equal("<span class=\"field-validation-valid\" id=\"baz_validationMessage\"></span>", html.ToHtmlString());
                Assert.NotNull(formContext.GetValidationMetadataForField("baz"));
                Assert.Equal("baz_validationMessage", formContext.FieldValidators["baz"].ValidationMessageId);
                Assert.Equal(expectedValidationRules, formContext.FieldValidators["baz"].ValidationRules.ToArray());
            }
            finally
            {
                ModelValidatorProviders.Providers.Clear();
                foreach (var provider in originalProviders)
                {
                    ModelValidatorProviders.Providers.Add(provider);
                }
            }
        }

        [Fact]
        public void ValidationMessageForWithClientValidation_Unobtrusive()
        {
            var originalProviders = ModelValidatorProviders.Providers.ToArray();
            ModelValidatorProviders.Providers.Clear();

            try
            {
                // Arrange
                HtmlHelper<ValidationModel> htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());
                FormContext formContext = new FormContext();
                htmlHelper.ViewContext.ClientValidationEnabled = true;
                htmlHelper.ViewContext.UnobtrusiveJavaScriptEnabled = true;
                htmlHelper.ViewContext.FormContext = formContext;

                ModelClientValidationRule[] expectedValidationRules = new ModelClientValidationRule[]
                {
                    new ModelClientValidationRule() { ValidationType = "ValidationRule1" },
                    new ModelClientValidationRule() { ValidationType = "ValidationRule2" }
                };

                Mock<ModelValidator> mockValidator = new Mock<ModelValidator>(ModelMetadata.FromStringExpression("", htmlHelper.ViewContext.ViewData), htmlHelper.ViewContext);
                mockValidator.Setup(v => v.GetClientValidationRules())
                    .Returns(expectedValidationRules);
                Mock<ModelValidatorProvider> mockValidatorProvider = new Mock<ModelValidatorProvider>();
                mockValidatorProvider.Setup(vp => vp.GetValidators(It.IsAny<ModelMetadata>(), It.IsAny<ControllerContext>()))
                    .Returns(new[] { mockValidator.Object });
                ModelValidatorProviders.Providers.Add(mockValidatorProvider.Object);

                // Act
                MvcHtmlString html = htmlHelper.ValidationMessageFor(m => m.baz);

                // Assert
                Assert.Equal("<span class=\"field-validation-valid\" data-valmsg-for=\"baz\" data-valmsg-replace=\"true\"></span>", html.ToHtmlString());
            }
            finally
            {
                ModelValidatorProviders.Providers.Clear();
                foreach (var provider in originalProviders)
                {
                    ModelValidatorProviders.Providers.Add(provider);
                }
            }
        }

        [Fact]
        public void ValidationMessageForWithModelStateAndNoErrors()
        {
            // Arrange
            HtmlHelper<ValidationModel> htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());

            // Act
            MvcHtmlString html = htmlHelper.ValidationMessageFor(m => m.baz);

            // Assert
            Assert.Null(html);
        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void ValidationMessageFor_AttributeEncodes_AddedHtmlAttributes(
            string text,
            bool htmlEncode,
            string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;

            var helper = MvcHelper.GetHtmlHelper(viewData);
            helper.EnableClientValidation();
            helper.EnableUnobtrusiveJavaScript();
            string name = null;

            // Act
            var result =
                helper.ValidationMessageFor(m => name, validationMessage: null, htmlAttributes: new { attribute = text, })
                .ToHtmlString();

            // Assert
            Assert.Equal(
                "<span attribute=\"" +
                    encodedText +
                    "\" class=\"field-validation-valid\" data-valmsg-for=\"name\" data-valmsg-replace=\"true\">" +
                    "</span>",
                result);
        }

        [Theory]
        [PropertyData("HtmlEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void ValidationMessageFor_HtmlEncodes_Message(
            string text,
            bool htmlEncode,
            string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;

            var helper = MvcHelper.GetHtmlHelper(viewData);
            helper.EnableClientValidation();
            helper.EnableUnobtrusiveJavaScript();
            string name = null;

            // Act
            var result = helper.ValidationMessageFor(m => name, validationMessage: text).ToHtmlString();

            // Assert
            Assert.Equal(
                "<span class=\"field-validation-valid\" data-valmsg-for=\"name\" data-valmsg-replace=\"false\">" +
                    encodedText +
                    "</span>",
                result);
        }

        [Theory]
        [PropertyData("HtmlEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void ValidationMessageFor_HtmlEncodes_ModelStateAttemptedValue(
            string text,
            bool htmlEncode,
            string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            viewData.ModelState.AddModelError(key: "name", errorMessage: null);
            var valueProvider = new ValueProviderResult(rawValue: null, attemptedValue: text, culture: null);
            viewData.ModelState.SetModelValue(key: "name", value: valueProvider);

            var helper = MvcHelper.GetHtmlHelper(viewData);
            helper.EnableClientValidation();
            helper.EnableUnobtrusiveJavaScript();
            string name = null;

            // Act
            var result = helper.ValidationMessageFor(m => name).ToHtmlString();

            // Assert
            Assert.Equal(
                "<span class=\"field-validation-error\" data-valmsg-for=\"name\" data-valmsg-replace=\"true\">The value &#39;" +
                    encodedText +
                    "&#39; is invalid.</span>",
                result);
        }

        [Theory]
        [PropertyData("HtmlEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void ValidationMessageFor_HtmlEncodes_ModelStateError(
            string text,
            bool htmlEncode,
            string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            viewData.ModelState.AddModelError("name", text);

            var helper = MvcHelper.GetHtmlHelper(viewData);
            helper.EnableClientValidation();
            helper.EnableUnobtrusiveJavaScript();
            string name = null;

            // Act
            var result = helper.ValidationMessageFor(m => name).ToHtmlString();

            // Assert
            Assert.Equal(
                "<span class=\"field-validation-error\" data-valmsg-for=\"name\" data-valmsg-replace=\"true\">" +
                    encodedText +
                    "</span>",
                result);
        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void ValidationMessageFor_AttributeEncodes_Prefix(string text, bool htmlEncode, string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            viewData.TemplateInfo.HtmlFieldPrefix = text;

            var helper = MvcHelper.GetHtmlHelper(viewData);
            helper.EnableClientValidation();
            helper.EnableUnobtrusiveJavaScript();
            string name = null;

            // Act
            var result = helper.ValidationMessageFor(m => name).ToHtmlString();

            // Assert
            Assert.Equal(
                "<span class=\"field-validation-valid\" data-valmsg-for=\"" +
                    encodedText +
                    ".name\" data-valmsg-replace=\"true\"></span>",
                result);
        }

        [Theory]
        [PropertyData("AttributeEncodedData_NoEncodedText", PropertyType = typeof(EncodedDataSets))]
        public void ValidationMessageFor_DoesNotEncode_Tag(string text, bool htmlEncode)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;

            var helper = MvcHelper.GetHtmlHelper(viewData);
            helper.EnableClientValidation();
            helper.EnableUnobtrusiveJavaScript();
            string name = null;

            // Act
            var result = helper.ValidationMessageFor(m => name, validationMessage: null, tag: text).ToHtmlString();

            // Assert
            Assert.Equal(
                "<" +
                    text +
                    " class=\"field-validation-valid\" data-valmsg-for=\"name\" data-valmsg-replace=\"true\"></" +
                    text +
                    ">",
                result);
        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void ValidationMessageForNonUnobtrusive_AttributeEncodes_AddedHtmlAttributes(
            string text,
            bool htmlEncode,
            string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;

            var helper = MvcHelper.GetHtmlHelper(viewData);
            helper.EnableClientValidation();
            helper.EnableUnobtrusiveJavaScript(enabled: false);
            string name = null;

            // Act
            var result = helper.ValidationMessageFor(
                    m => name,
                    validationMessage: null,
                    htmlAttributes: new { attribute = text, })
                .ToHtmlString();

            // Assert
            Assert.Equal(
                "<span attribute=\"" +
                    encodedText +
                    "\" class=\"field-validation-valid\" id=\"name_validationMessage\">" +
                    "</span>",
                result);
        }

        [Theory]
        [PropertyData("HtmlEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void ValidationMessageForNonUnobtrusive_HtmlEncodes_Message(
            string text,
            bool htmlEncode,
            string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;

            var helper = MvcHelper.GetHtmlHelper(viewData);
            helper.EnableClientValidation();
            helper.EnableUnobtrusiveJavaScript(enabled: false);
            string name = null;

            // Act
            var result = helper.ValidationMessageFor(m => name, validationMessage: text).ToHtmlString();

            // Assert
            Assert.Equal(
                "<span class=\"field-validation-valid\" id=\"name_validationMessage\">" + encodedText + "</span>",
                result);
        }

        [Theory]
        [PropertyData("HtmlEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void ValidationMessageForNonUnobtrusive_HtmlEncodes_ModelStateAttemptedValue(
            string text,
            bool htmlEncode,
            string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            viewData.ModelState.AddModelError(key: "name", errorMessage: null);
            var valueProvider = new ValueProviderResult(rawValue: null, attemptedValue: text, culture: null);
            viewData.ModelState.SetModelValue(key: "name", value: valueProvider);

            var helper = MvcHelper.GetHtmlHelper(viewData);
            helper.EnableClientValidation();
            helper.EnableUnobtrusiveJavaScript(enabled: false);
            string name = null;

            // Act
            var result = helper.ValidationMessageFor(m => name).ToHtmlString();

            // Assert
            Assert.Equal(
                "<span class=\"field-validation-error\" id=\"name_validationMessage\">The value &#39;" +
                    encodedText +
                    "&#39; is invalid.</span>",
                result);
        }

        [Theory]
        [PropertyData("HtmlEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void ValidationMessageForNonUnobtrusive_HtmlEncodes_ModelStateError(
            string text,
            bool htmlEncode,
            string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            viewData.ModelState.AddModelError("name", text);

            var helper = MvcHelper.GetHtmlHelper(viewData);
            helper.EnableClientValidation();
            helper.EnableUnobtrusiveJavaScript(enabled: false);
            string name = null;

            // Act
            var result = helper.ValidationMessageFor(m => name).ToHtmlString();

            // Assert
            Assert.Equal(
                "<span class=\"field-validation-error\" id=\"name_validationMessage\">" + encodedText + "</span>",
                result);
        }

        [Theory]
        [PropertyData("IdEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void ValidationMessageForNonUnobtrusive_IdEncodes_Prefix(string text, bool htmlEncode, string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            viewData.TemplateInfo.HtmlFieldPrefix = text;

            var helper = MvcHelper.GetHtmlHelper(viewData);
            helper.EnableClientValidation();
            helper.EnableUnobtrusiveJavaScript(enabled: false);
            string name = null;

            // Act
            var result = helper.ValidationMessageFor(m => name).ToHtmlString();

            // Assert
            Assert.Equal(
                "<span class=\"field-validation-valid\" id=\"" + encodedText + "_name_validationMessage\"></span>",
                result);
        }

        [Theory]
        [PropertyData("AttributeEncodedData_NoEncodedText", PropertyType = typeof(EncodedDataSets))]
        public void ValidationMessageForNonUnobtrusive_DoesNotEncode_Tag(string text, bool htmlEncode)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;

            var helper = MvcHelper.GetHtmlHelper(viewData);
            helper.EnableClientValidation();
            helper.EnableUnobtrusiveJavaScript(enabled: false);
            string name = null;

            // Act
            var result = helper.ValidationMessageFor(m => name, validationMessage: null, tag: text).ToHtmlString();

            // Assert
            Assert.Equal(
                "<" + text + " class=\"field-validation-valid\" id=\"name_validationMessage\"></" + text + ">",
                result);
        }

        // ValidationSummary

        [Fact]
        public void ValidationSummary()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());

            // Act
            MvcHtmlString html = htmlHelper.ValidationSummary();

            // Assert
            Assert.Equal(
                "<div class=\"validation-summary-errors\"><ul><li>foo error &lt;1&gt;</li>" + Environment.NewLine
              + "<li>foo error 2</li>" + Environment.NewLine
              + "<li>bar error &lt;1&gt;</li>" + Environment.NewLine
              + "<li>bar error 2</li>" + Environment.NewLine
              + "</ul></div>",
                html.ToHtmlString());
        }

        [Fact]
        public void ValidationSummaryAddsIdIfClientValidationEnabled()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());
            htmlHelper.ViewContext.FormContext = new FormContext();
            htmlHelper.ViewContext.ClientValidationEnabled = true;

            // Act
            MvcHtmlString html = htmlHelper.ValidationSummary();

            // Assert
            Assert.Equal(
                "<div class=\"validation-summary-errors\" id=\"validationSummary\"><ul><li>foo error &lt;1&gt;</li>" + Environment.NewLine
              + "<li>foo error 2</li>" + Environment.NewLine
              + "<li>bar error &lt;1&gt;</li>" + Environment.NewLine
              + "<li>bar error 2</li>" + Environment.NewLine
              + "</ul></div>",
                html.ToHtmlString());
            Assert.Equal("validationSummary", htmlHelper.ViewContext.FormContext.ValidationSummaryId);
        }

        [Fact]
        public void ValidationSummaryDoesNotAddIdIfUnobtrusiveJavaScriptEnabled()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());
            htmlHelper.ViewContext.FormContext = new FormContext();
            htmlHelper.ViewContext.ClientValidationEnabled = true;
            htmlHelper.ViewContext.UnobtrusiveJavaScriptEnabled = true;

            // Act
            MvcHtmlString html = htmlHelper.ValidationSummary();

            // Assert
            Assert.Equal(
                "<div class=\"validation-summary-errors\" data-valmsg-summary=\"true\"><ul><li>foo error &lt;1&gt;</li>" + Environment.NewLine
              + "<li>foo error 2</li>" + Environment.NewLine
              + "<li>bar error &lt;1&gt;</li>" + Environment.NewLine
              + "<li>bar error 2</li>" + Environment.NewLine
              + "</ul></div>",
                html.ToHtmlString());
        }

        [Fact]
        public void ValidationSummaryWithDictionary()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());
            RouteValueDictionary htmlAttributes = new RouteValueDictionary();
            htmlAttributes["class"] = "my-class";

            // Act
            MvcHtmlString html = htmlHelper.ValidationSummary(null /* message */, htmlAttributes);

            // Assert
            Assert.Equal(
                "<div class=\"validation-summary-errors my-class\"><ul><li>foo error &lt;1&gt;</li>" + Environment.NewLine
              + "<li>foo error 2</li>" + Environment.NewLine
              + "<li>bar error &lt;1&gt;</li>" + Environment.NewLine
              + "<li>bar error 2</li>" + Environment.NewLine
              + "</ul></div>",
                html.ToHtmlString());
        }

        [Fact]
        public void ValidationSummaryWithDictionaryAndMessage()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());
            RouteValueDictionary htmlAttributes = new RouteValueDictionary();
            htmlAttributes["class"] = "my-class";

            // Act
            MvcHtmlString html = htmlHelper.ValidationSummary("This is my message.", htmlAttributes);

            // Assert
            Assert.Equal(
                "<div class=\"validation-summary-errors my-class\"><span>This is my message.</span>" + Environment.NewLine
              + "<ul><li>foo error &lt;1&gt;</li>" + Environment.NewLine
              + "<li>foo error 2</li>" + Environment.NewLine
              + "<li>bar error &lt;1&gt;</li>" + Environment.NewLine
              + "<li>bar error 2</li>" + Environment.NewLine
              + "</ul></div>",
                html.ToHtmlString());
        }

        [Fact]
        public void ValidationSummaryWithNoErrors_ReturnsNullIfClientValidationDisabled()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(new ViewDataDictionary());

            // Act
            MvcHtmlString html = htmlHelper.ValidationSummary();

            // Assert
            Assert.Null(html);
        }

        [Fact]
        public void ValidationSummaryWithNoErrors_EmptyUlIfClientValidationEnabled()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(new ViewDataDictionary());
            htmlHelper.ViewContext.ClientValidationEnabled = true;
            htmlHelper.ViewContext.FormContext = new FormContext();

            // Act
            MvcHtmlString html = htmlHelper.ValidationSummary();

            // Assert
            Assert.Equal(
                "<div class=\"validation-summary-valid\" id=\"validationSummary\"><ul><li style=\"display:none\"></li>" + Environment.NewLine
              + "</ul></div>",
                html.ToHtmlString());
        }

        [Fact]
        public void ValidationSummaryWithObjectAttributes()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());

            // Act
            MvcHtmlString html = htmlHelper.ValidationSummary(null /* message */, new { baz = "baz" });

            // Assert
            Assert.Equal(
                "<div baz=\"baz\" class=\"validation-summary-errors\"><ul><li>foo error &lt;1&gt;</li>" + Environment.NewLine
              + "<li>foo error 2</li>" + Environment.NewLine
              + "<li>bar error &lt;1&gt;</li>" + Environment.NewLine
              + "<li>bar error 2</li>" + Environment.NewLine
              + "</ul></div>",
                html.ToHtmlString());
        }

        [Fact]
        public void ValidationSummaryWithObjectAttributesWithUnderscores()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());

            // Act
            MvcHtmlString html = htmlHelper.ValidationSummary(null /* message */, new { foo_baz = "baz" });

            // Assert
            Assert.Equal(
                "<div class=\"validation-summary-errors\" foo-baz=\"baz\"><ul><li>foo error &lt;1&gt;</li>" + Environment.NewLine
              + "<li>foo error 2</li>" + Environment.NewLine
              + "<li>bar error &lt;1&gt;</li>" + Environment.NewLine
              + "<li>bar error 2</li>" + Environment.NewLine
              + "</ul></div>",
                html.ToHtmlString());
        }

        [Fact]
        public void ValidationSummaryWithObjectAttributesAndMessage()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());

            // Act
            MvcHtmlString html = htmlHelper.ValidationSummary("This is my message.", new { baz = "baz" });

            // Assert
            Assert.Equal(
                "<div baz=\"baz\" class=\"validation-summary-errors\"><span>This is my message.</span>" + Environment.NewLine
              + "<ul><li>foo error &lt;1&gt;</li>" + Environment.NewLine
              + "<li>foo error 2</li>" + Environment.NewLine
              + "<li>bar error &lt;1&gt;</li>" + Environment.NewLine
              + "<li>bar error 2</li>" + Environment.NewLine
              + "</ul></div>",
                html.ToHtmlString());
        }

        [Fact]
        public void ValidationSummaryWithNoModelErrors()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());

            // Act
            MvcHtmlString html = htmlHelper.ValidationSummary(true /* excludePropertyErrors */, "This is my message.");

            // Assert
            Assert.Equal(
                "<div class=\"validation-summary-errors\"><span>This is my message.</span>" + Environment.NewLine
              + "<ul><li style=\"display:none\"></li>" + Environment.NewLine
              + "</ul></div>",
                html.ToHtmlString());
        }

        [Fact]
        public void ValidationSummaryWithOnlyModelErrors()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelAndPropertyErrors());

            // Act
            MvcHtmlString html = htmlHelper.ValidationSummary(true /* excludePropertyErrors */, "This is my message.");

            // Assert
            Assert.Equal(
                "<div class=\"validation-summary-errors\"><span>This is my message.</span>" + Environment.NewLine
              + "<ul><li>Something is wrong.</li>" + Environment.NewLine
              + "<li>Something else is also wrong.</li>" + Environment.NewLine
              + "</ul></div>",
                html.ToHtmlString());
        }

        [Fact]
        public void ValidationSummaryWithOnlyModelErrorsAndPrefix()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors("MyPrefix"));

            // Act
            MvcHtmlString html = htmlHelper.ValidationSummary(true /* excludePropertyErrors */, "This is my message.");

            // Assert
            Assert.Equal(
                "<div class=\"validation-summary-errors\"><span>This is my message.</span>" + Environment.NewLine
              + "<ul><li style=\"display:none\"></li>" + Environment.NewLine
              + "</ul></div>",
                html.ToHtmlString());
        }

        [Fact]
        public void ValidationSummaryWithOverriddenHeadingTag_UsesGivenTag()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());
            htmlHelper.SetValidationSummaryMessageElement("h4");

            // Act
            MvcHtmlString html = htmlHelper.ValidationSummary(true /* excludePropertyErrors */, "This is my message.");

            // Assert
            Assert.Equal(
                "<div class=\"validation-summary-errors\"><h4>This is my message.</h4>" + Environment.NewLine
              + "<ul><li style=\"display:none\"></li>" + Environment.NewLine
              + "</ul></div>",
                html.ToHtmlString());
        }

        [Fact]
        public void ValidationSummaryWithCustomHeadingTag()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());

            // Act
            MvcHtmlString html = htmlHelper.ValidationSummary(true /* excludePropertyErrors */, "This is my message.", "h2");

            // Assert
            Assert.Equal(
                "<div class=\"validation-summary-errors\"><h2>This is my message.</h2>" + Environment.NewLine
              + "<ul><li style=\"display:none\"></li>" + Environment.NewLine
              + "</ul></div>",
                html.ToHtmlString());
        }

        [Fact]
        public void ValidationSummaryWithDictionaryAndMessageWithCustomHeadingTag()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());
            RouteValueDictionary htmlAttributes = new RouteValueDictionary();
            htmlAttributes["class"] = "my-class";

            // Act
            MvcHtmlString html = htmlHelper.ValidationSummary("This is my message.", htmlAttributes, "h2");

            // Assert
            Assert.Equal(
                "<div class=\"validation-summary-errors my-class\"><h2>This is my message.</h2>" + Environment.NewLine
              + "<ul><li>foo error &lt;1&gt;</li>" + Environment.NewLine
              + "<li>foo error 2</li>" + Environment.NewLine
              + "<li>bar error &lt;1&gt;</li>" + Environment.NewLine
              + "<li>bar error 2</li>" + Environment.NewLine
              + "</ul></div>",
                html.ToHtmlString());
        }

        [Fact]
        public void ValidationSummaryWithObjectAttributesAndMessageWithCustomHeadingTag()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors());

            // Act
            MvcHtmlString html = htmlHelper.ValidationSummary("This is my message.", new { baz = "baz" }, "h2");

            // Assert
            Assert.Equal(
                "<div baz=\"baz\" class=\"validation-summary-errors\"><h2>This is my message.</h2>" + Environment.NewLine
              + "<ul><li>foo error &lt;1&gt;</li>" + Environment.NewLine
              + "<li>foo error 2</li>" + Environment.NewLine
              + "<li>bar error &lt;1&gt;</li>" + Environment.NewLine
              + "<li>bar error 2</li>" + Environment.NewLine
              + "</ul></div>",
                html.ToHtmlString());
        }

        [Fact]
        public void ValidationMessageWithPrefix()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelErrors("MyPrefix"));

            // Act
            MvcHtmlString html = htmlHelper.ValidationMessage("foo");

            // Assert
            Assert.Equal("<span class=\"field-validation-error\">foo error &lt;1&gt;</span>", html.ToHtmlString());
        }

        [Fact]
        public void ValidationErrorOrdering()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(GetViewDataWithModelWithDisplayOrderErrors());

            // Act
            MvcHtmlString html = htmlHelper.ValidationSummary();

            // Assert
            Assert.Equal(
                "<div class=\"validation-summary-errors\"><ul><li>Error 1</li>" + Environment.NewLine
              + "<li>Error 2</li>" + Environment.NewLine
              + "<li>Error 3</li>" + Environment.NewLine
              + "<li>Error 4</li>" + Environment.NewLine
              + "</ul></div>",
                html.ToHtmlString());

        }

        [Theory]
        [PropertyData("AttributeEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void ValidationSummary_AttributeEncodes_AddedHtmlAttributes(
            string text,
            bool htmlEncode,
            string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;

            var helper = MvcHelper.GetHtmlHelper(viewData);
            helper.EnableClientValidation();
            helper.EnableUnobtrusiveJavaScript(enabled: false);

            // Act
            var result = helper.ValidationSummary(
                    excludePropertyErrors: true,
                    message: null,
                    htmlAttributes: new { attribute = text, })
                .ToHtmlString();

            // Assert
            Assert.Equal(
                "<div attribute=\"" +
                    encodedText +
                    "\" class=\"validation-summary-valid\" id=\"validationSummary\"><ul><li style=\"display:none\"></li>" +
                    Environment.NewLine +
                    "</ul></div>",
                result);
        }

        [Theory]
        [PropertyData("HtmlEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void ValidationSummary_HtmlEncodes_Message(
            string text,
            bool htmlEncode,
            string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;

            var helper = MvcHelper.GetHtmlHelper(viewData);
            helper.EnableClientValidation();
            helper.EnableUnobtrusiveJavaScript(enabled: false);

            // Act
            var result = helper.ValidationSummary(message: text).ToHtmlString();

            // Assert
            Assert.Equal(
                "<div class=\"validation-summary-valid\" id=\"validationSummary\"><span>" +
                    encodedText +
                    "</span>" +
                    Environment.NewLine +
                    "<ul><li style=\"display:none\"></li>" +
                    Environment.NewLine +
                    "</ul></div>",
                result);
        }

        [Theory]
        [PropertyData("HtmlEncodedData", PropertyType = typeof(EncodedDataSets))]
        public void ValidationSummary_HtmlEncodes_ModelStateError(
            string text,
            bool htmlEncode,
            string encodedText)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;
            viewData.ModelState.AddModelError("", text);

            var helper = MvcHelper.GetHtmlHelper(viewData);
            helper.EnableClientValidation();
            helper.EnableUnobtrusiveJavaScript(enabled: false);

            // Act
            var result = helper.ValidationSummary().ToHtmlString();

            // Assert
            Assert.Equal(
                "<div class=\"validation-summary-errors\" id=\"validationSummary\"><ul><li>" +
                    encodedText +
                    "</li>" +
                    Environment.NewLine +
                    "</ul></div>",
                result);
        }

        [Theory]
        [PropertyData("AttributeEncodedData_NoEncodedText", PropertyType = typeof(EncodedDataSets))]
        public void ValidationSummary_DoesNotEncode_Tag(string text, bool htmlEncode)
        {
            // Arrange
            var viewData = new ViewDataDictionary<string>(model: null);
            viewData.ModelMetadata.HtmlEncode = htmlEncode;

            var helper = MvcHelper.GetHtmlHelper(viewData);
            helper.EnableClientValidation();
            helper.EnableUnobtrusiveJavaScript(enabled: false);

            // Act
            var result = helper.ValidationSummary(message: "message", headingTag: text).ToHtmlString();

            // Assert
            Assert.Equal(
                "<div class=\"validation-summary-valid\" id=\"validationSummary\"><" +
                    text +
                    ">message</" +
                    text +
                    ">" +
                    Environment.NewLine +
                    "<ul><li style=\"display:none\"></li>" +
                    Environment.NewLine +
                    "</ul></div>",
                result);
        }

        private class ValidationModel
        {
            public string foo { get; set; }
            public string bar { get; set; }
            public string baz { get; set; }
            public string quux { get; set; }
        }

        public class ModelWithOrdering
        {
            [Required]
            [Display(Order = 2)]
            public int Second { get; set; }

            [Required]
            [Display(Order = 1)]
            public string First { get; set; }

            [Required]
            [Display(Order = 4)]
            public string Fourth { get; set; }

            [Required]
            [Display(Order = 3)]
            public string Third { get; set; }
        }

        private static ViewDataDictionary<ValidationModel> GetViewDataWithNullModelState()
        {
            ViewDataDictionary<ValidationModel> viewData = new ViewDataDictionary<ValidationModel>();
            viewData.ModelState["foo"] = null;
            return viewData;
        }

        private static ViewDataDictionary<ValidationModel> GetViewDataWithModelErrors()
        {
            ViewDataDictionary<ValidationModel> viewData = new ViewDataDictionary<ValidationModel>();
            ModelState modelStateFoo = new ModelState();
            ModelState modelStateBar = new ModelState();
            ModelState modelStateBaz = new ModelState();

            modelStateFoo.Errors.Add(new ModelError(new InvalidOperationException("foo error from exception")));
            modelStateFoo.Errors.Add(new ModelError("foo error <1>"));
            modelStateFoo.Errors.Add(new ModelError("foo error 2"));
            modelStateBar.Errors.Add(new ModelError("bar error <1>"));
            modelStateBar.Errors.Add(new ModelError("bar error 2"));

            viewData.ModelState["foo"] = modelStateFoo;
            viewData.ModelState["bar"] = modelStateBar;
            viewData.ModelState["baz"] = modelStateBaz;

            viewData.ModelState.SetModelValue("quux", new ValueProviderResult(null, "quuxValue", null));
            viewData.ModelState.AddModelError("quux", new InvalidOperationException("Some error text."));
            return viewData;
        }

        private static ViewDataDictionary<ValidationModel> GetViewDataWithModelAndPropertyErrors()
        {
            ViewDataDictionary<ValidationModel> viewData = new ViewDataDictionary<ValidationModel>();
            ModelState modelStateFoo = new ModelState();
            ModelState modelStateBar = new ModelState();
            ModelState modelStateBaz = new ModelState();
            modelStateFoo.Errors.Add(new ModelError("foo error <1>"));
            modelStateFoo.Errors.Add(new ModelError("foo error 2"));
            modelStateBar.Errors.Add(new ModelError("bar error <1>"));
            modelStateBar.Errors.Add(new ModelError("bar error 2"));
            viewData.ModelState["foo"] = modelStateFoo;
            viewData.ModelState["bar"] = modelStateBar;
            viewData.ModelState["baz"] = modelStateBaz;
            viewData.ModelState.SetModelValue("quux", new ValueProviderResult(null, "quuxValue", null));
            viewData.ModelState.AddModelError("quux", new InvalidOperationException("Some error text."));
            viewData.ModelState.AddModelError(String.Empty, "Something is wrong.");
            viewData.ModelState.AddModelError(String.Empty, "Something else is also wrong.");
            return viewData;
        }

        private static ViewDataDictionary<ValidationModel> GetViewDataWithModelErrors(string prefix)
        {
            ViewDataDictionary<ValidationModel> viewData = new ViewDataDictionary<ValidationModel>();
            viewData.TemplateInfo.HtmlFieldPrefix = prefix;
            ModelState modelStateFoo = new ModelState();
            ModelState modelStateBar = new ModelState();
            ModelState modelStateBaz = new ModelState();
            modelStateFoo.Errors.Add(new ModelError("foo error <1>"));
            modelStateFoo.Errors.Add(new ModelError("foo error 2"));
            modelStateBar.Errors.Add(new ModelError("bar error <1>"));
            modelStateBar.Errors.Add(new ModelError("bar error 2"));
            viewData.ModelState[prefix + ".foo"] = modelStateFoo;
            viewData.ModelState[prefix + ".bar"] = modelStateBar;
            viewData.ModelState[prefix + ".baz"] = modelStateBaz;
            viewData.ModelState.SetModelValue(prefix + ".quux", new ValueProviderResult(null, "quuxValue", null));
            viewData.ModelState.AddModelError(prefix + ".quux", new InvalidOperationException("Some error text."));
            return viewData;
        }

        private static ViewDataDictionary<ModelWithOrdering> GetViewDataWithModelWithDisplayOrderErrors()
        {
            ViewDataDictionary<ModelWithOrdering> viewData = new ViewDataDictionary<ModelWithOrdering>();

            var model = new ModelWithOrdering();

            // Error names for each property on ModelWithOrdering.
            viewData.ModelState.AddModelError("First", "Error 1");
            viewData.ModelState.AddModelError("Second", "Error 2");
            viewData.ModelState.AddModelError("Third", "Error 3");
            viewData.ModelState.AddModelError("Fourth", "Error 4");

            return viewData;
        }

        public void Dispose()
        {
            // Reset ScopeStorage (written via e.g. ViewContext.ClientValidationEnabled) between tests to avoid unexpected interactions.
            ScopeStorage.CurrentProvider = new StaticScopeStorageProvider();
            ScopeStorage.GlobalScope.Clear();
        }
    }
}
