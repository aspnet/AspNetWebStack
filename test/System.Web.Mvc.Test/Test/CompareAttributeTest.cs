// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    [Xunit.Collection("Uses ScopeStorage or ViewEngines.Engines")] // Uses ModelMetadataProviders.Current
    public class CompareAttributeTest
    {
        [Fact]
        public void GuardClauses()
        {
            //Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { new CompareAttribute(null); }, "otherProperty");

            Assert.ThrowsArgumentNullOrEmpty(
                delegate { CompareAttribute.FormatPropertyForClientValidation(null); }, "property");
        }

        [Fact]
        public void FormatPropertyForClientValidationPrependsStarDot()
        {
            string prepended = CompareAttribute.FormatPropertyForClientValidation("test");
            Assert.Equal("*.test", prepended);
        }

        [Fact]
        public void ValidateDoesNotThrowWhenComparedObjectsAreEqual()
        {
            object otherObject = new CompareObject("test");
            CompareObject currentObject = new CompareObject("test");
            ValidationContext testContext = new ValidationContext(otherObject, null, null);

            CompareAttribute attr = new CompareAttribute("CompareProperty");
            attr.Validate(currentObject.CompareProperty, testContext);
        }

        [Fact]
        public void ValidateThrowsWhenComparedObjectsAreNotEqual()
        {
            CompareObject currentObject = new CompareObject("a");
            object otherObject = new CompareObject("b");

            ValidationContext testContext = new ValidationContext(otherObject, null, null);
            testContext.DisplayName = "CurrentProperty";

            CompareAttribute attr = new CompareAttribute("CompareProperty");
            Assert.Throws<ValidationException>(
                delegate { attr.Validate(currentObject.CompareProperty, testContext); }, "'CurrentProperty' and 'CompareProperty' do not match.");
        }

        [Fact]
        public void ValidateThrowsWithOtherPropertyDisplayName()
        {
            CompareObject currentObject = new CompareObject("a");
            object otherObject = new CompareObject("b");

            ValidationContext testContext = new ValidationContext(otherObject, null, null);
            testContext.DisplayName = "CurrentProperty";

            CompareAttribute attr = new CompareAttribute("ComparePropertyWithDisplayName");
            Assert.Throws<ValidationException>(
                delegate { attr.Validate(currentObject.CompareProperty, testContext); }, "'CurrentProperty' and 'DisplayName' do not match.");
        }

        [Fact]
        public void ValidateUsesSetDisplayName()
        {
            CompareObject currentObject = new CompareObject("a");
            object otherObject = new CompareObject("b");

            ValidationContext testContext = new ValidationContext(otherObject, null, null);
            testContext.DisplayName = "CurrentProperty";

            CompareAttribute attr = new CompareAttribute("ComparePropertyWithDisplayName");
            attr.OtherPropertyDisplayName = "SetDisplayName";

            Assert.Throws<ValidationException>(
                delegate { attr.Validate(currentObject.CompareProperty, testContext); }, "'CurrentProperty' and 'SetDisplayName' do not match.");
        }

        [Fact]
        public void ValidateThrowsWhenPropertyNameIsUnknown()
        {
            CompareObject currentObject = new CompareObject("a");
            object otherObject = new CompareObject("b");

            ValidationContext testContext = new ValidationContext(otherObject, null, null);
            testContext.DisplayName = "CurrentProperty";

            CompareAttribute attr = new CompareAttribute("UnknownPropertyName");
            Assert.Throws<ValidationException>(
                () => attr.Validate(currentObject.CompareProperty, testContext),
                "Could not find a property named UnknownPropertyName."
                );
        }

        [Fact]
        public void GetClientValidationRulesReturnsModelClientValidationEqualToRule()
        {
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            Mock<ModelMetadata> metadata = new Mock<ModelMetadata>(provider.Object, null, null, typeof(string), null);
            metadata.Setup(m => m.DisplayName).Returns("CurrentProperty");

            CompareAttribute attr = new CompareAttribute("CompareProperty");
            List<ModelClientValidationRule> ruleList = new List<ModelClientValidationRule>(attr.GetClientValidationRules(metadata.Object, null));

            ModelClientValidationRule rule = Assert.Single(ruleList);
            ModelClientValidationEqualToRule actualRule = Assert.IsType<ModelClientValidationEqualToRule>(rule);

            Assert.Equal("'CurrentProperty' and 'CompareProperty' do not match.", actualRule.ErrorMessage);
            Assert.Equal("equalto", actualRule.ValidationType);
            Assert.Equal("*.CompareProperty", actualRule.ValidationParameters["other"]);
        }

        [Fact]
        public void ModelClientValidationEqualToRuleErrorMessageUsesOtherPropertyDisplayName()
        {
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            ModelMetadata metadata = new ModelMetadata(provider.Object, typeof(CompareObject), null, typeof(string), null);
            metadata.DisplayName = "CurrentProperty";

            CompareAttribute attr = new CompareAttribute("ComparePropertyWithDisplayName");
            List<ModelClientValidationRule> ruleList = new List<ModelClientValidationRule>(attr.GetClientValidationRules(metadata, null));

            ModelClientValidationRule rule = Assert.Single(ruleList);
            ModelClientValidationEqualToRule actualRule = Assert.IsType<ModelClientValidationEqualToRule>(rule);

            Assert.Equal("'CurrentProperty' and 'DisplayName' do not match.", actualRule.ErrorMessage);
            Assert.Equal("equalto", actualRule.ValidationType);
            Assert.Equal("*.ComparePropertyWithDisplayName", actualRule.ValidationParameters["other"]);
        }

        [Fact]
        public void ModelClientValidationEqualToRuleUsesSetDisplayName()
        {
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            ModelMetadata metadata = new ModelMetadata(provider.Object, typeof(CompareObject), null, typeof(string), null);
            metadata.DisplayName = "CurrentProperty";

            CompareAttribute attr = new CompareAttribute("ComparePropertyWithDisplayName");
            attr.OtherPropertyDisplayName = "SetDisplayName";

            List<ModelClientValidationRule> ruleList = new List<ModelClientValidationRule>(attr.GetClientValidationRules(metadata, null));
            ModelClientValidationRule rule = Assert.Single(ruleList);
            ModelClientValidationEqualToRule actualRule = Assert.IsType<ModelClientValidationEqualToRule>(rule);

            Assert.Equal("'CurrentProperty' and 'SetDisplayName' do not match.", actualRule.ErrorMessage);
        }

        [Fact]
        public void CompareAttributeCanBeDerivedFromAndOverrideIsValid()
        {
            object otherObject = new CompareObject("a");
            CompareObject currentObject = new CompareObject("b");
            ValidationContext testContext = new ValidationContext(otherObject, null, null);

            DerivedCompareAttribute attr = new DerivedCompareAttribute("CompareProperty");
            attr.Validate(currentObject.CompareProperty, testContext);
        }

        private class DerivedCompareAttribute : CompareAttribute
        {
            public DerivedCompareAttribute(string otherProperty)
                : base(otherProperty)
            {
            }

            public override bool IsValid(object value)
            {
                return false;
            }

            protected override ValidationResult IsValid(object value, ValidationContext context)
            {
                return null;
            }
        }

        private class CompareObject
        {
            public string CompareProperty { get; set; }

            [Display(Name = "DisplayName")]
            public string ComparePropertyWithDisplayName { get; set; }

            public CompareObject(string otherValue)
            {
                CompareProperty = otherValue;
                ComparePropertyWithDisplayName = otherValue;
            }
        }
    }
}
