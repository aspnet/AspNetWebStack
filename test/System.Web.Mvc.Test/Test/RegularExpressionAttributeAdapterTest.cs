// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    [Xunit.Collection("Uses ScopeStorage or ViewEngines.Engines")] // Uses ModelMetadataProviders.Current
    public class RegularExpressionAttributeAdapterTest
    {
        [Fact]
        [ReplaceCulture]
        public void ClientRulesWithRegexAttribute()
        {
            // Arrange
            var metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => null, typeof(string), "Length");
            var context = new ControllerContext();
            var attribute = new RegularExpressionAttribute("the_pattern");
            var adapter = new RegularExpressionAttributeAdapter(metadata, context, attribute);

            // Act
            var rules = adapter.GetClientValidationRules()
                .OrderBy(r => r.ValidationType)
                .ToArray();

            // Assert
            ModelClientValidationRule rule = Assert.Single(rules);
            Assert.Equal("regex", rule.ValidationType);
            Assert.Single(rule.ValidationParameters);
            Assert.Equal("the_pattern", rule.ValidationParameters["pattern"]);
            Assert.Equal(@"The field Length must match the regular expression 'the_pattern'.", rule.ErrorMessage);
        }
    }
}
