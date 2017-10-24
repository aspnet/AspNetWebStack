// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using System.Web.WebPages.Html;
using System.Web.WebPages.Scope;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.WebPages.Validation.Test
{
    [Xunit.Collection("Uses ScopeStorage or ViewEngines.Engines")]
    public class ValidationHelperTest : IDisposable
    {
        [Fact]
        public void FormFieldKeyIsCommonToModelStateAndValidationHelper()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            string key = "_FORM";
            ValidationHelper validationHelper = GetValidationHelper(GetContext());

            // Act and Assert
            Assert.Equal(key, ModelStateDictionary.FormFieldKey);
            Assert.Equal(key, validationHelper.FormField);
        }

        [Fact]
        public void AddThrowsIfFieldIsEmpty()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            ValidationHelper validationHelper = GetValidationHelper(GetContext());

            // Act and Assert
            Assert.ThrowsArgumentNullOrEmptyString(() => validationHelper.Add(field: null), "field");
            Assert.ThrowsArgumentNullOrEmptyString(() => validationHelper.Add(field: String.Empty), "field");
        }

        [Fact]
        public void AddThrowsIfValidatorsParamsArrayIsNull()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            ValidationHelper validationHelper = GetValidationHelper(GetContext());

            // Act and Assert
            Assert.ThrowsArgumentNull(() => validationHelper.Add("foo", null), "validators");
        }

        [Fact]
        public void AddThrowsIfValidatorsAreNull()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            ValidationHelper validationHelper = GetValidationHelper(GetContext());

            // Act and Assert
            Assert.ThrowsArgumentNull(() => validationHelper.Add("foo", Validator.Required(), null, Validator.Range(0, 10)), "validators");
        }

        [Fact]
        public void RequiredReturnsErrorMessageIfFieldIsNotPresentInForm()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            string message = "Foo is required.";
            ValidationHelper validationHelper = GetValidationHelper(GetContext());

            // Act
            validationHelper.RequireField("foo", message);
            var results = validationHelper.Validate();

            // Assert
            ValidationResult result = Assert.Single(results);
            Assert.Equal(message, result.ErrorMessage);
        }

        [Fact]
        public void RequiredReturnsErrorMessageIfFieldIsEmpty()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            string message = "Foo is required.";
            ValidationHelper validationHelper = GetValidationHelper(GetContext(new { foo = "" }));

            // Act
            validationHelper.RequireField("foo", message);
            var results = validationHelper.Validate();

            // Assert
            ValidationResult result = Assert.Single(results);
            Assert.Equal(message, result.ErrorMessage);
        }

        [Fact]
        public void RequiredReturnsNoValidationResultsIfFieldIsPresent()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            string message = "Foo is required.";
            ValidationHelper validationHelper = GetValidationHelper(GetContext(new { foo = "some value" }));

            // Act
            validationHelper.RequireField("foo", message);
            var results = validationHelper.Validate();

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public void RequiredUsesDefaultErrorMessageIfNoValueIsProvided()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            ValidationHelper validationHelper = GetValidationHelper(GetContext());

            // Act
            validationHelper.RequireField("foo");
            var results = validationHelper.Validate();

            // Assert
            ValidationResult result = Assert.Single(results);
            Assert.Equal("This field is required.", result.ErrorMessage);
        }

        [Fact]
        public void RequiredReturnsValidationResultForEachFieldThatFailed()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            string message = "This field is required.";
            ValidationHelper validationHelper = GetValidationHelper(GetContext());

            // Act
            validationHelper.RequireFields("foo", "bar");
            var results = validationHelper.Validate();

            // Assert
            Assert.Equal(2, results.Count());
            Assert.Equal(message, results.First().ErrorMessage);
            Assert.Equal("foo", results.First().MemberNames.Single());

            Assert.Equal(message, results.Last().ErrorMessage);
            Assert.Equal("bar", results.Last().MemberNames.Single());
        }

        [Fact]
        public void RequiredReturnsValidationResultForEachFieldThatFailedWhenFieldsIsEmpty()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            string message = "This field is required.";
            ValidationHelper validationHelper = GetValidationHelper(GetContext());

            // Act
            validationHelper.RequireFields("foo", "bar");
            var results = validationHelper.Validate(fields: null);

            // Assert
            Assert.Equal(2, results.Count());
            Assert.Equal(message, results.First().ErrorMessage);
            Assert.Equal("foo", results.First().MemberNames.Single());

            Assert.Equal(message, results.Last().ErrorMessage);
            Assert.Equal("bar", results.Last().MemberNames.Single());
        }

        [Fact]
        public void GetValidationHtmlThrowsIfArgumentIsNullOrEmpty()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            ValidationHelper validationHelper = GetValidationHelper(GetContext());

            // Act and Assert
            Assert.ThrowsArgumentNullOrEmptyString(() => validationHelper.For(field: null), "field");
            Assert.ThrowsArgumentNullOrEmptyString(() => validationHelper.For(field: String.Empty), "field");
        }

        [Fact]
        public void RequireFieldThrowsIfValueIsNullOrEmpty()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            ValidationHelper validationHelper = GetValidationHelper(GetContext());

            // Act and Assert
            Assert.ThrowsArgumentNullOrEmptyString(() => validationHelper.RequireField(field: null), "field");
            Assert.ThrowsArgumentNullOrEmptyString(() => validationHelper.RequireField(field: String.Empty), "field");
            Assert.ThrowsArgumentNullOrEmptyString(() => validationHelper.RequireField(field: null, errorMessage: "baz"), "field");
            Assert.ThrowsArgumentNullOrEmptyString(() => validationHelper.RequireField(field: String.Empty, errorMessage: null), "field");
        }

        [Fact]
        public void RequireFieldsThrowsIfFieldsAreNullOrHasEmptyValues()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            ValidationHelper validationHelper = GetValidationHelper(GetContext());

            // Act and Assert
            Assert.ThrowsArgumentNull(() => validationHelper.RequireFields(fields: null), "fields");
            Assert.ThrowsArgumentNullOrEmptyString(() => validationHelper.RequireFields(fields: new[] { "foo", null }), "field");
            Assert.ThrowsArgumentNullOrEmptyString(() => validationHelper.RequireFields(fields: new[] { "foo", "" }), "field");
        }

        [Fact]
        public void AddThrowsIfFieldIsNullOrEmpty()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            ValidationHelper validationHelper = GetValidationHelper(GetContext());

            // Act and Assert
            Assert.ThrowsArgumentNullOrEmptyString(() => validationHelper.Add(field: null), "field");
            Assert.ThrowsArgumentNullOrEmptyString(() => validationHelper.Add(field: String.Empty), "field");
        }

        [Fact]
        public void AddThrowsIfValidatorsIsNullOrAnyValidatorIsNull()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            ValidationHelper validationHelper = GetValidationHelper(GetContext());

            // Act and Assert
            Assert.ThrowsArgumentNull(() => validationHelper.Add(field: "foo", validators: null), "validators");
            Assert.ThrowsArgumentNull(() => validationHelper.Add(field: "foo", validators: new[] { Validator.DateTime(), null }), "validators");
        }

        [Fact]
        public void AddFormErrorCallsMethodInUnderlyingModelStateDictionary()
        {
            // Arrange
            var message = "This is a form error.";
            var dictionary = new ModelStateDictionary();
            ValidationHelper validationHelper = GetValidationHelper(GetContext(), dictionary);

            // Act
            validationHelper.AddFormError(message);

            // Assert
            Assert.Equal(message, dictionary["_FORM"].Errors.Single());
        }

        [Fact]
        public void GetValidationHtmlForRequired()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            string message = "Foo is required.";
            ValidationHelper validationHelper = GetValidationHelper(GetContext());

            // Act
            validationHelper.RequireField("foo", message);
            var validationHtml = validationHelper.For("foo");

            // Assert
            Assert.Equal(@"data-val-required=""Foo is required."" data-val=""true""", validationHtml.ToString());
        }

        [Fact]
        public void ValidateReturnsAnEmptySequenceIfNoValidationsAreRegistered()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            ValidationHelper validationHelper = GetValidationHelper(GetContext());

            // Act
            var results = validationHelper.Validate();

            // Assert
            Assert.False(results.Any());
        }

        [Fact]
        public void ValidatePopulatesModelStateDictionary()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var modelStateDictionary = new ModelStateDictionary();
            ValidationHelper validationHelper = GetValidationHelper(GetContext(), modelStateDictionary);

            // Act
            validationHelper.RequireFields(new[] { "foo", "bar" });
            validationHelper.Validate();

            // Assert
            Assert.False(modelStateDictionary.IsValid);
            Assert.False(modelStateDictionary.IsValidField("foo"));
            Assert.False(modelStateDictionary.IsValidField("bar"));
            Assert.Equal("This field is required.", modelStateDictionary["foo"].Errors.Single());
            Assert.Equal("This field is required.", modelStateDictionary["bar"].Errors.Single());
        }

        [Fact]
        public void IsValidPopulatesModelStateDictionary()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var modelStateDictionary = new ModelStateDictionary();
            ValidationHelper validationHelper = GetValidationHelper(GetContext(), modelStateDictionary);

            // Act
            validationHelper.RequireFields("foo", "bar");
            validationHelper.IsValid();

            // Assert
            Assert.False(modelStateDictionary.IsValid);
            Assert.False(modelStateDictionary.IsValidField("foo"));
            Assert.False(modelStateDictionary.IsValidField("bar"));
            Assert.Equal("This field is required.", modelStateDictionary["foo"].Errors.Single());
            Assert.Equal("This field is required.", modelStateDictionary["bar"].Errors.Single());
        }

        [Fact]
        public void GetErrorsPopulatesModelStateDictionary()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var modelStateDictionary = new ModelStateDictionary();
            ValidationHelper validationHelper = GetValidationHelper(GetContext(), modelStateDictionary);

            // Act
            validationHelper.RequireFields("foo", "bar");
            validationHelper.GetErrors();

            // Assert
            Assert.False(modelStateDictionary.IsValid);
            Assert.False(modelStateDictionary.IsValidField("foo"));
            Assert.False(modelStateDictionary.IsValidField("bar"));
            Assert.Equal("This field is required.", modelStateDictionary["foo"].Errors.Single());
            Assert.Equal("This field is required.", modelStateDictionary["bar"].Errors.Single());
        }

        [Fact]
        public void GetErrorsReturnsAnEmptySequenceIfNoValidationsAreRegistered()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            ValidationHelper validationHelper = GetValidationHelper(GetContext());

            // Act
            var results = validationHelper.GetErrors();

            // Assert
            Assert.False(results.Any());
        }

        [Fact]
        public void GetErrorsReturnsErrorsAddedViaAddError()
        {
            // Arrange
            var modelStateDictionary = new ModelStateDictionary();
            ValidationHelper validationHelper = GetValidationHelper(GetContext(), modelStateDictionary);

            // Act
            modelStateDictionary.AddError("foo", "Foo error");
            var errors = validationHelper.GetErrors("foo");

            // Assert
            Assert.Equal(new[] { "Foo error" }, errors);
        }

        [Fact]
        public void GetErrorsReturnsFormErrors()
        {
            // Arrange
            string error = "Unable to connect to remote servers.";
            var modelStateDictionary = new ModelStateDictionary();
            ValidationHelper validationHelper = GetValidationHelper(GetContext(), modelStateDictionary);

            // Act
            validationHelper.AddFormError(error);
            var errors = validationHelper.GetErrors();

            // Assert
            Assert.Equal(error, errors.Single());
        }

        [Fact]
        public void InvokingValidateMultipleTimesDoesNotCauseErrorMessagesToBeDuplicated()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var modelStateDictionary = new ModelStateDictionary();
            ValidationHelper validationHelper = GetValidationHelper(GetContext(), modelStateDictionary);

            // Act
            validationHelper.RequireField("foo", "Foo is required.");
            validationHelper.RequireField("bar", "Bar is required.");
            validationHelper.Validate();
            Assert.False(validationHelper.IsValid());
            validationHelper.Validate();
            validationHelper.Validate();

            // Assert
            Assert.False(modelStateDictionary.IsValid);
            Assert.False(modelStateDictionary.IsValidField("foo"));
            Assert.False(modelStateDictionary.IsValidField("bar"));
            Assert.Equal("Foo is required.", modelStateDictionary["foo"].Errors.Single());
            Assert.Equal("Bar is required.", modelStateDictionary["bar"].Errors.Single());
        }

        [Fact]
        public void AddWorksForCustomValidator()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            string message = "Foo is not an odd number.";
            var oddValidator = new Mock<IValidator>();
            oddValidator.Setup(c => c.Validate(It.IsAny<ValidationContext>())).Returns<ValidationContext>(v =>
            {
                var context = Assert.IsAssignableFrom<HttpContextBase>(v.ObjectInstance);
                var value = Int32.Parse(context.Request.Form["foo"]);

                if (value % 2 != 0)
                {
                    return ValidationResult.Success;
                }
                return new ValidationResult(message);
            }).Verifiable();
            ValidationHelper validationHelper = GetValidationHelper(GetContext(new { foo = "6" }));

            // Act
            validationHelper.Add("foo", oddValidator.Object);
            var results = validationHelper.Validate();

            // Assert
            ValidationResult result = Assert.Single(results);
            Assert.Equal(message, result.ErrorMessage);
            oddValidator.Verify();
        }

        [Fact]
        public void ValidateRunsForSpecifiedFields()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            string message = "Foo is not an odd number.";
            var oddValidator = new Mock<IValidator>();
            oddValidator.Setup(c => c.Validate(It.IsAny<ValidationContext>())).Returns<ValidationContext>(v =>
            {
                var context = Assert.IsAssignableFrom<HttpContextBase>(v.ObjectInstance);
                if (context.Request.Form["foo"].IsEmpty())
                {
                    return ValidationResult.Success;
                }
                int value = context.Request.Form["foo"].AsInt();
                if (value % 2 != 0)
                {
                    return ValidationResult.Success;
                }
                return new ValidationResult(message);
            }).Verifiable();
            ValidationHelper validationHelper = GetValidationHelper(GetContext(new { foo = "", bar = "" }));

            // Act
            validationHelper.Add(new[] { "foo", "bar" }, oddValidator.Object);
            validationHelper.RequireField("foo");
            var results = validationHelper.Validate("foo");

            // Assert
            ValidationResult result = Assert.Single(results);
            Assert.Equal("This field is required.", result.ErrorMessage);
        }

        [Fact]
        public void GetErrorsReturnsAllErrorsIfNoParametersAreSpecified()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            string message = "Foo is not an odd number.";
            var oddValidator = new Mock<IValidator>();
            oddValidator.Setup(c => c.Validate(It.IsAny<ValidationContext>())).Returns<ValidationContext>(v =>
            {
                var context = Assert.IsAssignableFrom<HttpContextBase>(v.ObjectInstance);
                if (context.Request.Form["foo"].IsEmpty())
                {
                    return ValidationResult.Success;
                }
                int value = context.Request.Form["foo"].AsInt();
                if (value % 2 != 0)
                {
                    return ValidationResult.Success;
                }
                return new ValidationResult(message);
            }).Verifiable();
            ValidationHelper validationHelper = GetValidationHelper(GetContext(new { foo = "4", bar = "" }));

            // Act
            validationHelper.Add("foo", oddValidator.Object);
            validationHelper.RequireFields(new[] { "bar", "foo" });
            var result = validationHelper.GetErrors();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Equal("Foo is not an odd number.", result.First());
            Assert.Equal("This field is required.", result.Last());
        }

        [Fact]
        public void IsValidReturnsTrueIfAllValuesPassValidation()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            string message = "Foo is not an odd number.";
            var oddValidator = new Mock<IValidator>();
            oddValidator.Setup(c => c.Validate(It.IsAny<ValidationContext>())).Returns<ValidationContext>(v =>
            {
                var context = Assert.IsAssignableFrom<HttpContextBase>(v.ObjectInstance);
                if (context.Request.Form["foo"].IsEmpty())
                {
                    return ValidationResult.Success;
                }
                int value = context.Request.Form["foo"].AsInt();
                if (value % 2 != 0)
                {
                    return ValidationResult.Success;
                }
                return new ValidationResult(message);
            }).Verifiable();
            ValidationHelper validationHelper = GetValidationHelper(GetContext(new { foo = "5", bar = "2" }));

            // Act
            validationHelper.Add(new[] { "foo", "bar" }, oddValidator.Object);
            validationHelper.RequireField("foo");
            var result = validationHelper.IsValid();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValidValidatesSpecifiedFields()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            string message = "Foo is not an odd number.";
            var oddValidator = new Mock<IValidator>();
            oddValidator.Setup(c => c.Validate(It.IsAny<ValidationContext>())).Returns<ValidationContext>(v =>
            {
                var context = Assert.IsAssignableFrom<HttpContextBase>(v.ObjectInstance);
                int value;
                if (!Int32.TryParse(context.Request.Form["foo"], out value))
                {
                    return ValidationResult.Success;
                }
                if (value % 2 != 0)
                {
                    return ValidationResult.Success;
                }
                return new ValidationResult(message);
            }).Verifiable();
            ValidationHelper validationHelper = GetValidationHelper(GetContext(new { foo = "3", bar = "" }));

            // Act
            validationHelper.Add(new[] { "foo", "bar" }, oddValidator.Object);
            validationHelper.RequireFields(new[] { "foo", "bar" });
            var result = validationHelper.IsValid("foo");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GetValidationHtmlReturnsNullIfNoRulesAreRegistered()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            ValidationHelper validationHelper = GetValidationHelper(GetContext());

            // Assert
            var validationAttributes = validationHelper.For("bar");

            // Assert
            Assert.Null(validationAttributes);
        }

        [Fact]
        public void GetValidationHtmlReturnsAttributesForRegisteredValidators()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var validator = new Mock<IValidator>();
            var clientRules = new ModelClientValidationRule { ValidationType = "foo", ErrorMessage = "Foo error." };
            clientRules.ValidationParameters["qux"] = "some data";
            validator.Setup(c => c.ClientValidationRule).Returns(clientRules).Verifiable();
            var expected = @"data-val-required=""This field is required."" data-val-foo=""Foo error."" data-val-foo-qux=""some data"" data-val=""true""";

            ValidationHelper validationHelper = GetValidationHelper(GetContext());

            // Act
            validationHelper.RequireField("foo");
            validationHelper.Add("foo", validator.Object);
            var validationAttributes = validationHelper.For("foo");

            // Assert
            Assert.Equal(expected, validationAttributes.ToString());
        }

        [Fact]
        public void GetValidationHtmlHtmlEncodesAttributeValues()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var validator = new Mock<IValidator>();
            var clientRules = new ModelClientValidationRule { ValidationType = "biz", ErrorMessage = "<Biz error.>" };
            clientRules.ValidationParameters["qux"] = "<some ' data>";
            validator.Setup(c => c.ClientValidationRule).Returns(clientRules).Verifiable();
            var expected = @"data-val-required=""This field is required."" data-val-biz=""&lt;Biz error.&gt;"" data-val-biz-qux=""&lt;some &#39; data&gt;"" data-val=""true""";

            // Act
            ValidationHelper validationHelper = GetValidationHelper(GetContext());

            // Assert
            validationHelper.RequireField("foo");
            validationHelper.Add("foo", validator.Object);
            var validationAttributes = validationHelper.For("foo");

            // Assert
            Assert.Equal(expected, validationAttributes.ToString());
        }

        [Fact]
        public void GetValidationFromClientValidationRulesThrowsIfValidationTypeIsNullOrEmpty()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var clientRule = new ModelClientValidationRule { ValidationType = null };

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => ValidationHelper.GenerateHtmlFromClientValidationRules(new[] { clientRule }),
                                                              "Validation type names in unobtrusive client validation rules cannot be empty. Client rule type: System.Web.Mvc.ModelClientValidationRule");

            clientRule.ValidationType = String.Empty;
            Assert.Throws<InvalidOperationException>(() => ValidationHelper.GenerateHtmlFromClientValidationRules(new[] { clientRule }),
                                                              "Validation type names in unobtrusive client validation rules cannot be empty. Client rule type: System.Web.Mvc.ModelClientValidationRule");
        }

        [Fact]
        public void GetValidationFromClientValidationRulesThrowsIfSameValidationTypeIsSpecifiedMultipleTimes()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var clientRule1 = new ModelClientValidationRule { ValidationType = "foo" };
            var clientRule2 = new ModelClientValidationRule { ValidationType = "foo" };

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => ValidationHelper.GenerateHtmlFromClientValidationRules(new[] { clientRule1, clientRule2 }),
                                                              "Validation type names in unobtrusive client validation rules must be unique. The following validation type was seen more than once: foo");
        }

        [Fact]
        public void GetValidationFromClientValidationRulesThrowsIfValidationTypeDoesNotContainAllLowerCaseCharacters()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var clientRule = new ModelClientValidationRule { ValidationType = "Foo" };

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => ValidationHelper.GenerateHtmlFromClientValidationRules(new[] { clientRule }),
                                                              "Validation type names in unobtrusive client validation rules must consist of only lowercase letters. Invalid name: \"Foo\", client rule type: System.Web.Mvc.ModelClientValidationRule");

            clientRule.ValidationType = "bAr";
            Assert.Throws<InvalidOperationException>(() => ValidationHelper.GenerateHtmlFromClientValidationRules(new[] { clientRule }),
                                                              "Validation type names in unobtrusive client validation rules must consist of only lowercase letters. Invalid name: \"bAr\", client rule type: System.Web.Mvc.ModelClientValidationRule");

            clientRule.ValidationType = "bar123";
            Assert.Throws<InvalidOperationException>(() => ValidationHelper.GenerateHtmlFromClientValidationRules(new[] { clientRule }),
                                                              "Validation type names in unobtrusive client validation rules must consist of only lowercase letters. Invalid name: \"bar123\", client rule type: System.Web.Mvc.ModelClientValidationRule");
        }

        [Fact]
        public void GetValidationFromClientValidationRulesThrowsIfValidationParamaterContainsNonAlphaNumericCharacters()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var clientRule = new ModelClientValidationRule { ValidationType = "required" };
            clientRule.ValidationParameters["min^"] = "some-val";

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => ValidationHelper.GenerateHtmlFromClientValidationRules(new[] { clientRule }),
                                                              "Validation parameter names in unobtrusive client validation rules must start with a lowercase letter and consist of only lowercase letters or digits. Validation parameter name: min^, client rule type: System.Web.Mvc.ModelClientValidationRule");

            clientRule.ValidationParameters.Clear();
            clientRule.ValidationParameters["Min"] = "some-val";

            Assert.Throws<InvalidOperationException>(() => ValidationHelper.GenerateHtmlFromClientValidationRules(new[] { clientRule }),
                                                              "Validation parameter names in unobtrusive client validation rules must start with a lowercase letter and consist of only lowercase letters or digits. Validation parameter name: Min, client rule type: System.Web.Mvc.ModelClientValidationRule");
        }

        [Fact]
        public void DefaultValidCssClassIsNull()
        {
            Assert.Null(ValidationHelper.ValidCssClass);
        }

        [Fact]
        public void DefaultInvalidCssClassIsSameAsHtmlHelper()
        {
            Assert.Equal(HtmlHelper.DefaultValidationInputErrorCssClass, ValidationHelper.InvalidCssClass);
        }

        [Fact]
        public void InvalidCssClassIsNullIfExplicitlySetToNull()
        {
            using (ValidationHelper.OverrideScope())
            {
                ValidationHelper.InvalidCssClass = null;
                Assert.Null(ValidationHelper.InvalidCssClass);
            }
        }

        [Fact]
        public void ValidCssClassIsScopeBacked()
        {
            // Set a value
            string old = ValidationHelper.ValidCssClass;
            ValidationHelper.ValidCssClass = "outer";
            using (ScopeStorage.CreateTransientScope())
            {
                ValidationHelper.ValidCssClass = "inner";
                Assert.Equal("inner", ValidationHelper.ValidCssClass);
            }
            Assert.Equal("outer", ValidationHelper.ValidCssClass);
            ValidationHelper.ValidCssClass = old;
        }

        [Fact]
        public void InvalidCssClassIsScopeBacked()
        {
            // Set a value
            string old = ValidationHelper.InvalidCssClass;
            ValidationHelper.InvalidCssClass = "outer";
            using (ScopeStorage.CreateTransientScope())
            {
                ValidationHelper.InvalidCssClass = "inner";
                Assert.Equal("inner", ValidationHelper.InvalidCssClass);
            }
            Assert.Equal("outer", ValidationHelper.InvalidCssClass);
            ValidationHelper.InvalidCssClass = old;
        }

        [Fact]
        public void ClassForReturnsNullIfNotPost()
        {
            // Arrange
            ValidationHelper helper = GetValidationHelper();

            // Act/Assert
            Assert.Null(helper.ClassFor("foo"));
        }

        [Fact]
        public void ClassForReturnsValidClassNameIfNoErrorsAddedForField()
        {
            // Arrange
            ValidationHelper helper = GetPostValidationHelper();

            // Act/Assert
            HtmlString html = helper.ClassFor("foo");
            string str = html == null ? null : html.ToHtmlString();
            Assert.Equal(ValidationHelper.ValidCssClass, str);
        }

        [Fact]
        public void ClassForReturnsInvalidClassNameIfFieldHasErrors()
        {
            // Arrange
            ValidationHelper helper = GetPostValidationHelper();
            helper.Add("foo", new AutoFailValidator());

            // Act/Assert
            Assert.Equal(ValidationHelper.InvalidCssClass, helper.ClassFor("foo").ToHtmlString());
        }

        private static ValidationHelper GetPostValidationHelper()
        {
            HttpContextBase context = GetContext();
            Mock.Get(context.Request).SetupGet(c => c.HttpMethod).Returns("POST");
            ValidationHelper helper = GetValidationHelper(httpContext: context);
            return helper;
        }

        private static HttpContextBase GetContext(object formValues = null)
        {
            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();

            var nameValueCollection = new NameValueCollection();
            if (formValues != null)
            {
                foreach (var prop in formValues.GetType().GetProperties())
                {
                    nameValueCollection.Add(prop.Name, prop.GetValue(formValues, null).ToString());
                }
            }
            request.SetupGet(c => c.Form).Returns(nameValueCollection);
            context.SetupGet(c => c.Request).Returns(request.Object);

            return context.Object;
        }

        private static ValidationHelper GetValidationHelper(HttpContextBase httpContext = null, ModelStateDictionary modelStateDictionary = null)
        {
            httpContext = httpContext ?? GetContext();
            modelStateDictionary = modelStateDictionary ?? new ModelStateDictionary();

            return new ValidationHelper(httpContext, modelStateDictionary);
        }

        private class AutoFailValidator : IValidator
        {
            public ValidationResult Validate(ValidationContext validationContext)
            {
                return new ValidationResult("Failed!");
            }

            public ModelClientValidationRule ClientValidationRule
            {
                get { throw new NotImplementedException(); }
            }
        }

        public void Dispose()
        {
            // Reset ScopeStorage (written via e.g. ValidationHelper.InvalidCssClass) between tests to avoid unexpected interactions.
            ScopeStorage.CurrentProvider = new StaticScopeStorageProvider();
            ScopeStorage.GlobalScope.Clear();
        }
    }
}
