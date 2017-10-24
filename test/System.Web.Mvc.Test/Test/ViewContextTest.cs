// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Web.WebPages.Scope;
using System.Web.WebPages.TestUtils;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    [Xunit.Collection("Uses ScopeStorage or ViewEngines.Engines")]
    public class ViewContextTest
    {
        [Fact]
        public void GuardClauses()
        {
            // Arrange
            var controllerContext = new Mock<ControllerContext>().Object;
            var view = new Mock<IView>().Object;
            var viewData = new ViewDataDictionary();
            var tempData = new TempDataDictionary();
            var writer = new StringWriter();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => new ViewContext(null, view, viewData, tempData, writer),
                "controllerContext"
                );
            Assert.ThrowsArgumentNull(
                () => new ViewContext(controllerContext, null, viewData, tempData, writer),
                "view"
                );
            Assert.ThrowsArgumentNull(
                () => new ViewContext(controllerContext, view, null, tempData, writer),
                "viewData"
                );
            Assert.ThrowsArgumentNull(
                () => new ViewContext(controllerContext, view, viewData, null, writer),
                "tempData"
                );
            Assert.ThrowsArgumentNull(
                () => new ViewContext(controllerContext, view, viewData, tempData, null),
                "writer"
                );
        }

        [Fact]
        public void FormIdGeneratorProperty()
        {
            // Arrange
            var mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(o => o.Items).Returns(new Hashtable());
            var viewContext = new ViewContext
            {
                HttpContext = mockHttpContext.Object
            };

            // Act
            string form0Name = viewContext.FormIdGenerator();
            string form1Name = viewContext.FormIdGenerator();
            string form2Name = viewContext.FormIdGenerator();

            // Assert
            Assert.Equal("form0", form0Name);
            Assert.Equal("form1", form1Name);
            Assert.Equal("form2", form2Name);
        }

        [Fact]
        public void PropertiesAreSet()
        {
            // Arrange
            var mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(o => o.HttpContext.Items).Returns(new Hashtable());
            var view = new Mock<IView>().Object;
            var viewData = new ViewDataDictionary();
            var tempData = new TempDataDictionary();
            var writer = new StringWriter();

            // Act
            ViewContext viewContext = new ViewContext(mockControllerContext.Object, view, viewData, tempData, writer);

            // Setting FormContext to null will return the default one later
            viewContext.FormContext = null;

            // Assert
            Assert.Equal(view, viewContext.View);
            Assert.Equal(viewData, viewContext.ViewData);
            Assert.Equal(tempData, viewContext.TempData);
            Assert.Equal(writer, viewContext.Writer);
            Assert.False(viewContext.UnobtrusiveJavaScriptEnabled); // Unobtrusive JavaScript should be off by default
            Assert.NotNull(viewContext.FormContext); // We get the default FormContext
            Assert.Equal("span", viewContext.ValidationSummaryMessageElement); // gen a <span/> by default
            Assert.Equal("span", viewContext.ValidationMessageElement); // gen a <span/> by default
        }

        [Fact]
        public void ViewContextUsesScopeThunkForInstanceClientValidationFlag()
        {
            // Arrange
            var scope = new Dictionary<object, object>();
            var httpContext = new Mock<HttpContextBase>();
            var viewContext = new ViewContext { ScopeThunk = () => scope, HttpContext = httpContext.Object };
            httpContext.Setup(c => c.Items).Returns(new Hashtable());

            // Act & Assert
            Assert.False(viewContext.ClientValidationEnabled);
            viewContext.ClientValidationEnabled = true;
            Assert.True(viewContext.ClientValidationEnabled);
            Assert.Equal((object)true, scope[ViewContext.ClientValidationKeyName]);
            viewContext.ClientValidationEnabled = false;
            Assert.False(viewContext.ClientValidationEnabled);
            Assert.Equal((object)false, scope[ViewContext.ClientValidationKeyName]);
        }

        [Fact]
        public void ViewContextUsesScopeThunkForInstanceUnobstrusiveJavaScriptFlag()
        {
            // Arrange
            var scope = new Dictionary<object, object>();
            var httpContext = new Mock<HttpContextBase>();
            var viewContext = new ViewContext { ScopeThunk = () => scope, HttpContext = httpContext.Object };
            httpContext.Setup(c => c.Items).Returns(new Hashtable());

            // Act & Assert
            Assert.False(viewContext.UnobtrusiveJavaScriptEnabled);
            viewContext.UnobtrusiveJavaScriptEnabled = true;
            Assert.True(viewContext.UnobtrusiveJavaScriptEnabled);
            Assert.Equal((object)true, scope[ViewContext.UnobtrusiveJavaScriptKeyName]);
            viewContext.UnobtrusiveJavaScriptEnabled = false;
            Assert.False(viewContext.UnobtrusiveJavaScriptEnabled);
            Assert.Equal((object)false, scope[ViewContext.UnobtrusiveJavaScriptKeyName]);
        }

        [Fact]
        public void ViewContextUsesScopeThunkForValidationSummaryMessageElement()
        {
            // Arrange
            var scope = new Dictionary<object, object>();
            var httpContext = new Mock<HttpContextBase>();
            var viewContext = new ViewContext { ScopeThunk = () => scope, HttpContext = httpContext.Object };
            httpContext.Setup(c => c.Items).Returns(new Hashtable());

            // Act & Assert
            Assert.Equal("span", viewContext.ValidationSummaryMessageElement);
            viewContext.ValidationSummaryMessageElement = "h4";
            Assert.Equal("h4", viewContext.ValidationSummaryMessageElement);
            Assert.Equal("h4", scope[ViewContext.ValidationSummaryMessageElementKeyName]);
            viewContext.ValidationSummaryMessageElement = "div";
            Assert.Equal("div", viewContext.ValidationSummaryMessageElement);
            Assert.Equal("div", scope[ViewContext.ValidationSummaryMessageElementKeyName]);
        }

        [Fact]
        public void ViewContextUsesScopeThunkForValidationMessageElement()
        {
            // Arrange
            var scope = new Dictionary<object, object>();
            var httpContext = new Mock<HttpContextBase>();
            var viewContext = new ViewContext { ScopeThunk = () => scope, HttpContext = httpContext.Object };
            httpContext.Setup(c => c.Items).Returns(new Hashtable());

            // Act & Assert
            Assert.Equal("span", viewContext.ValidationMessageElement);
            viewContext.ValidationMessageElement = "h4";
            Assert.Equal("h4", viewContext.ValidationMessageElement);
            Assert.Equal("h4", scope[ViewContext.ValidationMessageElementKeyName]);
            viewContext.ValidationMessageElement = "div";
            Assert.Equal("div", viewContext.ValidationMessageElement);
            Assert.Equal("div", scope[ViewContext.ValidationMessageElementKeyName]);
        }

        [Fact]
        public void ViewContextGlobalValidationMessageElementAffectsLocalOne()
        {
            // Arrange
            AppDomainUtils.RunInSeparateAppDomain(() =>
            {
                var httpContext = new Mock<HttpContextBase>();
                ScopeStorageDictionary localScope = null;
                var globalViewContext = new ViewContext
                {
                    ScopeThunk = () => ScopeStorage.GlobalScope,
                    HttpContext = httpContext.Object
                };
                var localViewContext = new ViewContext
                {
                    ScopeThunk = () =>
                    {
                        if (localScope == null)
                        {
                            localScope = new ScopeStorageDictionary(ScopeStorage.GlobalScope);
                        };
                        return localScope;
                    },
                    HttpContext = httpContext.Object
                };
                // A ScopeCache object will be stored into the hash table but the ScopeCache class is private,
                // so we cannot get the validation message element from it for Assert.
                httpContext.Setup(c => c.Items).Returns(new Hashtable());

                // Act
                globalViewContext.ValidationMessageElement = "label";

                // Assert
                // Global element was changed from "span" to "label".
                Assert.Equal("label", HtmlHelper.ValidationMessageElement);
                Assert.Equal("label", globalViewContext.ValidationMessageElement);
                object value;
                ScopeStorage.GlobalScope.TryGetValue("ValidationMessageElement", out value);
                Assert.Equal("label", value);

                // Local element was also changed to "label".
                Assert.Equal("label", localViewContext.ValidationMessageElement);
                localScope.TryGetValue("ValidationMessageElement", out value);
                Assert.Equal("label", value);
            });
        }

        [Fact]
        public void ViewContextLocalValidationMessageElementDoesNotAffectGlobalOne()
        {
            // Arrange
            ScopeStorageDictionary localScope = null;
            var httpContext = new Mock<HttpContextBase>();
            var globalViewContext = new ViewContext
            {
                ScopeThunk = () => ScopeStorage.GlobalScope,
                HttpContext = httpContext.Object
            };
            var localViewContext = new ViewContext
            {
                ScopeThunk = () =>
                {
                    if (localScope == null)
                    {
                        localScope = new ScopeStorageDictionary(ScopeStorage.GlobalScope);
                    }
                    return localScope;
                },
                HttpContext = httpContext.Object
            };
            // A ScopeCache object will be stored into the hash table but the ScopeCache class is private,
            // so we cannot get the validation message element from it for Assert.
            httpContext.Setup(c => c.Items).Returns(new Hashtable());

            // Act & Assert
            // Local element will be changed from "span" to "h4".
            Assert.Equal("span", localViewContext.ValidationMessageElement);
            localViewContext.ValidationMessageElement = "h4";
            Assert.Equal("h4", localViewContext.ValidationMessageElement);
            object value;
            localScope.TryGetValue("ValidationMessageElement", out value);
            Assert.Equal("h4", value);

            // Global element is still "span".
            Assert.Equal("span", globalViewContext.ValidationMessageElement);
            Assert.Empty(ScopeStorage.GlobalScope);
            Assert.Equal("span", HtmlHelper.ValidationMessageElement);
        }

        [Fact]
        public void ViewBagProperty_ReflectsViewData()
        {
            // Arrange
            var mockControllerContext = new Mock<ControllerContext>();
            var view = new Mock<IView>().Object;
            var viewData = new ViewDataDictionary() { { "A", 1 } };

            // Act
            ViewContext viewContext = new ViewContext(mockControllerContext.Object, view, viewData, new TempDataDictionary(), new StringWriter());

            // Assert
            Assert.Equal(1, viewContext.ViewBag.A);
        }

        [Fact]
        public void ViewBagProperty_ReflectsNewViewDataInstance()
        {
            // Arrange
            var mockControllerContext = new Mock<ControllerContext>();
            var view = new Mock<IView>().Object;
            var viewData = new ViewDataDictionary() { { "A", 1 } };

            ViewContext viewContext = new ViewContext(mockControllerContext.Object, view, viewData, new TempDataDictionary(), new StringWriter());

            // Act
            viewContext.ViewData = new ViewDataDictionary() { { "A", "bar" } };

            // Assert
            Assert.Equal("bar", viewContext.ViewBag.A);
        }

        [Fact]
        public void ViewBag_PropagatesChangesToViewData()
        {
            // Arrange
            var mockControllerContext = new Mock<ControllerContext>();
            var view = new Mock<IView>().Object;
            var viewData = new ViewDataDictionary() { { "A", 1 } };

            ViewContext viewContext = new ViewContext(mockControllerContext.Object, view, viewData, new TempDataDictionary(), new StringWriter());

            // Act
            viewContext.ViewBag.A = "foo";
            viewContext.ViewBag.B = 2;

            // Assert
            Assert.Equal("foo", viewContext.ViewData["A"]);
            Assert.Equal(2, viewContext.ViewData["B"]);
        }
    }
}
