// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
#if ASPNETWEBAPI
using System.Web.Http.Controllers;
#endif
using Microsoft.TestCommon;
using Moq;

#if ASPNETWEBAPI
using TActionDescriptor = System.Web.Http.Controllers.HttpActionDescriptor;
using TRoute = System.Web.Http.Routing.IHttpRoute;
using TRouteDictionary = System.Collections.Generic.IDictionary<string, object>;
using TRouteDictionaryConcrete = System.Web.Http.Routing.HttpRouteValueDictionary;
#else
using TActionDescriptor = System.Web.Mvc.ActionDescriptor;
using TRoute = System.Web.Routing.Route;
using TRouteDictionary = System.Web.Routing.RouteValueDictionary;
using TRouteDictionaryConcrete = System.Web.Routing.RouteValueDictionary;
#endif

#if ASPNETWEBAPI
namespace System.Web.Http.Routing
#else
namespace System.Web.Mvc.Routing
#endif
{
    public class RouteFactoryAttributeTests
    {
        [Fact]
        public void TemplateGet_ReturnsSpecifiedInstance()
        {
            // Arrange
            string expectedTemplate = "RouteTemplate";
            RouteFactoryAttribute product = CreateProductUnderTest(expectedTemplate);

            // Act
            string template = product.Template;

            // Assert
            Assert.Same(expectedTemplate, template);
        }

        [Fact]
        public void NameGet_ReturnsNull()
        {
            // Arrange
            RouteFactoryAttribute product = CreateProductUnderTest();

            // Act
            string name = product.Name;

            // Assert
            Assert.Null(name);
        }

        [Fact]
        public void OrderGet_ReturnsZero()
        {
            // Arrange
            RouteFactoryAttribute product = CreateProductUnderTest();

            // Act
            int order = product.Order;

            // Assert
            Assert.Equal(0, order);
        }

        [Fact]
        public void ConstraintsGet_ReturnsNull()
        {
            // Arrange
            RouteFactoryAttribute product = CreateProductUnderTest();

            // Act
            TRouteDictionary constraints = product.Constraints;

            // Assert
            Assert.Null(constraints);
        }

        [Fact]
        public void CreateRoute_DelegatesToContextCreateBuilderBuild()
        {
            // Arrange
            string expectedTemplate = "RouteTemplate";
            IDirectRouteFactory product = CreateProductUnderTest(expectedTemplate);

            RouteEntry expectedEntry = CreateEntry();

            IDirectRouteBuilder builder = CreateBuilder(() => expectedEntry);
            DirectRouteFactoryContext context = CreateContext((template) => template == expectedTemplate ? builder :
                new DirectRouteBuilder(new TActionDescriptor[0], targetIsAction: true));

            // Act
            RouteEntry entry = product.CreateRoute(context);

            // Assert
            Assert.Same(expectedEntry, entry);
        }

        [Fact]
        public void CreateRoute_IfContextIsNull_Throws()
        {
            // Arrange
            DirectRouteFactoryContext context = null;
            IDirectRouteFactory product = CreateProductUnderTest();

            // Act & Assert
            Assert.ThrowsArgumentNull(() => product.CreateRoute(context), "context");
        }

        [Fact]
        public void CreateRoute_UsesNamePropertyWhenBuilding()
        {
            // Arrange
            string expectedName = "RouteName";
            RouteFactoryAttribute product = CreateProductUnderTest();
            product.Name = expectedName;

            string name = null;
            IDirectRouteBuilder builder = null;
            builder = CreateBuilder(() =>
            {
                name = builder.Name;
                return null;
            });
            DirectRouteFactoryContext context = CreateContext((i) => builder);

            // Act
            RouteEntry ignore = product.CreateRoute(context);

            // Assert
            Assert.Same(expectedName, name);
        }

        [Fact]
        public void CreateRoute_UsesOrderPropertyWhenBuilding()
        {
            // Arrange
            int expectedOrder = 123;
            RouteFactoryAttribute product = CreateProductUnderTest();
            product.Order = expectedOrder;

            int order = 0;
            IDirectRouteBuilder builder = null;
            builder = CreateBuilder(() =>
            {
                order = builder.Order;
                return null;
            });
            DirectRouteFactoryContext context = CreateContext((i) => builder);

            // Act
            RouteEntry ignore = product.CreateRoute(context);

            // Assert
            Assert.Equal(expectedOrder, order);
        }

        [Fact]
        public void CreateRoute_IfBuilderDefaultsIsNull_UsesDefaultsPropertyWhenBuilding()
        {
            // Arrange
            TRouteDictionary expectedDefaults = new TRouteDictionaryConcrete();
            Mock<RouteFactoryAttribute> productMock = CreateProductUnderTestMock();
            productMock.SetupGet(p => p.Defaults).Returns(expectedDefaults);
            IDirectRouteFactory product = productMock.Object;

            RouteEntry expectedEntry = CreateEntry();

            TRouteDictionary defaults = null;
            IDirectRouteBuilder builder = null;
            builder = CreateBuilder(() =>
            {
                defaults = builder.Defaults;
                return null;
            });
            Assert.Null(builder.Defaults); // Guard
            DirectRouteFactoryContext context = CreateContext((i) => builder);

            // Act
            RouteEntry ignore = product.CreateRoute(context);

            // Assert
            Assert.Same(expectedDefaults, defaults);
        }

        [Fact]
        public void CreateRoute_IfBuilderDefaultsIsNotNull_UpdatesDefaultsFromPropertyWhenBuilding()
        {
            // Arrange
            TRouteDictionary existingDefaults = new TRouteDictionaryConcrete();
            string existingDefaultKey = "ExistingDefaultKey";
            object existingDefaultValue = "ExistingDefault";
            existingDefaults.Add(existingDefaultKey, existingDefaultValue);
            string conflictingDefaultKey = "ConflictingDefaultKey";
            object oldConflictingDefaultValue = "OldConflictingDefault";
            existingDefaults.Add(conflictingDefaultKey, oldConflictingDefaultValue);

            TRouteDictionary additionalDefaults = new TRouteDictionaryConcrete();
            string additionalDefaultKey = "NewDefaultKey";
            string additionalDefaultValue = "NewDefault";
            additionalDefaults.Add(additionalDefaultKey, additionalDefaultValue);
            string newConflictingDefaultValue = "NewConflictingDefault";
            additionalDefaults.Add(conflictingDefaultKey, newConflictingDefaultValue);

            Mock<RouteFactoryAttribute> productMock = CreateProductUnderTestMock();
            productMock.SetupGet(p => p.Defaults).Returns(additionalDefaults);
            IDirectRouteFactory product = productMock.Object;

            RouteEntry expectedEntry = CreateEntry();

            TRouteDictionary defaults = null;
            IDirectRouteBuilder builder = null;
            builder = CreateBuilder(() =>
            {
                defaults = builder.Defaults;
                return null;
            });

            builder.Defaults = existingDefaults;

            DirectRouteFactoryContext context = CreateContext((i) => builder);

            // Act
            RouteEntry ignore = product.CreateRoute(context);

            // Assert
            Assert.Same(existingDefaults, defaults);
            Assert.Equal(3, defaults.Count);
            Assert.True(defaults.ContainsKey(existingDefaultKey));
            Assert.Same(existingDefaultValue, defaults[existingDefaultKey]);
            Assert.True(defaults.ContainsKey(conflictingDefaultKey));
            Assert.Same(newConflictingDefaultValue, defaults[conflictingDefaultKey]);
            Assert.True(defaults.ContainsKey(additionalDefaultKey));
            Assert.Same(additionalDefaultValue, defaults[additionalDefaultKey]);
        }

        [Fact]
        public void CreateRoute_IfBuilderConstraintsIsNotNullAndDefaultsPropertyIsNull_UsesBuilderDefaults()
        {
            // Arrange
            TRouteDictionary existingDefaults = new TRouteDictionaryConcrete();

            Mock<RouteFactoryAttribute> productMock = CreateProductUnderTestMock();
            productMock.SetupGet(p => p.Defaults).Returns((TRouteDictionary)null);
            IDirectRouteFactory product = productMock.Object;

            RouteEntry expectedEntry = CreateEntry();

            TRouteDictionary defaults = null;
            IDirectRouteBuilder builder = null;
            builder = CreateBuilder(() =>
            {
                defaults = builder.Defaults;
                return null;
            });

            builder.Defaults = existingDefaults;

            DirectRouteFactoryContext context = CreateContext((i) => builder);

            // Act
            RouteEntry ignore = product.CreateRoute(context);

            // Assert
            Assert.Same(existingDefaults, defaults);
        }

        [Fact]
        public void CreateRoute_IfBuilderConstraintsIsNull_UsesConstraintsPropertyWhenBuilding()
        {
            // Arrange
            TRouteDictionary expectedConstraints = new TRouteDictionaryConcrete();
            Mock<RouteFactoryAttribute> productMock = CreateProductUnderTestMock();
            productMock.SetupGet(p => p.Constraints).Returns(expectedConstraints);
            IDirectRouteFactory product = productMock.Object;

            RouteEntry expectedEntry = CreateEntry();

            TRouteDictionary constraints = null;
            IDirectRouteBuilder builder = null;
            builder = CreateBuilder(() =>
            {
                constraints = builder.Constraints;
                return null;
            });
            Assert.Null(builder.Constraints); // Guard
            DirectRouteFactoryContext context = CreateContext((i) => builder);

            // Act
            RouteEntry ignore = product.CreateRoute(context);

            // Assert
            Assert.Same(expectedConstraints, constraints);
        }

        [Fact]
        public void CreateRoute_IfBuilderConstraintsIsNotNull_UpdatesConstraintsFromPropertyWhenBuilding()
        {
            // Arrange
            TRouteDictionary existingConstraints = new TRouteDictionaryConcrete();
            string existingConstraintKey = "ExistingConstraintKey";
            object existingConstraintValue = "ExistingConstraint";
            existingConstraints.Add(existingConstraintKey, existingConstraintValue);
            string conflictingConstraintKey = "ConflictingConstraintKey";
            object oldConflictingConstraintValue = "OldConflictingConstraint";
            existingConstraints.Add(conflictingConstraintKey, oldConflictingConstraintValue);

            TRouteDictionary additionalConstraints = new TRouteDictionaryConcrete();
            string additionalConstraintKey = "NewConstraintKey";
            string additionalConstraintValue = "NewConstraint";
            additionalConstraints.Add(additionalConstraintKey, additionalConstraintValue);
            string newConflictingConstraintValue = "NewConflictingConstraint";
            additionalConstraints.Add(conflictingConstraintKey, newConflictingConstraintValue);

            Mock<RouteFactoryAttribute> productMock = CreateProductUnderTestMock();
            productMock.SetupGet(p => p.Constraints).Returns(additionalConstraints);
            IDirectRouteFactory product = productMock.Object;

            RouteEntry expectedEntry = CreateEntry();

            TRouteDictionary constraints = null;
            IDirectRouteBuilder builder = null;
            builder = CreateBuilder(() =>
            {
                constraints = builder.Constraints;
                return null;
            });

            builder.Constraints = existingConstraints;

            DirectRouteFactoryContext context = CreateContext((i) => builder);

            // Act
            RouteEntry ignore = product.CreateRoute(context);

            // Assert
            Assert.Same(existingConstraints, constraints);
            Assert.Equal(3, constraints.Count);
            Assert.True(constraints.ContainsKey(existingConstraintKey));
            Assert.Same(existingConstraintValue, constraints[existingConstraintKey]);
            Assert.True(constraints.ContainsKey(conflictingConstraintKey));
            Assert.Same(newConflictingConstraintValue, constraints[conflictingConstraintKey]);
            Assert.True(constraints.ContainsKey(additionalConstraintKey));
            Assert.Same(additionalConstraintValue, constraints[additionalConstraintKey]);
        }

        [Fact]
        public void CreateRoute_IfBuilderConstraintsIsNotNullAndConstraintsPropertyIsNull_UsesBuilderConstraints()
        {
            // Arrange
            TRouteDictionary existingConstraints = new TRouteDictionaryConcrete();

            Mock<RouteFactoryAttribute> productMock = CreateProductUnderTestMock();
            productMock.SetupGet(p => p.Constraints).Returns((TRouteDictionary)null);
            IDirectRouteFactory product = productMock.Object;

            RouteEntry expectedEntry = CreateEntry();

            TRouteDictionary constraints = null;
            IDirectRouteBuilder builder = null;
            builder = CreateBuilder(() =>
            {
                constraints = builder.Constraints;
                return null;
            });

            builder.Constraints = existingConstraints;

            DirectRouteFactoryContext context = CreateContext((i) => builder);

            // Act
            RouteEntry ignore = product.CreateRoute(context);

            // Assert
            Assert.Same(existingConstraints, constraints);
        }

        [Fact]
        public void CreateRoute_IfBuilderDataTokensIsNull_UsesDataTokensPropertyWhenBuilding()
        {
            // Arrange
            TRouteDictionary expectedDataTokens = new TRouteDictionaryConcrete();
            Mock<RouteFactoryAttribute> productMock = CreateProductUnderTestMock();
            productMock.SetupGet(p => p.DataTokens).Returns(expectedDataTokens);
            IDirectRouteFactory product = productMock.Object;

            RouteEntry expectedEntry = CreateEntry();

            TRouteDictionary dataTokens = null;
            IDirectRouteBuilder builder = null;
            builder = CreateBuilder(() =>
            {
                dataTokens = builder.DataTokens;
                return null;
            });
            Assert.Null(builder.DataTokens); // Guard
            DirectRouteFactoryContext context = CreateContext((i) => builder);

            // Act
            RouteEntry ignore = product.CreateRoute(context);

            // Assert
            Assert.Same(expectedDataTokens, dataTokens);
        }

        [Fact]
        public void CreateRoute_IfBuilderDataTokensIsNotNull_UpdatesDataTokensFromPropertyWhenBuilding()
        {
            // Arrange
            TRouteDictionary existingDataTokens = new TRouteDictionaryConcrete();
            string existingDataTokenKey = "ExistingDataTokenKey";
            object existingDataTokenValue = "ExistingDataToken";
            existingDataTokens.Add(existingDataTokenKey, existingDataTokenValue);
            string conflictingDataTokenKey = "ConflictingDataTokenKey";
            object oldConflictingDataTokenValue = "OldConflictingDataToken";
            existingDataTokens.Add(conflictingDataTokenKey, oldConflictingDataTokenValue);

            TRouteDictionary additionalDataTokens = new TRouteDictionaryConcrete();
            string additionalDataTokenKey = "NewDataTokenKey";
            string additionalDataTokenValue = "NewDataToken";
            additionalDataTokens.Add(additionalDataTokenKey, additionalDataTokenValue);
            string newConflictingDataTokenValue = "NewConflictingDataToken";
            additionalDataTokens.Add(conflictingDataTokenKey, newConflictingDataTokenValue);

            Mock<RouteFactoryAttribute> productMock = CreateProductUnderTestMock();
            productMock.SetupGet(p => p.DataTokens).Returns(additionalDataTokens);
            IDirectRouteFactory product = productMock.Object;

            RouteEntry expectedEntry = CreateEntry();

            TRouteDictionary dataTokens = null;
            IDirectRouteBuilder builder = null;
            builder = CreateBuilder(() =>
            {
                dataTokens = builder.DataTokens;
                return null;
            });

            builder.DataTokens = existingDataTokens;

            DirectRouteFactoryContext context = CreateContext((i) => builder);

            // Act
            RouteEntry ignore = product.CreateRoute(context);

            // Assert
            Assert.Same(existingDataTokens, dataTokens);
            Assert.Equal(3, dataTokens.Count);
            Assert.True(dataTokens.ContainsKey(existingDataTokenKey));
            Assert.Same(existingDataTokenValue, dataTokens[existingDataTokenKey]);
            Assert.True(dataTokens.ContainsKey(conflictingDataTokenKey));
            Assert.Same(newConflictingDataTokenValue, dataTokens[conflictingDataTokenKey]);
            Assert.True(dataTokens.ContainsKey(additionalDataTokenKey));
            Assert.Same(additionalDataTokenValue, dataTokens[additionalDataTokenKey]);
        }

        [Fact]
        public void CreateRoute_IfBuilderDataTokensIsNotNullAndDataTokensPropertyIsNull_UsesBuilderDataTokens()
        {
            // Arrange
            TRouteDictionary existingDataTokens = new TRouteDictionaryConcrete();

            Mock<RouteFactoryAttribute> productMock = CreateProductUnderTestMock();
            productMock.SetupGet(p => p.DataTokens).Returns((TRouteDictionary)null);
            IDirectRouteFactory product = productMock.Object;

            RouteEntry expectedEntry = CreateEntry();

            TRouteDictionary dataTokens = null;
            IDirectRouteBuilder builder = null;
            builder = CreateBuilder(() =>
            {
                dataTokens = builder.DataTokens;
                return null;
            });

            builder.DataTokens = existingDataTokens;

            DirectRouteFactoryContext context = CreateContext((i) => builder);

            // Act
            RouteEntry ignore = product.CreateRoute(context);

            // Assert
            Assert.Same(existingDataTokens, dataTokens);
        }

        [Fact]
        public void AttributeUsage_IsAsSpecified()
        {
            // Act
            AttributeUsageAttribute usage = (AttributeUsageAttribute)Attribute.GetCustomAttribute(
                typeof(RouteFactoryAttribute), typeof(AttributeUsageAttribute));

            // Assert
            Assert.NotNull(usage);
            Assert.Equal(AttributeTargets.Class | AttributeTargets.Method, usage.ValidOn);
            Assert.False(usage.Inherited);
            Assert.True(usage.AllowMultiple);
        }

        private static IDirectRouteBuilder CreateBuilder(Func<RouteEntry> build)
        {
            return new LambdaDirectRouteBuilder(build);
        }

        private static DirectRouteFactoryContext CreateContext(Func<string, IDirectRouteBuilder> createBuilder)
        {
            return new LambdaDirectRouteFactoryContext(createBuilder);
        }

        private static RouteEntry CreateEntry()
        {
#if ASPNETWEBAPI
            TRoute route = new Mock<TRoute>(MockBehavior.Strict).Object;
#else
            TRoute route = new Mock<TRoute>(MockBehavior.Strict, null, null).Object;
#endif
            return new RouteEntry("IgnoreEntry", route);
        }

        private static RouteFactoryAttribute CreateProductUnderTest()
        {
            return CreateProductUnderTest("IgnoreTemplate");
        }

        private static RouteFactoryAttribute CreateProductUnderTest(string template)
        {
            return CreateProductUnderTestMock(template).Object;
        }

        private static Mock<RouteFactoryAttribute> CreateProductUnderTestMock()
        {
            return CreateProductUnderTestMock("IgnoreTemplate");
        }

        private static Mock<RouteFactoryAttribute> CreateProductUnderTestMock(string template)
        {
            Mock<RouteFactoryAttribute> mock = new Mock<RouteFactoryAttribute>(template);
            mock.CallBase = true;
            return mock;
        }

        private class LambdaDirectRouteFactoryContext : DirectRouteFactoryContext
        {
            private readonly Func<string, IDirectRouteBuilder> _createBuilder;

            public LambdaDirectRouteFactoryContext(Func<string, IDirectRouteBuilder> createBuilder)
#if ASPNETWEBAPI
                : base(null, new TActionDescriptor[] { new Mock<TActionDescriptor>().Object },
                new Mock<IInlineConstraintResolver>(MockBehavior.Strict).Object,
                targetIsAction: true)
#else
                : base(null, null, new TActionDescriptor[] { CreateStubActionDescriptor() },
                new Mock<IInlineConstraintResolver>(MockBehavior.Strict).Object, targetIsAction: true)
#endif
            {
                Contract.Assert(createBuilder != null);
                _createBuilder = createBuilder;
            }

            internal override IDirectRouteBuilder CreateBuilderInternal(string template)
            {
                return _createBuilder.Invoke(template);
            }

#if !ASPNETWEBAPI
            private static ActionDescriptor CreateStubActionDescriptor()
            {
                Mock<ActionDescriptor> mock = new Mock<TActionDescriptor>();
                mock.Setup(m => m.ControllerDescriptor).Returns(new Mock<ControllerDescriptor>().Object);
                return mock.Object;
            }
#endif
        }

        private class LambdaDirectRouteBuilder : DirectRouteBuilder
        {
            private readonly Func<RouteEntry> _build;

            public LambdaDirectRouteBuilder(Func<RouteEntry> build)
                : base(new TActionDescriptor[0], targetIsAction: true)
            {
                Contract.Assert(build != null);
                _build = build;
            }

            public override RouteEntry Build()
            {
                return _build.Invoke();
            }
        }
    }
}
