// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.WebPages.Validation.Test
{
    public class ValidatorTest
    {
        [Fact]
        public void RequiredValidatorValidatesIfStringIsNull()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var requiredValidator = Validator.Required();
            var validationContext = GetValidationContext(GetContext(), "foo");

            // Act
            var result = requiredValidator.Validate(validationContext);

            // Assert
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.Equal("This field is required.", result.ErrorMessage);
            Assert.Equal("foo", result.MemberNames.Single());
        }

        [Fact]
        public void RequiredValidatorValidatesIfStringIsEmpty()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var requiredValidator = Validator.Required();
            var validationContext = GetValidationContext(GetContext(new { foo = "" }), "foo");

            // Act
            var result = requiredValidator.Validate(validationContext);

            // Assert
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.Equal("This field is required.", result.ErrorMessage);
            Assert.Equal("foo", result.MemberNames.Single());
        }

        [Fact]
        public void RequiredValidatorReturnsCustomErrorMessagesIfSpecified()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var requiredValidator = Validator.Required("There is no string");
            var httpContext = GetContext(new { foo = "" });
            var validationContext = GetValidationContext(httpContext, "foo");

            // Act
            var result = requiredValidator.Validate(validationContext);

            // Assert
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.Equal("There is no string", result.ErrorMessage);
            Assert.Equal("foo", result.MemberNames.Single());
        }

        [Fact]
        public void RequiredValidatorReturnsSuccessIfNoFieldIsNotNullOrEmpty()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var requiredValidator = Validator.Required("foo");
            var validationContext = GetValidationContext(GetContext(new { foo = "some value" }), "foo");

            // Act
            var result = requiredValidator.Validate(validationContext);

            // Assert
            Assert.Equal(ValidationResult.Success, result);
        }

        [Fact]
        public void GetClientValidationRulesForRequiredValidatorWithDefaultErrorMessage()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var requiredValidator = Validator.Required();

            // Act
            var result = requiredValidator.ClientValidationRule;

            // Assert
            Assert.Equal("required", result.ValidationType);
            Assert.Equal("This field is required.", result.ErrorMessage);
            Assert.False(result.ValidationParameters.Any());
        }

        [Fact]
        public void GetClientValidationRulesForRequiredValidatorWithCustomErrorMessage()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var requiredValidator = Validator.Required("custom message");

            // Act
            var result = requiredValidator.ClientValidationRule;

            // Assert
            Assert.Equal("required", result.ValidationType);
            Assert.Equal("custom message", result.ErrorMessage);
            Assert.False(result.ValidationParameters.Any());
        }

        [Fact]
        public void RangeValidatorReturnsSuccessIfValueIsInRange()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var rangeValidator = Validator.Range(10, 12);
            var validationContext = GetValidationContext(GetContext(new { foo = 11 }), "foo");

            // Act
            var result = rangeValidator.Validate(validationContext);

            // Assert
            Assert.Equal(ValidationResult.Success, result);
        }

        [Fact]
        public void RangeValidatorReturnsDefaultErrorMessageIfValueIsNotInRange()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var rangeValidator = Validator.Range(10, 12);
            var validationContext = GetValidationContext(GetContext(new { foo = 7 }), "foo");

            // Act
            var result = rangeValidator.Validate(validationContext);

            // Assert
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.Equal("Value must be an integer between 10 and 12.", result.ErrorMessage);
            Assert.Equal("foo", result.MemberNames.Single());
        }

        [Fact]
        public void RangeValidatorReturnsDefaultErrorMessageIfValueIsNotInRangeForFloatingPointValues()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var rangeValidator = Validator.Range(10.8, 12.2);
            var validationContext = GetValidationContext(GetContext(new { foo = 7 }), "foo");

            // Act
            var result = rangeValidator.Validate(validationContext);

            // Assert
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.Equal(
                String.Format(CultureInfo.CurrentCulture, "Value must be a decimal between {0} and {1}.", 10.8, 12.2),
                result.ErrorMessage);
            Assert.Equal("foo", result.MemberNames.Single());
        }

        [Fact]
        public void RangeValidatorReturnsCustomErrorMessageIfSpecified()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var rangeValidator = Validator.Range(10, 12, "Custom error message");
            var validationContext = GetValidationContext(GetContext(new { foo = 13 }), "foo");

            // Act
            var result = rangeValidator.Validate(validationContext);

            // Assert
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.Equal("Custom error message", result.ErrorMessage);
            Assert.Equal("foo", result.MemberNames.Single());
        }

        [Fact]
        public void RangeValidatorFormatsCustomErrorMessage()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var rangeValidator = Validator.Range(10, 12, "Valid range: {0}-{1}");
            var validationContext = GetValidationContext(GetContext(new { foo = 13 }), "foo");

            // Act
            var result = rangeValidator.Validate(validationContext);

            // Assert
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.Equal("Valid range: 10-12", result.ErrorMessage);
            Assert.Equal("foo", result.MemberNames.Single());
        }

        [Fact]
        public void GetClientValidationRulesForRangeValidatorWithDefaultErrorMessage()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var rangeValidator = Validator.Range(10, 12);

            // Act
            var result = rangeValidator.ClientValidationRule;

            // Assert
            Assert.Equal("range", result.ValidationType);
            Assert.Equal("Value must be an integer between 10 and 12.", result.ErrorMessage);
            Assert.Equal(10, result.ValidationParameters["min"]);
            Assert.Equal(12, result.ValidationParameters["max"]);
        }

        [Fact]
        public void GetClientValidationRulesForRangeValidatorWithCustomErrorMessage()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var rangeValidator = Validator.Range(10, 11, "Range: {0}-{1}");

            // Act
            var result = rangeValidator.ClientValidationRule;

            // Assert
            Assert.Equal("range", result.ValidationType);
            Assert.Equal("Range: 10-11", result.ErrorMessage);
            Assert.Equal(10, result.ValidationParameters["min"]);
            Assert.Equal(11, result.ValidationParameters["max"]);
        }

        [Fact]
        public void StringLengthValidatorReturnsSuccessIfStringLengthIsSmallerThanMaxValue()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var validator = Validator.StringLength(10);
            var validationContext = GetValidationContext(GetContext(new { baz = "hello" }), "baz");

            // Act
            var result = validator.Validate(validationContext);

            // Assert
            Assert.Equal(ValidationResult.Success, result);
        }

        [Fact]
        public void StringLengthValidatorReturnsSuccessIfStringLengthIsRangeOfMinAndMaxValue()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var validator = Validator.StringLength(10, minLength: 6);
            var validationContext = GetValidationContext(GetContext(new { bar = "woof woof" }), "bar");

            // Act
            var result = validator.Validate(validationContext);

            // Assert
            Assert.Equal(ValidationResult.Success, result);
        }

        [Fact]
        public void StringLengthValidatorReturnsFailureIfStringLengthIsLongerThanMaxValue()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var validator = Validator.StringLength(4);
            var validationContext = GetValidationContext(GetContext(new { baz = "woof woof" }), "baz");

            // Act
            var result = validator.Validate(validationContext);

            // Assert
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.Equal("Max length: 4.", result.ErrorMessage);
            Assert.Equal("baz", result.MemberNames.Single());
        }

        [Fact]
        public void StringLengthValidatorReturnsCustomErrorMessageIfStringLengthIsLongerThanMaxValue()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var validator = Validator.StringLength(4, errorMessage: "String must be at least {0} characters long.");
            var validationContext = GetValidationContext(GetContext(new { baz = "woof woof" }), "baz");

            // Act
            var result = validator.Validate(validationContext);

            // Assert
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.Equal("String must be at least 4 characters long.", result.ErrorMessage);
            Assert.Equal("baz", result.MemberNames.Single());
        }

        [Fact]
        public void StringLengthValidatorReturnsFailureIfStringLengthIsNotInRange()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var validator = Validator.StringLength(6, 4);
            var validationContext = GetValidationContext(GetContext(new { baz = "woof woof" }), "baz");

            // Act
            var result = validator.Validate(validationContext);

            // Assert
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.Equal("String must be between 4 and 6 characters.", result.ErrorMessage);
            Assert.Equal("baz", result.MemberNames.Single());
        }

        [Fact]
        public void StringLengthValidatorReturnsCustomErrorMessageIfStringLengthIsNotInRange()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var validator = Validator.StringLength(6, 4, "Range {0} - {1}");
            var validationContext = GetValidationContext(GetContext(new { baz = "woof woof" }), "baz");

            // Act
            var result = validator.Validate(validationContext);

            // Assert
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.Equal("Range 4 - 6", result.ErrorMessage);
            Assert.Equal("baz", result.MemberNames.Single());
        }

        [Fact]
        public void GetClientValidationRulesForStringLengthValidatorWithDefaultErrorMessage()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var validator = Validator.StringLength(6, 4);

            // Act
            var result = validator.ClientValidationRule;

            // Assert
            Assert.Equal("length", result.ValidationType);
            Assert.Equal("String must be between 4 and 6 characters.", result.ErrorMessage);
            Assert.Equal(4, result.ValidationParameters["min"]);
            Assert.Equal(6, result.ValidationParameters["max"]);
        }

        [Fact]
        public void GetClientValidationRulesForStringLengthValidatorWithCustomErrorMessage()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var validator = Validator.StringLength(6, errorMessage: "Must be at least 6 letters.");

            // Act
            var result = validator.ClientValidationRule;

            // Assert
            Assert.Equal("length", result.ValidationType);
            Assert.Equal("Must be at least 6 letters.", result.ErrorMessage);
            Assert.Equal(6, result.ValidationParameters["max"]);
        }

        [Fact]
        public void RegexThrowsIfPatternIsNullOrEmpty()
        {
            // Act and Assert
            Assert.ThrowsArgumentNullOrEmptyString(() => Validator.Regex(null), "pattern");
            Assert.ThrowsArgumentNullOrEmptyString(() => Validator.Regex(String.Empty), "pattern");
        }

        [Fact]
        public void RegexReturnsSuccessIfValueMatches()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var validator = Validator.Regex("^a+b+c$");
            var context = GetValidationContext(GetContext(new { foo = "aaabbc" }), "foo");

            // Act
            var result = validator.Validate(context);

            // Assert
            Assert.Equal(ValidationResult.Success, result);
        }

        [Fact]
        public void RegexReturnsDefaultErrorMessageIfValidationFails()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var validator = Validator.Regex("^a+b+c$");
            var context = GetValidationContext(GetContext(new { foo = "aaXabbc" }), "foo");

            // Act
            var result = validator.Validate(context);

            // Assert
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.Equal("Value is invalid.", result.ErrorMessage);
            Assert.Equal("foo", result.MemberNames.Single());
        }

        [Fact]
        public void RegexReturnsCustomErrorMessageIfValidationFails()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var validator = Validator.Regex("^a+b+c$");
            var context = GetValidationContext(GetContext(new { foo = "aaXabbc" }), "foo");

            // Act
            var result = validator.Validate(context);

            // Assert
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.Equal("Value is invalid.", result.ErrorMessage);
            Assert.Equal("foo", result.MemberNames.Single());
        }

        [Fact]
        public void IntegerReturnsSuccessIfValueIsNull()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var validator = Validator.Integer();
            var context = GetValidationContext(GetContext(new { Name = "Not-Age" }), "age");

            // Act
            var result = validator.Validate(context);

            // Assert
            Assert.Equal(ValidationResult.Success, result);
        }

        [Fact]
        public void IntegerReturnsSuccessIfValueIsEmpty()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var validator = Validator.Integer();
            var context = GetValidationContext(GetContext(new { Age = "" }), "age");

            // Act
            var result = validator.Validate(context);

            // Assert
            Assert.Equal(ValidationResult.Success, result);
        }

        [Fact]
        public void IntegerReturnsSuccessIfValueIsValidInteger()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var validator = Validator.Integer();
            var context = GetValidationContext(GetContext(new { Age = "10" }), "age");

            // Act
            var result = validator.Validate(context);

            // Assert
            Assert.Equal(ValidationResult.Success, result);
        }

        [Fact]
        public void IntegerReturnsSuccessIfValueIsNegativeInteger()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var validator = Validator.Integer();
            var context = GetValidationContext(GetContext(new { Age = "-42" }), "age");

            // Act
            var result = validator.Validate(context);

            // Assert
            Assert.Equal(ValidationResult.Success, result);
        }

        [Fact]
        public void IntegerReturnsSuccessIfValueIsZero()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var validator = Validator.Integer();
            var context = GetValidationContext(GetContext(new { Age = 0 }), "age");

            // Act
            var result = validator.Validate(context);

            // Assert
            Assert.Equal(ValidationResult.Success, result);
        }

        [Fact]
        public void IntegerReturnsErrorMessageIfValueIsFloatingPointValue()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var validator = Validator.Integer();
            var context = GetValidationContext(GetContext(new { Age = 1.3 }), "age");

            // Act
            var result = validator.Validate(context);

            // Assert
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.Equal("Input format is invalid.", result.ErrorMessage);
            Assert.Equal("age", result.MemberNames.Single());
        }

        [Fact]
        public void IntegerReturnsErrorMessageIfValueIsNotAnInteger()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var validator = Validator.Integer();
            var context = GetValidationContext(GetContext(new { Age = "2008-04-01" }), "age");

            // Act
            var result = validator.Validate(context);

            // Assert
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.Equal("Input format is invalid.", result.ErrorMessage);
            Assert.Equal("age", result.MemberNames.Single());
        }

        [Fact]
        public void FloatReturnsSuccessIfValueIsNull()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var validator = Validator.Float();
            var context = GetValidationContext(GetContext(new { }), "Price");

            // Act
            var result = validator.Validate(context);

            // Assert
            Assert.Equal(ValidationResult.Success, result);
        }

        [Fact]
        public void FloatReturnsSuccessIfValueIsEmptyString()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var validator = Validator.Float();
            var context = GetValidationContext(GetContext(new { Price = "" }), "Price");

            // Act
            var result = validator.Validate(context);

            // Assert
            Assert.Equal(ValidationResult.Success, result);
        }

        [Fact]
        public void FloatReturnsSuccessIfValueIsValidFloatString()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var validator = Validator.Float();
            var context = GetValidationContext(GetContext(new { Price = Single.MaxValue.ToString() }), "Price");

            // Act
            var result = validator.Validate(context);

            // Assert
            Assert.Equal(ValidationResult.Success, result);
        }

        [Fact]
        public void FloatReturnsSuccessIfValueIsValidInteger()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var validator = Validator.Float();
            var context = GetValidationContext(GetContext(new { Price = "1" }), "Price");

            // Act
            var result = validator.Validate(context);

            // Assert
            Assert.Equal(ValidationResult.Success, result);
        }

        [Fact]
        public void FloatReturnsErrorIfValueIsNotAValidFloat()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var validator = Validator.Float();
            var context = GetValidationContext(GetContext(new { Price = "Free!!!" }), "Price");

            // Act
            var result = validator.Validate(context);

            // Assert
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.Equal("Input format is invalid.", result.ErrorMessage);
            Assert.Equal("Price", result.MemberNames.Single());
        }

        [Fact]
        public void DateTimeReturnsSuccessIfValueIsNull()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var validator = Validator.DateTime();
            var context = GetValidationContext(GetContext(new { }), "dateOfBirth");

            // Act
            var result = validator.Validate(context);

            // Assert
            Assert.Equal(ValidationResult.Success, result);
        }

        [Fact]
        public void DateTimeReturnsSuccessIfValueIsEmpty()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var validator = Validator.DateTime();
            var context = GetValidationContext(GetContext(new { dateOfBirth = "" }), "dateOfBirth");

            // Act
            var result = validator.Validate(context);

            // Assert
            Assert.Equal(ValidationResult.Success, result);
        }

        [Fact]
        public void DateTimeReturnsSuccessIfValueIsValidDateTime()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var validator = Validator.DateTime();
            var context = GetValidationContext(GetContext(new { dateOfBirth = DateTime.Now.ToString() }), "dateOfBirth");

            // Act
            var result = validator.Validate(context);

            // Assert
            Assert.Equal(ValidationResult.Success, result);
        }

        [Fact]
        public void DateTimeReturnsErrorIfValueIsInvalidDateTime()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var validator = Validator.DateTime();
            var context = GetValidationContext(GetContext(new { dateOfBirth = "23.28" }), "dateOfBirth");

            // Act
            var result = validator.Validate(context);

            // Assert
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.Equal("Input format is invalid.", result.ErrorMessage);
            Assert.Equal("dateOfBirth", result.MemberNames.Single());
        }

        [Fact]
        public void UrlReturnsSuccessIfInputIsNull()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var validator = Validator.Url();
            var context = GetValidationContext(GetContext(new { }), "blogUrl");

            // Act
            var result = validator.Validate(context);

            // Assert
            Assert.Equal(ValidationResult.Success, result);
        }

        [Fact]
        public void UrlReturnsSuccessIfInputIsEmptyString()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var validator = Validator.Url();
            var context = GetValidationContext(GetContext(new { blogUrl = "" }), "blogUrl");

            // Act
            var result = validator.Validate(context);

            // Assert
            Assert.Equal(ValidationResult.Success, result);
        }

        [Fact]
        public void UrlReturnsSuccessIfInputIsValidUrl()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var validator = Validator.Url();
            var context = GetValidationContext(GetContext(new { blogUrl = "http://www.microsoft.com?query-param=query-param-value&some-val=&quot;true&quot;" }), "blogUrl");

            // Act
            var result = validator.Validate(context);

            // Assert
            Assert.Equal(ValidationResult.Success, result);
        }

        [Fact]
        public void UrlReturnsErrorMessageIfInputIsPhysicalPath()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var validator = Validator.Url();
            var context = GetValidationContext(GetContext(new { blogUrl = @"x:\some-path\foo.txt" }), "blogUrl");

            // Act
            var result = validator.Validate(context);

            // Assert
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.Equal("Input format is invalid.", result.ErrorMessage);
            Assert.Equal("blogUrl", result.MemberNames.Single());
        }

        [Fact]
        public void UrlReturnsErrorMessageIfInputIsNetworkPath()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var validator = Validator.Url();
            var context = GetValidationContext(GetContext(new { blogUrl = @"\\network-share\some-path\" }), "blogUrl");

            // Act
            var result = validator.Validate(context);

            // Assert
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.Equal("Input format is invalid.", result.ErrorMessage);
            Assert.Equal("blogUrl", result.MemberNames.Single());
        }

        [Fact]
        public void UrlReturnsErrorMessageIfInputIsNotAnUrl()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var validator = Validator.Url();
            var context = GetValidationContext(GetContext(new { blogUrl = 65 }), "blogUrl");

            // Act
            var result = validator.Validate(context);

            // Assert
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.Equal("Input format is invalid.", result.ErrorMessage);
            Assert.Equal("blogUrl", result.MemberNames.Single());
        }

        [Fact]
        public void EqualsToValidatorThrowsIfFieldIsNullOrEmpty()
        {
            // Act and Assert
            Assert.ThrowsArgumentNullOrEmptyString(() => Validator.EqualsTo(null), "otherFieldName");
        }

        [Fact]
        public void EqualsToValidatorReturnsFalseIfEitherFieldIsEmpty()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var validator = Validator.EqualsTo("password");
            var context = GetValidationContext(GetContext(new { password = "", confirmPassword = "abcd" }), "confirmPassword");

            // Act
            var result = validator.Validate(context);

            // Assert
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.Equal("Values do not match.", result.ErrorMessage);
            Assert.Equal("confirmPassword", result.MemberNames.Single());

            context = GetValidationContext(GetContext(new { password = "abcd", confirmPassword = "" }), "confirmPassword");

            // Act
            result = validator.Validate(context);

            // Assert
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.Equal("Values do not match.", result.ErrorMessage);
            Assert.Equal("confirmPassword", result.MemberNames.Single());
        }

        [Fact]
        public void EqualsToValidatorReturnsFalseIfValuesDoNotMatch()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var validator = Validator.EqualsTo("password");
            var context = GetValidationContext(GetContext(new { password = "password2", confirmPassword = "abcd" }), "confirmPassword");

            // Act
            var result = validator.Validate(context);

            // Assert
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.Equal("Values do not match.", result.ErrorMessage);
            Assert.Equal("confirmPassword", result.MemberNames.Single());
        }

        [Fact]
        public void EqualsToValidatorReturnsTrueIfValuesMatch()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var validator = Validator.EqualsTo("password");
            var context = GetValidationContext(GetContext(new { password = "abcd", confirmPassword = "abcd" }), "confirmPassword");

            // Act
            var result = validator.Validate(context);

            // Assert
            Assert.Equal(ValidationResult.Success, result);
        }

        [Fact]
        public void GetClientValidationRulesForRegex()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var validator = Validator.Regex("^a+b+c$");

            // Act
            var result = validator.ClientValidationRule;

            // Assert
            Assert.Equal("regex", result.ValidationType);
            Assert.Equal("Value is invalid.", result.ErrorMessage);
            Assert.Equal("^a+b+c$", result.ValidationParameters["pattern"]);
        }

        [Fact]
        public void GetClientValidationRulesForRegexWithCustomErrorMessage()
        {
            // Arrange
            RequestFieldValidatorBase.IgnoreUseUnvalidatedValues = true;
            var validator = Validator.Regex("^a+b+c$", "Example aaabbc");

            // Act
            var result = validator.ClientValidationRule;

            // Assert
            Assert.Equal("regex", result.ValidationType);
            Assert.Equal("Example aaabbc", result.ErrorMessage);
            Assert.Equal("^a+b+c$", result.ValidationParameters["pattern"]);
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

        private static ValidationContext GetValidationContext(HttpContextBase httpContext, string memberName)
        {
            return new ValidationContext(httpContext, null, null) { MemberName = memberName };
        }
    }
}
