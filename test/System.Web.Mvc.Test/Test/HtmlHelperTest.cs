// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Routing;
using System.Web.WebPages.Scope;
using Microsoft.TestCommon;
using Microsoft.Web.UnitTestUtil;
using Moq;

namespace System.Web.Mvc.Test
{
    [Xunit.Collection("Uses ScopeStorage or ViewEngines.Engines")]
    public class HtmlHelperTest : IDisposable
    {
        public static readonly RouteValueDictionary AttributesDictionary = new RouteValueDictionary(new { baz = "BazValue" });
        public static readonly object AttributesObjectDictionary = new { baz = "BazObjValue" };
        public static readonly object AttributesObjectUnderscoresDictionary = new { foo_baz = "BazObjValue" };

        // Constructor

        [Fact]
        public void ConstructorGuardClauses()
        {
            // Arrange
            var viewContext = new Mock<ViewContext>().Object;
            var viewDataContainer = MvcHelper.GetViewDataContainer(null);

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => new HtmlHelper(null, viewDataContainer),
                "viewContext"
                );
            Assert.ThrowsArgumentNull(
                () => new HtmlHelper(viewContext, null),
                "viewDataContainer"
                );
            Assert.ThrowsArgumentNull(
                () => new HtmlHelper(viewContext, viewDataContainer, null),
                "routeCollection"
                );
        }

        [Fact]
        public void PropertiesAreSet()
        {
            // Arrange
            var viewContext = new Mock<ViewContext>().Object;
            var viewData = new ViewDataDictionary<String>("The Model");
            var routes = new RouteCollection();
            var mockViewDataContainer = new Mock<IViewDataContainer>();
            mockViewDataContainer.Setup(vdc => vdc.ViewData).Returns(viewData);

            // Act
            var htmlHelper = new HtmlHelper(viewContext, mockViewDataContainer.Object, routes);

            // Assert
            Assert.Equal(viewContext, htmlHelper.ViewContext);
            Assert.Equal(mockViewDataContainer.Object, htmlHelper.ViewDataContainer);
            Assert.Equal(routes, htmlHelper.RouteCollection);
            Assert.Equal(viewData.Model, htmlHelper.ViewData.Model);
        }

        [Fact]
        public void DefaultRouteCollectionIsRouteTableRoutes()
        {
            // Arrange
            var viewContext = new Mock<ViewContext>().Object;
            var viewDataContainer = new Mock<IViewDataContainer>().Object;

            // Act
            var htmlHelper = new HtmlHelper(viewContext, viewDataContainer);

            // Assert
            Assert.Equal(RouteTable.Routes, htmlHelper.RouteCollection);
        }

        // AnonymousObjectToHtmlAttributes tests

        [Fact]
        public void ConvertsUnderscoresInNamesToDashes()
        {
            // Arrange
            var attributes = new { foo = "Bar", baz_bif = "pow_wow" };

            // Act
            RouteValueDictionary result = HtmlHelper.AnonymousObjectToHtmlAttributes(attributes);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("Bar", result["foo"]);
            Assert.Equal("pow_wow", result["baz-bif"]);
        }

        // AttributeEncode

        [Fact]
        public void AttributeEncodeObject()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            string encodedHtml = htmlHelper.AttributeEncode((object)@"<"">");

            // Assert
            Assert.Equal("&lt;&quot;>", encodedHtml);
        }

        [Fact]
        public void AttributeEncodeObjectNull()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            string encodedHtml = htmlHelper.AttributeEncode((object)null);

            // Assert
            Assert.Equal("", encodedHtml);
        }

        [Fact]
        public void AttributeEncodeString()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            string encodedHtml = htmlHelper.AttributeEncode(@"<"">");

            // Assert
            Assert.Equal("&lt;&quot;>", encodedHtml);
        }

        [Fact]
        public void AttributeEncodeStringNull()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            string encodedHtml = htmlHelper.AttributeEncode((string)null);

            // Assert
            Assert.Equal("", encodedHtml);
        }

        // EnableClientValidation

        [Fact]
        public void EnableClientValidation()
        {
            // Arrange
            var mockViewContext = new Mock<ViewContext>();
            var viewDataContainer = new Mock<IViewDataContainer>().Object;
            var htmlHelper = new HtmlHelper(mockViewContext.Object, viewDataContainer);

            // Act
            htmlHelper.EnableClientValidation();

            // Act & assert
            mockViewContext.VerifySet(vc => vc.ClientValidationEnabled = true);
        }

        // EnableUnobtrusiveJavaScript

        [Fact]
        public void EnableUnobtrusiveJavaScript()
        {
            // Arrange
            var mockViewContext = new Mock<ViewContext>();
            var viewDataContainer = new Mock<IViewDataContainer>().Object;
            var htmlHelper = new HtmlHelper(mockViewContext.Object, viewDataContainer);

            // Act
            htmlHelper.EnableUnobtrusiveJavaScript();

            // Act & assert
            mockViewContext.VerifySet(vc => vc.UnobtrusiveJavaScriptEnabled = true);
        }

        // Encode

        [Fact]
        public void EncodeObject()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            string encodedHtml = htmlHelper.Encode((object)"<br />");

            // Assert
            Assert.Equal("&lt;br /&gt;", encodedHtml);
        }

        [Fact]
        public void EncodeObjectNull()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            string encodedHtml = htmlHelper.Encode((object)null);

            // Assert
            Assert.Equal("", encodedHtml);
        }

        [Fact]
        public void EncodeString()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            string encodedHtml = htmlHelper.Encode("<br />");

            // Assert
            Assert.Equal("&lt;br /&gt;", encodedHtml);
        }

        [Fact]
        public void EncodeStringNull()
        {
            // Arrange
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper();

            // Act
            string encodedHtml = htmlHelper.Encode((string)null);

            // Assert
            Assert.Equal("", encodedHtml);
        }

        // GetModelStateValue

        [Fact]
        public void GetModelStateValueReturnsNullIfModelStateHasNoValue()
        {
            // Arrange
            ViewDataDictionary vdd = new ViewDataDictionary();
            vdd.ModelState.AddModelError("foo", "some error text"); // didn't call SetModelValue()

            HtmlHelper helper = new HtmlHelper(new ViewContext(), new SimpleViewDataContainer(vdd));

            // Act
            object retVal = helper.GetModelStateValue("foo", typeof(object));

            // Assert
            Assert.Null(retVal);
        }

        [Fact]
        public void GetModelStateValueReturnsNullIfModelStateKeyNotPresent()
        {
            // Arrange
            ViewDataDictionary vdd = new ViewDataDictionary();
            HtmlHelper helper = new HtmlHelper(new ViewContext(), new SimpleViewDataContainer(vdd));

            // Act
            object retVal = helper.GetModelStateValue("key_not_present", typeof(object));

            // Assert
            Assert.Null(retVal);
        }

        // GenerateIdFromName

        [Fact]
        public void GenerateIdFromNameTests()
        {
            // Guard clauses
            Assert.ThrowsArgumentNull(
                () => HtmlHelper.GenerateIdFromName(null),
                "name"
                );
            Assert.ThrowsArgumentNull(
                () => HtmlHelper.GenerateIdFromName(null, "?"),
                "name"
                );
            Assert.ThrowsArgumentNull(
                () => HtmlHelper.GenerateIdFromName("?", null),
                "idAttributeDotReplacement"
                );

            // Default replacement tests
            Assert.Equal("", HtmlHelper.GenerateIdFromName(""));
            Assert.Equal("Foo", HtmlHelper.GenerateIdFromName("Foo"));
            Assert.Equal("Foo_Bar", HtmlHelper.GenerateIdFromName("Foo.Bar"));
            Assert.Equal("Foo_Bar_Baz", HtmlHelper.GenerateIdFromName("Foo.Bar.Baz"));
            Assert.Null(HtmlHelper.GenerateIdFromName("1Foo"));
            Assert.Equal("Foo_0_", HtmlHelper.GenerateIdFromName("Foo[0]"));

            // Custom replacement tests
            Assert.Equal("", HtmlHelper.GenerateIdFromName("", "?"));
            Assert.Equal("Foo", HtmlHelper.GenerateIdFromName("Foo", "?"));
            Assert.Equal("Foo?Bar", HtmlHelper.GenerateIdFromName("Foo.Bar", "?"));
            Assert.Equal("Foo?Bar?Baz", HtmlHelper.GenerateIdFromName("Foo.Bar.Baz", "?"));
            Assert.Equal("FooBarBaz", HtmlHelper.GenerateIdFromName("Foo.Bar.Baz", ""));
            Assert.Null(HtmlHelper.GenerateIdFromName("1Foo", "?"));
            Assert.Equal("Foo?0?", HtmlHelper.GenerateIdFromName("Foo[0]", "?"));
        }

        // RenderPartialInternal

        [Fact]
        public void NullPartialViewNameThrows()
        {
            // Arrange
            TestableHtmlHelper helper = TestableHtmlHelper.Create();
            ViewDataDictionary viewData = new ViewDataDictionary();

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                () => helper.RenderPartialInternal(null /* partialViewName */, null /* viewData */, null /* model */, TextWriter.Null),
                "partialViewName");
        }

        [Fact]
        public void EmptyPartialViewNameThrows()
        {
            // Arrange
            TestableHtmlHelper helper = TestableHtmlHelper.Create();
            ViewDataDictionary viewData = new ViewDataDictionary();

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                () => helper.RenderPartialInternal(String.Empty /* partialViewName */, null /* viewData */, null /* model */, TextWriter.Null),
                "partialViewName");
        }

        [Fact]
        public void EngineLookupSuccessCallsRender()
        {
            // Arrange
            TestableHtmlHelper helper = TestableHtmlHelper.Create();
            TextWriter writer = helper.ViewContext.Writer;
            Mock<IViewEngine> engine = new Mock<IViewEngine>(MockBehavior.Strict);
            Mock<IView> view = new Mock<IView>(MockBehavior.Strict);
            engine
                .Setup(e => e.FindPartialView(It.IsAny<ControllerContext>(), "partial-view", It.IsAny<bool>()))
                .Returns(new ViewEngineResult(view.Object, engine.Object))
                .Verifiable();
            view
                .Setup(v => v.Render(It.IsAny<ViewContext>(), writer))
                .Callback<ViewContext, TextWriter>(
                    (viewContext, _) =>
                    {
                        Assert.Same(helper.ViewContext.View, viewContext.View);
                        Assert.Same(helper.ViewContext.TempData, viewContext.TempData);
                    })
                .Verifiable();

            // Act
            helper.RenderPartialInternal("partial-view", null /* viewData */, null /* model */, writer, engine.Object);

            // Assert
            engine.Verify();
            view.Verify();
        }

        [Fact]
        public void EngineLookupFailureThrows()
        {
            // Arrange
            TestableHtmlHelper helper = TestableHtmlHelper.Create();
            Mock<IViewEngine> engine = new Mock<IViewEngine>(MockBehavior.Strict);
            engine
                .Setup(e => e.FindPartialView(It.IsAny<ControllerContext>(), "partial-view", It.IsAny<bool>()))
                .Returns(new ViewEngineResult(new[] { "location1", "location2" }))
                .Verifiable();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => helper.RenderPartialInternal("partial-view", null /* viewData */, null /* model */, TextWriter.Null, engine.Object),
                "The partial view 'partial-view' was not found or no view engine supports the searched locations. The following locations were searched:" + Environment.NewLine
              + "location1" + Environment.NewLine
              + "location2");

            engine.Verify();
        }

        [Fact]
        public void RenderPartialInternalWithNullModelAndNullViewData()
        {
            // Arrange
            object model = new object();
            TestableHtmlHelper helper = TestableHtmlHelper.Create();
            helper.ViewData["Foo"] = "Bar";
            helper.ViewData.Model = model;
            Mock<IViewEngine> engine = new Mock<IViewEngine>(MockBehavior.Strict);
            Mock<IView> view = new Mock<IView>(MockBehavior.Strict);
            engine
                .Setup(e => e.FindPartialView(It.IsAny<ControllerContext>(), "partial-view", It.IsAny<bool>()))
                .Returns(new ViewEngineResult(view.Object, engine.Object))
                .Verifiable();
            view
                .Setup(v => v.Render(It.IsAny<ViewContext>(), TextWriter.Null))
                .Callback<ViewContext, TextWriter>(
                    (viewContext, writer) =>
                    {
                        Assert.NotSame(helper.ViewData, viewContext.ViewData); // New view data instance
                        Assert.Equal("Bar", viewContext.ViewData["Foo"]); // Copy of the existing view data
                        Assert.Same(model, viewContext.ViewData.Model); // Keep existing model
                    })
                .Verifiable();

            // Act
            helper.RenderPartialInternal("partial-view", null /* viewData */, null /* model */, TextWriter.Null, engine.Object);

            // Assert
            engine.Verify();
            view.Verify();
        }

        [Fact]
        public void RenderPartialInternalWithNonNullModelAndNullViewData()
        {
            // Arrange
            object model = new object();
            object newModel = new object();
            TestableHtmlHelper helper = TestableHtmlHelper.Create();
            helper.ViewData["Foo"] = "Bar";
            helper.ViewData.Model = model;
            Mock<IViewEngine> engine = new Mock<IViewEngine>(MockBehavior.Strict);
            Mock<IView> view = new Mock<IView>(MockBehavior.Strict);
            engine
                .Setup(e => e.FindPartialView(It.IsAny<ControllerContext>(), "partial-view", It.IsAny<bool>()))
                .Returns(new ViewEngineResult(view.Object, engine.Object))
                .Verifiable();
            view
                .Setup(v => v.Render(It.IsAny<ViewContext>(), TextWriter.Null))
                .Callback<ViewContext, TextWriter>(
                    (viewContext, writer) =>
                    {
                        Assert.NotSame(helper.ViewData, viewContext.ViewData); // New view data instance
                        Assert.Empty(viewContext.ViewData); // Empty (not copied)
                        Assert.Same(newModel, viewContext.ViewData.Model); // New model
                    })
                .Verifiable();

            // Act
            helper.RenderPartialInternal("partial-view", null /* viewData */, newModel, TextWriter.Null, engine.Object);

            // Assert
            engine.Verify();
            view.Verify();
        }

        [Fact]
        public void RenderPartialInternalWithNullModelAndNonNullViewData()
        {
            // Arrange
            object model = new object();
            object vddModel = new object();
            ViewDataDictionary vdd = new ViewDataDictionary();
            vdd["Baz"] = "Biff";
            vdd.Model = vddModel;
            TestableHtmlHelper helper = TestableHtmlHelper.Create();
            helper.ViewData["Foo"] = "Bar";
            helper.ViewData.Model = model;
            Mock<IViewEngine> engine = new Mock<IViewEngine>(MockBehavior.Strict);
            Mock<IView> view = new Mock<IView>(MockBehavior.Strict);
            engine
                .Setup(e => e.FindPartialView(It.IsAny<ControllerContext>(), "partial-view", It.IsAny<bool>()))
                .Returns(new ViewEngineResult(view.Object, engine.Object))
                .Verifiable();
            view
                .Setup(v => v.Render(It.IsAny<ViewContext>(), TextWriter.Null))
                .Callback<ViewContext, TextWriter>(
                    (viewContext, writer) =>
                    {
                        Assert.NotSame(helper.ViewData, viewContext.ViewData); // New view data instance
                        Assert.Single(viewContext.ViewData); // Copy of the passed view data, not original view data
                        Assert.Equal("Biff", viewContext.ViewData["Baz"]);
                        Assert.Same(vddModel, viewContext.ViewData.Model); // Keep model from passed view data, not original view data
                    })
                .Verifiable();

            // Act
            helper.RenderPartialInternal("partial-view", vdd, null /* model */, TextWriter.Null, engine.Object);

            // Assert
            engine.Verify();
            view.Verify();
        }

        [Fact]
        public void RenderPartialInternalWithNonNullModelAndNonNullViewData()
        {
            // Arrange
            object model = new object();
            object vddModel = new object();
            object newModel = new object();
            ViewDataDictionary vdd = new ViewDataDictionary();
            vdd["Baz"] = "Biff";
            vdd.Model = vddModel;
            TestableHtmlHelper helper = TestableHtmlHelper.Create();
            helper.ViewData["Foo"] = "Bar";
            helper.ViewData.Model = model;
            Mock<IViewEngine> engine = new Mock<IViewEngine>(MockBehavior.Strict);
            Mock<IView> view = new Mock<IView>(MockBehavior.Strict);
            engine
                .Setup(e => e.FindPartialView(It.IsAny<ControllerContext>(), "partial-view", It.IsAny<bool>()))
                .Returns(new ViewEngineResult(view.Object, engine.Object))
                .Verifiable();
            view
                .Setup(v => v.Render(It.IsAny<ViewContext>(), TextWriter.Null))
                .Callback<ViewContext, TextWriter>(
                    (viewContext, writer) =>
                    {
                        Assert.NotSame(helper.ViewData, viewContext.ViewData); // New view data instance
                        Assert.Single(viewContext.ViewData); // Copy of the passed view data, not original view data
                        Assert.Equal("Biff", viewContext.ViewData["Baz"]);
                        Assert.Same(newModel, viewContext.ViewData.Model); // New model
                    })
                .Verifiable();

            // Act
            helper.RenderPartialInternal("partial-view", vdd, newModel, TextWriter.Null, engine.Object);

            // Assert
            engine.Verify();
            view.Verify();
        }

        // HttpMethodOverride

        [Fact]
        public void HttpMethodOverrideGuardClauses()
        {
            // Arrange
            var viewContext = new Mock<ViewContext>().Object;
            var viewDataContainer = MvcHelper.GetViewDataContainer(null);
            var htmlHelper = new HtmlHelper(viewContext, viewDataContainer);

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                () => htmlHelper.HttpMethodOverride(null),
                "httpMethod"
                );
            Assert.Throws<ArgumentException>(
                () => htmlHelper.HttpMethodOverride((HttpVerbs)10000),
                "The specified HttpVerbs value is not supported. The supported values are Delete, Head, Put, Options, and Patch." + Environment.NewLine
              + "Parameter name: httpVerb"
                );
            Assert.Throws<ArgumentException>(
                () => htmlHelper.HttpMethodOverride(HttpVerbs.Get),
                "The specified HttpVerbs value is not supported. The supported values are Delete, Head, Put, Options, and Patch." + Environment.NewLine
              + "Parameter name: httpVerb"
                );
            Assert.Throws<ArgumentException>(
                () => htmlHelper.HttpMethodOverride(HttpVerbs.Post),
                "The specified HttpVerbs value is not supported. The supported values are Delete, Head, Put, Options, and Patch." + Environment.NewLine
              + "Parameter name: httpVerb"
                );
            Assert.Throws<ArgumentException>(
                () => htmlHelper.HttpMethodOverride("gEt"),
                "The GET and POST HTTP methods are not supported." + Environment.NewLine
              + "Parameter name: httpMethod"
                );
            Assert.Throws<ArgumentException>(
                () => htmlHelper.HttpMethodOverride("pOsT"),
                "The GET and POST HTTP methods are not supported." + Environment.NewLine
              + "Parameter name: httpMethod"
                );
        }

        [Fact]
        public void HttpMethodOverrideWithMethodRendersHiddenField()
        {
            // Arrange
            var viewContext = new Mock<ViewContext>().Object;
            var viewDataContainer = MvcHelper.GetViewDataContainer(null);
            var htmlHelper = new HtmlHelper(viewContext, viewDataContainer);

            // Act
            MvcHtmlString hiddenField = htmlHelper.HttpMethodOverride("PUT");

            // Assert
            Assert.Equal(@"<input name=""X-HTTP-Method-Override"" type=""hidden"" value=""PUT"" />", hiddenField.ToHtmlString());
        }

        [Fact]
        public void HttpMethodOverrideWithVerbRendersHiddenField()
        {
            // Arrange
            var viewContext = new Mock<ViewContext>().Object;
            var viewDataContainer = MvcHelper.GetViewDataContainer(null);
            var htmlHelper = new HtmlHelper(viewContext, viewDataContainer);

            // Act
            MvcHtmlString hiddenField = htmlHelper.HttpMethodOverride(HttpVerbs.Delete);

            // Assert
            Assert.Equal(@"<input name=""X-HTTP-Method-Override"" type=""hidden"" value=""DELETE"" />", hiddenField.ToHtmlString());
        }

        [Fact]
        public void ViewBagProperty_ReflectsViewData()
        {
            // Arrange
            Mock<IViewDataContainer> viewDataContainer = new Mock<IViewDataContainer>();
            ViewDataDictionary viewDataDictionary = new ViewDataDictionary() { { "A", 1 } };
            viewDataContainer.Setup(container => container.ViewData).Returns(viewDataDictionary);

            // Act
            HtmlHelper htmlHelper = new HtmlHelper(new Mock<ViewContext>().Object, viewDataContainer.Object);

            // Assert
            Assert.Equal(1, htmlHelper.ViewBag.A);
        }

        [Fact]
        public void ViewBagProperty_ReflectsNewViewDataContainerInstance()
        {
            // Arrange
            ViewDataDictionary viewDataDictionary = new ViewDataDictionary() { { "A", 1 } };
            Mock<IViewDataContainer> viewDataContainer = new Mock<IViewDataContainer>();
            viewDataContainer.Setup(container => container.ViewData).Returns(viewDataDictionary);

            ViewDataDictionary otherViewDataDictionary = new ViewDataDictionary() { { "A", 2 } };
            Mock<IViewDataContainer> otherViewDataContainer = new Mock<IViewDataContainer>();
            otherViewDataContainer.Setup(container => container.ViewData).Returns(otherViewDataDictionary);

            HtmlHelper htmlHelper = new HtmlHelper(new Mock<ViewContext>().Object, viewDataContainer.Object, new RouteCollection());

            // Act
            htmlHelper.ViewDataContainer = otherViewDataContainer.Object;

            // Assert
            Assert.Equal(2, htmlHelper.ViewBag.A);
        }

        [Fact]
        public void ViewBag_PropagatesChangesToViewData()
        {
            // Arrange
            ViewDataDictionary viewDataDictionary = new ViewDataDictionary() { { "A", 1 } };
            Mock<IViewDataContainer> viewDataContainer = new Mock<IViewDataContainer>();
            viewDataContainer.Setup(container => container.ViewData).Returns(viewDataDictionary);

            HtmlHelper htmlHelper = new HtmlHelper(new Mock<ViewContext>().Object, viewDataContainer.Object, new RouteCollection());

            // Act
            htmlHelper.ViewBag.A = "foo";
            htmlHelper.ViewBag.B = 2;

            // Assert
            Assert.Equal("foo", htmlHelper.ViewData["A"]);
            Assert.Equal(2, htmlHelper.ViewData["B"]);
        }

        // Unobtrusive validation attributes

        [Fact]
        public void GetUnobtrusiveValidationAttributesReturnsEmptySetWhenClientValidationIsNotEnabled()
        {
            // Arrange
            var formContext = new FormContext();
            formContext.RenderedField("foobar", true);
            var viewContext = new Mock<ViewContext>();
            viewContext.SetupGet(vc => vc.FormContext).Returns(formContext);
            var viewDataContainer = MvcHelper.GetViewDataContainer(new ViewDataDictionary());
            var htmlHelper = new HtmlHelper(viewContext.Object, viewDataContainer);

            // Act
            IDictionary<string, object> result = htmlHelper.GetUnobtrusiveValidationAttributes("foobar");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetUnobtrusiveValidationAttributesReturnsEmptySetWhenUnobtrusiveJavaScriptIsNotEnabled()
        {
            // Arrange
            var formContext = new FormContext();
            formContext.RenderedField("foobar", true);
            var viewContext = new Mock<ViewContext>();
            viewContext.SetupGet(vc => vc.FormContext).Returns(formContext);
            viewContext.SetupGet(vc => vc.ClientValidationEnabled).Returns(true);
            var viewDataContainer = MvcHelper.GetViewDataContainer(new ViewDataDictionary());
            var htmlHelper = new HtmlHelper(viewContext.Object, viewDataContainer);

            // Act
            IDictionary<string, object> result = htmlHelper.GetUnobtrusiveValidationAttributes("foobar");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetUnobtrusiveValidationAttributesReturnsEmptySetWhenFieldHasAlreadyBeenRendered()
        {
            // Arrange
            var formContext = new FormContext();
            formContext.RenderedField("foobar", true);
            var viewContext = new Mock<ViewContext>();
            viewContext.SetupGet(vc => vc.FormContext).Returns(formContext);
            viewContext.SetupGet(vc => vc.ClientValidationEnabled).Returns(true);
            viewContext.SetupGet(vc => vc.UnobtrusiveJavaScriptEnabled).Returns(true);
            var viewDataContainer = MvcHelper.GetViewDataContainer(new ViewDataDictionary());
            var htmlHelper = new HtmlHelper(viewContext.Object, viewDataContainer);

            // Act
            IDictionary<string, object> result = htmlHelper.GetUnobtrusiveValidationAttributes("foobar");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetUnobtrusiveValidationAttributesReturnsEmptySetAndSetsFieldAsRenderedForFieldWithNoClientRules()
        {
            // Arrange
            var formContext = new FormContext();
            var viewContext = new Mock<ViewContext>();
            viewContext.SetupGet(vc => vc.FormContext).Returns(formContext);
            viewContext.SetupGet(vc => vc.ClientValidationEnabled).Returns(true);
            viewContext.SetupGet(vc => vc.UnobtrusiveJavaScriptEnabled).Returns(true);
            var viewDataContainer = MvcHelper.GetViewDataContainer(new ViewDataDictionary());
            var htmlHelper = new HtmlHelper(viewContext.Object, viewDataContainer);
            htmlHelper.ClientValidationRuleFactory = delegate { return Enumerable.Empty<ModelClientValidationRule>(); };

            // Act
            IDictionary<string, object> result = htmlHelper.GetUnobtrusiveValidationAttributes("foobar");

            // Assert
            Assert.Empty(result);
            Assert.True(formContext.RenderedField("foobar"));
        }

        [Fact]
        public void GetUnobtrusiveValidationAttributesIncludesDataValTrueWithNonEmptyClientRuleList()
        {
            // Arrange
            var formContext = new FormContext();
            var viewContext = new Mock<ViewContext>();
            viewContext.SetupGet(vc => vc.FormContext).Returns(formContext);
            viewContext.SetupGet(vc => vc.ClientValidationEnabled).Returns(true);
            viewContext.SetupGet(vc => vc.UnobtrusiveJavaScriptEnabled).Returns(true);
            var viewDataContainer = MvcHelper.GetViewDataContainer(new ViewDataDictionary());
            var htmlHelper = new HtmlHelper(viewContext.Object, viewDataContainer);
            htmlHelper.ClientValidationRuleFactory = delegate { return new[] { new ModelClientValidationRule { ValidationType = "type" } }; };

            // Act
            IDictionary<string, object> result = htmlHelper.GetUnobtrusiveValidationAttributes("foobar");

            // Assert
            Assert.Equal("true", result["data-val"]);
        }

        [Fact]
        public void GetUnobtrusiveValidationAttributesWithEmptyMessage()
        {
            // Arrange
            var formContext = new FormContext();
            var viewContext = new Mock<ViewContext>();
            viewContext.SetupGet(vc => vc.FormContext).Returns(formContext);
            viewContext.SetupGet(vc => vc.ClientValidationEnabled).Returns(true);
            viewContext.SetupGet(vc => vc.UnobtrusiveJavaScriptEnabled).Returns(true);
            var viewDataContainer = MvcHelper.GetViewDataContainer(new ViewDataDictionary());
            var htmlHelper = new HtmlHelper(viewContext.Object, viewDataContainer);
            htmlHelper.ClientValidationRuleFactory = delegate { return new[] { new ModelClientValidationRule { ValidationType = "type" } }; };

            // Act
            IDictionary<string, object> result = htmlHelper.GetUnobtrusiveValidationAttributes("foobar");

            // Assert
            Assert.Equal("", result["data-val-type"]);
        }

        [Fact]
        public void GetUnobtrusiveValidationAttributesMessageIsNotHtmlEncoded()
        {
            // Arrange
            var formContext = new FormContext();
            var viewContext = new Mock<ViewContext>();
            viewContext.SetupGet(vc => vc.FormContext).Returns(formContext);
            viewContext.SetupGet(vc => vc.ClientValidationEnabled).Returns(true);
            viewContext.SetupGet(vc => vc.UnobtrusiveJavaScriptEnabled).Returns(true);
            var viewDataContainer = MvcHelper.GetViewDataContainer(new ViewDataDictionary());
            var htmlHelper = new HtmlHelper(viewContext.Object, viewDataContainer);
            htmlHelper.ClientValidationRuleFactory = delegate { return new[] { new ModelClientValidationRule { ValidationType = "type", ErrorMessage = "<script>alert('xss')</script>" } }; };

            // Act
            IDictionary<string, object> result = htmlHelper.GetUnobtrusiveValidationAttributes("foobar");

            // Assert
            Assert.Equal("<script>alert('xss')</script>", result["data-val-type"]);
        }

        [Fact]
        public void GetUnobtrusiveValidationAttributesWithMessageAndParameters()
        {
            // Arrange
            var formContext = new FormContext();
            var viewContext = new Mock<ViewContext>();
            viewContext.SetupGet(vc => vc.FormContext).Returns(formContext);
            viewContext.SetupGet(vc => vc.ClientValidationEnabled).Returns(true);
            viewContext.SetupGet(vc => vc.UnobtrusiveJavaScriptEnabled).Returns(true);
            var viewDataContainer = MvcHelper.GetViewDataContainer(new ViewDataDictionary());
            var htmlHelper = new HtmlHelper(viewContext.Object, viewDataContainer);
            htmlHelper.ClientValidationRuleFactory = delegate
            {
                ModelClientValidationRule rule = new ModelClientValidationRule { ValidationType = "type", ErrorMessage = "error" };
                rule.ValidationParameters["foo"] = "bar";
                rule.ValidationParameters["baz"] = "biff";
                return new[] { rule };
            };

            // Act
            IDictionary<string, object> result = htmlHelper.GetUnobtrusiveValidationAttributes("foobar");

            // Assert
            Assert.Equal("error", result["data-val-type"]);
            Assert.Equal("bar", result["data-val-type-foo"]);
            Assert.Equal("biff", result["data-val-type-baz"]);
        }

        [Fact]
        public void GetUnobtrusiveValidationAttributesWithTwoClientRules()
        {
            // Arrange
            var formContext = new FormContext();
            var viewContext = new Mock<ViewContext>();
            viewContext.SetupGet(vc => vc.FormContext).Returns(formContext);
            viewContext.SetupGet(vc => vc.ClientValidationEnabled).Returns(true);
            viewContext.SetupGet(vc => vc.UnobtrusiveJavaScriptEnabled).Returns(true);
            var viewDataContainer = MvcHelper.GetViewDataContainer(new ViewDataDictionary());
            var htmlHelper = new HtmlHelper(viewContext.Object, viewDataContainer);
            htmlHelper.ClientValidationRuleFactory = delegate
            {
                ModelClientValidationRule rule1 = new ModelClientValidationRule { ValidationType = "type", ErrorMessage = "error" };
                rule1.ValidationParameters["foo"] = "bar";
                rule1.ValidationParameters["baz"] = "biff";
                ModelClientValidationRule rule2 = new ModelClientValidationRule { ValidationType = "othertype", ErrorMessage = "othererror" };
                rule2.ValidationParameters["true3"] = "false4";
                return new[] { rule1, rule2 };
            };

            // Act
            IDictionary<string, object> result = htmlHelper.GetUnobtrusiveValidationAttributes("foobar");

            // Assert
            Assert.Equal("error", result["data-val-type"]);
            Assert.Equal("bar", result["data-val-type-foo"]);
            Assert.Equal("biff", result["data-val-type-baz"]);
            Assert.Equal("othererror", result["data-val-othertype"]);
            Assert.Equal("false4", result["data-val-othertype-true3"]);
        }

        [Fact]
        public void GetUnobtrusiveValidationAttributesUsesShortNameForModelMetadataLookup()
        {
            // Arrange
            string passedName = null;
            var formContext = new FormContext();
            var viewContext = new Mock<ViewContext>();
            var viewData = new ViewDataDictionary();
            viewContext.SetupGet(vc => vc.FormContext).Returns(formContext);
            viewContext.SetupGet(vc => vc.ClientValidationEnabled).Returns(true);
            viewContext.SetupGet(vc => vc.UnobtrusiveJavaScriptEnabled).Returns(true);
            viewData.TemplateInfo.HtmlFieldPrefix = "Prefix";
            var viewDataContainer = MvcHelper.GetViewDataContainer(viewData);
            var htmlHelper = new HtmlHelper(viewContext.Object, viewDataContainer);
            htmlHelper.ClientValidationRuleFactory = (name, _) =>
            {
                passedName = name;
                return Enumerable.Empty<ModelClientValidationRule>();
            };

            // Act
            htmlHelper.GetUnobtrusiveValidationAttributes("foobar");

            // Assert
            Assert.Equal("foobar", passedName);
        }

        [Fact]
        public void GetUnobtrusiveValidationAttributeUsesViewDataForModelMetadataLookup()
        {
            // Arrange
            var formContext = new FormContext();
            var viewContext = new Mock<ViewContext>();
            var viewData = new ViewDataDictionary<MyModel>();
            viewContext.SetupGet(vc => vc.FormContext).Returns(formContext);
            viewContext.SetupGet(vc => vc.ClientValidationEnabled).Returns(true);
            viewContext.SetupGet(vc => vc.UnobtrusiveJavaScriptEnabled).Returns(true);
            viewData.TemplateInfo.HtmlFieldPrefix = "Prefix";
            var viewDataContainer = MvcHelper.GetViewDataContainer(viewData);
            var htmlHelper = new HtmlHelper(viewContext.Object, viewDataContainer);

            // Act
            IDictionary<string, object> result = htmlHelper.GetUnobtrusiveValidationAttributes("MyProperty");

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("true", result["data-val"]);
            Assert.Equal("My required message", result["data-val-required"]);
        }

        class MyModel
        {
            [Required(ErrorMessage = "My required message")]
            public object MyProperty { get; set; }
        }

        [Fact]
        public void GetUnobtrusiveValidationAttributesMarksRenderedFieldsWithFullName()
        {
            // Arrange
            var formContext = new FormContext();
            var viewContext = new Mock<ViewContext>();
            var viewData = new ViewDataDictionary();
            viewContext.SetupGet(vc => vc.FormContext).Returns(formContext);
            viewContext.SetupGet(vc => vc.ClientValidationEnabled).Returns(true);
            viewContext.SetupGet(vc => vc.UnobtrusiveJavaScriptEnabled).Returns(true);
            viewData.TemplateInfo.HtmlFieldPrefix = "Prefix";
            var viewDataContainer = MvcHelper.GetViewDataContainer(viewData);
            var htmlHelper = new HtmlHelper(viewContext.Object, viewDataContainer);

            // Act
            htmlHelper.GetUnobtrusiveValidationAttributes("foobar");

            // Assert
            Assert.False(formContext.RenderedField("foobar"));
            Assert.True(formContext.RenderedField("Prefix.foobar"));
        }

        [Fact]
        public void GetUnobtrusiveValidationAttributesGuardClauses()
        {
            // Arrange
            var formContext = new FormContext();
            var viewContext = new Mock<ViewContext>();
            viewContext.SetupGet(vc => vc.FormContext).Returns(formContext);
            viewContext.SetupGet(vc => vc.ClientValidationEnabled).Returns(true);
            viewContext.SetupGet(vc => vc.UnobtrusiveJavaScriptEnabled).Returns(true);
            var viewDataContainer = MvcHelper.GetViewDataContainer(new ViewDataDictionary());
            var htmlHelper = new HtmlHelper(viewContext.Object, viewDataContainer);

            // Act & Assert
            AssertBadClientValidationRule(htmlHelper, "Validation type names in unobtrusive client validation rules cannot be empty. Client rule type: System.Web.Mvc.ModelClientValidationRule", new ModelClientValidationRule());
            AssertBadClientValidationRule(htmlHelper, "Validation type names in unobtrusive client validation rules must consist of only lowercase letters. Invalid name: \"OnlyLowerCase\", client rule type: System.Web.Mvc.ModelClientValidationRule", new ModelClientValidationRule { ValidationType = "OnlyLowerCase" });
            AssertBadClientValidationRule(htmlHelper, "Validation type names in unobtrusive client validation rules must consist of only lowercase letters. Invalid name: \"nonumb3rs\", client rule type: System.Web.Mvc.ModelClientValidationRule", new ModelClientValidationRule { ValidationType = "nonumb3rs" });
            AssertBadClientValidationRule(htmlHelper, "Validation type names in unobtrusive client validation rules must be unique. The following validation type was seen more than once: rule", new ModelClientValidationRule { ValidationType = "rule" }, new ModelClientValidationRule { ValidationType = "rule" });

            var emptyParamName = new ModelClientValidationRule { ValidationType = "type" };
            emptyParamName.ValidationParameters[""] = "foo";
            AssertBadClientValidationRule(htmlHelper, "Validation parameter names in unobtrusive client validation rules cannot be empty. Client rule type: System.Web.Mvc.ModelClientValidationRule", emptyParamName);

            var paramNameMixedCase = new ModelClientValidationRule { ValidationType = "type" };
            paramNameMixedCase.ValidationParameters["MixedCase"] = "foo";
            AssertBadClientValidationRule(htmlHelper, "Validation parameter names in unobtrusive client validation rules must start with a lowercase letter and consist of only lowercase letters or digits. Validation parameter name: MixedCase, client rule type: System.Web.Mvc.ModelClientValidationRule", paramNameMixedCase);

            var paramNameStartsWithNumber = new ModelClientValidationRule { ValidationType = "type" };
            paramNameStartsWithNumber.ValidationParameters["2112"] = "foo";
            AssertBadClientValidationRule(htmlHelper, "Validation parameter names in unobtrusive client validation rules must start with a lowercase letter and consist of only lowercase letters or digits. Validation parameter name: 2112, client rule type: System.Web.Mvc.ModelClientValidationRule", paramNameStartsWithNumber);
        }

        [Fact]
        public void RawReturnsWrapperMarkup()
        {
            // Arrange
            var viewContext = new Mock<ViewContext>().Object;
            var viewDataContainer = new Mock<IViewDataContainer>().Object;
            var htmlHelper = new HtmlHelper(viewContext, viewDataContainer);
            string markup = "<b>bold</b>";

            // Act
            IHtmlString markupHtml = htmlHelper.Raw(markup);

            // Assert
            Assert.Equal("<b>bold</b>", markupHtml.ToString());
            Assert.Equal("<b>bold</b>", markupHtml.ToHtmlString());
        }

        [Fact]
        public void RawAllowsNullValue()
        {
            // Arrange
            var viewContext = new Mock<ViewContext>().Object;
            var viewDataContainer = new Mock<IViewDataContainer>().Object;
            var htmlHelper = new HtmlHelper(viewContext, viewDataContainer);

            // Act
            IHtmlString markupHtml = htmlHelper.Raw(null);

            // Assert
            Assert.Null(markupHtml.ToString());
            Assert.Null(markupHtml.ToHtmlString());
        }

        [Fact]
        public void RawAllowsNullObjectValue()
        {
            // Arrange
            var viewContext = new Mock<ViewContext>().Object;
            var viewDataContainer = new Mock<IViewDataContainer>().Object;
            var htmlHelper = new HtmlHelper(viewContext, viewDataContainer);

            // Act
            IHtmlString markupHtml = htmlHelper.Raw((object)null);

            // Assert
            Assert.Null(markupHtml.ToString());
            Assert.Null(markupHtml.ToHtmlString());
        }

        [Fact]
        public void RawAllowsEmptyValue()
        {
            // Arrange
            var viewContext = new Mock<ViewContext>().Object;
            var viewDataContainer = new Mock<IViewDataContainer>().Object;
            var htmlHelper = new HtmlHelper(viewContext, viewDataContainer);

            // Act
            IHtmlString markupHtml = htmlHelper.Raw("");

            // Assert
            Assert.Equal("", markupHtml.ToString());
            Assert.Equal("", markupHtml.ToHtmlString());
        }

        [Fact]
        public void RawReturnsWrapperMarkupOfObject()
        {
            // Arrange
            var viewContext = new Mock<ViewContext>().Object;
            var viewDataContainer = new Mock<IViewDataContainer>().Object;
            var htmlHelper = new HtmlHelper(viewContext, viewDataContainer);
            ObjectWithWrapperMarkup obj = new ObjectWithWrapperMarkup();

            // Act
            IHtmlString markupHtml = htmlHelper.Raw(obj);

            // Assert
            Assert.Equal("<b>boldFromObject</b>", markupHtml.ToString());
            Assert.Equal("<b>boldFromObject</b>", markupHtml.ToHtmlString());
        }

        [Fact]
        public void EvalStringAndFormatValueWithNullValueReturnsEmptyString()
        {
            // Arrange
            var htmlHelper = MvcHelper.GetHtmlHelper(new ViewDataDictionary());

            // Act & Assert
            Assert.Equal(String.Empty, htmlHelper.FormatValue(null, "-{0}-"));
            Assert.Equal(String.Empty, htmlHelper.EvalString("nonExistant"));
            Assert.Equal(String.Empty, htmlHelper.EvalString("nonExistant", "-{0}-"));
        }

        [Fact]
        [ReplaceCulture]
        public void EvalStringAndFormatValueUseCurrentCulture()
        {
            // Arrange
            DateTime dt = new DateTime(1900, 1, 1, 0, 0, 0);
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(new ViewDataDictionary() { { "date", dt } });
            string expectedFormattedDate = "-01/01/1900 00:00:00-";

            // Act && Assert
            Assert.Equal(expectedFormattedDate, htmlHelper.FormatValue(dt, "-{0}-"));
            Assert.Equal(expectedFormattedDate, htmlHelper.EvalString("date", "-{0}-"));
        }

        [Fact]
        [ReplaceCulture]
        public void EvalStringAndFormatValueWithEmptyFormatConvertsValueToString()
        {
            // Arrange
            DateTime dt = new DateTime(1900, 1, 1, 0, 0, 0);
            HtmlHelper htmlHelper = MvcHelper.GetHtmlHelper(new ViewDataDictionary() { { "date", dt } });
            string expectedUnformattedDate = "01/01/1900 00:00:00";

            // Act && Assert
            Assert.Equal(expectedUnformattedDate, htmlHelper.FormatValue(dt, String.Empty));
            Assert.Equal(expectedUnformattedDate, htmlHelper.EvalString("date", String.Empty));
            Assert.Equal(expectedUnformattedDate, htmlHelper.EvalString("date"));
        }


        [Fact]
        public void ObjectToDictionaryWithAnonymousTypeLooksUpProperties()
        {
            // Arrange
            object obj = new { _test = "value", oth_er = 1 };

            // Act
            IDictionary<string, object> dictValues = HtmlHelper.ObjectToDictionary(obj);

            // Assert
            Assert.NotNull(dictValues);
            Assert.Equal(2, dictValues.Count);
            Assert.Equal("value", dictValues["_test"]);
            Assert.Equal(1, dictValues["oth_er"]);
        }

        // SetValidationSummaryMessageElement

        [Fact]
        public void SetValidationSummaryMessageElement()
        {
            // Arrange
            var mockViewContext = new Mock<ViewContext>();
            var viewDataContainer = new Mock<IViewDataContainer>().Object;
            var htmlHelper = new HtmlHelper(mockViewContext.Object, viewDataContainer);

            // Act
            htmlHelper.SetValidationSummaryMessageElement("label");

            // Act & assert
            mockViewContext.VerifySet(vc => vc.ValidationSummaryMessageElement = "label");
        }

        [Fact]
        public void SetValidationMessageElement()
        {
            // Arrange
            var mockViewContext = new Mock<ViewContext>();
            var viewDataContainer = new Mock<IViewDataContainer>().Object;
            var htmlHelper = new HtmlHelper(mockViewContext.Object, viewDataContainer);

            // Act
            htmlHelper.SetValidationMessageElement("label");

            // Act & assert
            mockViewContext.VerifySet(vc => vc.ValidationMessageElement = "label");
        }

        private class ObjectWithWrapperMarkup
        {
            public override string ToString()
            {
                return "<b>boldFromObject</b>";
            }
        }

        // Helpers

        private static void AssertBadClientValidationRule(HtmlHelper htmlHelper, string expectedMessage, params ModelClientValidationRule[] rules)
        {
            htmlHelper.ClientValidationRuleFactory = delegate { return rules; };
            Assert.Throws<InvalidOperationException>(
                () => htmlHelper.GetUnobtrusiveValidationAttributes(Guid.NewGuid().ToString()),
                expectedMessage
                );
        }

        internal static ValueProviderResult GetValueProviderResult(object rawValue, string attemptedValue)
        {
            return new ValueProviderResult(rawValue, attemptedValue, CultureInfo.InvariantCulture);
        }

        private class TestableHtmlHelper : HtmlHelper
        {
            TestableHtmlHelper(ViewContext viewContext, IViewDataContainer viewDataContainer)
                : base(viewContext, viewDataContainer)
            {
            }

            public static TestableHtmlHelper Create()
            {
                ViewDataDictionary viewData = new ViewDataDictionary();

                Mock<ViewContext> mockViewContext = new Mock<ViewContext>();
                mockViewContext.Setup(c => c.HttpContext.Response.Output).Throws(new Exception("Response.Output should never be called."));
                mockViewContext.SetupGet(c => c.TempData).Returns(new TempDataDictionary());
                mockViewContext.SetupGet(c => c.View).Returns(Mock.Of<IView>());
                mockViewContext.Setup(c => c.ViewData).Returns(viewData);
                mockViewContext.Setup(c => c.Writer).Returns(new StringWriter());

                Mock<IViewDataContainer> container = new Mock<IViewDataContainer>();
                container.Setup(c => c.ViewData).Returns(viewData);

                return new TestableHtmlHelper(mockViewContext.Object, container.Object);
            }

            public void RenderPartialInternal(string partialViewName,
                                              ViewDataDictionary viewData,
                                              object model,
                                              TextWriter writer,
                                              params IViewEngine[] engines)
            {
                base.RenderPartialInternal(partialViewName, viewData, model, writer, new ViewEngineCollection(engines));
            }
        }

        public void Dispose()
        {
            // Reset ScopeStorage (written via e.g. htmlHelper.SetValidationSummaryMessageElement()) between tests to avoid unexpected interactions.
            ScopeStorage.CurrentProvider = new StaticScopeStorageProvider();
            ScopeStorage.GlobalScope.Clear();
        }
    }
}
