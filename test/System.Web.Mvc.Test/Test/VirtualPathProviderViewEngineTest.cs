// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using System.Web.Routing;
using System.Web.WebPages;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class VirtualPathProviderViewEngineTest : IDisposable
    {
        private ControllerContext _context = CreateContext();
        private ControllerContext _mobileContext = CreateContext(isMobileDevice: true);
        private TestableVirtualPathProviderViewEngine _engine = new TestableVirtualPathProviderViewEngine();

        public void Dispose()
        {
            // If any mock failures get reported in this method they might mask other failures that occurred in the main test body.
            // If you are seeing any test failures, try commenting out these lines first to aid in debugging.
            _engine.MockCache.Verify();
            _engine.MockPathProvider.Verify();
        }

        [Fact]
        public void CreateCacheKey_IncludesAssemblyName()
        {
            // Arrange
            var engine = new DerivedVirtualPathProviderViewEngine();

            // Act
            var key = engine.CreateCacheKey("prefix", "viewName", "controllerName", "areaName");

            // Assert
            Assert.Equal(":ViewCacheEntry:System.Web.Mvc.Test.VirtualPathProviderViewEngineTest+DerivedVirtualPathProviderViewEngine, System.Web.Mvc.Test, Version=0.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35:prefix:viewName:controllerName:areaName:", key);
        }

        [Fact]
        public void FindView_NullControllerContext_Throws()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => _engine.FindView(null, "view name", null, false),
                "controllerContext"
                );
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void FindView_InvalidViewName_Throws(string viewName)
        {
            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                () => _engine.FindView(_context, viewName, null, false),
                "viewName"
                );
        }

        [Fact]
        public void FindView_ControllerNameNotInRequestContext_Throws()
        {
            // Arrange
            _context.RouteData.Values.Remove("controller");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => _engine.FindView(_context, "viewName", null, false),
                "The RouteData must contain an item named 'controller' with a non-empty string value."
                );
        }

        [Fact]
        public void FindView_EmptyViewLocations_Throws()
        {
            // Arrange
            _engine.ClearViewLocations();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => _engine.FindView(_context, "viewName", null, false),
                "The property 'ViewLocationFormats' cannot be null or empty."
                );
        }

        [Fact]
        public void FindView_ViewDoesNotExistAndNoMaster_ReturnsSearchedLocationsResult()
        {
            // Arrange
            SetupFileDoesNotExist("~/vpath/controllerName/viewName.view");

            // Act
            ViewEngineResult result = _engine.FindView(_context, "viewName", null, false);

            // Assert
            Assert.Null(result.View);
            Assert.Equal("~/vpath/controllerName/viewName.view", Assert.Single(result.SearchedLocations));
        }

        [Fact]
        public void FindView_ViewExistsAndNoMaster_ReturnsView()
        {
            // Arrange
            _engine.ClearMasterLocations(); // If master is not provided, master locations can be empty

            SetupFileExists("~/vpath/controllerName/viewName.view");
            SetupCacheHit(CreateCacheKey(Cache.View), "~/vpath/controllerName/viewName.view");

            SetupFileDoesNotExist("~/vpath/controllerName/viewName.Mobile.view");
            SetupCacheMiss(CreateCacheKey(Cache.View, displayMode: "Mobile"));

            // Act
            ViewEngineResult result = _engine.FindView(_context, "viewName", null, false);

            // Assert
            TestView view = Assert.IsType<TestView>(result.View);
            Assert.Null(result.SearchedLocations);
            Assert.Same(_context, view.ControllerContext);
            Assert.Equal("~/vpath/controllerName/viewName.view", view.Path);
            Assert.Equal(String.Empty, view.MasterPath);
        }

        [Theory]
        [InlineData("~/foo/bar.view")]
        [InlineData("/foo/bar.view")]
        public void FindView_PathViewExistsAndNoMaster_ReturnsView(string path)
        {
            // Arrange
            _engine.ClearMasterLocations();

            SetupFileExists(path);
            SetupCacheHit(CreateCacheBaseKey(Cache.View, path, "", ""), path);

            // Act
            ViewEngineResult result = _engine.FindView(_context, path, null, false);

            // Assert
            TestView view = Assert.IsType<TestView>(result.View);
            Assert.Null(result.SearchedLocations);
            Assert.Same(_context, view.ControllerContext);
            Assert.Equal(path, view.Path);
            Assert.Equal(String.Empty, view.MasterPath);
        }

        [Theory]
        [InlineData("~/foo/bar.unsupported")]
        [InlineData("/foo/bar.unsupported")]
        public void FindView_PathViewExistsAndNoMaster_Legacy_ReturnsView(string path)
        {
            // Arrange
            _engine.FileExtensions = null; // Set FileExtensions to null to simulate View Engines that do not set this property            
            _engine.ClearMasterLocations();

            SetupFileExists(path);
            SetupCacheHit(CreateCacheBaseKey(Cache.View, path, "", ""), path);

            // Act
            ViewEngineResult result = _engine.FindView(_context, path, null, false);

            // Assert
            TestView view = Assert.IsType<TestView>(result.View);
            Assert.Null(result.SearchedLocations);
            Assert.Same(_context, view.ControllerContext);
            Assert.Equal(path, view.Path);
            Assert.Equal(String.Empty, view.MasterPath);
        }

        [Theory]
        [InlineData("~/foo/bar.view")]
        [InlineData("/foo/bar.view")]
        public void FindView_PathViewDoesNotExistAndNoMaster_ReturnsSearchedLocationsResult(string path)
        {
            // Arrange
            SetupFileDoesNotExist(path);
            SetupCacheMiss(CreateCacheBaseKey(Cache.View, path, "", ""));

            // Act
            ViewEngineResult result = _engine.FindView(_context, path, null, false);

            // Assert
            Assert.Null(result.View);
            Assert.Equal(path, Assert.Single(result.SearchedLocations));
        }

        [Theory]
        [InlineData("~/foo/bar.unsupported")]
        [InlineData("/foo/bar.unsupported")]
        public void FindView_PathViewNotSupportedAndNoMaster_ReturnsSearchedLocationsResult(string path)
        {
            // Arrange
            SetupCacheMiss(CreateCacheBaseKey(Cache.View, path, "", ""));

            // Act
            ViewEngineResult result = _engine.FindView(_context, path, null, false);

            // Assert
            Assert.Null(result.View);
            Assert.Equal(path, Assert.Single(result.SearchedLocations));
            _engine.MockPathProvider.Verify(vpp => vpp.FileExists(path), Times.Never());
        }

        [Fact]
        public void FindView_ViewExistsAndMasterNameProvidedButEmptyMasterLocations_Throws()
        {
            // Arrange
            _engine.ClearMasterLocations();
            SetupFileExists("~/vpath/controllerName/viewName.view");
            SetupCacheHit(CreateCacheKey(Cache.View), "~/vpath/controllerName/viewName.view");
            SetupFileDoesNotExist("~/vpath/controllerName/viewName.Mobile.view");
            SetupCacheMiss(CreateCacheKey(Cache.View, displayMode: "Mobile"));

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => _engine.FindView(_context, "viewName", "masterName", false),
                "The property 'MasterLocationFormats' cannot be null or empty."
                );
        }

        [Fact]
        public void FindView_ViewDoesNotExistAndMasterDoesNotExist_ReturnsSearchedLocationsResult()
        {
            // Arrange
            SetupFileDoesNotExist("~/vpath/controllerName/viewName.view");
            SetupFileDoesNotExist("~/vpath/controllerName/masterName.master");

            // Act
            ViewEngineResult result = _engine.FindView(_context, "viewName", "masterName", false);

            // Assert
            Assert.Null(result.View);
            Assert.Equal(2, result.SearchedLocations.Count()); // Both view and master locations
            Assert.Contains("~/vpath/controllerName/viewName.view", result.SearchedLocations);
            Assert.Contains("~/vpath/controllerName/masterName.master", result.SearchedLocations);
        }

        [Fact]
        public void FindView_ViewExistsButMasterDoesNotExist_ReturnsSearchedLocationsResult()
        {
            // Arrange
            SetupFileExists("~/vpath/controllerName/viewName.view");
            SetupCacheHit(CreateCacheKey(Cache.View), "~/vpath/controllerName/viewName.view");

            SetupFileDoesNotExist("~/vpath/controllerName/viewName.Mobile.view");
            SetupCacheMiss(CreateCacheKey(Cache.View, displayMode: "Mobile"));

            SetupFileDoesNotExist("~/vpath/controllerName/masterName.master");

            // Act
            ViewEngineResult result = _engine.FindView(_context, "viewName", "masterName", false);

            // Assert
            Assert.Null(result.View);
            // View was found, not included in 'searched locations'
            Assert.Equal("~/vpath/controllerName/masterName.master", Assert.Single(result.SearchedLocations));
        }

        [Fact]
        public void FindView_MasterInAreaDoesNotExist_ReturnsSearchedLocationsResult()
        {
            // Arrange
            _context.RouteData.DataTokens["area"] = "areaName";
            SetupFileExists("~/vpath/areaName/controllerName/viewName.view");
            SetupCacheHit(CreateCacheKey(Cache.View, area: "areaName"), "~/vpath/areaName/controllerName/viewName.view");

            SetupFileDoesNotExist("~/vpath/areaName/controllerName/viewName.Mobile.view");
            SetupCacheMiss(CreateCacheKey(Cache.View, area: "areaName", displayMode: "Mobile"));

            SetupFileDoesNotExist("~/vpath/areaName/controllerName/masterName.master");
            SetupFileDoesNotExist("~/vpath/controllerName/masterName.master");

            // Act
            ViewEngineResult result = _engine.FindView(_context, "viewName", "masterName", false);

            // Assert
            Assert.Null(result.View);
            Assert.Equal(2, result.SearchedLocations.Count()); // View was found, not included in 'searched locations'
            Assert.Contains("~/vpath/areaName/controllerName/masterName.master", result.SearchedLocations);
            Assert.Contains("~/vpath/controllerName/masterName.master", result.SearchedLocations);
        }

        [Fact]
        public void FindView_ViewExistsAndMasterExists_ReturnsView()
        {
            // Arrange
            SetupFileExists("~/vpath/controllerName/viewName.view");
            SetupCacheHit(CreateCacheKey(Cache.View), "~/vpath/controllerName/viewName.view");

            SetupFileDoesNotExist("~/vpath/controllerName/viewName.Mobile.view");
            SetupCacheMiss(CreateCacheKey(Cache.View, displayMode: "Mobile"));

            SetupFileExists("~/vpath/controllerName/masterName.master");
            SetupCacheHit(CreateCacheKey(Cache.Master), "~/vpath/controllerName/masterName.master");

            SetupFileDoesNotExist("~/vpath/controllerName/masterName.Mobile.master");
            SetupCacheMiss(CreateCacheKey(Cache.Master, displayMode: "Mobile"));

            // Act
            ViewEngineResult result = _engine.FindView(_context, "viewName", "masterName", false);

            // Assert
            TestView view = Assert.IsType<TestView>(result.View);
            Assert.Null(result.SearchedLocations);
            Assert.Same(_context, view.ControllerContext);
            Assert.Equal("~/vpath/controllerName/viewName.view", view.Path);
            Assert.Equal("~/vpath/controllerName/masterName.master", view.MasterPath);
        }

        [Fact]
        public void FindView_ViewInAreaExistsAndMasterExists_ReturnsView()
        {
            // Arrange
            _context.RouteData.DataTokens["area"] = "areaName";
            SetupFileExists("~/vpath/areaName/controllerName/viewName.view");
            SetupCacheHit(CreateCacheKey(Cache.View, area: "areaName"), "~/vpath/areaName/controllerName/viewName.view");

            SetupFileDoesNotExist("~/vpath/areaName/controllerName/viewName.Mobile.view");
            SetupCacheMiss(CreateCacheKey(Cache.View, area: "areaName", displayMode: "Mobile"));

            SetupFileDoesNotExist("~/vpath/areaName/controllerName/masterName.master");

            SetupFileExists("~/vpath/controllerName/masterName.master");
            SetupCacheHit(CreateCacheKey(Cache.Master, area: "areaName"), "~/vpath/controllerName/masterName.master");

            SetupFileDoesNotExist("~/vpath/controllerName/masterName.Mobile.master");
            SetupCacheMiss(CreateCacheKey(Cache.Master, area: "areaName", displayMode: "Mobile"));


            // Act
            ViewEngineResult result = _engine.FindView(_context, "viewName", "masterName", false);

            // Assert
            TestView view = Assert.IsType<TestView>(result.View);
            Assert.Null(result.SearchedLocations);
            Assert.Same(_context, view.ControllerContext);
            Assert.Equal("~/vpath/areaName/controllerName/viewName.view", view.Path);
            Assert.Equal("~/vpath/controllerName/masterName.master", view.MasterPath);
        }

        [Fact]
        public void FindView_ViewInAreaExistsAndMasterExists_ReturnsView_Mobile()
        {
            // Arrange
            _mobileContext.RouteData.DataTokens["area"] = "areaName";
            SetupFileExists("~/vpath/areaName/controllerName/viewName.view");
            SetupCacheHit(CreateCacheKey(Cache.View, area: "areaName"), "~/vpath/areaName/controllerName/viewName.view");

            SetupFileExists("~/vpath/areaName/controllerName/viewName.Mobile.view");
            SetupCacheHit(CreateCacheKey(Cache.View, area: "areaName", displayMode: "Mobile"), "~/vpath/areaName/controllerName/viewName.Mobile.view");

            SetupFileDoesNotExist("~/vpath/areaName/controllerName/masterName.master");
            SetupFileDoesNotExist("~/vpath/areaName/controllerName/masterName.Mobile.master");

            SetupFileExists("~/vpath/controllerName/masterName.master");
            SetupCacheHit(CreateCacheKey(Cache.Master, area: "areaName"), "~/vpath/controllerName/masterName.master");

            SetupFileExists("~/vpath/controllerName/masterName.Mobile.master");
            SetupCacheHit(CreateCacheKey(Cache.Master, area: "areaName", displayMode: "Mobile"), "~/vpath/controllerName/masterName.Mobile.master");

            // Act
            ViewEngineResult result = _engine.FindView(_mobileContext, "viewName", "masterName", false);

            // Assert
            TestView view = Assert.IsType<TestView>(result.View);
            Assert.Null(result.SearchedLocations);
            Assert.Same(_mobileContext, view.ControllerContext);
            Assert.Equal("~/vpath/areaName/controllerName/viewName.Mobile.view", view.Path);
            Assert.Equal("~/vpath/controllerName/masterName.Mobile.master", view.MasterPath);
        }

        [Fact]
        public void FindPartialView_NullControllerContext_Throws()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => _engine.FindPartialView(null, "view name", false),
                "controllerContext"
                );
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void FindPartialView_InvalidPartialViewName_Throws(string partialViewName)
        {
            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                () => _engine.FindPartialView(_context, partialViewName, false),
                "partialViewName"
                );
        }

        [Fact]
        public void FindPartialView_ControllerNameNotInRequestContext_Throws()
        {
            // Arrange
            _context.RouteData.Values.Remove("controller");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => _engine.FindPartialView(_context, "partialName", false),
                "The RouteData must contain an item named 'controller' with a non-empty string value."
                );
        }

        [Fact]
        public void FindPartialView_EmptyPartialViewLocations_Throws()
        {
            // Arrange
            _engine.ClearPartialViewLocations();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => _engine.FindPartialView(_context, "partialName", false),
                "The property 'PartialViewLocationFormats' cannot be null or empty."
                );
        }

        [Fact]
        public void FindPartialView_ViewDoesNotExist_ReturnsSearchLocationsResult()
        {
            // Arrange
            SetupFileDoesNotExist("~/vpath/controllerName/partialName.partial");

            // Act
            ViewEngineResult result = _engine.FindPartialView(_context, "partialName", false);

            // Assert
            Assert.Null(result.View);
            Assert.Equal("~/vpath/controllerName/partialName.partial", Assert.Single(result.SearchedLocations));
        }

        [Fact]
        public void FindPartialView_ViewExists_ReturnsView()
        {
            // Arrange
            SetupFileExists("~/vpath/controllerName/partialName.partial");
            SetupCacheHit(CreateCacheKey(Cache.Partial), "~/vpath/controllerName/partialName.partial");

            SetupFileDoesNotExist("~/vpath/controllerName/partialName.Mobile.partial");
            SetupCacheMiss(CreateCacheKey(Cache.Partial, displayMode: "Mobile"));

            // Act
            ViewEngineResult result = _engine.FindPartialView(_context, "partialName", false);

            // Assert
            TestView view = Assert.IsType<TestView>(result.View);
            Assert.Null(result.SearchedLocations);
            Assert.Same(_context, view.ControllerContext);
            Assert.Equal("~/vpath/controllerName/partialName.partial", view.Path);
        }

        [Theory]
        [InlineData("~/foo/bar.partial")]
        [InlineData("/foo/bar.partial")]
        public void FindPartialView_PathViewExists_ReturnsView(string path)
        {
            // Arrange
            SetupFileExists(path);
            SetupCacheHit(CreateCacheBaseKey(Cache.Partial, path, "", ""), path);

            // Act
            ViewEngineResult result = _engine.FindPartialView(_context, path, false);

            // Assert
            TestView view = Assert.IsType<TestView>(result.View);
            Assert.Null(result.SearchedLocations);
            Assert.Same(_context, view.ControllerContext);
            Assert.Equal(path, view.Path);
        }

        [Theory]
        [InlineData("/foo/bar.unsupported")]
        [InlineData("~/foo/bar.unsupported")]
        public void FindPartialView_PathViewExists_Legacy_ReturnsView(string path)
        {
            // Arrange
            _engine.FileExtensions = null; // Set FileExtensions to null to simulate View Engines that do not set this property
            SetupFileExists(path);
            SetupCacheHit(CreateCacheBaseKey(Cache.Partial, path, "", ""), path);

            // Act
            ViewEngineResult result = _engine.FindPartialView(_context, path, false);

            // Assert
            TestView view = Assert.IsType<TestView>(result.View);
            Assert.Null(result.SearchedLocations);
            Assert.Same(_context, view.ControllerContext);
            Assert.Equal(path, view.Path);
        }

        [Theory]
        [InlineData("~/foo/bar.partial")]
        [InlineData("/foo/bar.partial")]
        public void FindPartialView_PathViewDoesNotExist_ReturnsSearchedLocationsResult(string path)
        {
            // Arrange
            SetupFileDoesNotExist(path);
            SetupCacheMiss(CreateCacheBaseKey(Cache.Partial, path, "", ""));

            // Act
            ViewEngineResult result = _engine.FindPartialView(_context, path, false);

            // Assert
            Assert.Null(result.View);
            Assert.Equal(path, Assert.Single(result.SearchedLocations));
        }

        [Theory]
        [InlineData("~/foo/bar.unsupported")]
        [InlineData("/foo/bar.unsupported")]
        public void FindPartialView_PathViewNotSupported_ReturnsSearchedLocationsResult(string path)
        {
            // Arrange
            SetupCacheMiss(CreateCacheBaseKey(Cache.Partial, path, "", ""));

            // Act
            ViewEngineResult result = _engine.FindPartialView(_context, path, false);

            // Assert
            Assert.Null(result.View);
            Assert.Equal(path, Assert.Single(result.SearchedLocations));
            _engine.MockPathProvider.Verify(vpp => vpp.FileExists(path), Times.Never());
        }

        [Fact]
        public void FileExtensions()
        {
            // Arrange + Assert
            Assert.Null(new Mock<VirtualPathProviderViewEngine>().Object.FileExtensions);
        }

        [Fact]
        public void GetExtensionThunk()
        {
            // Arrange and Assert
            Assert.Equal(VirtualPathUtility.GetExtension, new Mock<VirtualPathProviderViewEngine>().Object.GetExtensionThunk);
        }

        [Fact]
        public void DisplayModeSetOncePerRequest()
        {
            // Arrange
            SetupFileExists("~/vpath/controllerName/viewName.view");
            SetupCacheHit(CreateCacheKey(Cache.View), "~/vpath/controllerName/viewName.view");

            SetupFileDoesNotExist("~/vpath/controllerName/viewName.Mobile.view");
            SetupCacheMiss(CreateCacheKey(Cache.View, displayMode: "Mobile"));

            SetupFileDoesNotExist("~/vpath/controllerName/partialName.partial");
            SetupCacheMiss(CreateCacheKey(Cache.Partial));

            SetupFileExists("~/vpath/controllerName/partialName.Mobile.partial");
            _engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), "~/vpath/controllerName/partialName.Mobile.partial"))
                .Callback<HttpContextBase, string, string>((httpContext, key, virtualPath) =>
                {
                    _engine.MockCache
                        .Setup(c => c.GetViewLocation(It.IsAny<HttpContextBase>(), key))
                        .Returns("~/vpath/controllerName/partialName.Mobile.partial")
                        .Verifiable();
                })
                .Verifiable();

            // Act
            ViewEngineResult viewResult = _engine.FindView(_mobileContext, "viewName", masterName: null, useCache: false);

            // Mobile display mode will be used to locate the view with and without the cache.
            // In neither case should this set the DisplayModeId to Mobile because it has already been set.
            ViewEngineResult partialResult = _engine.FindPartialView(_mobileContext, "partialName", useCache: false);
            ViewEngineResult cachedPartialResult = _engine.FindPartialView(_mobileContext, "partialName", useCache: true);

            // Assert

            Assert.Equal(DisplayModeProvider.DefaultDisplayModeId, _mobileContext.DisplayMode.DisplayModeId);
        }

        // The core caching scenarios are covered in the FindView/FindPartialView tests. These
        // extra tests deal with the cache itself, rather than specifics around finding views.

        private const string MASTER_VIRTUAL = "~/vpath/controllerName/name.master";
        private const string PARTIAL_VIRTUAL = "~/vpath/controllerName/name.partial";
        private const string VIEW_VIRTUAL = "~/vpath/controllerName/name.view";
        private const string MOBILE_VIEW_VIRTUAL = "~/vpath/controllerName/name.Mobile.view";

        [Fact]
        public void UsesDifferentKeysForViewMasterAndPartial()
        {
            string keyMaster = null;
            string keyPartial = null;
            string keyView = null;

            // Arrange
            SetupFileExists(VIEW_VIRTUAL);
            SetupFileDoesNotExist(MOBILE_VIEW_VIRTUAL);
            SetupCacheMiss(CreateCacheKey(Cache.View, name: "name", displayMode: "Mobile"));
            SetupFileExists(MASTER_VIRTUAL);
            SetupFileDoesNotExist("~/vpath/controllerName/name.Mobile.master");
            SetupCacheMiss(CreateCacheKey(Cache.Master, name: "name", displayMode: "Mobile"));
            SetupFileExists(PARTIAL_VIRTUAL);
            SetupFileDoesNotExist("~/vpath/controllerName/name.Mobile.partial");
            SetupCacheMiss(CreateCacheKey(Cache.Partial, name: "name", displayMode: "Mobile"));
            _engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), VIEW_VIRTUAL))
                .Callback<HttpContextBase, string, string>((httpContext, key, path) => keyView = key)
                .Verifiable();
            _engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), MASTER_VIRTUAL))
                .Callback<HttpContextBase, string, string>((httpContext, key, path) => keyMaster = key)
                .Verifiable();
            _engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), PARTIAL_VIRTUAL))
                .Callback<HttpContextBase, string, string>((httpContext, key, path) => keyPartial = key)
                .Verifiable();

            // Act
            _engine.FindView(_context, "name", "name", false);
            _engine.FindPartialView(_context, "name", false);

            // Assert
            Assert.NotNull(keyMaster);
            Assert.NotNull(keyPartial);
            Assert.NotNull(keyView);
            Assert.NotEqual(keyMaster, keyPartial);
            Assert.NotEqual(keyMaster, keyView);
            Assert.NotEqual(keyPartial, keyView);
            _engine.MockPathProvider
                .Verify(vpp => vpp.FileExists(VIEW_VIRTUAL), Times.AtMostOnce());
            _engine.MockPathProvider
                .Verify(vpp => vpp.FileExists(MASTER_VIRTUAL), Times.AtMostOnce());
            _engine.MockPathProvider
                .Verify(vpp => vpp.FileExists(PARTIAL_VIRTUAL), Times.AtMostOnce());
            _engine.MockCache
                .Verify(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), VIEW_VIRTUAL), Times.AtMostOnce());
            _engine.MockCache
                .Verify(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), MASTER_VIRTUAL), Times.AtMostOnce());
            _engine.MockCache
                .Verify(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), PARTIAL_VIRTUAL), Times.AtMostOnce());
        }

        // This tests the protocol involved with two calls to FindView for the same view name
        // where the request succeeds. The calls happen in this order:
        //
        //    FindView("view")
        //      Cache.GetViewLocation(key for "view") -> returns null (not found)
        //      VirtualPathProvider.FileExists(virtual path for "view") -> returns true
        //      Cache.InsertViewLocation(key for "view", virtual path for "view")
        //    FindView("view")
        //      Cache.GetViewLocation(key for "view") -> returns virtual path for "view"
        //
        // The mocking code is written as it is because we don't want to make any assumptions
        // about the format of the cache key. So we intercept the first call to Cache.GetViewLocation and
        // take the key they gave us to set up the rest of the mock expectations.
        // The ViewCollection class will typically place to successive calls to FindView and FindPartialView and
        // set the useCache parameter to true/false respectively. To simulate this, both calls to FindView are executed
        // with useCache set to true. This mimics the behavior of always going to the cache first and after finding a
        // view, ensuring that subsequent calls from the cache are successful.

        [Fact]
        public void ValueInCacheBypassesVirtualPathProvider()
        {
            // Arrange
            string cacheKey = null;

            SetupFileExists(VIEW_VIRTUAL); // It wasn't found, so they call vpp.FileExists
            SetupFileDoesNotExist(MOBILE_VIEW_VIRTUAL);
            SetupCacheMiss(CreateCacheKey(Cache.View, name: "name", displayMode: "Mobile"));
            _engine.MockCache // Then they set the value into the cache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), VIEW_VIRTUAL))
                .Callback<HttpContextBase, string, string>((httpContext, key, virtualPath) =>
                {
                    cacheKey = key;
                    _engine.MockCache // Second time through, we give them a cache hit
                        .Setup(c => c.GetViewLocation(It.IsAny<HttpContextBase>(), key))
                        .Returns(VIEW_VIRTUAL)
                        .Verifiable();
                })
                .Verifiable();

            // Act
            _engine.FindView(_context, "name", null, false); // Call it once with false to seed the cache
            _engine.FindView(_context, "name", null, true); // Call it once with true to check the cache

            // Assert

            _engine.MockPathProvider.Verify(vpp => vpp.FileExists(VIEW_VIRTUAL), Times.AtMostOnce());
            _engine.MockCache.Verify(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), VIEW_VIRTUAL), Times.AtMostOnce());
            _engine.MockCache.Verify(c => c.GetViewLocation(It.IsAny<HttpContextBase>(), cacheKey), Times.AtMostOnce());

            // We seed the cache with all possible display modes but since the mobile view does not exist we don't insert it into the cache.
            _engine.MockPathProvider.Verify(vpp => vpp.FileExists(MOBILE_VIEW_VIRTUAL), Times.Exactly(1));
            _engine.MockCache.Verify(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), MOBILE_VIEW_VIRTUAL), Times.Never());
            _engine.MockCache.Verify(c => c.GetViewLocation(It.IsAny<HttpContextBase>(), VirtualPathProviderViewEngine.AppendDisplayModeToCacheKey(cacheKey, DisplayModeProvider.MobileDisplayModeId)), Times.Never());
        }

        [Fact]
        public void ValueInCacheBypassesVirtualPathProviderForAllAvailableDisplayModesForContext()
        {
            // Arrange
            string cacheKey = null;
            string mobileCacheKey = null;

            SetupFileExists(VIEW_VIRTUAL);
            SetupFileExists(MOBILE_VIEW_VIRTUAL);

            _engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), VIEW_VIRTUAL))
                .Callback<HttpContextBase, string, string>((httpContext, key, virtualPath) =>
                {
                    cacheKey = key;
                    _engine.MockCache
                        .Setup(c => c.GetViewLocation(It.IsAny<HttpContextBase>(), key))
                        .Returns(MOBILE_VIEW_VIRTUAL)
                        .Verifiable();
                })
                .Verifiable();
            _engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), MOBILE_VIEW_VIRTUAL))
                .Callback<HttpContextBase, string, string>((httpContext, key, virtualPath) =>
                {
                    mobileCacheKey = key;
                    _engine.MockCache
                        .Setup(c => c.GetViewLocation(It.IsAny<HttpContextBase>(), key))
                        .Returns(MOBILE_VIEW_VIRTUAL)
                        .Verifiable();
                })
                .Verifiable();

            // Act
            _engine.FindView(_mobileContext, "name", null, false);
            _engine.FindView(_mobileContext, "name", null, true);

            // Assert

            // DefaultDisplayMode with Mobile substitution is cached and hit on the second call to FindView
            _engine.MockPathProvider.Verify(vpp => vpp.FileExists(MOBILE_VIEW_VIRTUAL), Times.AtMostOnce());
            _engine.MockCache.Verify(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), MOBILE_VIEW_VIRTUAL), Times.AtMostOnce());
            _engine.MockCache.Verify(c => c.GetViewLocation(It.IsAny<HttpContextBase>(), VirtualPathProviderViewEngine.AppendDisplayModeToCacheKey(cacheKey, DisplayModeProvider.MobileDisplayModeId)), Times.AtMostOnce());

            _engine.MockPathProvider.Verify(vpp => vpp.FileExists(VIEW_VIRTUAL), Times.AtMostOnce());
            _engine.MockCache.Verify(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), VIEW_VIRTUAL), Times.Exactly(1));

            Assert.NotEqual(cacheKey, mobileCacheKey);

            // Act
            _engine.FindView(_context, "name", null, true);

            // Assert

            // The first call to FindView without a mobile browser results in a cache hit
            _engine.MockPathProvider.Verify(vpp => vpp.FileExists(VIEW_VIRTUAL), Times.AtMostOnce());
            _engine.MockCache.Verify(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>(), VIEW_VIRTUAL), Times.Exactly(1));
            _engine.MockCache.Verify(c => c.GetViewLocation(It.IsAny<HttpContextBase>(), cacheKey), Times.Exactly(1));
        }

        [Fact]
        public void NoValueInCacheButFileExists_ReturnsNullIfUsingCache()
        {
            // Arrange
            string mobileKey = CreateCacheKey(Cache.View, name: "name", displayMode: "Mobile");
            _engine.MockCache
                .Setup(c => c.GetViewLocation(It.IsAny<HttpContextBase>(), mobileKey))
                .Returns((string)null)
                .Verifiable();

            // Act
            IView viewNotInCache = _engine.FindView(_mobileContext, "name", masterName: null, useCache: true).View;

            // Assert
            Assert.Null(viewNotInCache);

            // On a cache miss we should never check the file system. FindView will be called on a second pass
            // without using the cache.
            _engine.MockPathProvider.Verify(vpp => vpp.FileExists(MOBILE_VIEW_VIRTUAL), Times.Never());
            _engine.MockPathProvider.Verify(vpp => vpp.FileExists(VIEW_VIRTUAL), Times.Never());

            SetupFileExists(MOBILE_VIEW_VIRTUAL);
            SetupCacheHit(CreateCacheKey(Cache.View, name: "name", displayMode: "Mobile"), MOBILE_VIEW_VIRTUAL);
            SetupFileExists(VIEW_VIRTUAL);
            SetupCacheHit(CreateCacheKey(Cache.View, name: "name"), VIEW_VIRTUAL);

            // Act & Assert
            ViewEngineResult result = _engine.FindView(_mobileContext, "name", masterName: null, useCache: false);
            //At this point the view on disk should be found and cached.
            var view = Assert.IsType<TestView>(result.View);
            Assert.Equal(MOBILE_VIEW_VIRTUAL, view.Path);
        }

        [Fact]
        public void NoValueInCacheAndFileDoesNotExist_ReturnsNullIfUsingCache()
        {
            // Arrange
            string mobileKey = CreateCacheKey(Cache.View, name: "name", displayMode: "Mobile");
            _engine.MockCache
                .Setup(c => c.GetViewLocation(It.IsAny<HttpContextBase>(), mobileKey))
                .Returns("")
                .Verifiable();
            string desktopKey = CreateCacheKey(Cache.View, name: "name");
            _engine.MockCache
                .Setup(c => c.GetViewLocation(It.IsAny<HttpContextBase>(), desktopKey))
                .Returns("")
                .Verifiable();

            // Act
            IView viewNotInCache = _engine.FindView(_mobileContext, "name", masterName: null, useCache: true).View;

            // Assert
            Assert.Null(viewNotInCache);
        }

        [Fact]
        public void ReleaseViewCallsDispose()
        {
            // Arrange
            IView view = new TestView();

            // Act
            _engine.ReleaseView(_context, view);

            // Assert
            Assert.True(((TestView)view).Disposed);
        }

        [Fact]
        public void ViewLocationCache_RoundTrips()
        {
            // Arrange & Act
            IViewLocationCache locationCache = _engine.ViewLocationCache;

            // Assert
            Assert.NotNull(locationCache);
            Assert.Equal(_engine.MockCache.Object, _engine.ViewLocationCache);
        }

        [Fact]
        public void ViewLocationCache_DefaultNullCache()
        {
            // Arrange
            Mock<VirtualPathProviderViewEngine> engineMock = new Mock<VirtualPathProviderViewEngine>();
            VirtualPathProviderViewEngine engine = engineMock.Object;

            // Act
            IViewLocationCache cache = engine.ViewLocationCache;

            // Assert
            Assert.NotNull(cache);
            Assert.Equal(DefaultViewLocationCache.Null, cache);
            Assert.IsNotType<DefaultViewLocationCache>(cache);
        }

        [Fact]
        public void ViewLocationCache_DefaultRealCache()
        {
            // Arrange
            Mock<VirtualPathProviderViewEngine> engineMock = new Mock<VirtualPathProviderViewEngine>();
            VirtualPathProviderViewEngine engine = engineMock.Object;

            HttpRequest request = new HttpRequest("foo.txt", "http://localhost", String.Empty);
            HttpResponse response = new HttpResponse(TextWriter.Null);
            HttpContext context = new HttpContext(request, response);
            HttpContext savedContext = HttpContext.Current;

            // Act
            IViewLocationCache cache;
            try
            {
                HttpContext.Current = context;
                cache = engine.ViewLocationCache;
            }
            finally
            {
                HttpContext.Current = savedContext;
            }

            // Assert
            Assert.NotNull(cache);
            Assert.IsType<DefaultViewLocationCache>(cache);
        }

        [Fact]
        public void VirtualPathProvider_RoundTrips()
        {
            // Arrange & Act & Assert
            Assert.Equal(_engine.MockPathProvider.Object, _engine.VirtualPathProvider);
        }

        // Not a valid production scenario -- no HostingEnvironment
        [Fact]
        public void VirtualPathProvider_Null()
        {
            // Arrange
            TestableVirtualPathProviderViewEngine viewEngine =
                new TestableVirtualPathProviderViewEngine(skipVPPInitialization: true);

            // Act & Assert
            Assert.Null(viewEngine.VirtualPathProvider);
        }

        [Fact]
        public void VirtualPathProvider_VPPRegistrationChanging()
        {
            // Arrange
            Mock<VirtualPathProvider> provider1 = new Mock<VirtualPathProvider>(MockBehavior.Strict);
            Mock<VirtualPathProvider> provider2 = new Mock<VirtualPathProvider>(MockBehavior.Strict);
            VirtualPathProvider provider = provider1.Object;

            // Act
            TestableVirtualPathProviderViewEngine viewEngine =
                new TestableVirtualPathProviderViewEngine(skipVPPInitialization: true)
            {
                VirtualPathProviderFunc = () => provider,
            };

            // The moral equivalent of HostingEnvironment.RegisterVirtualPathProvider(provider2.Object)
            provider = provider2.Object;

            // Assert
            Assert.Equal(provider2.Object, viewEngine.VirtualPathProvider);
            provider1.Verify();
            provider2.Verify();
        }

        [Fact]
        public void FileExists_VPPRegistrationChanging()
        {
            // Arrange
            Mock<VirtualPathProvider> provider1 = new Mock<VirtualPathProvider>(MockBehavior.Strict);
            provider1.Setup(vpp => vpp.FileExists(It.IsAny<string>())).Returns(true);
            Mock<VirtualPathProvider> provider2 = new Mock<VirtualPathProvider>(MockBehavior.Strict);
            provider2.Setup(vpp => vpp.FileExists(It.IsAny<string>())).Returns(true);
            VirtualPathProvider provider = provider1.Object;

            string path = "~/Index.cshtml";
            ControllerContext context = CreateContext();
            TestableVirtualPathProviderViewEngine viewEngine =
                new TestableVirtualPathProviderViewEngine(skipVPPInitialization: true)
            {
                VirtualPathProviderFunc = () => provider,
            };

            // Act
            bool fileExists1 = viewEngine.FileExists(context, path);

            // The moral equivalent of HostingEnvironment.RegisterVirtualPathProvider(provider2.Object)
            provider = provider2.Object;

            bool fileExists2 = viewEngine.FileExists(context, path);

            // Assert
            Assert.True(fileExists1);
            provider1.Verify(vpp => vpp.FileExists(path), Times.Once());
            Assert.True(fileExists2);
            provider2.Verify(vpp => vpp.FileExists(path), Times.Once());
        }

        private static string CreateCacheBaseKey(string prefix, string name, string controllerName, string area)
        {
            var r = String.Join(":", prefix, name, controllerName, area) + ":";
            return r;
        }

        private static string CreateCacheKey(Cache prefix, string name = null, string controller = "controllerName", string area = "", string displayMode = "")
        {
            if (name == null)
            {
                name = prefix.ToString().ToLowerInvariant() + "Name";
            }
            var r = CreateCacheBaseKey(prefix, name, controller, area) + displayMode + ":";
            return r;
        }

        private static ControllerContext CreateContext(bool isMobileDevice = false)
        {
            RouteData routeData = new RouteData();
            routeData.Values["controller"] = "controllerName";
            routeData.Values["action"] = "actionName";

            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(c => c.HttpContext.Request.Browser.IsMobileDevice).Returns(isMobileDevice);
            mockControllerContext.Setup(c => c.HttpContext.Items).Returns(new Hashtable());
            mockControllerContext.Setup(c => c.RouteData).Returns(routeData);

            mockControllerContext.Setup(c => c.HttpContext.Response.Cookies).Returns(new HttpCookieCollection());
            mockControllerContext.Setup(c => c.HttpContext.Request.Cookies).Returns(new HttpCookieCollection());

            return mockControllerContext.Object;
        }

        private void SetupFileExists(string path)
        {
            SetupFileExistsHelper(path, exists: true);
        }

        private void SetupFileDoesNotExist(string path)
        {
            SetupFileExistsHelper(path, exists: false);
        }

        private void SetupCacheHit(string key, string path)
        {
            _engine.MockCache
                .Setup(c => c.InsertViewLocation(It.IsAny<HttpContextBase>(), key, path))
                .Verifiable();
        }

        private void SetupCacheMiss(string key)
        {
            SetupCacheHit(key, "");
        }

        private void SetupFileExistsHelper(string path, bool exists)
        {
            _engine.MockPathProvider.Setup(vpp => vpp.FileExists(path)).Returns(exists).Verifiable();
        }

        private class Cache
        {
            private string _value;

            private Cache(string value)
            {
                _value = value;
            }

            public static Cache View = new Cache("View");
            public static Cache Partial = new Cache("Partial");
            public static Cache Master = new Cache("Master");

            public override string ToString()
            {
                return _value;
            }

            public static implicit operator string(Cache c)
            {
                return c.ToString();
            }
        }

        private class TestView : IView, IDisposable
        {
            public bool Disposed { get; set; }
            public ControllerContext ControllerContext { get; set; }
            public string Path { get; set; }
            public string MasterPath { get; set; }

            void IDisposable.Dispose()
            {
                Disposed = true;
            }

            void IView.Render(ViewContext viewContext, TextWriter writer)
            {
            }
        }

        private class TestableVirtualPathProviderViewEngine : VirtualPathProviderViewEngine
        {
            public Mock<IViewLocationCache> MockCache = new Mock<IViewLocationCache>(MockBehavior.Strict);
            public Mock<VirtualPathProvider> MockPathProvider = new Mock<VirtualPathProvider>(MockBehavior.Strict);

            public TestableVirtualPathProviderViewEngine()
                : this(skipVPPInitialization: false)
            {
            }

            public TestableVirtualPathProviderViewEngine(bool skipVPPInitialization)
            {
                MasterLocationFormats = new[] { "~/vpath/{1}/{0}.master" };
                ViewLocationFormats = new[] { "~/vpath/{1}/{0}.view" };
                PartialViewLocationFormats = new[] { "~/vpath/{1}/{0}.partial" };
                AreaMasterLocationFormats = new[] { "~/vpath/{2}/{1}/{0}.master" };
                AreaViewLocationFormats = new[] { "~/vpath/{2}/{1}/{0}.view" };
                AreaPartialViewLocationFormats = new[] { "~/vpath/{2}/{1}/{0}.partial" };
                FileExtensions = new[] { "view", "partial", "master" };

                ViewLocationCache = MockCache.Object;
                MockCache
                    .Setup(c => c.GetViewLocation(It.IsAny<HttpContextBase>(), It.IsAny<string>()))
                    .Returns((string)null);

                if (!skipVPPInitialization)
                {
                    VirtualPathProvider = MockPathProvider.Object;
                }

                GetExtensionThunk = GetExtension;
            }

            public new VirtualPathProvider VirtualPathProvider
            {
                get
                {
                    return base.VirtualPathProvider;
                }
                private set
                {
                    base.VirtualPathProvider = value;
                }
            }

            public void ClearViewLocations()
            {
                ViewLocationFormats = new string[0];
            }

            public void ClearMasterLocations()
            {
                MasterLocationFormats = new string[0];
            }

            public void ClearPartialViewLocations()
            {
                PartialViewLocationFormats = new string[0];
            }

            public new bool FileExists(ControllerContext controllerContext, string virtualPath)
            {
                return base.FileExists(controllerContext, virtualPath);
            }

            protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
            {
                return new TestView() { ControllerContext = controllerContext, Path = partialPath };
            }

            protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
            {
                return new TestView() { ControllerContext = controllerContext, Path = viewPath, MasterPath = masterPath };
            }

            internal override string CreateCacheKey(string prefix, string name, string controllerName, string areaName)
            {
                return VirtualPathProviderViewEngineTest.CreateCacheBaseKey(prefix, name, controllerName, areaName);
            }

            private static string GetExtension(string virtualPath)
            {
                var extension = virtualPath.Substring(virtualPath.LastIndexOf('.'));
                return extension;
            }
        }

        public class DerivedVirtualPathProviderViewEngine : VirtualPathProviderViewEngine
        {
            protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
            {
                throw new NotImplementedException();
            }

            protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
            {
                throw new NotImplementedException();
            }
        }
    }
}
