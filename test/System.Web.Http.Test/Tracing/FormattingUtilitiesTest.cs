// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.ModelBinding.Binders;
using System.Web.Http.Routing;
using System.Web.Http.ValueProviders;
using System.Web.Http.ValueProviders.Providers;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Tracing
{
    public class FormattingUtilitiesTest
    {
        [Theory]
        [TestDataSet(typeof(CommonUnitTestDataSets), "RefTypeTestDataCollection")]
        public void ValueToString_Formats(Type variationType, object testData)
        {
            // Arrange
            GC.KeepAlive(variationType); // Mark parameter as used. See xUnit1026, [Theory] method doesn't use all parameters.
            string expected = Convert.ToString(testData, CultureInfo.CurrentCulture);

            // Act
            string actual = FormattingUtilities.ValueToString(testData, CultureInfo.CurrentCulture);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ValueToString_Formats_Null_Value()
        {
            // Arrange & Act
            string actual = FormattingUtilities.ValueToString(null, CultureInfo.CurrentCulture);

            // Assert
            Assert.Equal("null", actual);
        }

        [Fact]
        public void ActionArgumentsToString_Formats()
        {
            // Arrange
            Dictionary<string, object> arguments = new Dictionary<string, object>()
                                                       {
                                                           {"p1", 1},
                                                           {"p2", true}
                                                       };

            string expected = String.Format("p1={0}, p2={1}",
                                    FormattingUtilities.ValueToString(arguments["p1"], CultureInfo.CurrentCulture),
                                    FormattingUtilities.ValueToString(arguments["p2"], CultureInfo.CurrentCulture));

            // Act
            string actual = FormattingUtilities.ActionArgumentsToString(arguments);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ActionDescriptorToString_Formats()
        {
            // Arrange
            Mock<HttpParameterDescriptor> paramDescriptor1 = new Mock<HttpParameterDescriptor>() { CallBase = true };
            paramDescriptor1.Setup(p => p.ParameterName).Returns("p1");
            paramDescriptor1.Setup(p => p.ParameterType).Returns(typeof(int));
            Mock<HttpParameterDescriptor> paramDescriptor2 = new Mock<HttpParameterDescriptor>() { CallBase = true };
            paramDescriptor2.Setup(p => p.ParameterName).Returns("p2");
            paramDescriptor2.Setup(p => p.ParameterType).Returns(typeof(bool));

            Collection<HttpParameterDescriptor> parameterCollection = new Collection<HttpParameterDescriptor>(
                new HttpParameterDescriptor[] { paramDescriptor1.Object, paramDescriptor2.Object });
            Mock<HttpActionDescriptor> mockActionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };
            mockActionDescriptor.Setup(a => a.GetParameters()).Returns(parameterCollection);
            mockActionDescriptor.Setup(a => a.ActionName).Returns("SampleAction");

            string expected = String.Format("SampleAction({0} p1, {1} p2)", typeof(int).Name, typeof(bool).Name);

            // Act
            string actual = FormattingUtilities.ActionDescriptorToString(mockActionDescriptor.Object);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ActionInvokeToString_Formats()
        {
            // Arrange
            Dictionary<string, object> arguments = new Dictionary<string, object>()
                                                       {
                                                           {"p1", 1},
                                                           {"p2", true}
                                                       };

            string expected = String.Format("SampleAction(p1={0}, p2={1})",
                                    FormattingUtilities.ValueToString(arguments["p1"], CultureInfo.CurrentCulture),
                                    FormattingUtilities.ValueToString(arguments["p2"], CultureInfo.CurrentCulture));

            // Act
            string actual = FormattingUtilities.ActionInvokeToString("SampleAction", arguments);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ActionInvokeToString_With_ActionContext_Formats()
        {
            // Arrange
            Mock<HttpParameterDescriptor> paramDescriptor1 = new Mock<HttpParameterDescriptor>() { CallBase = true };
            paramDescriptor1.Setup(p => p.ParameterName).Returns("p1");
            paramDescriptor1.Setup(p => p.ParameterType).Returns(typeof(int));
            Mock<HttpParameterDescriptor> paramDescriptor2 = new Mock<HttpParameterDescriptor>() { CallBase = true };
            paramDescriptor2.Setup(p => p.ParameterName).Returns("p2");
            paramDescriptor2.Setup(p => p.ParameterType).Returns(typeof(bool));

            Collection<HttpParameterDescriptor> parameterCollection = new Collection<HttpParameterDescriptor>(
                new HttpParameterDescriptor[] { paramDescriptor1.Object, paramDescriptor2.Object });
            Mock<HttpActionDescriptor> mockActionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };
            mockActionDescriptor.Setup(a => a.GetParameters()).Returns(parameterCollection);
            mockActionDescriptor.Setup(a => a.ActionName).Returns("SampleAction");

            HttpActionContext actionContext =
                ContextUtil.CreateActionContext(actionDescriptor: mockActionDescriptor.Object);
            actionContext.ActionArguments["p1"] = 1;
            actionContext.ActionArguments["p2"] = true;

            string expected = String.Format("SampleAction(p1={0}, p2={1})",
                                    FormattingUtilities.ValueToString(actionContext.ActionArguments["p1"], CultureInfo.CurrentCulture),
                                    FormattingUtilities.ValueToString(actionContext.ActionArguments["p2"], CultureInfo.CurrentCulture));

            // Act
            string actual = FormattingUtilities.ActionInvokeToString(actionContext);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FormattersToString_Formats()
        {
            // Arrange
            MediaTypeFormatterCollection formatters = new MediaTypeFormatterCollection();
            string expected = String.Join(", ", formatters.Select<MediaTypeFormatter, string>((f) => f.GetType().Name));

            // Act
            string actual = FormattingUtilities.FormattersToString(formatters);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ModelBinderToString_Formats()
        {
            // Arrange
            ModelBinderProvider provider = new SimpleModelBinderProvider(typeof(int), () => null);
            string expected = typeof(SimpleModelBinderProvider).Name;

            // Act
            string actual = FormattingUtilities.ModelBinderToString(provider);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ModelBinderToString_With_CompositeModelBinder_Formats()
        {
            // Arrange
            ModelBinderProvider innerProvider1 = new SimpleModelBinderProvider(typeof(int), () => null);
            ModelBinderProvider innerProvider2 = new ArrayModelBinderProvider();
            CompositeModelBinderProvider compositeProvider = new CompositeModelBinderProvider(new ModelBinderProvider[] { innerProvider1, innerProvider2 });
            string expected = String.Format(
                                "{0}({1}, {2})",
                                typeof(CompositeModelBinderProvider).Name,
                                typeof(SimpleModelBinderProvider).Name,
                                typeof(ArrayModelBinderProvider).Name);

            // Act
            string actual = FormattingUtilities.ModelBinderToString(compositeProvider);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ValueProviderToString_Formats()
        {
            // Arrange
            IValueProvider provider = new ElementalValueProvider("unused", 1, CultureInfo.CurrentCulture);
            string expected = typeof(ElementalValueProvider).Name;

            // Act
            string actual = FormattingUtilities.ValueProviderToString(provider);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ValueProviderToString_With_CompositeProvider_Formats()
        {
            // Arrange
            List<IValueProvider> providers = new List<IValueProvider>()
                                                 {
                                                    new ElementalValueProvider("unused", 1, CultureInfo.CurrentCulture),
                                                    new NameValuePairsValueProvider(() => null, CultureInfo.CurrentCulture)
                                                 };

            CompositeValueProvider compositeProvider = new CompositeValueProvider(providers);
            string expected = String.Format(
                                "{0}({1}, {2})",
                                typeof(CompositeValueProvider).Name,
                                typeof(ElementalValueProvider).Name,
                                typeof(NameValuePairsValueProvider).Name);

            // Act
            string actual = FormattingUtilities.ValueProviderToString(compositeProvider);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void RouteToString_Formats()
        {
            // Arrange
            Dictionary<string, object> routeDictionary = new Dictionary<string, object>()
                                                             {
                                                                 {"r1", "c1"},
                                                                 {"r2", "c2"}
                                                             };
            Mock<IHttpRouteData> mockRouteData = new Mock<IHttpRouteData>() { CallBase = true };
            mockRouteData.Setup(r => r.Values).Returns(routeDictionary);
            string expected = "r1:c1,r2:c2";

            // Act
            string actual = FormattingUtilities.RouteToString(mockRouteData.Object);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ModelStateToString_Formats_With_Valid_ModelState()
        {
            // Arrange
            ModelStateDictionary modelState = new ModelStateDictionary();
            string expected = String.Empty;

            // Act
            string actual = FormattingUtilities.ModelStateToString(modelState);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ModelStateToString_Formats_With_InValid_ModelState()
        {
            // Arrange
            ModelStateDictionary modelState = new ModelStateDictionary();
            modelState.AddModelError("p1", "is bad");

            string expected = "p1: is bad";

            // Act
            string actual = FormattingUtilities.ModelStateToString(modelState);

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
