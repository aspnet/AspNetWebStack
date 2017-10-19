// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Linq;
using Microsoft.TestCommon;
using AnnotationsCompareAttribute = System.ComponentModel.DataAnnotations.CompareAttribute;
using MyResources = System.Web.Properties.Resources;

namespace System.Web.Mvc.Test
{
    [Xunit.Collection("Uses ScopeStorage or ViewEngines.Engines")] // Uses ModelMetadataProviders.Current
    public class CompareAttributeAdapterTest
    {
        [Fact]
        [ReplaceCulture]
        public void ClientRulesWithCompareAttribute_ErrorMessageUsesDisplayName()
        {
            // Arrange
            var metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => null, typeof(PropertyDisplayNameModel), "MyProperty");
            var context = new ControllerContext();
            var attribute = new AnnotationsCompareAttribute("OtherProperty");
            var adapter = new CompareAttributeAdapter(metadata, context, attribute);

            // Act
            var rules = adapter.GetClientValidationRules()
                .OrderBy(r => r.ValidationType)
                .ToArray();

            // Assert
            ModelClientValidationRule rule = Assert.Single(rules);
            Assert.Equal("'MyPropertyDisplayName' and 'OtherPropertyDisplayName' do not match.", rule.ErrorMessage);
        }

        [Fact]
        [ReplaceCulture]
        public void ClientRulesWithCompareAttribute_ErrorMessageUsesPropertyName()
        {
            // Arrange
            var metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => null, typeof(PropertyNameModel), "MyProperty");
            var context = new ControllerContext();
            var attribute = new AnnotationsCompareAttribute("OtherProperty");
            var adapter = new CompareAttributeAdapter(metadata, context, attribute);

            // Act
            var rules = adapter.GetClientValidationRules()
                .OrderBy(r => r.ValidationType)
                .ToArray();

            // Assert
            ModelClientValidationRule rule = Assert.Single(rules);
            Assert.Equal("'MyProperty' and 'OtherProperty' do not match.", rule.ErrorMessage);
        }

        [Fact]
        public void ClientRulesWithCompareAttribute_ErrorMessageUsesOverride()
        {
            // Arrange
            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => null, typeof(PropertyNameModel), "MyProperty");
            ControllerContext context = new ControllerContext();
            AnnotationsCompareAttribute attribute = new AnnotationsCompareAttribute("OtherProperty")
            {
                ErrorMessage = "Hello '{0}', goodbye '{1}'.",
            };
            ModelValidator adapter = new CompareAttributeAdapter(metadata, context, attribute);

            // Act
            ModelClientValidationRule[] rules = adapter.GetClientValidationRules()
                .OrderBy(r => r.ValidationType)
                .ToArray();

            // Assert
            ModelClientValidationRule rule = Assert.Single(rules);
            Assert.Equal("Hello 'MyProperty', goodbye 'OtherProperty'.", rule.ErrorMessage);
        }

        [Fact]
        public void ClientRulesWithCompareAttribute_ErrorMessageUsesResourceOverride()
        {
            // Arrange
            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => null, typeof(PropertyNameModel), "MyProperty");
            ControllerContext context = new ControllerContext();
            AnnotationsCompareAttribute attribute = new AnnotationsCompareAttribute("OtherProperty")
            {
                ErrorMessageResourceName = "CompareAttributeTestResource",
                ErrorMessageResourceType = typeof(MyResources),
            };
            ModelValidator adapter = new CompareAttributeAdapter(metadata, context, attribute);

            // Act
            ModelClientValidationRule[] rules = adapter.GetClientValidationRules()
                .OrderBy(r => r.ValidationType)
                .ToArray();

            // Assert
            ModelClientValidationRule rule = Assert.Single(rules);
            Assert.Equal("Hello 'MyProperty', goodbye 'OtherProperty'.", rule.ErrorMessage);
        }

        private class PropertyDisplayNameModel
        {
            [DisplayName("MyPropertyDisplayName")]
            public string MyProperty { get; set; }

            [DisplayName("OtherPropertyDisplayName")]
            public string OtherProperty { get; set; }
        }

        private class PropertyNameModel
        {
            public string MyProperty { get; set; }

            public string OtherProperty { get; set; }
        }
    }
}