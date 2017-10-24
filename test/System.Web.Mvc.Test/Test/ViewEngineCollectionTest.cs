// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class ViewEngineCollectionTest
    {
        [Fact]
        public void ListWrappingConstructor()
        {
            // Arrange
            List<IViewEngine> list = new List<IViewEngine>() { new Mock<IViewEngine>().Object, new Mock<IViewEngine>().Object };

            // Act
            ViewEngineCollection collection = new ViewEngineCollection(list);

            // Assert
            Assert.Equal(2, collection.Count);
            Assert.Same(list[0], collection[0]);
            Assert.Same(list[1], collection[1]);
        }

        [Fact]
        public void ListWrappingConstructorThrowsIfListIsNull()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { new ViewEngineCollection((IList<IViewEngine>)null); },
                "list");
        }

        [Fact]
        public void DefaultConstructor()
        {
            // Act
            ViewEngineCollection collection = new ViewEngineCollection();

            // Assert
            Assert.Empty(collection);
        }

        [Fact]
        public void ViewEngineCollectionCombinedItemsCaches()
        {
            // Arrange
            var engines = new IViewEngine[] 
            {
                new Mock<IViewEngine>(MockBehavior.Strict).Object, 
                new Mock<IViewEngine>(MockBehavior.Strict).Object
            };
            var collection = new ViewEngineCollection(engines);

            // Act
            var combined1 = collection.CombinedItems;
            var combined2 = collection.CombinedItems;

            // Assert
            Assert.Equal(engines, combined1);
            Assert.Same(combined1, combined2);
        }

        [Fact]
        public void ViewEngineCollectionCombinedItemsClearResetsCache()
        {
            TestCacheReset((collection) => collection.Clear());
        }

        [Fact]
        public void ViewEngineCollectionCombinedItemsInsertResetsCache()
        {
            TestCacheReset((collection) => collection.Insert(0, new Mock<IViewEngine>(MockBehavior.Strict).Object));
        }

        [Fact]
        public void ViewEngineCollectionCombinedItemsRemoveResetsCache()
        {
            TestCacheReset((collection) => collection.RemoveAt(0));
        }

        [Fact]
        public void ViewEngineCollectionCombinedItemsSetResetsCache()
        {
            TestCacheReset((collection) => collection[0] = new Mock<IViewEngine>(MockBehavior.Strict).Object);
        }

        private static void TestCacheReset(Action<ViewEngineCollection> mutatingAction)
        {
            // Arrange
            var providers = new List<IViewEngine>() 
            {
                new Mock<IViewEngine>(MockBehavior.Strict).Object, 
                new Mock<IViewEngine>(MockBehavior.Strict).Object
            };
            var collection = new ViewEngineCollection(providers);

            // Act
            mutatingAction(collection);

            IViewEngine[] combined = collection.CombinedItems;

            // Assert
            Assert.Equal(providers, combined);
        }

        [Fact]
        public void ViewEngineCollectionCombinedItemsDelegatesToResolver()
        {
            // Arrange
            var firstEngine = new Mock<IViewEngine>();
            var secondEngine = new Mock<IViewEngine>();
            var thirdEngine = new Mock<IViewEngine>();
            var dependencyEngines = new IViewEngine[] { firstEngine.Object, secondEngine.Object };
            var collectionEngines = new IViewEngine[] { thirdEngine.Object };
            var expectedEngines = new IViewEngine[] { firstEngine.Object, secondEngine.Object, thirdEngine.Object };

            var resolver = new Mock<IDependencyResolver>();
            resolver.Setup(r => r.GetServices(typeof(IViewEngine))).Returns(dependencyEngines);

            var engines = new ViewEngineCollection(collectionEngines, resolver.Object);

            // Act
            IViewEngine[] combined = engines.CombinedItems;

            // Assert
            Assert.Equal(expectedEngines, combined);
        }

        [Fact]
        public void AddNullViewEngineThrows()
        {
            // Arrange
            ViewEngineCollection collection = new ViewEngineCollection();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { collection.Add(null); },
                "item");
        }

        [Fact]
        public void SetNullViewEngineThrows()
        {
            // Arrange
            ViewEngineCollection collection = new ViewEngineCollection();
            collection.Add(new Mock<IViewEngine>().Object);

            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { collection[0] = null; },
                "item");
        }

        [Fact]
        public void FindPartialViewAggregatesAllSearchedLocationsIfAllEnginesFail()
        {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            ViewEngineCollection viewEngineCollection = new ViewEngineCollection();
            Mock<IViewEngine> engine1 = new Mock<IViewEngine>();
            ViewEngineResult engine1Result = new ViewEngineResult(new[] { "location1", "location2" });
            engine1.Setup(e => e.FindPartialView(context, "partial", It.IsAny<bool>())).Returns(engine1Result);
            Mock<IViewEngine> engine2 = new Mock<IViewEngine>();
            ViewEngineResult engine2Result = new ViewEngineResult(new[] { "location3", "location4" });
            engine2.Setup(e => e.FindPartialView(context, "partial", It.IsAny<bool>())).Returns(engine2Result);
            viewEngineCollection.Add(engine1.Object);
            viewEngineCollection.Add(engine2.Object);

            // Act
            ViewEngineResult result = viewEngineCollection.FindPartialView(context, "partial");

            // Assert
            Assert.Null(result.View);
            Assert.Equal(4, result.SearchedLocations.Count());
            Assert.Contains("location1", result.SearchedLocations);
            Assert.Contains("location2", result.SearchedLocations);
            Assert.Contains("location3", result.SearchedLocations);
            Assert.Contains("location4", result.SearchedLocations);
        }

        [Fact]
        public void FindPartialViewFailureWithOneEngine()
        {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            ViewEngineCollection collection = new ViewEngineCollection();
            Mock<IViewEngine> engine = new Mock<IViewEngine>();
            ViewEngineResult engineResult = new ViewEngineResult(new[] { "location1", "location2" });
            engine.Setup(e => e.FindPartialView(context, "partial", It.IsAny<bool>())).Returns(engineResult);
            collection.Add(engine.Object);

            // Act
            ViewEngineResult result = collection.FindPartialView(context, "partial");

            // Assert
            Assert.Null(result.View);
            Assert.Equal(2, result.SearchedLocations.Count());
            Assert.Contains("location1", result.SearchedLocations);
            Assert.Contains("location2", result.SearchedLocations);
        }

        [Fact]
        public void FindPartialViewLooksAtCacheFirst()
        {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            Mock<IViewEngine> engine = new Mock<IViewEngine>();
            ViewEngineResult engineResult = new ViewEngineResult(new Mock<IView>().Object, engine.Object);
            engine.Setup(e => e.FindPartialView(context, "partial", true)).Returns(engineResult);
            ViewEngineCollection collection = new ViewEngineCollection()
            {
                engine.Object,
            };

            // Act
            ViewEngineResult result = collection.FindPartialView(context, "partial");

            // Assert
            Assert.Same(engineResult, result);
            engine.Verify(e => e.FindPartialView(context, "partial", true), Times.Once());
            engine.Verify(e => e.FindPartialView(context, "partial", false), Times.Never());
        }

        [Fact]
        public void FindPartialViewLooksAtLocatorIfCacheEmpty()
        {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            Mock<IViewEngine> engine = new Mock<IViewEngine>();
            ViewEngineResult engineResult = new ViewEngineResult(new Mock<IView>().Object, engine.Object);
            engine.Setup(e => e.FindPartialView(context, "partial", true)).Returns(new ViewEngineResult(new[] { "path" }));
            engine.Setup(e => e.FindPartialView(context, "partial", false)).Returns(engineResult);
            ViewEngineCollection collection = new ViewEngineCollection()
            {
                engine.Object,
            };

            // Act
            ViewEngineResult result = collection.FindPartialView(context, "partial");

            // Assert
            Assert.Same(engineResult, result);
            engine.Verify(e => e.FindPartialView(context, "partial", true), Times.Once());
            engine.Verify(e => e.FindPartialView(context, "partial", false), Times.Once());
        }

        [Fact]
        public void FindPartialViewIgnoresSearchLocationsFromCache()
        {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            Mock<IViewEngine> engine = new Mock<IViewEngine>();
            engine.Setup(e => e.FindPartialView(context, "partial", true)).Returns(new ViewEngineResult(new[] { "cachePath" }));
            engine.Setup(e => e.FindPartialView(context, "partial", false)).Returns(new ViewEngineResult(new[] { "locatorPath" }));
            ViewEngineCollection collection = new ViewEngineCollection()
            {
                engine.Object,
            };

            // Act
            ViewEngineResult result = collection.FindPartialView(context, "partial");

            // Assert
            string searchedLocation = Assert.Single(result.SearchedLocations);
            Assert.Equal("locatorPath", searchedLocation);
            engine.Verify(e => e.FindPartialView(context, "partial", true), Times.Once());
            engine.Verify(e => e.FindPartialView(context, "partial", false), Times.Once());
        }

        [Fact]
        public void FindPartialViewIteratesThroughCollectionUntilFindsSuccessfulEngine()
        {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            ViewEngineCollection collection = new ViewEngineCollection();
            Mock<IViewEngine> engine1 = new Mock<IViewEngine>();
            ViewEngineResult engine1Result = new ViewEngineResult(new[] { "location1", "location2" });
            engine1.Setup(e => e.FindPartialView(context, "partial", It.IsAny<bool>())).Returns(engine1Result);
            Mock<IViewEngine> engine2 = new Mock<IViewEngine>();
            ViewEngineResult engine2Result = new ViewEngineResult(new Mock<IView>().Object, engine2.Object);
            engine2.Setup(e => e.FindPartialView(context, "partial", It.IsAny<bool>())).Returns(engine2Result);
            collection.Add(engine1.Object);
            collection.Add(engine2.Object);

            // Act
            ViewEngineResult result = collection.FindPartialView(context, "partial");

            // Assert
            Assert.Same(engine2Result, result);
        }

        [Fact]
        public void FindPartialViewRemovesDuplicateSearchedLocationsFromMultipleEngines()
        {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            Mock<IViewEngine> engine1 = new Mock<IViewEngine>();
            ViewEngineResult engine1Result = new ViewEngineResult(new[] { "repeatLocation", "location1" });
            engine1.Setup(e => e.FindPartialView(context, "partial", It.IsAny<bool>())).Returns(engine1Result);
            Mock<IViewEngine> engine2 = new Mock<IViewEngine>();
            ViewEngineResult engine2Result = new ViewEngineResult(new[] { "location2", "repeatLocation" });
            engine2.Setup(e => e.FindPartialView(context, "partial", It.IsAny<bool>())).Returns(engine2Result);
            ViewEngineCollection viewEngineCollection = new ViewEngineCollection()
            {
                engine1.Object,
                engine2.Object,
            };

            // Act
            ViewEngineResult result = viewEngineCollection.FindPartialView(context, "partial");

            // Assert
            var expectedLocations = new[] { "repeatLocation", "location1", "location2" };
            Assert.Null(result.View);
            Assert.Equal(expectedLocations, result.SearchedLocations.ToArray());
        }

        [Fact]
        public void FindPartialViewReturnsNoViewAndEmptySearchedLocationsIfCollectionEmpty()
        {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            ViewEngineCollection collection = new ViewEngineCollection();

            // Act
            ViewEngineResult result = collection.FindPartialView(context, "partial");

            // Assert
            Assert.Null(result.View);
            Assert.Empty(result.SearchedLocations);
        }

        [Fact]
        public void FindPartialViewReturnsValueFromFirstSuccessfulEngine()
        {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            ViewEngineCollection collection = new ViewEngineCollection();
            Mock<IViewEngine> engine1 = new Mock<IViewEngine>();
            ViewEngineResult engine1Result = new ViewEngineResult(new Mock<IView>().Object, engine1.Object);
            engine1.Setup(e => e.FindPartialView(context, "partial", It.IsAny<bool>())).Returns(engine1Result);
            Mock<IViewEngine> engine2 = new Mock<IViewEngine>();
            ViewEngineResult engine2Result = new ViewEngineResult(new Mock<IView>().Object, engine2.Object);
            engine2.Setup(e => e.FindPartialView(context, "partial", It.IsAny<bool>())).Returns(engine2Result);
            collection.Add(engine1.Object);
            collection.Add(engine2.Object);

            // Act
            ViewEngineResult result = collection.FindPartialView(context, "partial");

            // Assert
            Assert.Same(engine1Result, result);
        }

        [Fact]
        public void FindPartialViewSuccessWithOneEngine()
        {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            ViewEngineCollection collection = new ViewEngineCollection();
            Mock<IViewEngine> engine = new Mock<IViewEngine>();
            ViewEngineResult engineResult = new ViewEngineResult(new Mock<IView>().Object, engine.Object);
            engine.Setup(e => e.FindPartialView(context, "partial", It.IsAny<bool>())).Returns(engineResult);
            collection.Add(engine.Object);

            // Act
            ViewEngineResult result = collection.FindPartialView(context, "partial");

            // Assert
            Assert.Same(engineResult, result);
        }

        [Fact]
        public void FindPartialViewThrowsIfPartialViewNameIsEmpty()
        {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            ViewEngineCollection collection = new ViewEngineCollection();

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                () => collection.FindPartialView(context, ""),
                "partialViewName");
        }

        [Fact]
        public void FindPartialViewThrowsIfPartialViewNameIsNull()
        {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            ViewEngineCollection collection = new ViewEngineCollection();

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                () => collection.FindPartialView(context, null),
                "partialViewName");
        }

        [Fact]
        public void FindPartialViewThrowsIfControllerContextIsNull()
        {
            // Arrange
            ViewEngineCollection collection = new ViewEngineCollection();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => collection.FindPartialView(null, "partial"),
                "controllerContext");
        }

        [Fact]
        public void FindViewAggregatesAllSearchedLocationsIfAllEnginesFail()
        {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            ViewEngineCollection collection = new ViewEngineCollection();
            Mock<IViewEngine> engine1 = new Mock<IViewEngine>();
            ViewEngineResult engine1Result = new ViewEngineResult(new[] { "location1", "location2" });
            engine1.Setup(e => e.FindView(context, "view", "master", It.IsAny<bool>())).Returns(engine1Result);
            Mock<IViewEngine> engine2 = new Mock<IViewEngine>();
            ViewEngineResult engine2Result = new ViewEngineResult(new[] { "location3", "location4" });
            engine2.Setup(e => e.FindView(context, "view", "master", It.IsAny<bool>())).Returns(engine2Result);
            collection.Add(engine1.Object);
            collection.Add(engine2.Object);

            // Act
            ViewEngineResult result = collection.FindView(context, "view", "master");

            // Assert
            Assert.Null(result.View);
            Assert.Equal(4, result.SearchedLocations.Count());
            Assert.Contains("location1", result.SearchedLocations);
            Assert.Contains("location2", result.SearchedLocations);
            Assert.Contains("location3", result.SearchedLocations);
            Assert.Contains("location4", result.SearchedLocations);
        }

        [Fact]
        public void FindViewFailureWithOneEngine()
        {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            ViewEngineCollection collection = new ViewEngineCollection();
            Mock<IViewEngine> engine = new Mock<IViewEngine>();
            ViewEngineResult engineResult = new ViewEngineResult(new[] { "location1", "location2" });
            engine.Setup(e => e.FindView(context, "view", "master", It.IsAny<bool>())).Returns(engineResult);
            collection.Add(engine.Object);

            // Act
            ViewEngineResult result = collection.FindView(context, "view", "master");

            // Assert
            Assert.Null(result.View);
            Assert.Equal(2, result.SearchedLocations.Count());
            Assert.Contains("location1", result.SearchedLocations);
            Assert.Contains("location2", result.SearchedLocations);
        }

        [Fact]
        public void FindViewLooksAtCacheFirst()
        {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            Mock<IViewEngine> engine = new Mock<IViewEngine>();
            ViewEngineResult engineResult = new ViewEngineResult(new Mock<IView>().Object, engine.Object);
            engine.Setup(e => e.FindView(context, "view", "master", true)).Returns(engineResult);
            ViewEngineCollection collection = new ViewEngineCollection()
            {
                engine.Object,
            };

            // Act
            ViewEngineResult result = collection.FindView(context, "view", "master");

            // Assert
            Assert.Same(engineResult, result);
            engine.Verify(e => e.FindView(context, "view", "master", true), Times.Once());
            engine.Verify(e => e.FindView(context, "view", "master", false), Times.Never());
        }

        [Fact]
        public void FindViewLooksAtLocatorIfCacheEmpty()
        {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            Mock<IViewEngine> engine = new Mock<IViewEngine>();
            ViewEngineResult engineResult = new ViewEngineResult(new Mock<IView>().Object, engine.Object);
            engine.Setup(e => e.FindView(context, "view", "master", true)).Returns(new ViewEngineResult(new[] { "path" }));
            engine.Setup(e => e.FindView(context, "view", "master", false)).Returns(engineResult);
            ViewEngineCollection collection = new ViewEngineCollection()
            {
                engine.Object,
            };

            // Act
            ViewEngineResult result = collection.FindView(context, "view", "master");

            // Assert
            Assert.Same(engineResult, result);
            engine.Verify(e => e.FindView(context, "view", "master", true), Times.Once());
            engine.Verify(e => e.FindView(context, "view", "master", false), Times.Once());
        }

        [Fact]
        public void FindViewIgnoresSearchLocationsFromCache()
        {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            Mock<IViewEngine> engine = new Mock<IViewEngine>();
            engine.Setup(e => e.FindView(context, "view", "master", true)).Returns(new ViewEngineResult(new[] { "cachePath" }));
            engine.Setup(e => e.FindView(context, "view", "master", false)).Returns(new ViewEngineResult(new[] { "locatorPath" }));
            ViewEngineCollection collection = new ViewEngineCollection()
            {
                engine.Object,
            };

            // Act
            ViewEngineResult result = collection.FindView(context, "view", "master");

            // Assert
            string searchedLocation = Assert.Single(result.SearchedLocations);
            Assert.Equal("locatorPath", searchedLocation);
            engine.Verify(e => e.FindView(context, "view", "master", true), Times.Once());
            engine.Verify(e => e.FindView(context, "view", "master", false), Times.Once());
        }

        [Fact]
        public void FindViewIteratesThroughCollectionUntilFindsSuccessfulEngine()
        {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            ViewEngineCollection collection = new ViewEngineCollection();
            Mock<IViewEngine> engine1 = new Mock<IViewEngine>();
            ViewEngineResult engine1Result = new ViewEngineResult(new[] { "location1", "location2" });
            engine1.Setup(e => e.FindView(context, "view", "master", It.IsAny<bool>())).Returns(engine1Result);
            Mock<IViewEngine> engine2 = new Mock<IViewEngine>();
            ViewEngineResult engine2Result = new ViewEngineResult(new Mock<IView>().Object, engine2.Object);
            engine2.Setup(e => e.FindView(context, "view", "master", It.IsAny<bool>())).Returns(engine2Result);
            collection.Add(engine1.Object);
            collection.Add(engine2.Object);

            // Act
            ViewEngineResult result = collection.FindView(context, "view", "master");

            // Assert
            Assert.Same(engine2Result, result);
        }

        [Fact]
        public void FindViewRemovesDuplicateSearchedLocationsFromMultipleEngines()
        {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            Mock<IViewEngine> engine1 = new Mock<IViewEngine>();
            ViewEngineResult engine1Result = new ViewEngineResult(new[] { "repeatLocation", "location1" });
            engine1.Setup(e => e.FindView(context, "view", "master", It.IsAny<bool>())).Returns(engine1Result);
            Mock<IViewEngine> engine2 = new Mock<IViewEngine>();
            ViewEngineResult engine2Result = new ViewEngineResult(new[] { "location2", "repeatLocation" });
            engine2.Setup(e => e.FindView(context, "view", "master", It.IsAny<bool>())).Returns(engine2Result);
            ViewEngineCollection collection = new ViewEngineCollection()
            {
                engine1.Object,
                engine2.Object,
            };

            // Act
            ViewEngineResult result = collection.FindView(context, "view", "master");

            // Assert
            Assert.Null(result.View);
            var expectedLocations = new[] { "repeatLocation", "location1", "location2" };
            Assert.Equal(expectedLocations, result.SearchedLocations.ToArray());
        }

        [Fact]
        public void FindViewReturnsNoViewAndEmptySearchedLocationsIfCollectionEmpty()
        {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            ViewEngineCollection collection = new ViewEngineCollection();

            // Act
            ViewEngineResult result = collection.FindView(context, "view", null);

            // Assert
            Assert.Null(result.View);
            Assert.Empty(result.SearchedLocations);
        }

        [Fact]
        public void FindViewReturnsValueFromFirstSuccessfulEngine()
        {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            ViewEngineCollection collection = new ViewEngineCollection();
            Mock<IViewEngine> engine1 = new Mock<IViewEngine>();
            ViewEngineResult engine1Result = new ViewEngineResult(new Mock<IView>().Object, engine1.Object);
            engine1.Setup(e => e.FindView(context, "view", "master", It.IsAny<bool>())).Returns(engine1Result);
            Mock<IViewEngine> engine2 = new Mock<IViewEngine>();
            ViewEngineResult engine2Result = new ViewEngineResult(new Mock<IView>().Object, engine2.Object);
            engine2.Setup(e => e.FindView(context, "view", "master", It.IsAny<bool>())).Returns(engine2Result);
            collection.Add(engine1.Object);
            collection.Add(engine2.Object);

            // Act
            ViewEngineResult result = collection.FindView(context, "view", "master");

            // Assert
            Assert.Same(engine1Result, result);
        }

        [Fact]
        public void FindViewSuccessWithOneEngine()
        {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            ViewEngineCollection collection = new ViewEngineCollection();
            Mock<IViewEngine> engine = new Mock<IViewEngine>();
            ViewEngineResult engineResult = new ViewEngineResult(new Mock<IView>().Object, engine.Object);
            engine.Setup(e => e.FindView(context, "view", "master", It.IsAny<bool>())).Returns(engineResult);
            collection.Add(engine.Object);

            // Act
            ViewEngineResult result = collection.FindView(context, "view", "master");

            // Assert
            Assert.Same(engineResult, result);
        }

        [Fact]
        public void FindViewThrowsIfControllerContextIsNull()
        {
            // Arrange
            ViewEngineCollection collection = new ViewEngineCollection();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => collection.FindView(null, "view", null),
                "controllerContext"
                );
        }

        [Fact]
        public void FindViewThrowsIfViewNameIsEmpty()
        {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            ViewEngineCollection collection = new ViewEngineCollection();

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                () => collection.FindView(context, "", null),
                "viewName"
                );
        }

        [Fact]
        public void FindViewThrowsIfViewNameIsNull()
        {
            // Arrange
            ControllerContext context = new Mock<ControllerContext>().Object;
            ViewEngineCollection collection = new ViewEngineCollection();

            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                () => collection.FindView(context, null, null),
                "viewName"
                );
        }

        [Fact]
        public void FindViewDelegatesToResolver()
        {
            // Arrange
            Mock<IView> view = new Mock<IView>();
            ControllerContext context = new ControllerContext();
            Mock<IViewEngine> locatedEngine = new Mock<IViewEngine>();
            ViewEngineResult engineResult = new ViewEngineResult(view.Object, locatedEngine.Object);
            locatedEngine.Setup(e => e.FindView(context, "ViewName", "MasterName", true))
                .Returns(engineResult);
            Mock<IViewEngine> secondEngine = new Mock<IViewEngine>();

            var resolver = new Mock<IDependencyResolver>();
            resolver.Setup(r => r.GetServices(typeof(IViewEngine))).Returns(new IViewEngine[] { locatedEngine.Object, secondEngine.Object });

            ViewEngineCollection engines = new ViewEngineCollection(new IViewEngine[0], resolver.Object);

            // Act
            ViewEngineResult result = engines.FindView(context, "ViewName", "MasterName");

            // Assert
            Assert.Same(engineResult, result);
            secondEngine.Verify(e => e.FindView(context, "ViewName", "MasterName", It.IsAny<bool>()), Times.Never());
        }

        [Fact]
        public void FindPartialViewDelegatesToResolver()
        {
            // Arrange
            Mock<IView> view = new Mock<IView>();
            ControllerContext context = new ControllerContext();
            Mock<IViewEngine> locatedEngine = new Mock<IViewEngine>();
            ViewEngineResult engineResult = new ViewEngineResult(view.Object, locatedEngine.Object);
            locatedEngine.Setup(e => e.FindPartialView(context, "ViewName", true))
                .Returns(engineResult);
            Mock<IViewEngine> secondEngine = new Mock<IViewEngine>();

            var resolver = new Mock<IDependencyResolver>();
            resolver.Setup(r => r.GetServices(typeof(IViewEngine))).Returns(new IViewEngine[] { locatedEngine.Object, secondEngine.Object });

            ViewEngineCollection engines = new ViewEngineCollection(new IViewEngine[0], resolver.Object);

            // Act
            ViewEngineResult result = engines.FindPartialView(context, "ViewName");

            // Assert
            Assert.Same(engineResult, result);
            secondEngine.Verify(e => e.FindPartialView(context, "ViewName", It.IsAny<bool>()), Times.Never());
        }
    }
}
