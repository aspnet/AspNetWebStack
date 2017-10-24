// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using Microsoft.TestCommon;

namespace System.Web.WebPages.Test
{
    public class HtmlAttributePropertyHelperTest
    {
        [Fact]
        public void HtmlAttributePropertyHelperRenamesPropertyNames()
        {
            // Arrange
            var anonymous = new { bar_baz = "foo" };
            PropertyInfo property = anonymous.GetType().GetProperties().First();

            // Act
            HtmlAttributePropertyHelper helper = new HtmlAttributePropertyHelper(property);

            // Assert
            Assert.Equal("bar_baz", property.Name);
            Assert.Equal("bar-baz", helper.Name);
        }

        [Fact]
        public void HtmlAttributePropertyHelperReturnsNameCorrectly()
        {
            // Arrange
            var anonymous = new { foo = "bar" };
            PropertyInfo property = anonymous.GetType().GetProperties().First();

            // Act
            HtmlAttributePropertyHelper helper = new HtmlAttributePropertyHelper(property);

            // Assert
            Assert.Equal("foo", property.Name);
            Assert.Equal("foo", helper.Name);
        }

        [Fact]
        public void HtmlAttributePropertyHelperReturnsValueCorrectly()
        {
            // Arrange
            var anonymous = new { bar = "baz" };
            PropertyInfo property = anonymous.GetType().GetProperties().First();

            // Act
            HtmlAttributePropertyHelper helper = new HtmlAttributePropertyHelper(property);

            // Assert
            Assert.Equal("bar", helper.Name);
            Assert.Equal("baz", helper.GetValue(anonymous));
        }

        [Fact]
        public void HtmlAttributePropertyHelperReturnsValueCorrectlyForValueTypes()
        {
            // Arrange
            var anonymous = new { foo = 32 };
            PropertyInfo property = anonymous.GetType().GetProperties().First();

            // Act
            HtmlAttributePropertyHelper helper = new HtmlAttributePropertyHelper(property);

            // Assert
            Assert.Equal("foo", helper.Name);
            Assert.Equal(32, helper.GetValue(anonymous));
        }

        [Fact]
        public void HtmlAttributePropertyHelperReturnsCachedPropertyHelper()
        {
            // Arrange
            var anonymous = new { foo = "bar" };

            // Act
            PropertyHelper[] helpers1 = HtmlAttributePropertyHelper.GetProperties(anonymous);
            PropertyHelper[] helpers2 = HtmlAttributePropertyHelper.GetProperties(anonymous);

            // Assert
            Assert.Single(helpers1);
            Assert.ReferenceEquals(helpers1, helpers2);
            Assert.ReferenceEquals(helpers1[0], helpers2[0]);
        }

        [Fact]
        public void HtmlAttributeDoesNotShareCacheWithPropertyHelper()
        {
            // Arrange
            var anonymous = new { bar_baz1 = "foo" };

            // Act
            PropertyHelper[] helpers1 = HtmlAttributePropertyHelper.GetProperties(anonymous);
            PropertyHelper[] helpers2 = PropertyHelper.GetProperties(anonymous);

            // Assert
            PropertyHelper helper1 = Assert.Single(helpers1);
            PropertyHelper helper2 = Assert.Single(helpers2);

            Assert.NotEqual(helpers1, helpers2);
            Assert.NotEqual(helper1, helper2);

            Assert.IsType<HtmlAttributePropertyHelper>(helper1);
            Assert.IsNotType<HtmlAttributePropertyHelper>(helper2);

            Assert.Equal("bar-baz1", helper1.Name);
            Assert.Equal("bar_baz1", helper2.Name);
        }
    }
}
