// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    [Xunit.Collection("Uses ScopeStorage or ViewEngines.Engines")] // Uses ModelMetadataProviders.Current
    public class RequiredAttributeAdapterTest
    {
        [Fact]
        [ReplaceCulture]
        public void ClientRulesWithRequiredAttribute()
        {
            // Arrange
            var metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => null, typeof(string), "Length");
            var context = new ControllerContext();
            var attribute = new RequiredAttribute();
            var adapter = new RequiredAttributeAdapter(metadata, context, attribute);

            // Act
            var rules = adapter.GetClientValidationRules()
                .OrderBy(r => r.ValidationType)
                .ToArray();

            // Assert
            ModelClientValidationRule rule = Assert.Single(rules);
            Assert.Equal("required", rule.ValidationType);
            Assert.Empty(rule.ValidationParameters);
            Assert.Equal(@"The Length field is required.", rule.ErrorMessage);
        }
    }
}
