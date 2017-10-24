// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.TestUtil;
using System.Web.UI;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class OutputCacheAttributeTest
    {
        [Fact]
        public void CacheProfileProperty()
        {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute();

            // Act & assert
            MemberHelper.TestStringProperty(attr, "CacheProfile", String.Empty);
        }

        [Fact]
        public void CacheSettingsProperty()
        {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute()
            {
                CacheProfile = "SomeProfile",
                Duration = 50,
                Location = OutputCacheLocation.Downstream,
                NoStore = true,
                SqlDependency = "SomeSqlDependency",
                VaryByContentEncoding = "SomeContentEncoding",
                VaryByCustom = "SomeCustom",
                VaryByHeader = "SomeHeader",
                VaryByParam = "SomeParam",
            };

            // Act
            OutputCacheParameters cacheSettings = attr.CacheSettings;

            // Assert
            Assert.Equal("SomeProfile", cacheSettings.CacheProfile);
            Assert.Equal(50, cacheSettings.Duration);
            Assert.Equal(OutputCacheLocation.Downstream, cacheSettings.Location);
            Assert.True(cacheSettings.NoStore);
            Assert.Equal("SomeSqlDependency", cacheSettings.SqlDependency);
            Assert.Equal("SomeContentEncoding", cacheSettings.VaryByContentEncoding);
            Assert.Equal("SomeCustom", cacheSettings.VaryByCustom);
            Assert.Equal("SomeHeader", cacheSettings.VaryByHeader);
            Assert.Equal("SomeParam", cacheSettings.VaryByParam);
        }

        [Fact]
        public void DurationProperty()
        {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute();

            // Act & assert
            MemberHelper.TestInt32Property(attr, "Duration", 10, 20);
        }

        [Fact]
        public void LocationProperty()
        {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute();

            // Act & assert
            MemberHelper.TestPropertyValue(attr, "Location", OutputCacheLocation.ServerAndClient);
        }

        [Fact]
        public void NoStoreProperty()
        {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute();

            // Act & assert
            MemberHelper.TestBooleanProperty(attr, "NoStore", false /* initialValue */, false /* testDefaultValue */);
        }

        [Fact]
        public void OnResultExecutingThrowsIfFilterContextIsNull()
        {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute();

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { attr.OnResultExecuting(null); }, "filterContext");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void OnActionExecuting_Throws_IfDurationIsNotPositive(int duration)
        {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute();
            attr.Duration = duration;
            Mock<ActionExecutingContext> context = new Mock<ActionExecutingContext>();
            context.Setup(c => c.IsChildAction).Returns(true);

            // Act & assert
            Assert.Throws<InvalidOperationException>(
                () => attr.OnActionExecuting(context.Object),
                "Duration must be a positive number.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void OnActionExecuting_Throws_IfVaryByParamIsNullOrEmptyAndDurationIsPositive(string varyByParam)
        {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute();
            attr.Duration = 1;
            attr.VaryByParam = varyByParam;
            Mock<ActionExecutingContext> context = new Mock<ActionExecutingContext>();
            context.Setup(c => c.IsChildAction).Returns(true);

            // Act & assert
            Assert.Throws<InvalidOperationException>(
                () => attr.OnActionExecuting(context.Object),
                "VaryByParam must be '*', 'none', or a semicolon-delimited list of keys.");
        }

        [Fact]
        public void OnActionExecuting_Throws_IfCacheProfileSet()
        {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute();
            attr.CacheProfile = "something";
            Mock<ActionExecutingContext> context = new Mock<ActionExecutingContext>();
            context.Setup(c => c.IsChildAction).Returns(true);

            // Act & assert
            Assert.Throws<InvalidOperationException>(
                () => attr.OnActionExecuting(context.Object),
                "OutputCacheAttribute for child actions only supports Duration, VaryByCustom, and VaryByParam values. Please do not set CacheProfile, Location, NoStore, SqlDependency, VaryByContentEncoding, or VaryByHeader values for child actions.");
        }

        [Fact]
        public void SqlDependencyProperty()
        {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute();

            // Act & assert
            MemberHelper.TestStringProperty(attr, "SqlDependency", String.Empty);
        }

        [Fact]
        public void VaryByContentEncodingProperty()
        {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute();

            // Act & assert
            MemberHelper.TestStringProperty(attr, "VaryByContentEncoding", String.Empty);
        }

        [Fact]
        public void VaryByCustomProperty()
        {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute();

            // Act & assert
            MemberHelper.TestStringProperty(attr, "VaryByCustom", String.Empty);
        }

        [Fact]
        public void VaryByHeaderProperty()
        {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute();

            // Act & assert
            MemberHelper.TestStringProperty(attr, "VaryByHeader", String.Empty);
        }

        [Fact]
        public void VaryByParamProperty()
        {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute();

            // Act & assert
            MemberHelper.TestStringProperty(attr, "VaryByParam", "*");
        }

        [Fact]
        public void OutputCacheDoesNotExecuteIfInChildAction()
        {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute();
            Mock<ResultExecutingContext> context = new Mock<ResultExecutingContext>();
            context.Setup(c => c.IsChildAction).Returns(true);

            // Act
            attr.OnResultExecuting(context.Object);

            // Assert
            context.Verify();
            context.Verify(c => c.Result, Times.Never());
        }

        [Fact]
        public void OutputCacheDurationDoesNotSetIfInChildAction()
        {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute();
            Mock<ActionExecutingContext> context = new Mock<ActionExecutingContext>();
            context.Setup(c => c.IsChildAction).Returns(true);

            // Act & assert
            Assert.Throws<InvalidOperationException>(delegate { attr.OnActionExecuting(context.Object); });
        }

        [Fact]
        public void OutputCacheLocationSetNoneIfInChildAction()
        {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute()
            {
                Location = OutputCacheLocation.None
            };
            Mock<ActionExecutingContext> context = new Mock<ActionExecutingContext>();
            context.Setup(c => c.IsChildAction).Returns(true);

            // Act & assert
            Assert.DoesNotThrow(delegate { attr.OnActionExecuting(context.Object); });
        }

        // GetChildActionUniqueId

        [Fact]
        public void GetChildActionUniqueId_ReturnsRepeatableValueForIdenticalContext()
        {
            // Arrange
            var attr = new OutputCacheAttribute();
            var context = new MockActionExecutingContext();

            // Act
            string result1 = attr.GetChildActionUniqueId(context.Object);
            string result2 = attr.GetChildActionUniqueId(context.Object);

            // Assert
            Assert.Equal(result1, result2);
        }

        [Fact]
        public void GetChildActionUniqueId_VariesByActionDescriptorsUniqueId()
        {
            // Arrange
            var attr = new OutputCacheAttribute();
            var context1 = new MockActionExecutingContext();
            context1.Setup(c => c.ActionDescriptor.UniqueId).Returns("1");
            var context2 = new MockActionExecutingContext();
            context2.Setup(c => c.ActionDescriptor.UniqueId).Returns("2");

            // Act
            string result1 = attr.GetChildActionUniqueId(context1.Object);
            string result2 = attr.GetChildActionUniqueId(context2.Object);

            // Assert
            Assert.NotEqual(result1, result2);
        }

        [Fact]
        public void GetChildActionUniqueId_VariesByCustom()
        {
            // Arrange
            var attr = new OutputCacheAttribute { VaryByCustom = "foo" };
            var application1 = new Mock<HttpApplication>();
            application1.Setup(a => a.GetVaryByCustomString(It.IsAny<HttpContext>(), "foo")).Returns("1");
            var context1 = new MockActionExecutingContext();
            context1.SetupGet(c => c.HttpContext.ApplicationInstance).Returns(application1.Object);

            var application2 = new Mock<HttpApplication>();
            application2.Setup(a => a.GetVaryByCustomString(It.IsAny<HttpContext>(), "foo")).Returns("2");
            var context2 = new MockActionExecutingContext();
            context2.SetupGet(c => c.HttpContext.ApplicationInstance).Returns(application2.Object);

            // Act
            string result1 = attr.GetChildActionUniqueId(context1.Object);
            string result2 = attr.GetChildActionUniqueId(context2.Object);

            // Assert
            Assert.Equal(@"VjXrM/nTu6zOLCi+teZcx7qDQRk/Q+G5ZirKHhH7MOA=", result1);
            Assert.Equal(@"Wi7TLgf052Ao0ZJX890MgynId6jByOf+xZ1G+5RHJUU=", result2);
        }

        [Fact]
        public void GetChildActionUniqueId_VariesByActionParameters_AllParametersByDefault()
        {
            // Arrange
            var attr = new OutputCacheAttribute();
            var context1 = new MockActionExecutingContext();
            context1.ActionParameters["foo"] = "1";
            var context2 = new MockActionExecutingContext();
            context2.ActionParameters["foo"] = "2";

            // Act
            string result1 = attr.GetChildActionUniqueId(context1.Object);
            string result2 = attr.GetChildActionUniqueId(context2.Object);

            // Assert
            Assert.Equal("tXBiB5qdEzfISoXUUqXAzPcd7y/9Ik6eFqy8w9i3aDw=", result1);
            Assert.Equal("Bb+E/t8aWfczVF2UFVr+FUpuGYjR/SPhzt71q+oAlHk=", result2);
        }

        [Fact]
        public void GetChildActionUniqueId_DoesNotVaryByActionParametersWhenVaryByParamIsNone()
        {
            // Arrange
            var attr = new OutputCacheAttribute { VaryByParam = "none" };
            var context1 = new MockActionExecutingContext();
            context1.ActionParameters["foo"] = "1";
            var context2 = new MockActionExecutingContext();
            context2.ActionParameters["foo"] = "2";

            // Act
            string result1 = attr.GetChildActionUniqueId(context1.Object);
            string result2 = attr.GetChildActionUniqueId(context2.Object);

            // Assert
            Assert.Equal(result1, result2);
            Assert.Equal("IA6+zemlkqp8Ye59mwMEMYGo69mUVkAcFLa5z0keL50=", result1);
        }

        [Fact]
        public void GetChildActionUniqueId_VariesByActionParameters_OnlyVariesByTheGivenParameters()
        {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute { VaryByParam = "bar" };
            MockActionExecutingContext context1 = new MockActionExecutingContext();
            context1.ActionParameters["foo"] = "1";
            MockActionExecutingContext context2 = new MockActionExecutingContext();
            context2.ActionParameters["foo"] = "2";

            // Act
            string result1 = attr.GetChildActionUniqueId(context1.Object);
            string result2 = attr.GetChildActionUniqueId(context2.Object);

            // Assert
            Assert.Equal(result1, result2);
            Assert.Equal("sx9+LHPOjtSGct8z6Cn4ml+2yKODPojZtrLhrhJofKM=", result1);
        }

        [Fact]
        public void GetChildActionUniqueId_VariesByActionParameters_OnlyVariesByTheGivenParameters_SingleSpecified_Different()
        {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute { VaryByParam = "foo" };
            MockActionExecutingContext context1 = new MockActionExecutingContext();
            context1.ActionParameters["foo"] = "1";
            MockActionExecutingContext context2 = new MockActionExecutingContext();
            context2.ActionParameters["foo"] = "2";

            // Act
            string result1 = attr.GetChildActionUniqueId(context1.Object);
            string result2 = attr.GetChildActionUniqueId(context2.Object);

            // Assert
            Assert.Equal("tXBiB5qdEzfISoXUUqXAzPcd7y/9Ik6eFqy8w9i3aDw=", result1);
            Assert.Equal("Bb+E/t8aWfczVF2UFVr+FUpuGYjR/SPhzt71q+oAlHk=", result2);
        }

        [Fact]
        public void GetChildActionUniqueId_VariesByActionParameters_OnlyVariesByTheGivenParameters_SingleSpecified_Same()
        {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute { VaryByParam = "foo" };
            MockActionExecutingContext context1 = new MockActionExecutingContext();
            context1.ActionParameters["foo"] = "1";
            MockActionExecutingContext context2 = new MockActionExecutingContext();
            context2.ActionParameters["foo"] = "1";

            // Act
            string result1 = attr.GetChildActionUniqueId(context1.Object);
            string result2 = attr.GetChildActionUniqueId(context2.Object);

            // Assert
            Assert.Equal("tXBiB5qdEzfISoXUUqXAzPcd7y/9Ik6eFqy8w9i3aDw=", result1);
            Assert.Equal("tXBiB5qdEzfISoXUUqXAzPcd7y/9Ik6eFqy8w9i3aDw=", result2);
        }

        [Fact]
        public void GetChildActionUniqueId_VariesByActionParameters_OnlyVariesByTheGivenParameters_MultipleSpecified_Different()
        {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute { VaryByParam = "foo;bar;blap" };
            MockActionExecutingContext context1 = new MockActionExecutingContext();
            context1.ActionParameters["foo"] = "1";
            context1.ActionParameters["bar"] = "2";
            MockActionExecutingContext context2 = new MockActionExecutingContext();
            context2.ActionParameters["foo"] = "1";
            context2.ActionParameters["bar"] = "3";

            // Act
            string result1 = attr.GetChildActionUniqueId(context1.Object);
            string result2 = attr.GetChildActionUniqueId(context2.Object);

            // Assert
            Assert.NotEqual(result1, result2);
        }

        public static TheoryDataSet<ActionExecutingContext, string, string>
            GetUniqueIdFromActionParameters_ReturnsValuesThatExistInFilterContextData
        {
            get
            {
                TheoryDataSet<ActionExecutingContext, string, string> theoryData =
                    new TheoryDataSet<ActionExecutingContext, string, string>();

                MockActionExecutingContext context1 = new MockActionExecutingContext();
                context1.ActionParameters["foo"] = "foo-value";
                context1.ActionParameters["bar"] = "bar-value";
                context1.ActionParameters["blap"] = "blap-value";

                string expected1 = "[3]FOO[9]foo-value[3]BAR[9]bar-value[4]BLAP[10]blap-value";

                theoryData.Add(context1.Object, "foo;bar;blap", expected1);
                theoryData.Add(context1.Object, "foo; bar;  blap;", expected1);
                theoryData.Add(context1.Object, ";foo;   bar; blap  ", expected1);
                theoryData.Add(context1.Object, "foo; BAR;Blap", expected1);

                MockActionExecutingContext context2 = new MockActionExecutingContext();
                context2.ActionParameters["test"] = "test-value";
                context2.ActionParameters["shouldnot-exist"] = "shouldnot-exist";
                context2.ActionParameters["test2"] = "test2-value";

                theoryData.Add(context2.Object, "test;test2", "[4]TEST[10]test-value[5]TEST2[11]test2-value");
                theoryData.Add(context2.Object, "test;test3", "[4]TEST[10]test-value[5]TEST3[-1]");

                MockActionExecutingContext context3 = new MockActionExecutingContext();
                theoryData.Add(context3.Object, "test1;test2", "[5]TEST1[-1][5]TEST2[-1]");

                return theoryData;
            }
        }

        [Theory]
        [PropertyData("GetUniqueIdFromActionParameters_ReturnsValuesThatExistInFilterContextData")]
        public static void BuildUniqueIdFromActionParameters_UsesValuesForActionParametersThatExistInVaryByParams(
            ActionExecutingContext context, string varyByParam, string expected)
        {
            // Arrange
            StringBuilder builder = new StringBuilder();
            OutputCacheAttribute attribute = new OutputCacheAttribute { VaryByParam = varyByParam };

            // Act
            attribute.BuildUniqueIdFromActionParameters(builder, context);

            // Assert
            Assert.Equal(expected, builder.ToString());
        }

        [Theory]
        [InlineData("Foo;bar")]
        [InlineData("FOO;bAr")]
        [InlineData("foo;bar")]
        public static void BuildUniqueIdFromActionParameters_LooksUpActionParametersInCaseInsensitiveManner(
            string varyByParam)
        {
            // Arrange
            MockActionExecutingContext context = new MockActionExecutingContext();
            Dictionary<string, object> dictionary = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                { "foo",  "foo-value" },
                { "Bar", "bar-value" }
            };
            context.ActionParameters = dictionary;
            string expected = "[3]FOO[9]foo-value[3]BAR[9]bar-value";

            StringBuilder builder = new StringBuilder();
            OutputCacheAttribute attribute = new OutputCacheAttribute { VaryByParam = varyByParam };

            // Act
            attribute.BuildUniqueIdFromActionParameters(builder, context.Object);

            // Assert
            Assert.Equal(expected, builder.ToString());
        }

        [Theory]
        [InlineData("none")]
        [InlineData("NONE")]
        [InlineData("None")]
        public static void BuildUniqueIdFromActionParameters_NoOpsIfVaryByParamIsNone(string varyByParam)
        {
            // Arrange
            StringBuilder builder = new StringBuilder();
            ActionExecutingContext context = new MockActionExecutingContext().Object;
            OutputCacheAttribute attribute = new OutputCacheAttribute { VaryByParam = varyByParam };

            // Act
            attribute.BuildUniqueIdFromActionParameters(builder, context);

            // Assert
            Assert.Empty(builder.ToString());
        }

        [Fact]
        public static void BuildUniqueIdFromActionParameters_UsesAllActionParametersIfStar()
        {
            // Arrange
            OutputCacheAttribute attribute = new OutputCacheAttribute { VaryByParam = "*" };
            StringBuilder builder1 = new StringBuilder();
            MockActionExecutingContext context1 = new MockActionExecutingContext();
            context1.ActionParameters["test1"] = "test1-value";
            context1.ActionParameters["test2"] = 3;
            context1.ActionParameters["test3"] = null;
            string expected = "[5]TEST1[11]test1-value[5]TEST2[1]3[5]TEST3[-1]";

            // Act - 1
            attribute.BuildUniqueIdFromActionParameters(builder1, context1.Object);

            // Assert - 1
            Assert.Equal(expected, builder1.ToString());

            // Arrange - 2
            // Verify that the keys are stable sorted when "*" is used.
            StringBuilder builder2 = new StringBuilder();
            MockActionExecutingContext context2 = new MockActionExecutingContext();
            context2.ActionParameters["test3"] = null;
            context2.ActionParameters["test2"] = 3;
            context2.ActionParameters["test1"] = "test1-value";

            // Act - 2
            attribute.BuildUniqueIdFromActionParameters(builder2, context2.Object);

            // Assert - 2
            Assert.Equal(expected, builder2.ToString());
        }

        [Fact]
        public static void BuildUniqueIdFromActionParameters_DoesNotCacheTokenizedStringsWhenStar()
        {
            // Arrange
            OutputCacheAttribute attribute = new OutputCacheAttribute { VaryByParam = "*" };
            StringBuilder builder1 = new StringBuilder();
            MockActionExecutingContext context1 = new MockActionExecutingContext();
            context1.ActionParameters["test1"] = "test1-value";
            context1.ActionParameters["test2"] = "Test2-Value";
            context1.ActionParameters["test3"] = null;
            string expected1 = "[5]TEST1[11]test1-value[5]TEST2[11]Test2-Value[5]TEST3[-1]";

            // Act - 1
            attribute.BuildUniqueIdFromActionParameters(builder1, context1.Object);

            // Assert - 1
            Assert.Equal(expected1, builder1.ToString());

            // Arrange - 2
            // Verify that the keys are stable sorted when "*" is used.
            StringBuilder builder2 = new StringBuilder();
            MockActionExecutingContext context2 = new MockActionExecutingContext();
            context2.ActionParameters["Foo"] = null;
            context2.ActionParameters["bar"] = "bar-value";
            string expected2 = "[3]BAR[9]bar-value[3]FOO[-1]";

            // Act - 2
            attribute.BuildUniqueIdFromActionParameters(builder2, context2.Object);

            // Assert - 2
            Assert.Equal(expected2, builder2.ToString());
        }

        public static IEnumerable<string> VaryByParamWithWhitespaceData = new[]
        {
            "foo;bar;blap",
            "foo; bar; blap",
            ";foo; bar; blap",
            "  foo;   bar;  blap",
            "  foo;bar;  blap; ",
            "  ;foo;   bar  ;  blap; ",
        };

        public static IEnumerable<object[]> GetChildActionUniqueId_VariesByActionParametersData
        {
            get
            {
                return VaryByParamWithWhitespaceData.Select(v => new[] { v });
            }
        }

        [Theory]
        [PropertyData("GetChildActionUniqueId_VariesByActionParametersData")]
        public void GetChildActionUniqueId_VariesByActionParameters_OnlyVariesByGivenParameters(string varbyParam)
        {
            // Arrange
            string expected = "z2Fr6HAipKCkLdVkHMdHgeDBJyYutdqqTW07BMO31fQ=";
            OutputCacheAttribute attr = new OutputCacheAttribute { VaryByParam = varbyParam };
            MockActionExecutingContext context1 = new MockActionExecutingContext();
            context1.ActionParameters["foo"] = "1";
            context1.ActionParameters["bar"] = "2";
            context1.ActionParameters["blap"] = "3";
            context1.ActionParameters["xyz"] = Guid.NewGuid().ToString();
            context1.ActionParameters["some-key"] = Guid.NewGuid().ToString();
            MockActionExecutingContext context2 = new MockActionExecutingContext();
            context2.ActionParameters["foo"] = "1";
            context2.ActionParameters["bar"] = "2";
            context2.ActionParameters["blap"] = "3";
            context2.ActionParameters["xyz"] = Guid.NewGuid().ToString();
            context2.ActionParameters["different-key"] = Guid.NewGuid().ToString();

            // Act
            string result1 = attr.GetChildActionUniqueId(context1.Object);
            string result2 = attr.GetChildActionUniqueId(context2.Object);

            // Assert
            Assert.Equal(expected, result1);
            Assert.Equal(expected, result2);
        }

        public static IEnumerable<object[]> GetChildActionUniqueId_VariesByActionParameters_WithDifferentValuesData
        {
            get
            {
                return VaryByParamWithWhitespaceData.Select(v => new[] { v, "20", "34" });
            }
        }

        [Theory]
        [PropertyData("GetChildActionUniqueId_VariesByActionParameters_WithDifferentValuesData")]
        public void GetChildActionUniqueId_VariesByActionParameters_OnlyVariesByTheGivenParameters_MultipleSpecified_WithDifferentValues(
            string varyByParam, string value1, string value2)
        {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute { VaryByParam = varyByParam };
            MockActionExecutingContext context1 = new MockActionExecutingContext();
            context1.ActionParameters["foo"] = value1;
            context1.ActionParameters["bar"] = "2";
            context1.ActionParameters["blap"] = "3";
            context1.ActionParameters["xyz"] = "abc";
            MockActionExecutingContext context2 = new MockActionExecutingContext();
            context2.ActionParameters["foo"] = value2;
            context2.ActionParameters["bar"] = "2";
            context2.ActionParameters["blap"] = "3";
            context2.ActionParameters["xyz"] = "abc";

            // Act
            string result1 = attr.GetChildActionUniqueId(context1.Object);
            string result2 = attr.GetChildActionUniqueId(context2.Object);

            // Assert
            Assert.Equal("WBXQR5lxerJG33HR3XQ9sJsXuKu6eVPgmMqeWMu8t2s=", result1);
            Assert.Equal("CLMC7fAU4959ECaI79HRFglt7SnNRANMJeYderjF4m8=", result2);
        }

        [Theory]
        [PropertyData("GetChildActionUniqueId_VariesByActionParameters_WithDifferentValuesData")]
        public void GetChildActionUniqueId_VariesByActionParameters_OnlyVariesByTheGivenParameters_MultipleSpecified_Whitespace_DifferentSecond(
            string varyByParam, string value1, string value2)
        {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute { VaryByParam = varyByParam };
            MockActionExecutingContext context1 = new MockActionExecutingContext();
            context1.ActionParameters["foo"] = "1";
            context1.ActionParameters["bar"] = value1;
            context1.ActionParameters["blap"] = "3";
            context1.ActionParameters["xyz"] = "abc";
            MockActionExecutingContext context2 = new MockActionExecutingContext();
            context2.ActionParameters["foo"] = "1";
            context2.ActionParameters["bar"] = value2;
            context2.ActionParameters["blap"] = "3";
            context2.ActionParameters["xyz"] = "abc";

            // Act
            string result1 = attr.GetChildActionUniqueId(context1.Object);
            string result2 = attr.GetChildActionUniqueId(context2.Object);

            // Assert
            Assert.NotEqual(result1, result2);
        }

        [Theory]
        [PropertyData("GetChildActionUniqueId_VariesByActionParameters_WithDifferentValuesData")]
        public void GetChildActionUniqueId_VariesByActionParameters_OnlyVariesByTheGivenParameters_MultipleSpecified_Whitespace_DifferentThird(
            string varyByParam, string value1, string value2)
        {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute { VaryByParam = varyByParam };
            MockActionExecutingContext context1 = new MockActionExecutingContext();
            context1.ActionParameters["foo"] = "1";
            context1.ActionParameters["bar"] = "2";
            context1.ActionParameters["blap"] = value1;
            context1.ActionParameters["xyz"] = "abc";
            MockActionExecutingContext context2 = new MockActionExecutingContext();
            context2.ActionParameters["foo"] = "1";
            context2.ActionParameters["bar"] = "2";
            context2.ActionParameters["blap"] = value2;
            context2.ActionParameters["xyz"] = "abc";

            // Act
            string result1 = attr.GetChildActionUniqueId(context1.Object);
            string result2 = attr.GetChildActionUniqueId(context2.Object);

            // Assert
            Assert.NotEqual(result1, result2);
        }

        [Fact]
        public void GetChildActionUniqueId_VariesByActionParameters_MatchesActionParametersInCaseInsensitiveManner()
        {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute { VaryByParam = "foo ; bar ; blap" };
            MockActionExecutingContext context1 = new MockActionExecutingContext();
            context1.ActionParameters["foo"] = "1";
            context1.ActionParameters["BAR"] = "2";
            context1.ActionParameters["blap"] = "3";
            context1.ActionParameters["xyz"] = "abc";
            MockActionExecutingContext context2 = new MockActionExecutingContext();
            context2.ActionParameters["foo"] = "1";
            context2.ActionParameters["bar"] = "2";
            context2.ActionParameters["BLAP"] = "3";
            context2.ActionParameters["xyz"] = "abc";

            // Act
            string result1 = attr.GetChildActionUniqueId(context1.Object);
            string result2 = attr.GetChildActionUniqueId(context2.Object);

            // Assert
            Assert.Equal(result1, result2);
        }

        [Fact]
        public void GetChildActionUniqueId_VariesByActionParameters_OnlyVariesByTheGivenParameters_Wildcard_Same()
        {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute { VaryByParam = "*" };
            MockActionExecutingContext context1 = new MockActionExecutingContext();
            context1.ActionParameters["foo"] = "1";
            context1.ActionParameters["bar"] = "2";
            context1.ActionParameters["blap"] = "3";
            context1.ActionParameters["xyz"] = "abc";
            MockActionExecutingContext context2 = new MockActionExecutingContext();
            context2.ActionParameters["foo"] = "1";
            context2.ActionParameters["bar"] = "2";
            context2.ActionParameters["blap"] = "3";
            context2.ActionParameters["xyz"] = "abc";

            // Act
            string result1 = attr.GetChildActionUniqueId(context1.Object);
            string result2 = attr.GetChildActionUniqueId(context2.Object);

            // Assert
            Assert.Equal(result1, result2);
        }

        [Fact]
        public void GetChildActionUniqueId_VariesByActionParameters_OnlyVariesByTheGivenParameters_Wildcard_CaseOnly_Same()
        {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute { VaryByParam = "*" };
            MockActionExecutingContext context1 = new MockActionExecutingContext();
            context1.ActionParameters["foo"] = "1";
            context1.ActionParameters["bar"] = "2";
            context1.ActionParameters["BLAP"] = "3";
            context1.ActionParameters["xyz"] = "abc";
            MockActionExecutingContext context2 = new MockActionExecutingContext();
            context2.ActionParameters["FOO"] = "1";
            context2.ActionParameters["bar"] = "2";
            context2.ActionParameters["blap"] = "3";
            context2.ActionParameters["xyz"] = "abc";

            // Act
            string result1 = attr.GetChildActionUniqueId(context1.Object);
            string result2 = attr.GetChildActionUniqueId(context2.Object);

            // Assert
            Assert.Equal(result1, result2);
        }

        [Fact]
        public void GetChildActionUniqueId_VariesByActionParameters_OnlyVariesByTheGivenParameters_Wildcard_Different()
        {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute { VaryByParam = "*" };
            MockActionExecutingContext context1 = new MockActionExecutingContext();
            context1.ActionParameters["foo"] = "1";
            context1.ActionParameters["bar"] = "2";
            context1.ActionParameters["blap"] = "3";
            context1.ActionParameters["xyz"] = "abc";
            MockActionExecutingContext context2 = new MockActionExecutingContext();
            context2.ActionParameters["foo"] = "1";
            context2.ActionParameters["bar"] = "4";
            context2.ActionParameters["blap"] = "3";
            context2.ActionParameters["xyz"] = "abc";

            // Act
            string result1 = attr.GetChildActionUniqueId(context1.Object);
            string result2 = attr.GetChildActionUniqueId(context2.Object);

            // Assert
            Assert.NotEqual(result1, result2);
        }

        [Fact]
        public void GetChildActionUniqueId_VariesByActionParameters_OnlyVariesByTheGivenParameters_None()
        {
            // Arrange
            OutputCacheAttribute attr = new OutputCacheAttribute { VaryByParam = "none" };
            MockActionExecutingContext context1 = new MockActionExecutingContext();
            context1.ActionParameters["foo"] = "1";
            MockActionExecutingContext context2 = new MockActionExecutingContext();
            context2.ActionParameters["foo"] = "2";

            // Act
            string result1 = attr.GetChildActionUniqueId(context1.Object);
            string result2 = attr.GetChildActionUniqueId(context2.Object);

            // Assert
            Assert.Equal(result1, result2);
        }

        [Theory]
        [InlineData("bar", "VmmIPvA0bdX40A9EDzcQdDGenn9mW2fLitLhN3Q+q0o=")]
        [InlineData("*", "lRpCzLPVjS7Lwbix/vbtbdUWELWQOxiJaVemFCgM0ew=")]
        [InlineData("none", "IA6+zemlkqp8Ye59mwMEMYGo69mUVkAcFLa5z0keL50=")]
        public void GetChildActionUniqueId_ReturnsDifferentValuesIfVaryByParamValueIsModified(string varyByParam, string expected)
        {
            // Arrange
            MockActionExecutingContext context = new MockActionExecutingContext();
            context.ActionParameters["foo"] = "1";
            context.ActionParameters["bar"] = "2";

            OutputCacheAttribute attr = new OutputCacheAttribute { VaryByParam = "foo" };

            // Act - 1
            string result1 = attr.GetChildActionUniqueId(context.Object);

            // Assert - 1
            Assert.Equal("tXBiB5qdEzfISoXUUqXAzPcd7y/9Ik6eFqy8w9i3aDw=", result1);

            // Act - 2
            attr.VaryByParam = varyByParam;
            string result2 = attr.GetChildActionUniqueId(context.Object);

            // Assert - 2
            Assert.Equal(expected, result2);
        }

        class MockActionExecutingContext : Mock<ActionExecutingContext>
        {
            // StringComparer.OrdinalIgnoreCase matches the behavior of ControllerActionInvoker.
            public Dictionary<string, object> ActionParameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            public MockActionExecutingContext()
            {
                Setup(c => c.ActionDescriptor.UniqueId).Returns("abc123");
                Setup(c => c.ActionParameters).Returns(() => ActionParameters);
            }
        }
    }
}
