// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Web.Mvc.Filters;
using System.Web.Routing;
using System.Web.Routing.Test;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;
using Moq;

namespace System.Web.Mvc.Test
{
    [Xunit.Collection("Uses ScopeStorage or ViewEngines.Engines")] // Uses ModelMetadataProviders.Current
    public class ControllerActionInvokerTest
    {
        [Fact]
        public void CreateActionResultWithActionResultParameterReturnsParameterUnchanged()
        {
            // Arrange
            ControllerActionInvokerHelper invoker = new ControllerActionInvokerHelper();
            ActionResult originalResult = new JsonResult();

            // Act
            ActionResult returnedActionResult = invoker.PublicCreateActionResult(null, null, originalResult);

            // Assert
            Assert.Same(originalResult, returnedActionResult);
        }

        [Fact]
        public void CreateActionResultWithNullParameterReturnsEmptyResult()
        {
            // Arrange
            ControllerActionInvokerHelper invoker = new ControllerActionInvokerHelper();

            // Act
            ActionResult returnedActionResult = invoker.PublicCreateActionResult(null, null, null);

            // Assert
            Assert.IsType<EmptyResult>(returnedActionResult);
        }

        [Fact]
        public void CreateActionResultWithObjectParameterReturnsContentResult()
        {
            // Arrange
            ControllerActionInvokerHelper invoker = new ControllerActionInvokerHelper();
            object originalReturnValue = new CultureReflector();

            // Act
            ActionResult returnedActionResult = invoker.PublicCreateActionResult(null, null, originalReturnValue);

            // Assert
            ContentResult contentResult = Assert.IsType<ContentResult>(returnedActionResult);
            Assert.Equal("ivl", contentResult.Content);
        }

        [Fact]
        public void FindAction()
        {
            // Arrange
            EmptyController controller = new EmptyController();
            ControllerContext controllerContext = GetControllerContext(controller);
            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();

            ActionDescriptor expectedAd = new Mock<ActionDescriptor>().Object;
            Mock<ControllerDescriptor> mockCd = new Mock<ControllerDescriptor>();
            mockCd.Setup(cd => cd.FindAction(controllerContext, "someAction")).Returns(expectedAd);

            // Act
            ActionDescriptor returnedAd = helper.PublicFindAction(controllerContext, mockCd.Object, "someAction");

            // Assert
            Assert.Equal(expectedAd, returnedAd);
        }

        [Fact]
        public void FindActionDoesNotMatchConstructor()
        {
            // FindActionMethod() shouldn't match special-named methods like type constructors.

            // Arrange
            Controller controller = new FindMethodController();
            ControllerContext context = GetControllerContext(controller);
            ControllerDescriptor cd = new ReflectedControllerDescriptor(typeof(FindMethodController));

            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();

            // Act
            ActionDescriptor ad = helper.PublicFindAction(context, cd, ".ctor");
            ActionDescriptor ad2 = helper.PublicFindAction(context, cd, "FindMethodController");

            // Assert
            Assert.Null(ad);
            Assert.Null(ad2);
        }

        [Fact]
        public void FindActionDoesNotMatchEvent()
        {
            // FindActionMethod() should skip methods that aren't publicly visible.

            // Arrange
            Controller controller = new FindMethodController();
            ControllerContext context = GetControllerContext(controller);
            ControllerDescriptor cd = new ReflectedControllerDescriptor(typeof(FindMethodController));

            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();

            // Act
            ActionDescriptor ad = helper.PublicFindAction(context, cd, "add_Event");

            // Assert
            Assert.Null(ad);
        }

        [Fact]
        public void FindActionDoesNotMatchInternalMethod()
        {
            // FindActionMethod() should skip methods that aren't publicly visible.

            // Arrange
            Controller controller = new FindMethodController();
            ControllerContext context = GetControllerContext(controller);
            ControllerDescriptor cd = new ReflectedControllerDescriptor(typeof(FindMethodController));

            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();

            // Act
            ActionDescriptor ad = helper.PublicFindAction(context, cd, "InternalMethod");

            // Assert
            Assert.Null(ad);
        }

        [Fact]
        public void FindActionDoesNotMatchMethodsDefinedOnControllerType()
        {
            // FindActionMethod() shouldn't match methods originally defined on the Controller type, e.g. Dispose().

            // Arrange
            Controller controller = new BlankController();
            ControllerDescriptor cd = new ReflectedControllerDescriptor(typeof(BlankController));
            ControllerContext context = GetControllerContext(controller);
            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();
            var methods = typeof(Controller).GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            // Act & Assert
            foreach (var method in methods)
            {
                bool wasFound = true;
                try
                {
                    ActionDescriptor ad = helper.PublicFindAction(context, cd, method.Name);
                    wasFound = (ad != null);
                }
                finally
                {
                    Assert.False(wasFound, "FindAction() should return false for methods defined on the Controller class: " + method);
                }
            }
        }

        [Fact]
        public void FindActionDoesNotMatchMethodsDefinedOnObjectType()
        {
            // FindActionMethod() shouldn't match methods originally defined on the Object type, e.g. ToString().

            // Arrange
            Controller controller = new FindMethodController();
            ControllerContext context = GetControllerContext(controller);
            ControllerDescriptor cd = new ReflectedControllerDescriptor(typeof(FindMethodController));

            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();

            // Act
            ActionDescriptor ad = helper.PublicFindAction(context, cd, "ToString");

            // Assert
            Assert.Null(ad);
        }

        [Fact]
        public void FindActionDoesNotMatchNonActionMethod()
        {
            // FindActionMethod() should respect the [NonAction] attribute.

            // Arrange
            Controller controller = new FindMethodController();
            ControllerContext context = GetControllerContext(controller);
            ControllerDescriptor cd = new ReflectedControllerDescriptor(typeof(FindMethodController));

            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();

            // Act
            ActionDescriptor ad = helper.PublicFindAction(context, cd, "NonActionMethod");

            // Assert
            Assert.Null(ad);
        }

        [Fact]
        public void FindActionDoesNotMatchOverriddenNonActionMethod()
        {
            // FindActionMethod() should trace the method's inheritance chain looking for the [NonAction] attribute.

            // Arrange
            Controller controller = new DerivedFindMethodController();
            ControllerContext context = GetControllerContext(controller);
            ControllerDescriptor cd = new ReflectedControllerDescriptor(typeof(DerivedFindMethodController));

            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();

            // Act
            ActionDescriptor ad = helper.PublicFindAction(context, cd, "InternalMethod");

            // Assert
            Assert.Null(ad);
        }

        [Fact]
        public void FindActionDoesNotMatchPrivateMethod()
        {
            // FindActionMethod() should skip methods that aren't publicly visible.

            // Arrange
            Controller controller = new FindMethodController();
            ControllerContext context = GetControllerContext(controller);
            ControllerDescriptor cd = new ReflectedControllerDescriptor(typeof(FindMethodController));

            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();

            // Act
            ActionDescriptor ad = helper.PublicFindAction(context, cd, "PrivateMethod");

            // Assert
            Assert.Null(ad);
        }

        [Fact]
        public void FindActionDoesNotMatchProperty()
        {
            // FindActionMethod() shouldn't match special-named methods like property getters.

            // Arrange
            Controller controller = new FindMethodController();
            ControllerContext context = GetControllerContext(controller);
            ControllerDescriptor cd = new ReflectedControllerDescriptor(typeof(FindMethodController));

            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();

            // Act
            ActionDescriptor ad = helper.PublicFindAction(context, cd, "get_Property");

            // Assert
            Assert.Null(ad);
        }

        [Fact]
        public void FindActionDoesNotMatchProtectedMethod()
        {
            // FindActionMethod() should skip methods that aren't publicly visible.

            // Arrange
            Controller controller = new FindMethodController();
            ControllerContext context = GetControllerContext(controller);
            ControllerDescriptor cd = new ReflectedControllerDescriptor(typeof(FindMethodController));

            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();

            // Act
            ActionDescriptor ad = helper.PublicFindAction(context, cd, "ProtectedMethod");

            // Assert
            Assert.Null(ad);
        }

        [Fact]
        public void FindActionIsCaseInsensitive()
        {
            // Arrange
            Controller controller = new FindMethodController();
            ControllerContext context = GetControllerContext(controller);
            ControllerDescriptor cd = new ReflectedControllerDescriptor(typeof(FindMethodController));
            MethodInfo expectedMethodInfo = typeof(FindMethodController).GetMethod("ValidActionMethod");

            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();

            // Act
            ActionDescriptor ad1 = helper.PublicFindAction(context, cd, "validactionmethod");
            ActionDescriptor ad2 = helper.PublicFindAction(context, cd, "VALIDACTIONMETHOD");

            // Assert
            ReflectedActionDescriptor rad1 = Assert.IsType<ReflectedActionDescriptor>(ad1);
            Assert.Same(expectedMethodInfo, rad1.MethodInfo);
            ReflectedActionDescriptor rad2 = Assert.IsType<ReflectedActionDescriptor>(ad2);
            Assert.Same(expectedMethodInfo, rad2.MethodInfo);
        }

        [Fact]
        public void FindActionMatchesActionMethodWithClosedGenerics()
        {
            // FindActionMethod() should work with generic methods as long as there are no open types.

            // Arrange
            Controller controller = new GenericFindMethodController<int>();
            ControllerContext context = GetControllerContext(controller);
            ControllerDescriptor cd = new ReflectedControllerDescriptor(typeof(GenericFindMethodController<int>));
            MethodInfo expectedMethodInfo = typeof(GenericFindMethodController<int>).GetMethod("ClosedGenericMethod");

            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();

            // Act
            ActionDescriptor ad = helper.PublicFindAction(context, cd, "ClosedGenericMethod");

            // Assert
            ReflectedActionDescriptor rad = Assert.IsType<ReflectedActionDescriptor>(ad);
            Assert.Same(expectedMethodInfo, rad.MethodInfo);
        }

        [Fact]
        public void FindActionMatchesNewActionMethodsHidingNonActionMethods()
        {
            // FindActionMethod() should stop looking for [NonAction] in the method's inheritance chain when it sees
            // that a method in a derived class hides the a method in the base class.

            // Arrange
            Controller controller = new DerivedFindMethodController();
            ControllerContext context = GetControllerContext(controller);
            ControllerDescriptor cd = new ReflectedControllerDescriptor(typeof(DerivedFindMethodController));
            MethodInfo expectedMethodInfo = typeof(DerivedFindMethodController).GetMethod("DerivedIsActionMethod");

            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();

            // Act
            ActionDescriptor ad = helper.PublicFindAction(context, cd, "DerivedIsActionMethod");

            // Assert
            ReflectedActionDescriptor rad = Assert.IsType<ReflectedActionDescriptor>(ad);
            Assert.Same(expectedMethodInfo, rad.MethodInfo);
        }

        [Fact]
        public void GetControllerDescriptor()
        {
            // Arrange
            EmptyController controller = new EmptyController();
            ControllerContext controllerContext = GetControllerContext(controller);
            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();

            // Act
            ControllerDescriptor cd = helper.PublicGetControllerDescriptor(controllerContext);

            // Assert
            Assert.IsType<ReflectedControllerDescriptor>(cd);
            Assert.Equal(typeof(EmptyController), cd.ControllerType);
        }

        [Fact]
        public void GetFiltersSplitsFilterObjectsIntoFilterInfo()
        {
            // Arrange
            IActionFilter actionFilter = new Mock<IActionFilter>().Object;
            IResultFilter resultFilter = new Mock<IResultFilter>().Object;
            IAuthorizationFilter authorizationFilter = new Mock<IAuthorizationFilter>().Object;
            IAuthenticationFilter authenticationFilter = new Mock<IAuthenticationFilter>().Object;
            IExceptionFilter exFilter = new Mock<IExceptionFilter>().Object;
            object noneOfTheAbove = new object();
            ControllerActionInvokerHelper invoker = new ControllerActionInvokerHelper(actionFilter, authorizationFilter, authenticationFilter, exFilter, resultFilter, noneOfTheAbove);
            ControllerContext context = new ControllerContext();
            ActionDescriptor descriptor = new Mock<ActionDescriptor>().Object;

            // Act
            FilterInfo result = invoker.PublicGetFilters(context, descriptor);

            // Assert
            Assert.Same(actionFilter, result.ActionFilters.Single());
            Assert.Same(authorizationFilter, result.AuthorizationFilters.Single());
            Assert.Same(authenticationFilter, result.AuthenticationFilters.Single());
            Assert.Same(exFilter, result.ExceptionFilters.Single());
            Assert.Same(resultFilter, result.ResultFilters.Single());
        }

        [Fact]
        public void GetParameterValueAllowsAllSubpropertiesIfBindAttributeNotSpecified()
        {
            // Arrange
            CustomConverterController controller = new CustomConverterController();
            ControllerContext controllerContext = GetControllerContext(controller);
            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();

            ParameterInfo paramWithoutBindAttribute = typeof(CustomConverterController).GetMethod("ParameterWithoutBindAttribute").GetParameters()[0];
            ReflectedParameterDescriptor pd = new ReflectedParameterDescriptor(paramWithoutBindAttribute, new Mock<ActionDescriptor>().Object);

            // Act
            object valueWithoutBindAttribute = helper.PublicGetParameterValue(controllerContext, pd);

            // Assert
            Assert.Equal("foo=True&bar=True", valueWithoutBindAttribute);
        }

        [Fact]
        public void GetParameterValueResolvesConvertersInCorrectOrderOfPrecedence()
        {
            // Order of precedence:
            //   1. Attributes on the parameter itself
            //   2. Query the global converter provider

            // Arrange
            CustomConverterController controller = new CustomConverterController();
            Dictionary<string, object> values = new Dictionary<string, object> { { "foo", "fooValue" } };
            ControllerContext controllerContext = GetControllerContext(controller, values);
            controller.ControllerContext = controllerContext;
            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();

            ParameterInfo paramWithOneConverter = typeof(CustomConverterController).GetMethod("ParameterHasOneConverter").GetParameters()[0];
            ReflectedParameterDescriptor pdOneConverter = new ReflectedParameterDescriptor(paramWithOneConverter, new Mock<ActionDescriptor>().Object);
            ParameterInfo paramWithNoConverters = typeof(CustomConverterController).GetMethod("ParameterHasNoConverters").GetParameters()[0];
            ReflectedParameterDescriptor pdNoConverters = new ReflectedParameterDescriptor(paramWithNoConverters, new Mock<ActionDescriptor>().Object);

            // Act
            object valueWithOneConverter = helper.PublicGetParameterValue(controllerContext, pdOneConverter);
            object valueWithNoConverters = helper.PublicGetParameterValue(controllerContext, pdNoConverters);

            // Assert
            Assert.Equal("foo_String", valueWithOneConverter);
            Assert.Equal("fooValue", valueWithNoConverters);
        }

        [Fact]
        public void GetParameterValueRespectsBindAttribute()
        {
            // Arrange
            CustomConverterController controller = new CustomConverterController();
            ControllerContext controllerContext = GetControllerContext(controller);
            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();

            ParameterInfo paramWithBindAttribute = typeof(CustomConverterController).GetMethod("ParameterHasBindAttribute").GetParameters()[0];
            ReflectedParameterDescriptor pd = new ReflectedParameterDescriptor(paramWithBindAttribute, new Mock<ActionDescriptor>().Object);

            // Act
            object valueWithBindAttribute = helper.PublicGetParameterValue(controllerContext, pd);

            // Assert
            Assert.Equal("foo=True&bar=False", valueWithBindAttribute);
        }

        [Fact]
        public void GetParameterValueRespectsBindAttributePrefix()
        {
            // Arrange
            CustomConverterController controller = new CustomConverterController();
            Dictionary<string, object> values = new Dictionary<string, object> { { "foo", "fooValue" }, { "bar", "barValue" } };
            ControllerContext controllerContext = GetControllerContext(controller, values);
            controller.ControllerContext = controllerContext;

            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();

            ParameterInfo paramWithFieldPrefix = typeof(CustomConverterController).GetMethod("ParameterHasFieldPrefix").GetParameters()[0];
            ReflectedParameterDescriptor pd = new ReflectedParameterDescriptor(paramWithFieldPrefix, new Mock<ActionDescriptor>().Object);

            // Act
            object parameterValue = helper.PublicGetParameterValue(controllerContext, pd);

            // Assert
            Assert.Equal("barValue", parameterValue);
        }

        [Fact]
        public void GetParameterValueRespectsBindAttributePrefixOnComplexType()
        {
            // Arrange
            CustomConverterController controller = new CustomConverterController();
            Dictionary<string, object> values = new Dictionary<string, object> { { "intprop", "123" }, { "stringprop", "hello" } };
            ControllerContext controllerContext = GetControllerContext(controller, values);
            controller.ControllerContext = controllerContext;

            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();

            ParameterInfo paramWithFieldPrefix = typeof(CustomConverterController).GetMethod("ParameterHasPrefixAndComplexType").GetParameters()[0];
            ReflectedParameterDescriptor pd = new ReflectedParameterDescriptor(paramWithFieldPrefix, new Mock<ActionDescriptor>().Object);

            // Act
            MySimpleModel parameterValue = helper.PublicGetParameterValue(controllerContext, pd) as MySimpleModel;

            // Assert
            Assert.Null(parameterValue);
        }

        [Fact]
        public void GetParameterValueRespectsBindAttributeNullPrefix()
        {
            // Arrange
            CustomConverterController controller = new CustomConverterController();
            Dictionary<string, object> values = new Dictionary<string, object> { { "foo", "fooValue" }, { "bar", "barValue" } };
            ControllerContext controllerContext = GetControllerContext(controller, values);
            controller.ControllerContext = controllerContext;

            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();

            ParameterInfo paramWithFieldPrefix = typeof(CustomConverterController).GetMethod("ParameterHasNullFieldPrefix").GetParameters()[0];
            ReflectedParameterDescriptor pd = new ReflectedParameterDescriptor(paramWithFieldPrefix, new Mock<ActionDescriptor>().Object);

            // Act
            object parameterValue = helper.PublicGetParameterValue(controllerContext, pd);

            // Assert
            Assert.Equal("fooValue", parameterValue);
        }

        [Fact]
        public void GetParameterValueRespectsBindAttributeNullPrefixOnComplexType()
        {
            // Arrange
            CustomConverterController controller = new CustomConverterController();
            Dictionary<string, object> values = new Dictionary<string, object> { { "intprop", "123" }, { "stringprop", "hello" } };
            ControllerContext controllerContext = GetControllerContext(controller, values);
            controller.ControllerContext = controllerContext;

            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();

            ParameterInfo paramWithFieldPrefix = typeof(CustomConverterController).GetMethod("ParameterHasNoPrefixAndComplexType").GetParameters()[0];
            ReflectedParameterDescriptor pd = new ReflectedParameterDescriptor(paramWithFieldPrefix, new Mock<ActionDescriptor>().Object);

            // Act
            MySimpleModel parameterValue = helper.PublicGetParameterValue(controllerContext, pd) as MySimpleModel;

            // Assert
            Assert.NotNull(parameterValue);
            Assert.Equal(123, parameterValue.IntProp);
            Assert.Equal("hello", parameterValue.StringProp);
        }

        [Fact]
        public void GetParameterValueRespectsBindAttributeEmptyPrefix()
        {
            // Arrange
            CustomConverterController controller = new CustomConverterController();
            Dictionary<string, object> values = new Dictionary<string, object> { { "foo", "fooValue" }, { "bar", "barValue" }, { "intprop", "123" }, { "stringprop", "hello" } };
            ControllerContext controllerContext = GetControllerContext(controller, values);
            controller.ControllerContext = controllerContext;

            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();

            ParameterInfo paramWithFieldPrefix = typeof(CustomConverterController).GetMethod("ParameterHasEmptyFieldPrefix").GetParameters()[0];
            ReflectedParameterDescriptor pd = new ReflectedParameterDescriptor(paramWithFieldPrefix, new Mock<ActionDescriptor>().Object);

            // Act
            MySimpleModel parameterValue = helper.PublicGetParameterValue(controllerContext, pd) as MySimpleModel;

            // Assert
            Assert.NotNull(parameterValue);
            Assert.Equal(123, parameterValue.IntProp);
            Assert.Equal("hello", parameterValue.StringProp);
        }

        [Fact]
        public void GetParameterValueRespectsDefaultValueAttribute()
        {
            // Arrange
            CustomConverterController controller = new CustomConverterController();
            ControllerContext controllerContext = GetControllerContext(controller);
            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();
            controller.ValueProvider = new SimpleValueProvider();

            ParameterInfo paramWithDefaultValueAttribute = typeof(CustomConverterController).GetMethod("ParameterHasDefaultValueAttribute").GetParameters()[0];
            ReflectedParameterDescriptor pd = new ReflectedParameterDescriptor(paramWithDefaultValueAttribute, new Mock<ActionDescriptor>().Object);

            // Act
            object valueWithDefaultValueAttribute = helper.PublicGetParameterValue(controllerContext, pd);

            // Assert
            Assert.Equal(42, valueWithDefaultValueAttribute);
        }

        [Fact]
        public void GetParameterValueReturnsNullIfCannotConvertNonRequiredParameter()
        {
            // Arrange
            Dictionary<string, object> dict = new Dictionary<string, object>()
            {
                { "id", DateTime.Now } // cannot convert DateTime to Nullable<int>
            };
            var controller = new ParameterTestingController();
            ControllerContext context = GetControllerContext(controller, dict);
            controller.ControllerContext = context;

            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();
            MethodInfo mi = typeof(ParameterTestingController).GetMethod("TakesNullableInt");
            ParameterInfo[] pis = mi.GetParameters();
            ReflectedParameterDescriptor pd = new ReflectedParameterDescriptor(pis[0], new Mock<ActionDescriptor>().Object);

            // Act
            object oValue = helper.PublicGetParameterValue(context, pd);

            // Assert
            Assert.Null(oValue);
        }

        [Fact]
        public void GetParameterValueReturnsNullIfNullableTypeValueNotFound()
        {
            // Arrange
            var controller = new ParameterTestingController();
            ControllerContext context = GetControllerContext(controller);
            controller.ControllerContext = context;
            controller.ValueProvider = new SimpleValueProvider();

            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();
            MethodInfo mi = typeof(ParameterTestingController).GetMethod("TakesNullableInt");
            ParameterInfo[] pis = mi.GetParameters();
            ReflectedParameterDescriptor pd = new ReflectedParameterDescriptor(pis[0], new Mock<ActionDescriptor>().Object);

            // Act
            object oValue = helper.PublicGetParameterValue(context, pd);

            // Assert
            Assert.Null(oValue);
        }

        [Fact]
        public void GetParameterValueReturnsNullIfReferenceTypeValueNotFound()
        {
            // Arrange
            var controller = new ParameterTestingController();
            ControllerContext context = GetControllerContext(controller);
            controller.ControllerContext = context;
            controller.ValueProvider = new SimpleValueProvider();

            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();
            MethodInfo mi = typeof(ParameterTestingController).GetMethod("Foo");
            ParameterInfo[] pis = mi.GetParameters();
            ReflectedParameterDescriptor pd = new ReflectedParameterDescriptor(pis[0], new Mock<ActionDescriptor>().Object);

            // Act
            object oValue = helper.PublicGetParameterValue(context, pd);

            // Assert
            Assert.Null(oValue);
        }

        [Fact]
        public void GetParameterValuesCallsGetParameterValue()
        {
            // Arrange
            ControllerBase controller = new ParameterTestingController();
            IDictionary<string, object> dict = new Dictionary<string, object>();
            ControllerContext context = GetControllerContext(controller);
            MethodInfo mi = typeof(ParameterTestingController).GetMethod("Foo");
            ReflectedActionDescriptor ad = new ReflectedActionDescriptor(mi, "Foo", new Mock<ControllerDescriptor>().Object);
            ParameterDescriptor[] pds = ad.GetParameters();

            Mock<ControllerActionInvokerHelper> mockHelper = new Mock<ControllerActionInvokerHelper>() { CallBase = true };
            mockHelper.Setup(h => h.PublicGetParameterValue(context, pds[0])).Returns("Myfoo").Verifiable();
            mockHelper.Setup(h => h.PublicGetParameterValue(context, pds[1])).Returns("Mybar").Verifiable();
            mockHelper.Setup(h => h.PublicGetParameterValue(context, pds[2])).Returns("Mybaz").Verifiable();
            ControllerActionInvokerHelper helper = mockHelper.Object;

            // Act
            IDictionary<string, object> parameters = helper.PublicGetParameterValues(context, ad);

            // Assert
            Assert.Equal(3, parameters.Count);
            Assert.Equal("Myfoo", parameters["foo"]);
            Assert.Equal("Mybar", parameters["bar"]);
            Assert.Equal("Mybaz", parameters["baz"]);
            mockHelper.Verify();
        }

        [Fact]
        public void GetParameterValuesReturnsEmptyDictionaryForParameterlessMethod()
        {
            // Arrange
            var controller = new ParameterTestingController();
            ControllerContext context = GetControllerContext(controller);
            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();
            MethodInfo mi = typeof(ParameterTestingController).GetMethod("Parameterless");
            ReflectedActionDescriptor ad = new ReflectedActionDescriptor(mi, "Parameterless", new Mock<ControllerDescriptor>().Object);

            // Act
            IDictionary<string, object> parameters = helper.PublicGetParameterValues(context, ad);

            // Assert
            Assert.Empty(parameters);
        }

        [Fact]
        public void GetParameterValuesReturnsValuesForParametersInOrder()
        {
            // We need to hook into GetParameterValue() to make sure that GetParameterValues() is calling it.

            // Arrange
            var controller = new ParameterTestingController();
            Dictionary<string, object> dict = new Dictionary<string, object>()
            {
                { "foo", "MyFoo" },
                { "bar", "MyBar" },
                { "baz", "MyBaz" }
            };
            ControllerContext context = GetControllerContext(controller, dict);
            controller.ControllerContext = context;

            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();
            MethodInfo mi = typeof(ParameterTestingController).GetMethod("Foo");
            ReflectedActionDescriptor ad = new ReflectedActionDescriptor(mi, "Foo", new Mock<ControllerDescriptor>().Object);

            // Act
            IDictionary<string, object> parameters = helper.PublicGetParameterValues(context, ad);

            // Assert
            Assert.Equal(3, parameters.Count);
            Assert.Equal("MyFoo", parameters["foo"]);
            Assert.Equal("MyBar", parameters["bar"]);
            Assert.Equal("MyBaz", parameters["baz"]);
        }

        [Fact]
        public void GetParameterValueUsesControllerValueProviderAsValueProvider()
        {
            // Arrange
            Dictionary<string, object> values = new Dictionary<string, object>()
            {
                { "foo", "fooValue" }
            };

            CustomConverterController controller = new CustomConverterController();
            ControllerContext controllerContext = GetControllerContext(controller, values);
            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();

            ParameterInfo parameter = typeof(CustomConverterController).GetMethod("ParameterHasNoConverters").GetParameters()[0];
            ReflectedParameterDescriptor pd = new ReflectedParameterDescriptor(parameter, new Mock<ActionDescriptor>().Object);

            // Act
            object parameterValue = helper.PublicGetParameterValue(controllerContext, pd);

            // Assert
            Assert.Equal("fooValue", parameterValue);
        }

        [Fact]
        public void InvokeAction()
        {
            // Arrange
            ControllerBase controller = new Mock<ControllerBase>().Object;

            ControllerContext context = GetControllerContext(controller);
            ControllerDescriptor cd = new Mock<ControllerDescriptor>().Object;
            ActionDescriptor ad = new Mock<ActionDescriptor>().Object;
            FilterInfo filterInfo = new FilterInfo();

            IDictionary<string, object> parameters = new Dictionary<string, object>();
            MethodInfo methodInfo = typeof(object).GetMethod("ToString");
            ActionResult actionResult = new EmptyResult();
            ActionExecutedContext postContext = new ActionExecutedContext(context, ad, false /* canceled */, null /* exception */)
            {
                Result = actionResult
            };
            ActionResult challengeResult = new EmptyResult();
            AuthenticationContext authenticationContext = new AuthenticationContext();
            AuthorizationContext authorizationContext = new AuthorizationContext();
            AuthenticationChallengeContext authenticationChallengeContext = new AuthenticationChallengeContext() { Result = challengeResult };

            Mock<ControllerActionInvokerHelper> mockHelper = new Mock<ControllerActionInvokerHelper>() { CallBase = true };
            mockHelper.Setup(h => h.PublicGetControllerDescriptor(context)).Returns(cd).Verifiable();
            mockHelper.Setup(h => h.PublicFindAction(context, cd, "SomeMethod")).Returns(ad).Verifiable();
            mockHelper.Setup(h => h.PublicGetFilters(context, ad)).Returns(filterInfo).Verifiable();
            mockHelper.Setup(h => h.PublicInvokeAuthenticationFilters(context, filterInfo.AuthenticationFilters, ad)).Returns(authenticationContext).Verifiable();
            mockHelper.Setup(h => h.PublicInvokeAuthorizationFilters(context, filterInfo.AuthorizationFilters, ad)).Returns(authorizationContext).Verifiable();
            mockHelper.Setup(h => h.PublicGetParameterValues(context, ad)).Returns(parameters).Verifiable();
            mockHelper.Setup(h => h.PublicInvokeActionMethodWithFilters(context, filterInfo.ActionFilters, ad, parameters)).Returns(postContext).Verifiable();
            mockHelper.Setup(h => h.PublicInvokeAuthenticationFiltersChallenge(context, filterInfo.AuthenticationFilters, ad, actionResult)).Returns(authenticationChallengeContext).Verifiable();
            mockHelper.Setup(h => h.PublicInvokeActionResultWithFilters(context, filterInfo.ResultFilters, challengeResult)).Returns((ResultExecutedContext)null).Verifiable();
            ControllerActionInvokerHelper helper = mockHelper.Object;

            // Act
            bool retVal = helper.InvokeAction(context, "SomeMethod");
            Assert.True(retVal);
            mockHelper.Verify();
        }

        [Fact]
        public void InvokeActionCallsValidateRequestIfAsked()
        {
            // Arrange
            ControllerBase controller = new Mock<ControllerBase>().Object;
            controller.ValidateRequest = true;
            bool validateInputWasCalled = false;

            ControllerContext context = GetControllerContext(controller, null, validateInputCallback: () => { validateInputWasCalled = true; });
            ControllerDescriptor cd = new Mock<ControllerDescriptor>().Object;
            ActionDescriptor ad = new Mock<ActionDescriptor>().Object;
            FilterInfo filterInfo = new FilterInfo();
            AuthenticationContext authenticationContext = new AuthenticationContext();
            AuthorizationContext authorizationContext = new AuthorizationContext();

            Mock<ControllerActionInvokerHelper> mockHelper = new Mock<ControllerActionInvokerHelper>();
            mockHelper.CallBase = true;
            mockHelper.Setup(h => h.PublicGetControllerDescriptor(context)).Returns(cd).Verifiable();
            mockHelper.Setup(h => h.PublicFindAction(context, cd, "SomeMethod")).Returns(ad).Verifiable();
            mockHelper.Setup(h => h.PublicGetFilters(context, ad)).Returns(filterInfo).Verifiable();
            mockHelper.Setup(h => h.PublicInvokeAuthenticationFilters(context, filterInfo.AuthenticationFilters, ad)).Returns(authenticationContext).Verifiable();
            mockHelper.Setup(h => h.PublicInvokeAuthorizationFilters(context, filterInfo.AuthorizationFilters, ad)).Returns(authorizationContext).Verifiable();
            ControllerActionInvokerHelper helper = mockHelper.Object;

            // Act
            helper.InvokeAction(context, "SomeMethod");

            // Assert
            Assert.True(validateInputWasCalled);
            mockHelper.Verify();
        }

        [Fact]
        public void InvokeActionDoesNotCallValidateRequestForChildActions()
        {
            // Arrange
            ControllerBase controller = new Mock<ControllerBase>().Object;
            controller.ValidateRequest = true;

            ControllerContext context = GetControllerContext(controller, null);
            Mock.Get<ControllerContext>(context).SetupGet(c => c.IsChildAction).Returns(true);
            ControllerDescriptor cd = new Mock<ControllerDescriptor>().Object;
            ActionDescriptor ad = new Mock<ActionDescriptor>().Object;
            FilterInfo filterInfo = new FilterInfo();
            AuthenticationContext authenticationContext = new AuthenticationContext();
            AuthorizationContext authorizationContext = new AuthorizationContext();

            Mock<ControllerActionInvokerHelper> mockHelper = new Mock<ControllerActionInvokerHelper>();
            mockHelper.CallBase = true;
            mockHelper.Setup(h => h.PublicGetControllerDescriptor(context)).Returns(cd).Verifiable();
            mockHelper.Setup(h => h.PublicFindAction(context, cd, "SomeMethod")).Returns(ad).Verifiable();
            mockHelper.Setup(h => h.PublicGetFilters(context, ad)).Returns(filterInfo).Verifiable();
            mockHelper.Setup(h => h.PublicInvokeAuthenticationFilters(context, filterInfo.AuthenticationFilters, ad)).Returns(authenticationContext).Verifiable();
            mockHelper.Setup(h => h.PublicInvokeAuthorizationFilters(context, filterInfo.AuthorizationFilters, ad)).Returns(authorizationContext).Verifiable();
            ControllerActionInvokerHelper helper = mockHelper.Object;

            // Act
            helper.InvokeAction(context, "SomeMethod"); // No exception thrown

            // Assert
            mockHelper.Verify();
        }

        [Fact]
        public void InvokeActionMethodFilterWhereContinuationThrowsExceptionAndIsHandled()
        {
            // Arrange
            List<string> actions = new List<string>();
            MethodInfo mi = typeof(object).GetMethod("ToString");
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            Exception exception = new Exception();
            ActionDescriptor action = new Mock<ActionDescriptor>().Object;

            ActionFilterImpl filter = new ActionFilterImpl()
            {
                OnActionExecutingImpl = delegate(ActionExecutingContext filterContext) { actions.Add("OnActionExecuting"); },
                OnActionExecutedImpl = delegate(ActionExecutedContext filterContext)
                {
                    actions.Add("OnActionExecuted");
                    Assert.Same(exception, filterContext.Exception);
                    Assert.Same(action, filterContext.ActionDescriptor);
                    Assert.False(filterContext.ExceptionHandled);
                    filterContext.ExceptionHandled = true;
                }
            };
            Func<ActionExecutedContext> continuation = delegate
            {
                actions.Add("Continuation");
                throw exception;
            };

            ActionExecutingContext context = new ActionExecutingContext(GetControllerContext(new EmptyController()), action, parameters);

            // Act
            ActionExecutedContext result = ControllerActionInvoker.InvokeActionMethodFilter(filter, context, continuation);

            // Assert
            Assert.Equal(3, actions.Count);
            Assert.Equal("OnActionExecuting", actions[0]);
            Assert.Equal("Continuation", actions[1]);
            Assert.Equal("OnActionExecuted", actions[2]);
            Assert.Same(exception, result.Exception);
            Assert.Same(action, result.ActionDescriptor);
            Assert.True(result.ExceptionHandled);
        }

        [Fact]
        public void InvokeActionMethodFilterWhereContinuationThrowsExceptionAndIsNotHandled()
        {
            // Arrange
            List<string> actions = new List<string>();
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            ActionDescriptor action = new Mock<ActionDescriptor>().Object;

            ActionFilterImpl filter = new ActionFilterImpl()
            {
                OnActionExecutingImpl = delegate(ActionExecutingContext filterContext) { actions.Add("OnActionExecuting"); },
                OnActionExecutedImpl = delegate(ActionExecutedContext filterContext)
                {
                    Assert.NotNull(filterContext.Exception);
                    Assert.Equal("Some exception message.", filterContext.Exception.Message);
                    Assert.Same(action, filterContext.ActionDescriptor);
                    actions.Add("OnActionExecuted");
                }
            };
            Func<ActionExecutedContext> continuation = delegate
            {
                actions.Add("Continuation");
                throw new Exception("Some exception message.");
            };

            ActionExecutingContext context = new ActionExecutingContext(GetControllerContext(new EmptyController()), action, parameters);

            // Act & Assert
            Assert.Throws<Exception>(
                delegate { ControllerActionInvoker.InvokeActionMethodFilter(filter, context, continuation); },
                "Some exception message.");
            Assert.Equal(3, actions.Count);
            Assert.Equal("OnActionExecuting", actions[0]);
            Assert.Equal("Continuation", actions[1]);
            Assert.Equal("OnActionExecuted", actions[2]);
        }

        [Fact]
        public void InvokeActionMethodFilterWhereContinuationThrowsThreadAbortException()
        {
            // Arrange
            List<string> actions = new List<string>();
            ActionResult actionResult = new EmptyResult();
            ActionDescriptor action = new Mock<ActionDescriptor>().Object;

            ActionFilterImpl filter = new ActionFilterImpl()
            {
                OnActionExecutingImpl = delegate(ActionExecutingContext filterContext) { actions.Add("OnActionExecuting"); },
                OnActionExecutedImpl = delegate(ActionExecutedContext filterContext)
                {
                    Thread.ResetAbort();
                    actions.Add("OnActionExecuted");
                    Assert.Null(filterContext.Exception);
                    Assert.False(filterContext.ExceptionHandled);
                    Assert.Same(action, filterContext.ActionDescriptor);
                }
            };
            Func<ActionExecutedContext> continuation = delegate
            {
                actions.Add("Continuation");
                Thread.CurrentThread.Abort();
                return null;
            };

            ActionExecutingContext context = new ActionExecutingContext(new Mock<ControllerContext>().Object, action, new Dictionary<string, object>());

            // Act & Assert
            Assert.Throws<ThreadAbortException>(
                delegate { ControllerActionInvoker.InvokeActionMethodFilter(filter, context, continuation); },
                "Thread was being aborted.");
            Assert.Equal(3, actions.Count);
            Assert.Equal("OnActionExecuting", actions[0]);
            Assert.Equal("Continuation", actions[1]);
            Assert.Equal("OnActionExecuted", actions[2]);
        }

        [Fact]
        public void InvokeActionMethodFilterWhereOnActionExecutingCancels()
        {
            // Arrange
            bool wasCalled = false;
            ActionDescriptor ad = new Mock<ActionDescriptor>().Object;
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            ActionResult actionResult = new EmptyResult();
            ActionDescriptor action = new Mock<ActionDescriptor>().Object;

            ActionFilterImpl filter = new ActionFilterImpl()
            {
                OnActionExecutingImpl = delegate(ActionExecutingContext filterContext)
                {
                    Assert.False(wasCalled);
                    wasCalled = true;
                    filterContext.Result = actionResult;
                },
            };
            Func<ActionExecutedContext> continuation = delegate
            {
                Assert.True(false, "The continuation should not be called.");
                return null;
            };

            ActionExecutingContext context = new ActionExecutingContext(GetControllerContext(new EmptyController()), action, parameters);

            // Act
            ActionExecutedContext result = ControllerActionInvoker.InvokeActionMethodFilter(filter, context, continuation);

            // Assert
            Assert.True(wasCalled);
            Assert.Null(result.Exception);
            Assert.True(result.Canceled);
            Assert.Same(actionResult, result.Result);
            Assert.Same(action, result.ActionDescriptor);
        }

        [Fact]
        public void InvokeActionMethodFilterWithNormalControlFlow()
        {
            // Arrange
            List<string> actions = new List<string>();
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            ActionDescriptor action = new Mock<ActionDescriptor>().Object;

            ActionExecutingContext preContext = new ActionExecutingContext(GetControllerContext(new EmptyController()), action, parameters);
            Mock<ActionExecutedContext> mockPostContext = new Mock<ActionExecutedContext>();

            ActionFilterImpl filter = new ActionFilterImpl()
            {
                OnActionExecutingImpl = delegate(ActionExecutingContext filterContext)
                {
                    Assert.Same(parameters, filterContext.ActionParameters);
                    Assert.Null(filterContext.Result);
                    actions.Add("OnActionExecuting");
                },
                OnActionExecutedImpl = delegate(ActionExecutedContext filterContext)
                {
                    Assert.Equal(mockPostContext.Object, filterContext);
                    actions.Add("OnActionExecuted");
                }
            };
            Func<ActionExecutedContext> continuation = delegate
            {
                actions.Add("Continuation");
                return mockPostContext.Object;
            };

            // Act
            ActionExecutedContext result = ControllerActionInvoker.InvokeActionMethodFilter(filter, preContext, continuation);

            // Assert
            Assert.Equal(3, actions.Count);
            Assert.Equal("OnActionExecuting", actions[0]);
            Assert.Equal("Continuation", actions[1]);
            Assert.Equal("OnActionExecuted", actions[2]);
            Assert.Same(result, mockPostContext.Object);
        }

        [Fact]
        public void InvokeActionInvokesExceptionFiltersAndExecutesResultIfExceptionHandled()
        {
            // Arrange
            ControllerBase controller = new Mock<ControllerBase>().Object;

            ControllerContext context = GetControllerContext(controller);
            ControllerDescriptor cd = new Mock<ControllerDescriptor>().Object;
            ActionDescriptor ad = new Mock<ActionDescriptor>().Object;
            FilterInfo filterInfo = new FilterInfo();

            AuthenticationContext authenticationContext = new AuthenticationContext();
            Exception exception = new Exception();
            ActionResult actionResult = new EmptyResult();
            ExceptionContext exContext = new ExceptionContext(context, exception)
            {
                ExceptionHandled = true,
                Result = actionResult
            };

            Mock<ControllerActionInvokerHelper> mockHelper = new Mock<ControllerActionInvokerHelper>() { CallBase = true };
            mockHelper.Setup(h => h.PublicGetControllerDescriptor(context)).Returns(cd).Verifiable();
            mockHelper.Setup(h => h.PublicFindAction(context, cd, "SomeMethod")).Returns(ad).Verifiable();
            mockHelper.Setup(h => h.PublicGetFilters(context, ad)).Returns(filterInfo).Verifiable();
            mockHelper.Setup(h => h.PublicInvokeAuthenticationFilters(context, filterInfo.AuthenticationFilters, ad)).Throws(exception).Verifiable();
            mockHelper.Setup(h => h.PublicInvokeExceptionFilters(context, filterInfo.ExceptionFilters, exception)).Returns(exContext).Verifiable();
            mockHelper.Setup(h => h.PublicInvokeActionResult(context, actionResult)).Verifiable();
            ControllerActionInvokerHelper helper = mockHelper.Object;

            // Act
            bool retVal = helper.InvokeAction(context, "SomeMethod");
            Assert.True(retVal);
            mockHelper.Verify();
        }

        [Fact]
        public void InvokeActionInvokesExceptionFiltersAndRethrowsExceptionIfNotHandled()
        {
            // Arrange
            ControllerBase controller = new Mock<ControllerBase>().Object;

            ControllerContext context = GetControllerContext(controller);
            ControllerDescriptor cd = new Mock<ControllerDescriptor>().Object;
            ActionDescriptor ad = new Mock<ActionDescriptor>().Object;
            FilterInfo filterInfo = new FilterInfo();

            AuthenticationContext authenticationContext = new AuthenticationContext();
            Exception exception = new Exception();
            ExceptionContext exContext = new ExceptionContext(context, exception);

            Mock<ControllerActionInvokerHelper> mockHelper = new Mock<ControllerActionInvokerHelper>() { CallBase = true };
            mockHelper.Setup(h => h.PublicGetControllerDescriptor(context)).Returns(cd).Verifiable();
            mockHelper.Setup(h => h.PublicFindAction(context, cd, "SomeMethod")).Returns(ad).Verifiable();
            mockHelper.Setup(h => h.PublicGetFilters(context, ad)).Returns(filterInfo).Verifiable();
            mockHelper.Setup(h => h.PublicInvokeAuthenticationFilters(context, filterInfo.AuthenticationFilters, ad)).Returns(authenticationContext).Verifiable();
            mockHelper.Setup(h => h.PublicInvokeAuthorizationFilters(context, filterInfo.AuthorizationFilters, ad)).Throws(exception).Verifiable();
            mockHelper.Setup(h => h.PublicInvokeExceptionFilters(context, filterInfo.ExceptionFilters, exception)).Returns(exContext).Verifiable();
            mockHelper.Setup(h => h.PublicInvokeActionResult(context, It.IsAny<ActionResult>())).Callback(delegate { Assert.True(false, "InvokeActionResult() shouldn't be called if the exception was unhandled by filters."); });
            ControllerActionInvokerHelper helper = mockHelper.Object;

            // Act
            Exception thrownException = Assert.Throws<Exception>(
                delegate { helper.InvokeAction(context, "SomeMethod"); });

            // Assert
            Assert.Same(exception, thrownException);
            mockHelper.Verify();
        }

        [Fact]
        public void InvokeActionInvokesResultIfAuthenticationFilterReturnsResult()
        {
            // Arrange
            ControllerBase controller = new Mock<ControllerBase>().Object;

            ControllerContext context = GetControllerContext(controller);
            ControllerDescriptor cd = new Mock<ControllerDescriptor>().Object;
            ActionDescriptor ad = new Mock<ActionDescriptor>().Object;
            FilterInfo filterInfo = new FilterInfo();

            ActionResult actionResult = new EmptyResult();
            AuthenticationContext authenticationContext = new AuthenticationContext() { Result = actionResult };
            ActionResult challengeActionResult = new EmptyResult();
            AuthenticationChallengeContext authenticationChallengeContext = new AuthenticationChallengeContext() { Result = challengeActionResult };

            Mock<ControllerActionInvokerHelper> mockHelper = new Mock<ControllerActionInvokerHelper>() { CallBase = true };
            mockHelper.Setup(h => h.PublicGetControllerDescriptor(context)).Returns(cd).Verifiable();
            mockHelper.Setup(h => h.PublicFindAction(context, cd, "SomeMethod")).Returns(ad).Verifiable();
            mockHelper.Setup(h => h.PublicGetFilters(context, ad)).Returns(filterInfo).Verifiable();
            mockHelper.Setup(h => h.PublicInvokeAuthenticationFilters(context, filterInfo.AuthenticationFilters, ad)).Returns(authenticationContext).Verifiable();
            mockHelper.Setup(h => h.PublicInvokeAuthenticationFiltersChallenge(context, filterInfo.AuthenticationFilters, ad, actionResult)).Returns(authenticationChallengeContext).Verifiable();
            mockHelper.Setup(h => h.PublicInvokeActionResult(context, challengeActionResult)).Verifiable();
            ControllerActionInvokerHelper helper = mockHelper.Object;

            // Act
            bool retVal = helper.InvokeAction(context, "SomeMethod");
            Assert.True(retVal);
            mockHelper.Verify();
        }

        [Fact]
        public void InvokeActionInvokesResultIfAuthorizationFilterReturnsResult()
        {
            // Arrange
            ControllerBase controller = new Mock<ControllerBase>().Object;

            ControllerContext context = GetControllerContext(controller);
            ControllerDescriptor cd = new Mock<ControllerDescriptor>().Object;
            ActionDescriptor ad = new Mock<ActionDescriptor>().Object;
            FilterInfo filterInfo = new FilterInfo();

            ActionResult actionResult = new EmptyResult();
            AuthenticationContext authenticationContext = new AuthenticationContext();
            AuthorizationContext authorizationContext = new AuthorizationContext() { Result = actionResult };
            ActionResult challengeResult = new EmptyResult();
            AuthenticationChallengeContext authenticationChallengeContext = new AuthenticationChallengeContext() { Result = challengeResult };

            Mock<ControllerActionInvokerHelper> mockHelper = new Mock<ControllerActionInvokerHelper>() { CallBase = true };
            mockHelper.Setup(h => h.PublicGetControllerDescriptor(context)).Returns(cd).Verifiable();
            mockHelper.Setup(h => h.PublicFindAction(context, cd, "SomeMethod")).Returns(ad).Verifiable();
            mockHelper.Setup(h => h.PublicGetFilters(context, ad)).Returns(filterInfo).Verifiable();
            mockHelper.Setup(h => h.PublicInvokeAuthenticationFilters(context, filterInfo.AuthenticationFilters, ad)).Returns(authenticationContext).Verifiable();
            mockHelper.Setup(h => h.PublicInvokeAuthorizationFilters(context, filterInfo.AuthorizationFilters, ad)).Returns(authorizationContext).Verifiable();
            mockHelper.Setup(h => h.PublicInvokeAuthenticationFiltersChallenge(context, filterInfo.AuthenticationFilters, ad, actionResult)).Returns(authenticationChallengeContext).Verifiable();
            mockHelper.Setup(h => h.PublicInvokeActionResult(context, challengeResult)).Verifiable();
            ControllerActionInvokerHelper helper = mockHelper.Object;

            // Act
            bool retVal = helper.InvokeAction(context, "SomeMethod");
            Assert.True(retVal);
            mockHelper.Verify();
        }

        [Fact]
        public void InvokeActionMethod()
        {
            // Arrange
            EmptyController controller = new EmptyController();
            ControllerContext controllerContext = GetControllerContext(controller);
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            ActionResult expectedResult = new Mock<ActionResult>().Object;

            Mock<ActionDescriptor> mockAd = new Mock<ActionDescriptor>();
            mockAd.Setup(ad => ad.Execute(controllerContext, parameters)).Returns("hello world");

            Mock<ControllerActionInvokerHelper> mockHelper = new Mock<ControllerActionInvokerHelper>() { CallBase = true };
            mockHelper.Setup(h => h.PublicCreateActionResult(controllerContext, mockAd.Object, "hello world")).Returns(expectedResult);
            ControllerActionInvokerHelper helper = mockHelper.Object;

            // Act
            ActionResult returnedResult = helper.PublicInvokeActionMethod(controllerContext, mockAd.Object, parameters);

            // Assert
            Assert.Same(expectedResult, returnedResult);
        }

        [Fact]
        public void InvokeActionMethodWithFiltersOrdersFiltersCorrectly()
        {
            // Arrange
            List<string> actions = new List<string>();
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            ActionResult actionResult = new EmptyResult();

            ActionFilterImpl filter1 = new ActionFilterImpl()
            {
                OnActionExecutingImpl = delegate(ActionExecutingContext filterContext) { actions.Add("OnActionExecuting1"); },
                OnActionExecutedImpl = delegate(ActionExecutedContext filterContext) { actions.Add("OnActionExecuted1"); }
            };
            ActionFilterImpl filter2 = new ActionFilterImpl()
            {
                OnActionExecutingImpl = delegate(ActionExecutingContext filterContext) { actions.Add("OnActionExecuting2"); },
                OnActionExecutedImpl = delegate(ActionExecutedContext filterContext) { actions.Add("OnActionExecuted2"); }
            };
            Func<ActionResult> continuation = delegate
            {
                actions.Add("Continuation");
                return new EmptyResult();
            };
            ControllerBase controller = new ContinuationController(continuation);
            ControllerContext context = GetControllerContext(controller);
            ActionDescriptor actionDescriptor = new ReflectedActionDescriptor(ContinuationController.GoMethod, "someName", new Mock<ControllerDescriptor>().Object);
            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();
            List<IActionFilter> filters = new List<IActionFilter>() { filter1, filter2 };

            // Act
            helper.PublicInvokeActionMethodWithFilters(context, filters, actionDescriptor, parameters);

            // Assert
            Assert.Equal(5, actions.Count);
            Assert.Equal("OnActionExecuting1", actions[0]);
            Assert.Equal("OnActionExecuting2", actions[1]);
            Assert.Equal("Continuation", actions[2]);
            Assert.Equal("OnActionExecuted2", actions[3]);
            Assert.Equal("OnActionExecuted1", actions[4]);
        }

        [Fact]
        public void InvokeActionMethodWithFiltersPassesArgumentsCorrectly()
        {
            // Arrange
            bool wasCalled = false;
            MethodInfo mi = ContinuationController.GoMethod;
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            ActionResult actionResult = new EmptyResult();
            ActionFilterImpl filter = new ActionFilterImpl()
            {
                OnActionExecutingImpl = delegate(ActionExecutingContext filterContext)
                {
                    Assert.Same(parameters, filterContext.ActionParameters);
                    Assert.False(wasCalled);
                    wasCalled = true;
                    filterContext.Result = actionResult;
                }
            };
            Func<ActionResult> continuation = delegate
            {
                Assert.True(false, "Continuation should not be called.");
                return null;
            };
            ControllerBase controller = new ContinuationController(continuation);
            ControllerContext context = GetControllerContext(controller);
            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();
            ActionDescriptor actionDescriptor = new ReflectedActionDescriptor(ContinuationController.GoMethod, "someName", new Mock<ControllerDescriptor>().Object);
            List<IActionFilter> filters = new List<IActionFilter>() { filter };

            // Act
            ActionExecutedContext result = helper.PublicInvokeActionMethodWithFilters(context, filters, actionDescriptor, parameters);

            // Assert
            Assert.True(wasCalled);
            Assert.Null(result.Exception);
            Assert.False(result.ExceptionHandled);
            Assert.Same(actionResult, result.Result);
            Assert.Same(actionDescriptor, result.ActionDescriptor);
        }

        [Fact]
        public void InvokeActionPropagatesThreadAbortException()
        {
            // Arrange
            ControllerBase controller = new Mock<ControllerBase>().Object;

            ControllerContext context = GetControllerContext(controller);
            ControllerDescriptor cd = new Mock<ControllerDescriptor>().Object;
            ActionDescriptor ad = new Mock<ActionDescriptor>().Object;
            FilterInfo filterInfo = new FilterInfo();

            ActionResult actionResult = new EmptyResult();
            ActionExecutedContext postContext = new ActionExecutedContext(context, ad, false /* canceled */, null /* exception */)
            {
                Result = actionResult
            };
            AuthenticationContext authenticationContext = new AuthenticationContext();
            AuthorizationContext authorizationContext = new AuthorizationContext() { Result = actionResult };

            Mock<ControllerActionInvokerHelper> mockHelper = new Mock<ControllerActionInvokerHelper>() { CallBase = true };
            mockHelper.Setup(h => h.PublicGetControllerDescriptor(context)).Returns(cd).Verifiable();
            mockHelper.Setup(h => h.PublicFindAction(context, cd, "SomeMethod")).Returns(ad).Verifiable();
            mockHelper.Setup(h => h.PublicGetFilters(context, ad)).Returns(filterInfo).Verifiable();
            mockHelper.Setup(h => h.PublicInvokeAuthenticationFilters(context, filterInfo.AuthenticationFilters, ad)).Returns(authenticationContext).Verifiable();
            mockHelper
                .Setup(h => h.PublicInvokeAuthorizationFilters(context, filterInfo.AuthorizationFilters, ad))
                .Returns(
                    delegate(ControllerContext cc, IList<IAuthorizationFilter> f, ActionDescriptor a)
                    {
                        Thread.CurrentThread.Abort();
                        return null;
                    });
            ControllerActionInvokerHelper helper = mockHelper.Object;

            bool wasAborted = false;

            // Act
            try
            {
                helper.InvokeAction(context, "SomeMethod");
            }
            catch (ThreadAbortException)
            {
                wasAborted = true;
                Thread.ResetAbort();
            }

            // Assert
            Assert.True(wasAborted);
            mockHelper.Verify();
        }

        [Fact]
        public void InvokeActionReturnsFalseIfMethodNotFound()
        {
            // Arrange
            var controller = new BlankController();
            ControllerContext context = GetControllerContext(controller);
            ControllerActionInvoker invoker = new ControllerActionInvoker();

            // Act
            bool retVal = invoker.InvokeAction(context, "foo");

            // Assert
            Assert.False(retVal);
        }

        [Fact]
        public void InvokeActionThrowsIfControllerContextIsNull()
        {
            // Arrange
            ControllerActionInvoker invoker = new ControllerActionInvoker();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { invoker.InvokeAction(null, "actionName"); }, "controllerContext");
        }

        [Fact]
        public void InvokeActionWithEmptyActionNameThrows()
        {
            // Arrange
            var controller = new BasicMethodInvokeController();
            ControllerContext context = GetControllerContext(controller);
            ControllerActionInvoker invoker = new ControllerActionInvoker();

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { invoker.InvokeAction(context, String.Empty); },
                "actionName");
        }

        [Fact]
        public void InvokeActionWithNullActionNameThrows()
        {
            // Arrange
            var controller = new BasicMethodInvokeController();
            ControllerContext context = GetControllerContext(controller);
            ControllerActionInvoker invoker = new ControllerActionInvoker();

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { invoker.InvokeAction(context, null /* actionName */); },
                "actionName");
        }

        [Fact]
        public void InvokeActionWithResultExceptionInvokesExceptionFiltersAndExecutesResultIfExceptionHandled()
        {
            // Arrange
            ControllerBase controller = new Mock<ControllerBase>().Object;

            ControllerContext context = GetControllerContext(controller);
            ControllerDescriptor cd = new Mock<ControllerDescriptor>().Object;
            ActionDescriptor ad = new Mock<ActionDescriptor>().Object;
            IDictionary<string, object> parameters = new Dictionary<string, object>();
            FilterInfo filterInfo = new FilterInfo();
            AuthenticationContext authenticationContext = new AuthenticationContext();
            AuthorizationContext authorizationContext = new AuthorizationContext();
            AuthenticationChallengeContext authenticationChallengeContext = new AuthenticationChallengeContext();

            Exception exception = new Exception();
            ActionResult actionResult = new EmptyResult();
            ActionExecutedContext postContext = new ActionExecutedContext(context, ad, false /* canceled */, null /* exception */)
            {
                Result = actionResult
            };
            ExceptionContext exContext = new ExceptionContext(context, exception)
            {
                ExceptionHandled = true,
                Result = actionResult
            };

            Mock<ControllerActionInvokerHelper> mockHelper = new Mock<ControllerActionInvokerHelper>() { CallBase = true };
            mockHelper.Setup(h => h.PublicGetControllerDescriptor(context)).Returns(cd).Verifiable();
            mockHelper.Setup(h => h.PublicFindAction(context, cd, "SomeMethod")).Returns(ad).Verifiable();
            mockHelper.Setup(h => h.PublicGetFilters(context, ad)).Returns(filterInfo).Verifiable();
            mockHelper.Setup(h => h.PublicInvokeAuthenticationFilters(context, filterInfo.AuthenticationFilters, ad)).Returns(authenticationContext).Verifiable();
            mockHelper.Setup(h => h.PublicInvokeAuthorizationFilters(context, filterInfo.AuthorizationFilters, ad)).Returns(authorizationContext).Verifiable();
            mockHelper.Setup(h => h.PublicGetParameterValues(context, ad)).Returns(parameters).Verifiable();
            mockHelper.Setup(h => h.PublicInvokeActionMethodWithFilters(context, filterInfo.ActionFilters, ad, parameters)).Returns(postContext).Verifiable();
            mockHelper.Setup(h => h.PublicInvokeActionResultWithFilters(context, filterInfo.ResultFilters, actionResult)).Throws(exception).Verifiable();
            mockHelper.Setup(h => h.PublicInvokeAuthenticationFiltersChallenge(context, filterInfo.AuthenticationFilters, ad, actionResult)).Returns(authenticationChallengeContext).Verifiable();
            mockHelper.Setup(h => h.PublicInvokeExceptionFilters(context, filterInfo.ExceptionFilters, exception)).Returns(exContext).Verifiable();
            mockHelper.Setup(h => h.PublicInvokeActionResult(context, actionResult)).Verifiable();
            ControllerActionInvokerHelper helper = mockHelper.Object;

            // Act
            bool retVal = helper.InvokeAction(context, "SomeMethod");
            Assert.True(retVal, "InvokeAction() should return True on success.");
            mockHelper.Verify();
        }

        [Fact]
        public void InvokeAuthorizationFilters()
        {
            // Arrange
            ControllerBase controller = new Mock<ControllerBase>().Object;
            ActionDescriptor ad = new Mock<ActionDescriptor>().Object;
            ControllerContext controllerContext = GetControllerContext(controller);
            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();

            List<AuthorizationFilterHelper> callQueue = new List<AuthorizationFilterHelper>();
            AuthorizationFilterHelper filter1 = new AuthorizationFilterHelper(callQueue);
            AuthorizationFilterHelper filter2 = new AuthorizationFilterHelper(callQueue);
            IAuthorizationFilter[] filters = new IAuthorizationFilter[] { filter1, filter2 };

            // Act
            AuthorizationContext postContext = helper.PublicInvokeAuthorizationFilters(controllerContext, filters, ad);

            // Assert
            Assert.Equal(ad, postContext.ActionDescriptor);
            Assert.Equal(2, callQueue.Count);
            Assert.Same(filter1, callQueue[0]);
            Assert.Same(filter2, callQueue[1]);
        }

        [Fact]
        public void InvokeAuthorizationFiltersStopsExecutingIfResultProvided()
        {
            // Arrange
            ControllerBase controller = new Mock<ControllerBase>().Object;
            ActionDescriptor ad = new Mock<ActionDescriptor>().Object;
            ControllerContext controllerContext = GetControllerContext(controller);
            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();
            ActionResult result = new EmptyResult();

            List<AuthorizationFilterHelper> callQueue = new List<AuthorizationFilterHelper>();
            AuthorizationFilterHelper filter1 = new AuthorizationFilterHelper(callQueue) { ShortCircuitResult = result };
            AuthorizationFilterHelper filter2 = new AuthorizationFilterHelper(callQueue);
            IAuthorizationFilter[] filters = new IAuthorizationFilter[] { filter1, filter2 };

            // Act
            AuthorizationContext postContext = helper.PublicInvokeAuthorizationFilters(controllerContext, filters, ad);

            // Assert
            Assert.Equal(ad, postContext.ActionDescriptor);
            Assert.Same(result, postContext.Result);
            AuthorizationFilterHelper resultFilter = Assert.Single(callQueue);
            Assert.Same(filter1, resultFilter);
        }

        [Fact]
        public void InvokeAuthenticationFilters()
        {
            // Arrange
            ControllerBase controller = new Mock<ControllerBase>().Object;
            ActionDescriptor ad = new Mock<ActionDescriptor>().Object;
            ControllerContext controllerContext = GetControllerContext(controller);
            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();

            List<AuthenticationFilterHelper> callQueue = new List<AuthenticationFilterHelper>();
            AuthenticationFilterHelper filter1 = new AuthenticationFilterHelper(callQueue);
            AuthenticationFilterHelper filter2 = new AuthenticationFilterHelper(callQueue);
            IAuthenticationFilter[] filters = new IAuthenticationFilter[] { filter1, filter2 };
            AuthenticationContext postContext;

            using (new ThreadCurrentPrincipalContext())
            {
                // Act
                postContext = helper.PublicInvokeAuthenticationFilters(controllerContext, filters, ad);
            }

            // Assert
            Assert.Same(ad, postContext.ActionDescriptor);
            Assert.Same(controller, postContext.Controller);
            Assert.Equal(2, callQueue.Count);
            Assert.Same(filter1, callQueue[0]);
            Assert.Same(filter2, callQueue[1]);
        }

        [Fact]
        public void InvokeAuthenticationFiltersProvidesHttpContextUserAsPrincipalInFilterContext()
        {
            // Arrange
            ControllerBase controller = new Mock<ControllerBase>().Object;
            ActionDescriptor ad = new Mock<ActionDescriptor>().Object;
            ControllerContext controllerContext = GetControllerContext(controller);
            IPrincipal principal = new ClaimsPrincipal();
            controllerContext.HttpContext.User = principal;
            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();

            List<AuthenticationFilterHelper> callQueue = new List<AuthenticationFilterHelper>();
            AuthenticationFilterHelper filter = new AuthenticationFilterHelper(callQueue);
            IAuthenticationFilter[] filters = new IAuthenticationFilter[] { filter };
            AuthenticationContext postContext;

            using (new ThreadCurrentPrincipalContext())
            {
                // Act
                postContext = helper.PublicInvokeAuthenticationFilters(controllerContext, filters, ad);
            }

            // Assert
            Assert.Same(ad, postContext.ActionDescriptor);
            Assert.Same(controller, postContext.Controller);
            AuthenticationFilterHelper actualHelper = Assert.Single(callQueue);
            Assert.Same(filter, actualHelper);
            Assert.Same(principal, postContext.Principal);
        }

        [Fact]
        public void InvokeAuthenticationFiltersStopsExecutingIfResultProvided()
        {
            // Arrange
            ControllerBase controller = new Mock<ControllerBase>().Object;
            ActionDescriptor ad = new Mock<ActionDescriptor>().Object;
            ControllerContext controllerContext = GetControllerContext(controller);
            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();
            ActionResult result = new EmptyResult();

            List<AuthenticationFilterHelper> callQueue = new List<AuthenticationFilterHelper>();
            AuthenticationFilterHelper filter1 = new AuthenticationFilterHelper(callQueue) { ShortCircuitResult = result };
            AuthenticationFilterHelper filter2 = new AuthenticationFilterHelper(callQueue);
            IAuthenticationFilter[] filters = new IAuthenticationFilter[] { filter1, filter2 };
            AuthenticationContext postContext;

            using (new ThreadCurrentPrincipalContext())
            {
                // Act
                postContext = helper.PublicInvokeAuthenticationFilters(controllerContext, filters, ad);
            }

            // Assert
            Assert.Same(ad, postContext.ActionDescriptor);
            Assert.Same(controller, postContext.Controller);
            Assert.Same(result, postContext.Result);
            AuthenticationFilterHelper resultFilter = Assert.Single(callQueue);
            Assert.Same(filter1, resultFilter);
        }

        [Fact]
        public void InvokeAuthenticationFiltersSetsHttpContextUserWhenPrincipalIsProvided()
        {
            // Arrange
            ControllerBase controller = new Mock<ControllerBase>().Object;
            ActionDescriptor ad = new Mock<ActionDescriptor>().Object;
            ControllerContext controllerContext = GetControllerContext(controller);
            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();
            IPrincipal principal = new ClaimsPrincipal();

            List<AuthenticationFilterHelper> callQueue = new List<AuthenticationFilterHelper>();
            AuthenticationFilterHelper filter = new AuthenticationFilterHelper(callQueue) { Principal = principal };
            IAuthenticationFilter[] filters = new IAuthenticationFilter[] { filter };
            AuthenticationContext postContext;

            using (new ThreadCurrentPrincipalContext())
            {
                // Act
                postContext = helper.PublicInvokeAuthenticationFilters(controllerContext, filters, ad);
            }

            // Assert
            Assert.Same(ad, postContext.ActionDescriptor);
            Assert.Same(controller, postContext.Controller);
            Assert.Same(principal, postContext.HttpContext.User);
            AuthenticationFilterHelper resultFilter = Assert.Single(callQueue);
            Assert.Same(filter, resultFilter);
        }

        [Fact]
        public void InvokeAuthenticationFiltersSetsThreadCurrentPrincipalWhenPrincipalIsProvided()
        {
            // Arrange
            ControllerBase controller = new Mock<ControllerBase>().Object;
            ActionDescriptor ad = new Mock<ActionDescriptor>().Object;
            ControllerContext controllerContext = GetControllerContext(controller);
            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();
            IPrincipal principal = new ClaimsPrincipal();

            List<AuthenticationFilterHelper> callQueue = new List<AuthenticationFilterHelper>();
            AuthenticationFilterHelper filter = new AuthenticationFilterHelper(callQueue) { Principal = principal };
            IAuthenticationFilter[] filters = new IAuthenticationFilter[] { filter };

            using (new ThreadCurrentPrincipalContext())
            {
                // Act
                AuthenticationContext postContext = helper.PublicInvokeAuthenticationFilters(controllerContext, filters, ad);

                // Assert
                Assert.Same(ad, postContext.ActionDescriptor);
                Assert.Same(controller, postContext.Controller);
                Assert.Same(principal, Thread.CurrentPrincipal);
                AuthenticationFilterHelper resultFilter = Assert.Single(callQueue);
                Assert.Same(filter, resultFilter);
            }
        }

        [Fact]
        public void InvokeAuthenticationFiltersDoesNotSetThreadCurrentPrincipalWhenPrincipalIsUnchanged()
        {
            // Arrange
            ControllerBase controller = new Mock<ControllerBase>().Object;
            ActionDescriptor ad = new Mock<ActionDescriptor>().Object;
            ControllerContext controllerContext = GetControllerContext(controller);
            IPrincipal principal = new ClaimsPrincipal();
            controllerContext.HttpContext.User = principal;
            Assert.NotSame(controllerContext.HttpContext.User, Thread.CurrentPrincipal); // Guard
            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();

            List<AuthenticationFilterHelper> callQueue = new List<AuthenticationFilterHelper>();
            AuthenticationFilterHelper filter = new AuthenticationFilterHelper(callQueue);
            IAuthenticationFilter[] filters = new IAuthenticationFilter[] { filter };

            using (new ThreadCurrentPrincipalContext())
            {
                // Act
                AuthenticationContext postContext = helper.PublicInvokeAuthenticationFilters(controllerContext, filters, ad);

                // Assert
                Assert.Same(ad, postContext.ActionDescriptor);
                Assert.Same(controller, postContext.Controller);
                Assert.NotSame(controllerContext.HttpContext.User, Thread.CurrentPrincipal);
                AuthenticationFilterHelper resultFilter = Assert.Single(callQueue);
                Assert.Same(filter, resultFilter);
            }
        }

        [Fact]
        public void InvokeAuthenticationFiltersChallenge()
        {
            // Arrange
            ControllerBase controller = new Mock<ControllerBase>().Object;
            ActionDescriptor ad = new Mock<ActionDescriptor>().Object;
            ControllerContext controllerContext = GetControllerContext(controller);
            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();

            List<AuthenticationFilterHelper> callQueue = new List<AuthenticationFilterHelper>();
            AuthenticationFilterHelper filter1 = new AuthenticationFilterHelper(callQueue);
            AuthenticationFilterHelper filter2 = new AuthenticationFilterHelper(callQueue);
            IAuthenticationFilter[] filters = new IAuthenticationFilter[] { filter1, filter2 };
            ActionResult result = new EmptyResult();

            // Act
            AuthenticationChallengeContext postContext = helper.PublicInvokeAuthenticationFiltersChallenge(controllerContext, filters, ad, result);

            // Assert
            Assert.Same(ad, postContext.ActionDescriptor);
            Assert.Same(controller, postContext.Controller);
            Assert.Same(result, postContext.Result);
            Assert.Equal(2, callQueue.Count);
            Assert.Same(filter1, callQueue[0]);
            Assert.Same(filter2, callQueue[1]);
        }

        [Fact]
        public void InvokeExceptionFilters()
        {
            // Arrange
            ControllerBase controller = new Mock<ControllerBase>().Object;
            Exception exception = new Exception();
            ControllerContext controllerContext = GetControllerContext(controller);
            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();

            List<ExceptionFilterHelper> callQueue = new List<ExceptionFilterHelper>();
            ExceptionFilterHelper filter1 = new ExceptionFilterHelper(callQueue);
            ExceptionFilterHelper filter2 = new ExceptionFilterHelper(callQueue);
            IExceptionFilter[] filters = new IExceptionFilter[] { filter1, filter2 };

            // Act
            ExceptionContext postContext = helper.PublicInvokeExceptionFilters(controllerContext, filters, exception);

            // Assert
            Assert.Same(exception, postContext.Exception);
            Assert.False(postContext.ExceptionHandled);
            Assert.Same(filter1.ContextPassed, filter2.ContextPassed);
            Assert.Equal(2, callQueue.Count);
            Assert.Same(filter2, callQueue[0]); // Exception filters are executed in reverse order
            Assert.Same(filter1, callQueue[1]);
        }

        [Fact]
        public void InvokeExceptionFiltersContinuesExecutingIfExceptionHandled()
        {
            // Arrange
            ControllerBase controller = new Mock<ControllerBase>().Object;
            Exception exception = new Exception();
            ControllerContext controllerContext = GetControllerContext(controller);
            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();

            List<ExceptionFilterHelper> callQueue = new List<ExceptionFilterHelper>();
            ExceptionFilterHelper filter1 = new ExceptionFilterHelper(callQueue) { ShouldHandleException = true };
            ExceptionFilterHelper filter2 = new ExceptionFilterHelper(callQueue);
            IExceptionFilter[] filters = new IExceptionFilter[] { filter1, filter2 };

            // Act
            ExceptionContext postContext = helper.PublicInvokeExceptionFilters(controllerContext, filters, exception);

            // Assert
            Assert.Same(exception, postContext.Exception);
            Assert.True(postContext.ExceptionHandled);
            Assert.Same(filter1.ContextPassed, filter2.ContextPassed);
            Assert.Equal(2, callQueue.Count);
            Assert.Same(filter2, callQueue[0]); // Exception filters are executed in reverse order
            Assert.Same(filter1, callQueue[1]);
        }

        [Fact]
        public void InvokeActionResultWithFiltersOrdersFiltersCorrectly()
        {
            // Arrange
            List<string> actions = new List<string>();
            ActionFilterImpl filter1 = new ActionFilterImpl()
            {
                OnResultExecutingImpl = delegate(ResultExecutingContext filterContext) { actions.Add("OnResultExecuting1"); },
                OnResultExecutedImpl = delegate(ResultExecutedContext filterContext) { actions.Add("OnResultExecuted1"); }
            };
            ActionFilterImpl filter2 = new ActionFilterImpl()
            {
                OnResultExecutingImpl = delegate(ResultExecutingContext filterContext) { actions.Add("OnResultExecuting2"); },
                OnResultExecutedImpl = delegate(ResultExecutedContext filterContext) { actions.Add("OnResultExecuted2"); }
            };
            Action continuation = delegate { actions.Add("Continuation"); };
            ActionResult actionResult = new ContinuationResult(continuation);
            ControllerBase controller = new Mock<ControllerBase>().Object;
            ControllerContext context = GetControllerContext(controller);
            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();
            List<IResultFilter> filters = new List<IResultFilter>() { filter1, filter2 };

            // Act
            helper.PublicInvokeActionResultWithFilters(context, filters, actionResult);

            // Assert
            Assert.Equal(5, actions.Count);
            Assert.Equal("OnResultExecuting1", actions[0]);
            Assert.Equal("OnResultExecuting2", actions[1]);
            Assert.Equal("Continuation", actions[2]);
            Assert.Equal("OnResultExecuted2", actions[3]);
            Assert.Equal("OnResultExecuted1", actions[4]);
        }

        [Fact]
        public void InvokeActionResultWithFiltersPassesArgumentsCorrectly()
        {
            // Arrange
            bool wasCalled = false;
            Action continuation = delegate { Assert.True(false, "Continuation should not be called."); };
            ActionResult actionResult = new ContinuationResult(continuation);
            ControllerBase controller = new Mock<ControllerBase>().Object;
            ControllerContext context = GetControllerContext(controller);
            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();
            ActionFilterImpl filter = new ActionFilterImpl()
            {
                OnResultExecutingImpl = delegate(ResultExecutingContext filterContext)
                {
                    Assert.Same(actionResult, filterContext.Result);
                    Assert.False(wasCalled);
                    wasCalled = true;
                    filterContext.Cancel = true;
                }
            };

            List<IResultFilter> filters = new List<IResultFilter>() { filter };

            // Act
            ResultExecutedContext result = helper.PublicInvokeActionResultWithFilters(context, filters, actionResult);

            // Assert
            Assert.True(wasCalled);
            Assert.Null(result.Exception);
            Assert.False(result.ExceptionHandled);
            Assert.Same(actionResult, result.Result);
        }

        [Fact]
        public void InvokeActionResultWithFiltersPassesSameContextObjectToInnerFilters()
        {
            // Arrange
            ControllerBase controller = new Mock<ControllerBase>().Object;
            ControllerContext context = GetControllerContext(controller);

            ResultExecutingContext storedContext = null;
            ActionResult result = new EmptyResult();
            List<IResultFilter> filters = new List<IResultFilter>()
            {
                new ActionFilterImpl()
                {
                    OnResultExecutingImpl = delegate(ResultExecutingContext ctx)
                    {
                        Assert.NotNull(ctx);
                        storedContext = ctx;
                    },
                    OnResultExecutedImpl = delegate { }
                },
                new ActionFilterImpl()
                {
                    OnResultExecutingImpl = delegate(ResultExecutingContext ctx)
                    {
                        Assert.NotNull(ctx);
                        Assert.Same(storedContext, ctx);
                    },
                    OnResultExecutedImpl = delegate { }
                },
            };
            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();

            // Act
            ResultExecutedContext postContext = helper.PublicInvokeActionResultWithFilters(context, filters, result);

            // Assert
            Assert.Same(result, postContext.Result);
        }

        [Fact]
        public void InvokeActionResultWithFiltersWhereContinuationThrowsExceptionAndIsHandled()
        {
            // Arrange
            List<string> actions = new List<string>();
            Exception exception = new Exception();
            Action continuation = delegate
            {
                actions.Add("Continuation");
                throw exception;
            };
            ActionResult actionResult = new ContinuationResult(continuation);
            ActionFilterImpl filter = new ActionFilterImpl()
            {
                OnResultExecutingImpl = delegate(ResultExecutingContext filterContext) { actions.Add("OnResultExecuting"); },
                OnResultExecutedImpl = delegate(ResultExecutedContext filterContext)
                {
                    actions.Add("OnResultExecuted");
                    Assert.Same(actionResult, filterContext.Result);
                    Assert.Same(exception, filterContext.Exception);
                    Assert.False(filterContext.ExceptionHandled);
                    filterContext.ExceptionHandled = true;
                }
            };

            Mock<ResultExecutingContext> mockResultExecutingContext = new Mock<ResultExecutingContext>() { DefaultValue = DefaultValue.Mock };
            mockResultExecutingContext.Setup(c => c.Result).Returns(actionResult);
            List<IResultFilter> filters = new List<IResultFilter>() { filter, };
            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();

            // Act
            ResultExecutedContext result = helper.PublicInvokeActionResultWithFilters(mockResultExecutingContext.Object, filters, actionResult);

            // Assert
            Assert.Equal(3, actions.Count);
            Assert.Equal("OnResultExecuting", actions[0]);
            Assert.Equal("Continuation", actions[1]);
            Assert.Equal("OnResultExecuted", actions[2]);
            Assert.Same(exception, result.Exception);
            Assert.True(result.ExceptionHandled);
            Assert.Same(actionResult, result.Result);
        }

        [Fact]
        public void InvokeActionResultWithFiltersWhereContinuationThrowsExceptionAndIsNotHandled()
        {
            // Arrange
            List<string> actions = new List<string>();
            Action continuation = delegate
            {
                actions.Add("Continuation");
                throw new Exception("Some exception message.");
            };
            ActionResult actionResult = new ContinuationResult(continuation);
            ActionFilterImpl filter = new ActionFilterImpl()
            {
                OnResultExecutingImpl = delegate(ResultExecutingContext filterContext) { actions.Add("OnResultExecuting"); },
                OnResultExecutedImpl = delegate(ResultExecutedContext filterContext) { actions.Add("OnResultExecuted"); }
            };
            Mock<ResultExecutingContext> mockResultExecutingContext = new Mock<ResultExecutingContext>() { DefaultValue = DefaultValue.Mock };
            mockResultExecutingContext.Setup(c => c.Result).Returns(actionResult);
            List<IResultFilter> filters = new List<IResultFilter>() { filter, };
            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();

            // Act & Assert
            Assert.Throws<Exception>(
                delegate { helper.PublicInvokeActionResultWithFilters(mockResultExecutingContext.Object, filters, actionResult); },
                "Some exception message.");
            Assert.Equal(3, actions.Count);
            Assert.Equal("OnResultExecuting", actions[0]);
            Assert.Equal("Continuation", actions[1]);
            Assert.Equal("OnResultExecuted", actions[2]);
        }

        [Fact]
        public void InvokeActionResultWithFiltersWhereContinuationThrowsThreadAbortException()
        {
            // Arrange
            List<string> actions = new List<string>();

            Action continuation = delegate
            {
                actions.Add("Continuation");
                Thread.CurrentThread.Abort();
            };
            ActionResult actionResult = new ContinuationResult(continuation);
            Mock<ResultExecutingContext> mockPreContext = new Mock<ResultExecutingContext>() { DefaultValue = DefaultValue.Mock };
            mockPreContext.Setup(c => c.Result).Returns(actionResult);
            ActionFilterImpl filter = new ActionFilterImpl()
            {
                OnResultExecutingImpl = delegate(ResultExecutingContext filterContext) { actions.Add("OnResultExecuting"); },
                OnResultExecutedImpl = delegate(ResultExecutedContext filterContext)
                {
                    Thread.ResetAbort();
                    actions.Add("OnResultExecuted");
                    Assert.Same(actionResult, filterContext.Result);
                    Assert.Null(filterContext.Exception);
                    Assert.False(filterContext.ExceptionHandled);
                }
            };
            List<IResultFilter> filters = new List<IResultFilter>() { filter, };
            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();

            // Act & Assert
            Assert.Throws<ThreadAbortException>(
                delegate { helper.PublicInvokeActionResultWithFilters(mockPreContext.Object, filters, actionResult); },
                "Thread was being aborted.");
            Assert.Equal(3, actions.Count);
            Assert.Equal("OnResultExecuting", actions[0]);
            Assert.Equal("Continuation", actions[1]);
            Assert.Equal("OnResultExecuted", actions[2]);
        }

        [Fact]
        public void InvokeActionResultWithFiltersWhereContinuationThrowsUnhandledWithMultipleFilters()
        {
            // Arrange
            List<string> actions = new List<string>();
            string expectedExceptionMessage = "Some exception message.";
            Action continuation = delegate
            {
                actions.Add("Continuation");
                throw new Exception(expectedExceptionMessage);
            };
            ActionFilterImpl filter1 = new ActionFilterImpl()
            {
                OnResultExecutingImpl = delegate(ResultExecutingContext filterContext)
                {
                    actions.Add("OnResultExecuting1");
                },
                OnResultExecutedImpl = delegate(ResultExecutedContext filterContext)
                {
                    actions.Add("OnResultExecuted1");
                    Assert.NotNull(filterContext.Exception);
                    Assert.Equal(filterContext.Exception.Message, expectedExceptionMessage);
                }
            };
            ActionFilterImpl filter2 = new ActionFilterImpl()
            {
                OnResultExecutingImpl = delegate(ResultExecutingContext filterContext)
                {
                    actions.Add("OnResultExecuting2");
                },
                OnResultExecutedImpl = delegate(ResultExecutedContext filterContext)
                {
                    actions.Add("OnResultExecuted2");
                    Assert.NotNull(filterContext.Exception);
                    Assert.Equal(filterContext.Exception.Message, expectedExceptionMessage);
                }
            };
            ActionResult actionResult = new ContinuationResult(continuation);
            ControllerBase controller = new Mock<ControllerBase>().Object;
            ControllerContext context = GetControllerContext(controller);
            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();
            List<IResultFilter> filters = new List<IResultFilter>() { filter1, filter2 };

            // Act
            Assert.Throws<Exception>(
                delegate { helper.PublicInvokeActionResultWithFilters(context, filters, actionResult); },
                expectedExceptionMessage);

            // Assert
            Assert.Equal(5, actions.Count);
            Assert.Equal("OnResultExecuting1", actions[0]);
            Assert.Equal("OnResultExecuting2", actions[1]);
            Assert.Equal("Continuation", actions[2]);
            Assert.Equal("OnResultExecuted2", actions[3]);
            Assert.Equal("OnResultExecuted1", actions[4]);
        }

        [Fact]
        public void InvokeActionResultWithFiltersWhereContinuationThrowsHandledByEarlierFilter()
        {
            // Arrange
            List<string> actions = new List<string>();
            Exception exception = new Exception();
            Action continuation = delegate
            {
                actions.Add("Continuation");
                throw exception;
            };
            ActionFilterImpl filter1 = new ActionFilterImpl()
            {
                OnResultExecutingImpl = delegate(ResultExecutingContext filterContext)
                {
                    actions.Add("OnResultExecuting1");
                },
                OnResultExecutedImpl = delegate(ResultExecutedContext filterContext)
                {
                    actions.Add("OnResultExecuted1");
                    filterContext.ExceptionHandled = true;
                    Assert.Same(exception, filterContext.Exception);
                }
            };
            ActionFilterImpl filter2 = new ActionFilterImpl()
            {
                OnResultExecutingImpl = delegate(ResultExecutingContext filterContext)
                {
                    actions.Add("OnResultExecuting2");
                },
                OnResultExecutedImpl = delegate(ResultExecutedContext filterContext)
                {
                    actions.Add("OnResultExecuted2");
                    Assert.Same(exception, filterContext.Exception);
                }
            };
            ActionResult actionResult = new ContinuationResult(continuation);
            ControllerBase controller = new Mock<ControllerBase>().Object;
            ControllerContext context = GetControllerContext(controller);
            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();
            List<IResultFilter> filters = new List<IResultFilter>() { filter1, filter2 };

            // Act
            ResultExecutedContext result = helper.PublicInvokeActionResultWithFilters(context, filters, actionResult);

            // Assert
            Assert.Equal(5, actions.Count);
            Assert.Equal("OnResultExecuting1", actions[0]);
            Assert.Equal("OnResultExecuting2", actions[1]);
            Assert.Equal("Continuation", actions[2]);
            Assert.Equal("OnResultExecuted2", actions[3]);
            Assert.Equal("OnResultExecuted1", actions[4]);
            Assert.Same(exception, result.Exception);
            Assert.True(result.ExceptionHandled);
            Assert.Same(actionResult, result.Result);
        }

        [Fact]
        public void InvokeActionResultWithFiltersWhereContinuationThrowsHandledByLaterFilter()
        {
            // Arrange
            List<string> actions = new List<string>();
            Exception exception = new Exception();
            Action continuation = delegate
            {
                actions.Add("Continuation");
                throw exception;
            };
            ActionFilterImpl filter1 = new ActionFilterImpl()
            {
                OnResultExecutingImpl = delegate(ResultExecutingContext filterContext)
                {
                    actions.Add("OnResultExecuting1");
                },
                OnResultExecutedImpl = delegate(ResultExecutedContext filterContext)
                {
                    actions.Add("OnResultExecuted1");
                    Assert.True(filterContext.ExceptionHandled);
                }
            };
            ActionFilterImpl filter2 = new ActionFilterImpl()
            {
                OnResultExecutingImpl = delegate(ResultExecutingContext filterContext)
                {
                    actions.Add("OnResultExecuting2");
                },
                OnResultExecutedImpl = delegate(ResultExecutedContext filterContext)
                {
                    actions.Add("OnResultExecuted2");
                    Assert.Same(exception, filterContext.Exception);
                    filterContext.ExceptionHandled = true;
                }
            };
            ActionResult actionResult = new ContinuationResult(continuation);
            ControllerBase controller = new Mock<ControllerBase>().Object;
            ControllerContext context = GetControllerContext(controller);
            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();
            List<IResultFilter> filters = new List<IResultFilter>() { filter1, filter2 };

            // Act
            ResultExecutedContext result = helper.PublicInvokeActionResultWithFilters(context, filters, actionResult);

            // Assert
            Assert.Equal(5, actions.Count);
            Assert.Equal("OnResultExecuting1", actions[0]);
            Assert.Equal("OnResultExecuting2", actions[1]);
            Assert.Equal("Continuation", actions[2]);
            Assert.Equal("OnResultExecuted2", actions[3]);
            Assert.Equal("OnResultExecuted1", actions[4]);
            Assert.Same(exception, result.Exception);
            Assert.True(result.ExceptionHandled);
            Assert.Same(actionResult, result.Result);
        }

        [Fact]
        public void InvokeActionResultWithFiltersWhereCancelled()
        {
            // Arrange
            bool wasCalled = false;
            MethodInfo mi = typeof(object).GetMethod("ToString");
            object[] paramValues = new object[0];
            ActionFilterImpl filter = new ActionFilterImpl()
            {
                OnResultExecutingImpl = delegate(ResultExecutingContext filterContext)
                {
                    Assert.False(wasCalled);
                    wasCalled = true;
                    filterContext.Cancel = true;
                },
            };
            Action continuation = delegate
            {
                Assert.True(false, "The continuation should not be called.");
            };
            ActionResult actionResult = new ContinuationResult(continuation);

            Mock<ResultExecutingContext> mockResultExecutingContext = new Mock<ResultExecutingContext>() { DefaultValue = DefaultValue.Mock };
            mockResultExecutingContext.Setup(c => c.Result).Returns(actionResult);
            List<IResultFilter> filters = new List<IResultFilter>() { filter, };
            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();

            // Act
            ResultExecutedContext result = helper.PublicInvokeActionResultWithFilters(mockResultExecutingContext.Object, filters, actionResult);

            // Assert
            Assert.True(wasCalled);
            Assert.Null(result.Exception);
            Assert.True(result.Canceled);
            Assert.Same(actionResult, result.Result);
        }

        [Fact]
        public void InvokeActionResultWithFiltersWhereCancelledByEarlierFilter()
        {
            // Arrange
            List<string> actions = new List<string>();
            ActionFilterImpl filter1 = new ActionFilterImpl()
            {
                OnResultExecutingImpl = delegate(ResultExecutingContext filterContext)
                {
                    actions.Add("OnResultExecuting1");
                    filterContext.Cancel = true;
                },
                OnResultExecutedImpl = delegate(ResultExecutedContext filterContext)
                {
                    actions.Add("OnResultExecuted1");
                }
            };
            ActionFilterImpl filter2 = new ActionFilterImpl()
            {
                OnResultExecutingImpl = delegate(ResultExecutingContext filterContext)
                {
                    actions.Add("OnResultExecuting2");
                },
                OnResultExecutedImpl = delegate(ResultExecutedContext filterContext)
                {
                    actions.Add("OnResultExecuted2");
                }
            };
            Action continuation = delegate
            {
                actions.Add("Continuation");
            };
            ActionResult actionResult = new ContinuationResult(continuation);
            ControllerBase controller = new Mock<ControllerBase>().Object;
            ControllerContext context = GetControllerContext(controller);
            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();
            List<IResultFilter> filters = new List<IResultFilter>() { filter1, filter2 };

            // Act
            ResultExecutedContext result = helper.PublicInvokeActionResultWithFilters(context, filters, actionResult);

            // Assert
            string action = Assert.Single(actions);
            Assert.Equal("OnResultExecuting1", action);
            Assert.Null(result.Exception);
            Assert.True(result.Canceled);
            Assert.Same(actionResult, result.Result);
        }

        [Fact]
        public void InvokeActionResultWithFiltersWhereCancelledByLaterFilter()
        {
            // Arrange
            List<string> actions = new List<string>();
            ActionFilterImpl filter1 = new ActionFilterImpl()
            {
                OnResultExecutingImpl = delegate(ResultExecutingContext filterContext)
                {
                    actions.Add("OnResultExecuting1");
                },
                OnResultExecutedImpl = delegate(ResultExecutedContext filterContext)
                {
                    actions.Add("OnResultExecuted1");
                    Assert.True(filterContext.Canceled);
                }
            };
            ActionFilterImpl filter2 = new ActionFilterImpl()
            {
                OnResultExecutingImpl = delegate(ResultExecutingContext filterContext)
                {
                    actions.Add("OnResultExecuting2");
                    filterContext.Cancel = true;
                },
                OnResultExecutedImpl = delegate(ResultExecutedContext filterContext)
                {
                    actions.Add("OnResultExecuted2");
                }
            };
            Action continuation = delegate
            {
                actions.Add("Continuation");
            };
            ActionResult actionResult = new ContinuationResult(continuation);
            ControllerBase controller = new Mock<ControllerBase>().Object;
            ControllerContext context = GetControllerContext(controller);
            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();
            List<IResultFilter> filters = new List<IResultFilter>() { filter1, filter2 };

            // Act
            ResultExecutedContext result = helper.PublicInvokeActionResultWithFilters(context, filters, actionResult);

            // Assert
            Assert.Equal(3, actions.Count);
            Assert.Equal("OnResultExecuting1", actions[0]);
            Assert.Equal("OnResultExecuting2", actions[1]);
            Assert.Equal("OnResultExecuted1", actions[2]);
            Assert.Null(result.Exception);
            Assert.True(result.Canceled);
            Assert.Same(actionResult, result.Result);
        }

        [Fact]
        public void InvokeActionResultWithFiltersWithNormalControlFlow()
        {
            // Arrange
            List<string> actions = new List<string>();
            Action continuation = delegate
            {
                actions.Add("Continuation");
            };
            ActionResult actionResult = new ContinuationResult(continuation);
            ActionFilterImpl filter = new ActionFilterImpl()
            {
                OnResultExecutingImpl = delegate(ResultExecutingContext filterContext)
                {
                    Assert.Same(actionResult, filterContext.Result);
                    Assert.False(filterContext.Cancel);
                    actions.Add("OnResultExecuting");
                },
                OnResultExecutedImpl = delegate(ResultExecutedContext filterContext)
                {
                    Assert.Same(actionResult, filterContext.Result);
                    Assert.False(filterContext.Canceled);
                    Assert.Null(filterContext.Exception);
                    Assert.False(filterContext.ExceptionHandled);
                    actions.Add("OnResultExecuted");
                }
            };
            Mock<ResultExecutingContext> mockResultExecutingContext = new Mock<ResultExecutingContext>();
            mockResultExecutingContext.Setup(c => c.Result).Returns(actionResult);
            List<IResultFilter> filters = new List<IResultFilter>() { filter, };
            ControllerActionInvokerHelper helper = new ControllerActionInvokerHelper();

            // Act
            ResultExecutedContext result = helper.PublicInvokeActionResultWithFilters(mockResultExecutingContext.Object, filters, actionResult);

            // Assert
            Assert.Equal(3, actions.Count);
            Assert.Equal("OnResultExecuting", actions[0]);
            Assert.Equal("Continuation", actions[1]);
            Assert.Equal("OnResultExecuted", actions[2]);
            Assert.Same(result.Result, actionResult);
        }

        [Fact]
        public void InvokeMethodCallsOverriddenCreateActionResult()
        {
            // Arrange
            CustomResultInvokerController controller = new CustomResultInvokerController();
            ControllerContext context = GetControllerContext(controller);
            CustomResultInvoker helper = new CustomResultInvoker();
            MethodInfo mi = typeof(CustomResultInvokerController).GetMethod("ReturnCustomResult");
            ReflectedActionDescriptor ad = new ReflectedActionDescriptor(mi, "ReturnCustomResult", new Mock<ControllerDescriptor>().Object);
            IDictionary<string, object> parameters = new Dictionary<string, object>();

            // Act
            ActionResult actionResult = helper.PublicInvokeActionMethod(context, ad, parameters);

            // Assert (arg got passed to method + back correctly)
            CustomResult customResult = Assert.IsType<CustomResult>(actionResult);
            Assert.Equal("abc123", customResult.ReturnValue);
        }

        [Fact]
        public void FindAction_MultipleMethodsSameActionOneWithRouteAttributeAndRouteWasMatched_ReturnsMethodWithRoutingAttribute()
        {
            // Arrange
            Type controllerType = typeof(WithRoutingAttributeController);
            ControllerDescriptor controllerDescriptor = new ReflectedControllerDescriptor(controllerType);

            var context = new ControllerContext();
            context.RouteData = new RouteData();
            context.RouteData.Route = DirectRouteTestHelpers.BuildDirectRouteFromMethod<WithRoutingAttributeController>(c => c.Action());
            context.RouteData.AddDirectRouteMatches();

            var actionInvoker = new ControllerActionInvokerHelper();

            // Act
            ActionDescriptor actionDescriptor = actionInvoker.PublicFindAction(context, controllerDescriptor, actionName: null);

            var matchedMethod = ((ReflectedActionDescriptor)actionDescriptor).MethodInfo;

            // Assert
            Assert.Equal(typeof(WithRoutingAttributeController).GetMethod("Action"), matchedMethod);
        }

        [Fact]
        public void FindAction_ControllerLevelDirectRoute_FindsMatch()
        {
            // Arrange
            Type controllerType = typeof(AttributeRoutingOnTheController);
            ControllerDescriptor controllerDescriptor = new ReflectedControllerDescriptor(controllerType);

            var context = new ControllerContext();
            context.RouteData = new RouteData();
            context.RouteData.Route = DirectRouteTestHelpers.BuildDirectRouteFromController<AttributeRoutingOnTheController>();

            // Simulate a match that binds the action parameter
            context.RouteData.AddDirectRouteMatches((r, rd) => { rd.Values.Add("action", "Action1"); return true; });

            ControllerActionInvokerHelper invoker = new ControllerActionInvokerHelper();

            // Act
            ActionDescriptor actionDescriptor = invoker.PublicFindAction(context, controllerDescriptor, actionName: null);

            var matchedMethod = ((ReflectedActionDescriptor)actionDescriptor).MethodInfo;

            // Assert
            Assert.Equal(typeof(AttributeRoutingOnTheController).GetMethod("Action1"), matchedMethod);
        }

        [Fact]
        public void FindAction_ControllerLevelDirectRoute_NoMatch()
        {
            // Arrange
            Type controllerType = typeof(AttributeRoutingOnTheController);
            ControllerDescriptor controllerDescriptor = new ReflectedControllerDescriptor(controllerType);

            var context = new ControllerContext();
            context.RouteData = new RouteData();
            context.RouteData.Route = DirectRouteTestHelpers.BuildDirectRouteFromController<AttributeRoutingOnTheController>();

            // Simulate a match that binds the action parameter
            context.RouteData.AddDirectRouteMatches((r, rd) => { rd.Values.Add("action", "Action3"); return true; });

            ControllerActionInvokerHelper invoker = new ControllerActionInvokerHelper();

            // Act
            ActionDescriptor actionDescriptor = invoker.PublicFindAction(context, controllerDescriptor, actionName: null);

            // Assert
            Assert.Null(actionDescriptor);
        }

        [Fact]
        public void FindAction_ControllerLevelDirectRoute_AmbiguousMatch()
        {
            // Arrange
            Type controllerType = typeof(AttributeRoutingOnTheController);
            ControllerDescriptor controllerDescriptor = new ReflectedControllerDescriptor(controllerType);

            var context = new ControllerContext();
            context.RouteData = new RouteData();
            context.RouteData.Route = DirectRouteTestHelpers.BuildDirectRouteFromController<AttributeRoutingOnTheController>();

            // Simulate a match that binds the action parameter
            context.RouteData.AddDirectRouteMatches((r, rd) => { rd.Values.Add("action", "Action2"); return true; });

            ControllerActionInvokerHelper invoker = new ControllerActionInvokerHelper();

            // Act & Assert
            Assert.Throws<AmbiguousMatchException>(() => invoker.PublicFindAction(context, controllerDescriptor, actionName: null));
        }

        [Fact]
        public void FindActionMethod_MultipleMethodsSameActionOneWithRouteAttributeAndRouteWasMatched_ReturnsMethodWithRoutingAttribute()
        {
            // Arrange
            Type controllerType = typeof(WithRoutingAttributeController);
            ControllerDescriptor controllerDescriptor = new ReflectedControllerDescriptor(controllerType);

            var context = new ControllerContext();
            context.RouteData = new RouteData();
            context.RouteData.Route = DirectRouteTestHelpers.BuildDirectRouteFromMethod<WithRoutingAttributeController>(c => c.Action());
            context.RouteData.AddDirectRouteMatches();

            ControllerActionInvokerHelper invoker = new ControllerActionInvokerHelper();

            // Act
            ActionDescriptor actionDescriptor = invoker.PublicFindAction(context, controllerDescriptor, actionName: null);
            MethodInfo matchedMethod = ((ReflectedActionDescriptor)actionDescriptor).MethodInfo;

            // Assert
            Assert.Equal("Action", matchedMethod.Name);
        }

        private static ControllerContext GetControllerContext(ControllerBase controller)
        {
            return GetControllerContext(controller, null);
        }

        private static ControllerContext GetControllerContext(ControllerBase controller, IDictionary<string, object> values, Action validateInputCallback = null)
        {
            SimpleValueProvider valueProvider = new SimpleValueProvider();
            controller.ValueProvider = valueProvider;
            if (values != null)
            {
                foreach (var entry in values)
                {
                    valueProvider[entry.Key] = entry.Value;
                }
            }

            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.SetupGet(c => c.RouteData).Returns(new RouteData());

            mockControllerContext.Setup(c => c.HttpContext.Request.ValidateInput()).Callback(() =>
            {
                if (!controller.ValidateRequest)
                {
                    Assert.True(false, "ValidateRequest() should not be called if the controller opted out.");
                }
                if (validateInputCallback != null)
                {
                    // signal to caller that ValidateInput was called
                    validateInputCallback();
                }
            });

            mockControllerContext.Setup(c => c.HttpContext.Session).Returns((HttpSessionStateBase)null);
            mockControllerContext.Setup(c => c.Controller).Returns(controller);
            return mockControllerContext.Object;
        }

        private class ActionFilterImpl : IActionFilter, IResultFilter
        {
            public Action<ActionExecutingContext> OnActionExecutingImpl { get; set; }

            public void OnActionExecuting(ActionExecutingContext filterContext)
            {
                OnActionExecutingImpl(filterContext);
            }

            public Action<ActionExecutedContext> OnActionExecutedImpl { get; set; }

            public void OnActionExecuted(ActionExecutedContext filterContext)
            {
                OnActionExecutedImpl(filterContext);
            }

            public Action<ResultExecutingContext> OnResultExecutingImpl { get; set; }

            public void OnResultExecuting(ResultExecutingContext filterContext)
            {
                OnResultExecutingImpl(filterContext);
            }

            public Action<ResultExecutedContext> OnResultExecutedImpl { get; set; }

            public void OnResultExecuted(ResultExecutedContext filterContext)
            {
                OnResultExecutedImpl(filterContext);
            }
        }

        // This controller serves only to test vanilla method invocation - nothing exciting here
        private class BasicMethodInvokeController : Controller
        {
            public ActionResult ReturnsRenderView(object viewItem)
            {
                return View("ReturnsRenderView", viewItem);
            }
        }

        private class BlankController : Controller
        {
        }

        private sealed class CustomResult : ActionResult
        {
            public object ReturnValue { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                throw new NotImplementedException();
            }
        }

        private sealed class CustomResultInvokerController : Controller
        {
            public object ReturnCustomResult()
            {
                return "abc123";
            }
        }

        private sealed class CustomResultInvoker : ControllerActionInvokerHelper
        {
            protected override ActionResult CreateActionResult(ControllerContext controllerContext, ActionDescriptor actionDescriptor, object actionReturnValue)
            {
                return new CustomResult
                {
                    ReturnValue = actionReturnValue
                };
            }
        }

        private class ContinuationController : Controller
        {
            private Func<ActionResult> _continuation;

            public ContinuationController(Func<ActionResult> continuation)
            {
                _continuation = continuation;
            }

            public ActionResult Go()
            {
                return _continuation();
            }

            public static MethodInfo GoMethod
            {
                get { return typeof(ContinuationController).GetMethod("Go"); }
            }
        }

        private class ContinuationResult : ActionResult
        {
            private Action _continuation;

            public ContinuationResult(Action continuation)
            {
                _continuation = continuation;
            }

            public override void ExecuteResult(ControllerContext context)
            {
                _continuation();
            }
        }

        private class EmptyController : Controller
        {
        }

        // This controller serves to test the default action method matching mechanism
        private class FindMethodController : Controller
        {
            public ActionResult ValidActionMethod()
            {
                return null;
            }

            [NonAction]
            public virtual ActionResult NonActionMethod()
            {
                return null;
            }

            [NonAction]
            public ActionResult DerivedIsActionMethod()
            {
                return null;
            }

            public ActionResult MethodOverloaded()
            {
                return null;
            }

            public ActionResult MethodOverloaded(string s)
            {
                return null;
            }

            public void WrongReturnType()
            {
            }

            protected ActionResult ProtectedMethod()
            {
                return null;
            }

            private ActionResult PrivateMethod()
            {
                return null;
            }

            internal ActionResult InternalMethod()
            {
                return null;
            }

            public override string ToString()
            {
                // originally defined on Object
                return base.ToString();
            }

            public ActionResult Property
            {
                get { return null; }
            }

#pragma warning disable 0067
            // CS0067: Event declared but never used. We use reflection to access this member.
            public event EventHandler Event;
#pragma warning restore 0067
        }

        private class DerivedFindMethodController : FindMethodController
        {
            public override ActionResult NonActionMethod()
            {
                return base.NonActionMethod();
            }

            // FindActionMethod() should accept this as a valid method since [NonAction] doesn't appear
            // in its inheritance chain.
            public new ActionResult DerivedIsActionMethod()
            {
                return base.DerivedIsActionMethod();
            }
        }

        // Similar to FindMethodController, but tests generics support specifically
        private class GenericFindMethodController<T> : Controller
        {
            public ActionResult ClosedGenericMethod(T t)
            {
                return null;
            }

            public ActionResult OpenGenericMethod<U>(U t)
            {
                return null;
            }
        }

        // Allows for testing parameter conversions, etc.
        private class ParameterTestingController : Controller
        {
            public ParameterTestingController()
            {
                Values = new Dictionary<string, object>();
            }

            public IDictionary<string, object> Values { get; private set; }

            public void Foo(string foo, string bar, string baz)
            {
                Values["foo"] = foo;
                Values["bar"] = bar;
                Values["baz"] = baz;
            }

            public void HasOutParam(out string foo)
            {
                foo = null;
            }

            public void HasRefParam(ref string foo)
            {
            }

            public void Parameterless()
            {
            }

            public void TakesInt(int id)
            {
                Values["id"] = id;
            }

            public ActionResult TakesNullableInt(int? id)
            {
                Values["id"] = id;
                return null;
            }

            public void TakesString(string id)
            {
            }

            public void TakesDateTime(DateTime id)
            {
            }
        }

        // Provides access to the protected members of ControllerActionInvoker
        public class ControllerActionInvokerHelper : ControllerActionInvoker
        {
            public ControllerActionInvokerHelper()
            {
                // set instance caches to prevent modifying global test application state
                DescriptorCache = new ControllerDescriptorCache();
            }

            public ControllerActionInvokerHelper(params object[] filters)
                : base(filters)
            {
                // set instance caches to prevent modifying global test application state
                DescriptorCache = new ControllerDescriptorCache();
            }

            public virtual ActionResult PublicCreateActionResult(ControllerContext controllerContext, ActionDescriptor actionDescriptor, object actionReturnValue)
            {
                return base.CreateActionResult(controllerContext, actionDescriptor, actionReturnValue);
            }

            protected override ActionResult CreateActionResult(ControllerContext controllerContext, ActionDescriptor actionDescriptor, object actionReturnValue)
            {
                return PublicCreateActionResult(controllerContext, actionDescriptor, actionReturnValue);
            }

            public virtual ActionDescriptor PublicFindAction(ControllerContext controllerContext, ControllerDescriptor controllerDescriptor, string actionName)
            {
                return base.FindAction(controllerContext, controllerDescriptor, actionName);
            }

            protected override ActionDescriptor FindAction(ControllerContext controllerContext, ControllerDescriptor controllerDescriptor, string actionName)
            {
                return PublicFindAction(controllerContext, controllerDescriptor, actionName);
            }

            public virtual ControllerDescriptor PublicGetControllerDescriptor(ControllerContext controllerContext)
            {
                return base.GetControllerDescriptor(controllerContext);
            }

            protected override ControllerDescriptor GetControllerDescriptor(ControllerContext controllerContext)
            {
                return PublicGetControllerDescriptor(controllerContext);
            }

            public virtual FilterInfo PublicGetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
            {
                return base.GetFilters(controllerContext, actionDescriptor);
            }

            protected override FilterInfo GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
            {
                return PublicGetFilters(controllerContext, actionDescriptor);
            }

            public virtual object PublicGetParameterValue(ControllerContext controllerContext, ParameterDescriptor parameterDescriptor)
            {
                return base.GetParameterValue(controllerContext, parameterDescriptor);
            }

            protected override object GetParameterValue(ControllerContext controllerContext, ParameterDescriptor parameterDescriptor)
            {
                return PublicGetParameterValue(controllerContext, parameterDescriptor);
            }

            public virtual IDictionary<string, object> PublicGetParameterValues(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
            {
                return base.GetParameterValues(controllerContext, actionDescriptor);
            }

            protected override IDictionary<string, object> GetParameterValues(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
            {
                return PublicGetParameterValues(controllerContext, actionDescriptor);
            }

            public virtual ActionResult PublicInvokeActionMethod(ControllerContext controllerContext, ActionDescriptor actionDescriptor, IDictionary<string, object> parameters)
            {
                return base.InvokeActionMethod(controllerContext, actionDescriptor, parameters);
            }

            protected override ActionResult InvokeActionMethod(ControllerContext controllerContext, ActionDescriptor actionDescriptor, IDictionary<string, object> parameters)
            {
                return PublicInvokeActionMethod(controllerContext, actionDescriptor, parameters);
            }

            public virtual ActionExecutedContext PublicInvokeActionMethodWithFilters(ControllerContext controllerContext, IList<IActionFilter> filters, ActionDescriptor actionDescriptor, IDictionary<string, object> parameters)
            {
                return base.InvokeActionMethodWithFilters(controllerContext, filters, actionDescriptor, parameters);
            }

            protected override ActionExecutedContext InvokeActionMethodWithFilters(ControllerContext controllerContext, IList<IActionFilter> filters, ActionDescriptor actionDescriptor, IDictionary<string, object> parameters)
            {
                return PublicInvokeActionMethodWithFilters(controllerContext, filters, actionDescriptor, parameters);
            }

            public virtual void PublicInvokeActionResult(ControllerContext controllerContext, ActionResult actionResult)
            {
                base.InvokeActionResult(controllerContext, actionResult);
            }

            protected override void InvokeActionResult(ControllerContext controllerContext, ActionResult actionResult)
            {
                PublicInvokeActionResult(controllerContext, actionResult);
            }

            public virtual ResultExecutedContext PublicInvokeActionResultWithFilters(ControllerContext controllerContext, IList<IResultFilter> filters, ActionResult actionResult)
            {
                return base.InvokeActionResultWithFilters(controllerContext, filters, actionResult);
            }

            protected override ResultExecutedContext InvokeActionResultWithFilters(ControllerContext controllerContext, IList<IResultFilter> filters, ActionResult actionResult)
            {
                return PublicInvokeActionResultWithFilters(controllerContext, filters, actionResult);
            }

            public virtual AuthorizationContext PublicInvokeAuthorizationFilters(ControllerContext controllerContext, IList<IAuthorizationFilter> filters, ActionDescriptor actionDescriptor)
            {
                return base.InvokeAuthorizationFilters(controllerContext, filters, actionDescriptor);
            }

            protected override AuthorizationContext InvokeAuthorizationFilters(ControllerContext controllerContext, IList<IAuthorizationFilter> filters, ActionDescriptor actionDescriptor)
            {
                return PublicInvokeAuthorizationFilters(controllerContext, filters, actionDescriptor);
            }

            public virtual AuthenticationContext PublicInvokeAuthenticationFilters(ControllerContext controllerContext, IList<IAuthenticationFilter> filters, ActionDescriptor actionDescriptor)
            {
                return base.InvokeAuthenticationFilters(controllerContext, filters, actionDescriptor);
            }

            protected override AuthenticationContext InvokeAuthenticationFilters(ControllerContext controllerContext, IList<IAuthenticationFilter> filters, ActionDescriptor actionDescriptor)
            {
                return PublicInvokeAuthenticationFilters(controllerContext, filters, actionDescriptor);
            }

            public virtual AuthenticationChallengeContext PublicInvokeAuthenticationFiltersChallenge(ControllerContext controllerContext, IList<IAuthenticationFilter> filters, ActionDescriptor actionDescriptor, ActionResult result)
            {
                return base.InvokeAuthenticationFiltersChallenge(controllerContext, filters, actionDescriptor, result);
            }

            protected override AuthenticationChallengeContext InvokeAuthenticationFiltersChallenge(ControllerContext controllerContext, IList<IAuthenticationFilter> filters, ActionDescriptor actionDescriptor, ActionResult result)
            {
                return PublicInvokeAuthenticationFiltersChallenge(controllerContext, filters, actionDescriptor, result);
            }

            public virtual ExceptionContext PublicInvokeExceptionFilters(ControllerContext controllerContext, IList<IExceptionFilter> filters, Exception exception)
            {
                return base.InvokeExceptionFilters(controllerContext, filters, exception);
            }

            protected override ExceptionContext InvokeExceptionFilters(ControllerContext controllerContext, IList<IExceptionFilter> filters, Exception exception)
            {
                return PublicInvokeExceptionFilters(controllerContext, filters, exception);
            }
        }

        public class AuthorizationFilterHelper : IAuthorizationFilter
        {
            private IList<AuthorizationFilterHelper> _callQueue;
            public ActionResult ShortCircuitResult;

            public AuthorizationFilterHelper(IList<AuthorizationFilterHelper> callQueue)
            {
                _callQueue = callQueue;
            }

            public void OnAuthorization(AuthorizationContext filterContext)
            {
                _callQueue.Add(this);
                if (ShortCircuitResult != null)
                {
                    filterContext.Result = ShortCircuitResult;
                }
            }
        }

        public class AuthenticationFilterHelper : IAuthenticationFilter
        {
            private IList<AuthenticationFilterHelper> _callQueue;
            public ActionResult ShortCircuitResult;
            public IPrincipal Principal;

            public AuthenticationFilterHelper(IList<AuthenticationFilterHelper> callQueue)
            {
                _callQueue = callQueue;
            }

            public void OnAuthentication(AuthenticationContext filterContext)
            {
                _callQueue.Add(this);
                if (ShortCircuitResult != null)
                {
                    filterContext.Result = ShortCircuitResult;
                }
                if (Principal != null)
                {
                    filterContext.Principal = Principal;
                }
            }

            public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
            {
                _callQueue.Add(this);
            }
        }

        public class ExceptionFilterHelper : IExceptionFilter
        {
            private IList<ExceptionFilterHelper> _callQueue;
            public bool ShouldHandleException;
            public ExceptionContext ContextPassed;

            public ExceptionFilterHelper(IList<ExceptionFilterHelper> callQueue)
            {
                _callQueue = callQueue;
            }

            public void OnException(ExceptionContext filterContext)
            {
                _callQueue.Add(this);
                if (ShouldHandleException)
                {
                    filterContext.ExceptionHandled = true;
                }
                ContextPassed = filterContext;
            }
        }

        private class CustomConverterController : Controller
        {
            public void ParameterWithoutBindAttribute([PredicateReflector] string someParam)
            {
            }

            public void ParameterHasBindAttribute([Bind(Include = "foo"), PredicateReflector] string someParam)
            {
            }

            public void ParameterHasDefaultValueAttribute([DefaultValue(42)] int foo)
            {
            }

            public void ParameterHasFieldPrefix([Bind(Prefix = "bar")] string foo)
            {
            }

            public void ParameterHasNullFieldPrefix([Bind(Include = "whatever")] string foo)
            {
            }

            public void ParameterHasEmptyFieldPrefix([Bind(Prefix = "")] MySimpleModel foo)
            {
            }

            public void ParameterHasNoPrefixAndComplexType(MySimpleModel foo)
            {
            }

            public void ParameterHasPrefixAndComplexType([Bind(Prefix = "badprefix")] MySimpleModel foo)
            {
            }

            public void ParameterHasNoConverters(string foo)
            {
            }

            public void ParameterHasOneConverter([MyCustomConverter] string foo)
            {
            }

            public void ParameterHasTwoConverters([MyCustomConverter, MyCustomConverter] string foo)
            {
            }
        }

        public class MySimpleModel
        {
            public int IntProp { get; set; }
            public string StringProp { get; set; }
        }

        [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true, Inherited = false)]
        private class PredicateReflectorAttribute : CustomModelBinderAttribute
        {
            public override IModelBinder GetBinder()
            {
                return new MyConverter();
            }

            private class MyConverter : IModelBinder
            {
                public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
                {
                    string s = String.Format("foo={0}&bar={1}", bindingContext.PropertyFilter("foo"), bindingContext.PropertyFilter("bar"));
                    return s;
                }
            }
        }

        [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true, Inherited = false)]
        private class MyCustomConverterAttribute : CustomModelBinderAttribute
        {
            public override IModelBinder GetBinder()
            {
                return new MyConverter();
            }

            private class MyConverter : IModelBinder
            {
                public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
                {
                    string s = bindingContext.ModelName + "_" + bindingContext.ModelType.Name;
                    return s;
                }
            }
        }

        // helper class for making sure that we're performing culture-invariant string conversions
        public class CultureReflector : IFormattable
        {
            string IFormattable.ToString(string format, IFormatProvider formatProvider)
            {
                CultureInfo cInfo = (CultureInfo)formatProvider;
                return cInfo.ThreeLetterISOLanguageName;
            }
        }

        private sealed class ThreadCurrentPrincipalContext : IDisposable
        {
            private readonly IPrincipal _originalPrincipal;

            private bool _disposed;

            public ThreadCurrentPrincipalContext()
            {
                _originalPrincipal = Thread.CurrentPrincipal;
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    Thread.CurrentPrincipal = _originalPrincipal;
                    _disposed = true;
                }
            }
        }

        private class WithRoutingAttributeController : Controller
        {
            [Route("route")]
            [ActionName("Action")] // to make things confusing
            public void ActionWithoutRoute()
            {
            }

            [Route("route")]
            public void Action()
            {
            }
        }

        [Route("controller/{action}")]
        private class AttributeRoutingOnTheController : Controller
        {
            public void Action1()
            {
            }

            public void Action2()
            {
            }

            public void Action2(string name)
            {
            }
        }

        private class MatchAttribute : ActionMethodSelectorAttribute
        {
            private bool _match;

            public MatchAttribute(bool match)
            {
                _match = match;
            }

            public override bool IsValidForRequest(ControllerContext controllerContext, MethodInfo methodInfo)
            {
                return _match;
            }
        }
    }
}
