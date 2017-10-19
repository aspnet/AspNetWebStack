// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    [Xunit.Collection("Uses ScopeStorage or ViewEngines.Engines")] // Uses ModelMetadataProviders.Current
    public class RangeAttributeAdapterTest
    {
        [Fact]
        [ReplaceCulture]
        public void ClientRulesWithRangeAttribute()
        {
            // Arrange
            var metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => null, typeof(string), "Length");
            var context = new ControllerContext();
            var attribute = new RangeAttribute(typeof(decimal), "0", "100");
            var adapter = new RangeAttributeAdapter(metadata, context, attribute);

            // Act
            var rules = adapter.GetClientValidationRules()
                .OrderBy(r => r.ValidationType)
                .ToArray();

            // Assert
            ModelClientValidationRule rule = Assert.Single(rules);
            Assert.Equal("range", rule.ValidationType);
            Assert.Equal(2, rule.ValidationParameters.Count);
            Assert.Equal(0m, rule.ValidationParameters["min"]);
            Assert.Equal(100m, rule.ValidationParameters["max"]);
            Assert.Equal(@"The field Length must be between 0 and 100.", rule.ErrorMessage);
        }
    }
}
