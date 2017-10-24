// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using System.Web.Http.Description;
using System.Web.Http.Dispatcher;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Filters;
using System.Web.Http.Hosting;
using System.Web.Http.Metadata;
using System.Web.Http.Metadata.Providers;
using System.Web.Http.ModelBinding;
using System.Web.Http.ModelBinding.Binders;
using System.Web.Http.Tracing;
using System.Web.Http.Validation;
using System.Web.Http.Validation.Providers;
using System.Web.Http.ValueProviders;
using System.Web.Http.ValueProviders.Providers;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Services
{
    public class DefaultServicesTests
    {
        // Constructor tests

        [Fact]
        public void Constructor_GuardClauses()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(() => new DefaultServices(configuration: null), "configuration");
        }

        [Fact]
        public void Constructor_DefaultServicesInContainer()
        {
            // Arrange
            var config = new HttpConfiguration();

            // Act
            var defaultServices = new DefaultServices(config);

            // Assert
            Assert.Null(defaultServices.GetService(typeof(IDocumentationProvider)));
            Assert.Null(defaultServices.GetService(typeof(ITraceWriter)));

            Assert.IsType<DefaultActionValueBinder>(defaultServices.GetService(typeof(IActionValueBinder)));
            Assert.IsType<ApiExplorer>(defaultServices.GetService(typeof(IApiExplorer)));
            Assert.IsType<DefaultAssembliesResolver>(defaultServices.GetService(typeof(IAssembliesResolver)));
            Assert.IsType<DefaultBodyModelValidator>(defaultServices.GetService(typeof(IBodyModelValidator)));
            Assert.IsType<DefaultContentNegotiator>(defaultServices.GetService(typeof(IContentNegotiator)));
            Assert.IsType<ApiControllerActionInvoker>(defaultServices.GetService(typeof(IHttpActionInvoker)));
            Assert.IsType<ApiControllerActionSelector>(defaultServices.GetService(typeof(IHttpActionSelector)));
            Assert.IsType<DefaultHttpControllerActivator>(defaultServices.GetService(typeof(IHttpControllerActivator)));
            Assert.IsType<DefaultHttpControllerSelector>(defaultServices.GetService(typeof(IHttpControllerSelector)));
            Assert.IsType<DefaultHttpControllerTypeResolver>(defaultServices.GetService(typeof(IHttpControllerTypeResolver)));
            Assert.IsType<TraceManager>(defaultServices.GetService(typeof(ITraceManager)));
            Assert.IsType<DataAnnotationsModelMetadataProvider>(defaultServices.GetService(typeof(ModelMetadataProvider)));
            Assert.IsType<ModelValidatorCache>(defaultServices.GetService(typeof(IModelValidatorCache)));
            Assert.IsType<DefaultExceptionHandler>(defaultServices.GetService(typeof(IExceptionHandler)));

            object[] filterProviders = defaultServices.GetServices(typeof(IFilterProvider)).ToArray();
            Assert.Equal(2, filterProviders.Length);
            Assert.IsType<ConfigurationFilterProvider>(filterProviders[0]);
            Assert.IsType<ActionDescriptorFilterProvider>(filterProviders[1]);

            object[] modelBinderProviders = defaultServices.GetServices(typeof(ModelBinderProvider)).ToArray();
            Assert.Equal(8, modelBinderProviders.Length);
            Assert.IsType<TypeConverterModelBinderProvider>(modelBinderProviders[0]);
            Assert.IsType<TypeMatchModelBinderProvider>(modelBinderProviders[1]);
            Assert.IsType<KeyValuePairModelBinderProvider>(modelBinderProviders[2]);
            Assert.IsType<ComplexModelDtoModelBinderProvider>(modelBinderProviders[3]);
            Assert.IsType<ArrayModelBinderProvider>(modelBinderProviders[4]);
            Assert.IsType<DictionaryModelBinderProvider>(modelBinderProviders[5]);
            Assert.IsType<CollectionModelBinderProvider>(modelBinderProviders[6]);
            Assert.IsType<MutableObjectModelBinderProvider>(modelBinderProviders[7]);

            object[] validatorProviders = defaultServices.GetServices(typeof(ModelValidatorProvider)).ToArray();
            Assert.Equal(2, validatorProviders.Length);
            Assert.IsType<DataAnnotationsModelValidatorProvider>(validatorProviders[0]);
            Assert.IsType<DataMemberModelValidatorProvider>(validatorProviders[1]);

            object[] valueProviderFactories = defaultServices.GetServices(typeof(ValueProviderFactory)).ToArray();
            Assert.Equal(2, valueProviderFactories.Length);
            Assert.IsType<QueryStringValueProviderFactory>(valueProviderFactories[0]);
            Assert.IsType<RouteDataValueProviderFactory>(valueProviderFactories[1]);

            object[] exceptionLoggers = defaultServices.GetServices(typeof(IExceptionLogger)).ToArray();
            Assert.Empty(exceptionLoggers);
        }

        // Add tests

        [Fact]
        public void Add_GuardClauses()
        {
            // Arrange
            var config = new HttpConfiguration();
            var defaultServices = new DefaultServices(config);

            // Act & assert
            Assert.ThrowsArgumentNull(() => defaultServices.Add(serviceType: null, service: new object()), "serviceType");
            Assert.ThrowsArgumentNull(() => defaultServices.Add(typeof(object), service: null), "service");
            Assert.ThrowsArgument(
                () => defaultServices.Add(typeof(object), new object()),
                "serviceType",
                "The service type Object is not supported.");
            Assert.ThrowsArgument(
                () => defaultServices.Add(typeof(IHttpActionInvoker), new object()),
                "service",
                "The type Object must derive from IHttpActionInvoker.");
        }

        [Fact]
        public void Add_AddsServiceToEndOfServicesList()
        {
            // Arrange
            var config = new HttpConfiguration();
            var defaultServices = new DefaultServices(config);
            var filterProvider = new Mock<IFilterProvider>().Object;
            IEnumerable<object> servicesBefore = defaultServices.GetServices(typeof(IFilterProvider));

            // Act
            defaultServices.Add(typeof(IFilterProvider), filterProvider);

            // Assert
            IEnumerable<object> servicesAfter = defaultServices.GetServices(typeof(IFilterProvider));
            Assert.Equal(servicesBefore.Concat(new[] { filterProvider }), servicesAfter);
        }

        // AddRange tests

        [Fact]
        public void AddRange_GuardClauses()
        {
            // Arrange
            var config = new HttpConfiguration();
            var defaultServices = new DefaultServices(config);

            // Act & assert
            Assert.ThrowsArgumentNull(() => defaultServices.AddRange(serviceType: null, services: new[] { new object() }), "serviceType");
            Assert.ThrowsArgumentNull(() => defaultServices.AddRange(typeof(object), services: null), "services");
            Assert.ThrowsArgument(
                () => defaultServices.AddRange(typeof(object), new[] { new object() }),
                "serviceType",
                "The service type Object is not supported.");
            Assert.ThrowsArgument(() => defaultServices.AddRange(typeof(ValueProviderFactory), new[] { new object() }),
                "services",
                "The type Object must derive from ValueProviderFactory.");
        }

        [Fact]
        public void AddRange_AddsServicesToEndOfServicesList()
        {
            // Arrange
            var config = new HttpConfiguration();
            var defaultServices = new DefaultServices(config);
            var filterProvider = new Mock<IFilterProvider>().Object;
            IEnumerable<object> servicesBefore = defaultServices.GetServices(typeof(IFilterProvider));

            // Act
            defaultServices.AddRange(typeof(IFilterProvider), new[] { filterProvider });

            // Assert
            IEnumerable<object> servicesAfter = defaultServices.GetServices(typeof(IFilterProvider));
            Assert.Equal(servicesBefore.Concat(new[] { filterProvider }), servicesAfter);
        }

        [Fact]
        public void AddRange_SkipsNullObjects()
        {
            // Arrange
            var config = new HttpConfiguration();
            var defaultServices = new DefaultServices(config);
            IEnumerable<object> servicesBefore = defaultServices.GetServices(typeof(IFilterProvider));

            // Act
            defaultServices.AddRange(typeof(IFilterProvider), new object[] { null });

            // Assert
            IEnumerable<object> servicesAfter = defaultServices.GetServices(typeof(IFilterProvider));
            Assert.Equal(servicesBefore, servicesAfter);
        }

        // Clear tests

        [Fact]
        public void Clear_GuardClauses()
        {
            // Arrange
            var config = new HttpConfiguration();
            var defaultServices = new DefaultServices(config);

            // Act & assert
            Assert.ThrowsArgumentNull(() => defaultServices.Clear(serviceType: null), "serviceType");
            Assert.ThrowsArgument(
                () => defaultServices.Clear(typeof(object)),
                "serviceType",
                "The service type Object is not supported.");
        }

        [Fact]
        public void Clear_RemovesAllServices()
        {
            // Arrange
            var config = new HttpConfiguration();
            var defaultServices = new DefaultServices(config);
            Assert.NotEmpty(defaultServices.GetServices(typeof(IFilterProvider)));

            // Act
            defaultServices.Clear(typeof(IFilterProvider));

            // Assert
            Assert.Empty(defaultServices.GetServices(typeof(IFilterProvider)));
        }

        // FindIndex tests

        [Fact]
        public void FindIndex_GuardClauses()
        {
            // Arrange
            var config = new HttpConfiguration();
            var defaultServices = new DefaultServices(config);

            // Act & assert
            Assert.ThrowsArgumentNull(() => defaultServices.FindIndex(serviceType: null, match: _ => true), "serviceType");
            Assert.ThrowsArgumentNull(() => defaultServices.FindIndex(typeof(object), match: null), "match");
            Assert.ThrowsArgument(
                () => defaultServices.FindIndex(typeof(object), _ => true),
                "serviceType",
                "The service type Object is not supported.");
        }

        [Fact]
        public void FindIndex_SuccessfulFind()
        {
            // Arrange
            var config = new HttpConfiguration();
            var defaultServices = new DefaultServices(config);

            // Act
            int index = defaultServices.FindIndex(typeof(IFilterProvider), _ => true);

            // Assert
            Assert.Equal(0, index);
        }

        [Fact]
        public void FindIndex_FailedFind()
        {
            // Arrange
            var config = new HttpConfiguration();
            var defaultServices = new DefaultServices(config);

            // Act
            int index = defaultServices.FindIndex(typeof(IFilterProvider), _ => false);

            // Assert
            Assert.Equal(-1, index);
        }

        [Fact]
        public void FindIndex_EmptyServiceListAlwaysReturnsFailure()
        {
            // Arrange
            var config = new HttpConfiguration();
            var defaultServices = new DefaultServices(config);
            defaultServices.Clear(typeof(IFilterProvider));

            // Act
            int index = defaultServices.FindIndex(typeof(IFilterProvider), _ => true);

            // Assert
            Assert.Equal(-1, index);
        }

        // GetService tests

        [Fact]
        public void GetService_GuardClauses()
        {
            // Arrange
            var config = new HttpConfiguration();
            var defaultServices = new DefaultServices(config);

            // Act & assert
            Assert.ThrowsArgumentNull(() => defaultServices.GetService(serviceType: null), "serviceType");
            Assert.ThrowsArgument(
                () => defaultServices.GetService(typeof(object)),
                "serviceType",
                "The service type Object is not supported.");
        }

        [Fact]
        public void GetService_ReturnsNullWhenServiceListEmpty()
        {
            // Arrange
            var config = new HttpConfiguration();
            var defaultServices = new DefaultServices(config);
            defaultServices.Clear(typeof(IActionValueBinder));

            // Act
            object service = defaultServices.GetService(typeof(IActionValueBinder));

            // Assert
            Assert.Null(service);
        }

        [Fact]
        public void GetService_PrefersServiceInDependencyInjectionContainer()
        {
            // Arrange
            var config = new HttpConfiguration();
            var defaultServices = new DefaultServices(config);
            var filterProvider = new Mock<IActionValueBinder>().Object;
            var mockDependencyResolver = new Mock<IDependencyResolver>();
            mockDependencyResolver.Setup(dr => dr.GetService(typeof(IActionValueBinder))).Returns(filterProvider);
            config.DependencyResolver = mockDependencyResolver.Object;

            // Act
            object service = defaultServices.GetService(typeof(IActionValueBinder));

            // Assert
            Assert.Same(filterProvider, service);
        }

        [Fact]
        public void GetService_CachesResultFromDependencyInjectionContainer()
        {
            // Arrange
            var config = new HttpConfiguration();
            var defaultServices = new DefaultServices(config);
            var mockDependencyResolver = new Mock<IDependencyResolver>();
            config.DependencyResolver = mockDependencyResolver.Object;

            // Act
            defaultServices.GetService(typeof(IActionValueBinder));
            defaultServices.GetService(typeof(IActionValueBinder));

            // Assert
            mockDependencyResolver.Verify(dr => dr.GetService(typeof(IActionValueBinder)), Times.Once());
        }

        // GetServicesTests

        [Fact]
        public void GetServices_GuardClauses()
        {
            // Arrange
            var config = new HttpConfiguration();
            var defaultServices = new DefaultServices(config);

            // Act & assert
            Assert.ThrowsArgumentNull(() => defaultServices.GetServices(serviceType: null), "serviceType");
            Assert.ThrowsArgument(
                () => defaultServices.GetServices(typeof(object)),
                "serviceType",
                "The service type Object is not supported.");
        }

        [Fact]
        public void GetServices_ReturnsEmptyEnumerationWhenServiceListEmpty()
        {
            // Arrange
            var config = new HttpConfiguration();
            var defaultServices = new DefaultServices(config);
            defaultServices.Clear(typeof(IFilterProvider));

            // Act
            IEnumerable<object> services = defaultServices.GetServices(typeof(IFilterProvider));

            // Assert
            Assert.Empty(services);
        }

        [Fact]
        public void GetServices_PrependsServiceInDependencyInjectionContainer()
        {
            // Arrange
            var config = new HttpConfiguration();
            var defaultServices = new DefaultServices(config);
            IEnumerable<object> servicesBefore = defaultServices.GetServices(typeof(IFilterProvider));
            var filterProvider = new Mock<IFilterProvider>().Object;
            var mockDependencyResolver = new Mock<IDependencyResolver>();
            mockDependencyResolver.Setup(dr => dr.GetServices(typeof(IFilterProvider))).Returns(new[] { filterProvider });
            config.DependencyResolver = mockDependencyResolver.Object;

            // Act
            IEnumerable<object> servicesAfter = defaultServices.GetServices(typeof(IFilterProvider));

            // Assert
            Assert.Equal(new[] { filterProvider }.Concat(servicesBefore), servicesAfter);
        }

        [Fact]
        public void GetServices_CachesResultFromDependencyInjectionContainer()
        {
            // Arrange
            var config = new HttpConfiguration();
            var defaultServices = new DefaultServices(config);
            var mockDependencyResolver = new Mock<IDependencyResolver>();
            config.DependencyResolver = mockDependencyResolver.Object;

            // Act
            defaultServices.GetServices(typeof(IFilterProvider));
            defaultServices.GetServices(typeof(IFilterProvider));

            // Assert
            mockDependencyResolver.Verify(dr => dr.GetServices(typeof(IFilterProvider)), Times.Once());
        }

        // Insert tests

        [Fact]
        public void Insert_GuardClauses()
        {
            // Arrange
            var config = new HttpConfiguration();
            var defaultServices = new DefaultServices(config);

            // Act & assert
            Assert.ThrowsArgumentNull(() => defaultServices.Insert(serviceType: null, index: 0, service: new object()), "serviceType");
            Assert.ThrowsArgumentNull(() => defaultServices.Insert(typeof(object), 0, service: null), "service");
            Assert.ThrowsArgument(
                () => defaultServices.Insert(typeof(object), 0, new object()),
                "serviceType",
                "The service type Object is not supported.");
            Assert.ThrowsArgument(
                () => defaultServices.Insert(typeof(IHttpActionInvoker), 0, new object()),
                "service",
                "The type Object must derive from IHttpActionInvoker.");
            Assert.ThrowsArgumentOutOfRange(
                () => defaultServices.Insert(typeof(ValueProviderFactory), -1, new Mock<ValueProviderFactory>().Object),
                "index",
                "Index must be within the bounds of the List.");
        }

        [Fact]
        public void Insert_AddsElementAtTheRequestedLocation()
        {
            // Arrange
            var config = new HttpConfiguration();
            var defaultServices = new DefaultServices(config);
            var filterProvider1 = new Mock<IFilterProvider>().Object;
            var filterProvider2 = new Mock<IFilterProvider>().Object;
            var newFilterProvider = new Mock<IFilterProvider>().Object;
            defaultServices.ReplaceRange(typeof(IFilterProvider), new[] { filterProvider1, filterProvider2 });

            // Act
            defaultServices.Insert(typeof(IFilterProvider), 1, newFilterProvider);

            // Assert
            Assert.Equal(new[] { filterProvider1, newFilterProvider, filterProvider2 }, defaultServices.GetServices(typeof(IFilterProvider)));
        }

        // InsertRange tests

        [Fact]
        public void InsertRange_GuardClauses()
        {
            // Arrange
            var config = new HttpConfiguration();
            var defaultServices = new DefaultServices(config);

            // Act & assert
            Assert.ThrowsArgumentNull(() => defaultServices.InsertRange(serviceType: null, index: 0, services: new[] { new object() }), "serviceType");
            Assert.ThrowsArgumentNull(() => defaultServices.InsertRange(typeof(object), 0, services: null), "services");
            Assert.ThrowsArgument(
                () => defaultServices.InsertRange(typeof(object), 0, new[] { new object() }),
                "serviceType",
                "The service type Object is not supported.");
            Assert.ThrowsArgument(
                () => defaultServices.InsertRange(typeof(IHttpActionInvoker), 0, new[] { new object() }),
                "services",
                "The type Object must derive from IHttpActionInvoker.");
            Assert.ThrowsArgumentOutOfRange(
                () => defaultServices.InsertRange(typeof(ValueProviderFactory), -1, new[] { new Mock<ValueProviderFactory>().Object }),
                "index",
                "Index was out of range. Must be non-negative and less than the size of the collection.");
        }

        [Fact]
        public void InsertRange_AddsElementAtTheRequestedLocation()
        {
            // Arrange
            var config = new HttpConfiguration();
            var defaultServices = new DefaultServices(config);
            var filterProvider1 = new Mock<IFilterProvider>().Object;
            var filterProvider2 = new Mock<IFilterProvider>().Object;
            var newFilterProvider1 = new Mock<IFilterProvider>().Object;
            var newFilterProvider2 = new Mock<IFilterProvider>().Object;
            defaultServices.ReplaceRange(typeof(IFilterProvider), new[] { filterProvider1, filterProvider2 });

            // Act
            defaultServices.InsertRange(typeof(IFilterProvider), 1, new[] { newFilterProvider1, newFilterProvider2 });

            // Assert
            Assert.Equal(new[] { filterProvider1, newFilterProvider1, newFilterProvider2, filterProvider2 }, defaultServices.GetServices(typeof(IFilterProvider)));
        }

        // Remove tests

        [Fact]
        public void Remove_GuardClauses()
        {
            // Arrange
            var config = new HttpConfiguration();
            var defaultServices = new DefaultServices(config);

            // Act & assert
            Assert.ThrowsArgumentNull(() => defaultServices.Remove(serviceType: null, service: new object()), "serviceType");
            Assert.ThrowsArgumentNull(() => defaultServices.Remove(typeof(object), service: null), "service");
            Assert.ThrowsArgument(
                () => defaultServices.Remove(typeof(object), new object()),
                "serviceType",
                "The service type Object is not supported.");
        }

        [Fact]
        public void Remove_ObjectFound()
        {
            // Arrange
            var config = new HttpConfiguration();
            var defaultServices = new DefaultServices(config);
            var filterProvider1 = new Mock<IFilterProvider>().Object;
            var filterProvider2 = new Mock<IFilterProvider>().Object;
            defaultServices.ReplaceRange(typeof(IFilterProvider), new[] { filterProvider1, filterProvider2 });

            // Act
            defaultServices.Remove(typeof(IFilterProvider), filterProvider1);

            // Assert
            Assert.Equal(new[] { filterProvider2 }, defaultServices.GetServices(typeof(IFilterProvider)));
        }

        [Fact]
        public void Remove_ObjectNotFound()
        {
            // Arrange
            var config = new HttpConfiguration();
            var defaultServices = new DefaultServices(config);
            var filterProvider1 = new Mock<IFilterProvider>().Object;
            var filterProvider2 = new Mock<IFilterProvider>().Object;
            var notPresentFilterProvider = new Mock<IFilterProvider>().Object;
            defaultServices.ReplaceRange(typeof(IFilterProvider), new[] { filterProvider1, filterProvider2 });

            // Act
            defaultServices.Remove(typeof(IFilterProvider), notPresentFilterProvider);

            // Assert
            Assert.Equal(new[] { filterProvider1, filterProvider2 }, defaultServices.GetServices(typeof(IFilterProvider)));
        }

        // RemoveAll tests

        [Fact]
        public void RemoveAll_GuardClauses()
        {
            // Arrange
            var config = new HttpConfiguration();
            var defaultServices = new DefaultServices(config);

            // Act & assert
            Assert.ThrowsArgumentNull(() => defaultServices.RemoveAll(serviceType: null, match: _ => true), "serviceType");
            Assert.ThrowsArgumentNull(() => defaultServices.RemoveAll(typeof(object), match: null), "match");
            Assert.ThrowsArgument(
                () => defaultServices.RemoveAll(typeof(object), _ => true),
                "serviceType",
                "The service type Object is not supported.");
        }

        [Fact]
        public void RemoveAll_SuccessfulMatch()
        {
            // Arrange
            var config = new HttpConfiguration();
            var defaultServices = new DefaultServices(config);
            var filterProvider1 = new Mock<IFilterProvider>().Object;
            var filterProvider2 = new Mock<IFilterProvider>().Object;
            defaultServices.ReplaceRange(typeof(IFilterProvider), new[] { filterProvider1, filterProvider2 });

            // Act
            defaultServices.RemoveAll(typeof(IFilterProvider), _ => true);

            // Assert
            Assert.Empty(defaultServices.GetServices(typeof(IFilterProvider)));
        }

        [Fact]
        public void RemoveAll_PartialMatch()
        {
            // Arrange
            var config = new HttpConfiguration();
            var defaultServices = new DefaultServices(config);
            var filterProvider1 = new Mock<IFilterProvider>().Object;
            var filterProvider2 = new Mock<IFilterProvider>().Object;
            defaultServices.ReplaceRange(typeof(IFilterProvider), new[] { filterProvider1, filterProvider2 });

            // Act
            defaultServices.RemoveAll(typeof(IFilterProvider), obj => obj == filterProvider2);

            // Assert
            Assert.Equal(new[] { filterProvider1 }, defaultServices.GetServices(typeof(IFilterProvider)));
        }

        // RemoveAt tests

        [Fact]
        public void RemoveAt_GuardClauses()
        {
            // Arrange
            var config = new HttpConfiguration();
            var defaultServices = new DefaultServices(config);

            // Act & assert
            Assert.ThrowsArgumentNull(() => defaultServices.RemoveAt(serviceType: null, index: 0), "serviceType");
            Assert.ThrowsArgument(
                () => defaultServices.RemoveAt(typeof(object), 0),
                "serviceType",
                "The service type Object is not supported.");
            Assert.ThrowsArgumentOutOfRange(
                () => defaultServices.RemoveAt(typeof(IFilterProvider), -1),
                "index",
                "Index was out of range. Must be non-negative and less than the size of the collection.");
        }

        [Fact]
        public void RemoteAt_RemovesService()
        {
            // Arrange
            var config = new HttpConfiguration();
            var defaultServices = new DefaultServices(config);
            var filterProvider1 = new Mock<IFilterProvider>().Object;
            var filterProvider2 = new Mock<IFilterProvider>().Object;
            defaultServices.ReplaceRange(typeof(IFilterProvider), new[] { filterProvider1, filterProvider2 });

            // Act
            defaultServices.RemoveAt(typeof(IFilterProvider), 1);

            // Assert
            Assert.Equal(new[] { filterProvider1 }, defaultServices.GetServices(typeof(IFilterProvider)));
        }

        // Replace tests
        [Fact]
        public void Replace_SetsNull()
        {
            // Arrange
            var config = new HttpConfiguration();
            var defaultServices = new DefaultServices(config);

            defaultServices.Replace(typeof(IActionValueBinder), service: null);
        }

        [Fact]
        public void Replace_GuardClauses()
        {
            // Arrange
            var config = new HttpConfiguration();
            var defaultServices = new DefaultServices(config);

            // Act & assert
            Assert.ThrowsArgumentNull(() => defaultServices.Replace(serviceType: null, service: new object()), "serviceType");
            Assert.ThrowsArgument(
                () => defaultServices.Replace(typeof(object), new object()),
                "serviceType",
                "The service type Object is not supported.");
            Assert.ThrowsArgument(
                () => defaultServices.Replace(typeof(IHttpActionInvoker), new object()),
                "service",
                "The type Object must derive from IHttpActionInvoker.");
        }

        [Fact]
        public void Replace_ReplacesAllValuesWithTheGivenService()
        {
            // Arrange
            var config = new HttpConfiguration();
            var defaultServices = new DefaultServices(config);
            var filterProvider1 = new Mock<IFilterProvider>().Object;
            var filterProvider2 = new Mock<IFilterProvider>().Object;
            var newFilterProvider = new Mock<IFilterProvider>().Object;
            defaultServices.ReplaceRange(typeof(IFilterProvider), new[] { filterProvider1, filterProvider2 });

            // Act
            defaultServices.Replace(typeof(IFilterProvider), newFilterProvider);

            // Assert
            Assert.Equal(new[] { newFilterProvider }, defaultServices.GetServices(typeof(IFilterProvider)));
        }

        // ReplaceRange tests

        [Fact]
        public void ReplaceRange_GuardClauses()
        {
            // Arrange
            var config = new HttpConfiguration();
            var defaultServices = new DefaultServices(config);

            // Act & assert
            Assert.ThrowsArgumentNull(() => defaultServices.ReplaceRange(serviceType: null, services: new[] { new object() }), "serviceType");
            Assert.ThrowsArgumentNull(() => defaultServices.ReplaceRange(typeof(object), services: null), "services");
            Assert.ThrowsArgument(
                () => defaultServices.ReplaceRange(typeof(object), new[] { new object() }),
                "serviceType",
                "The service type Object is not supported.");
            Assert.ThrowsArgument(
                () => defaultServices.ReplaceRange(typeof(ValueProviderFactory), new[] { new object() }),
                "services",
                "The type Object must derive from ValueProviderFactory.");
        }

        [Fact]
        public void ReplaceRange_ReplacesAllValuesWithTheGivenServices()
        {
            // Arrange
            var config = new HttpConfiguration();
            var defaultServices = new DefaultServices(config);
            var filterProvider1 = new Mock<IFilterProvider>().Object;
            var filterProvider2 = new Mock<IFilterProvider>().Object;

            // Act
            defaultServices.ReplaceRange(typeof(IFilterProvider), new[] { filterProvider1, filterProvider2 });

            // Assert
            Assert.Equal(new[] { filterProvider1, filterProvider2 }, defaultServices.GetServices(typeof(IFilterProvider)));
        }
    }
}
