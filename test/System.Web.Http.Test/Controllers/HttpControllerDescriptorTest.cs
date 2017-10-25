// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http
{
    public class HttpControllerDescriptorTest
    {
        [Fact]
        public void Default_Constructor()
        {
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor();

            Assert.Null(controllerDescriptor.ControllerName);
            Assert.Null(controllerDescriptor.Configuration);
            Assert.Null(controllerDescriptor.ControllerType);
            Assert.NotNull(controllerDescriptor.Properties);
        }

        [Fact]
        public void Parameter_Constructor()
        {
            HttpConfiguration config = new HttpConfiguration();
            string controllerName = "UsersController";
            Type controllerType = typeof(UsersController);

            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(config, controllerName, controllerType);
            Assert.NotNull(controllerDescriptor.ControllerName);
            Assert.NotNull(controllerDescriptor.Configuration);
            Assert.NotNull(controllerDescriptor.ControllerType);
            Assert.NotNull(controllerDescriptor.Properties);
            Assert.Equal(config, controllerDescriptor.Configuration);
            Assert.Equal(controllerName, controllerDescriptor.ControllerName);
            Assert.Equal(controllerType, controllerDescriptor.ControllerType);
        }

        [Fact]
        public void Constructor_Throws_IfConfigurationIsNull()
        {
            Assert.ThrowsArgumentNull(
                () => new HttpControllerDescriptor(null, "UsersController", typeof(UsersController)),
                "configuration");
        }

        [Fact]
        public void Constructor_Throws_IfControllerNameIsNull()
        {
            Assert.ThrowsArgumentNull(
                () => new HttpControllerDescriptor(new HttpConfiguration(), null, typeof(UsersController)),
                "controllerName");
        }

        [Fact]
        public void Constructor_Throws_IfControllerTypeIsNull()
        {
            Assert.ThrowsArgumentNull(
                () => new HttpControllerDescriptor(new HttpConfiguration(), "UsersController", null),
                "controllerType");
        }

        [Fact]
        public void Configuration_Property()
        {
            HttpConfiguration config = new HttpConfiguration();
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor();

            Assert.Reflection.Property<HttpControllerDescriptor, HttpConfiguration>(
                instance: controllerDescriptor,
                propertyGetter: cd => cd.Configuration,
                expectedDefaultValue: null,
                allowNull: false,
                roundTripTestValue: config);
        }

        [Fact]
        public void ControllerName_Property()
        {
            string controllerName = "UsersController";
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor();

            Assert.Reflection.Property<HttpControllerDescriptor, string>(
                instance: controllerDescriptor,
                propertyGetter: cd => cd.ControllerName,
                expectedDefaultValue: null,
                allowNull: false,
                roundTripTestValue: controllerName);
        }

        [Fact]
        public void ControllerType_Property()
        {
            Type controllerType = typeof(UsersController);
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor();

            Assert.Reflection.Property<HttpControllerDescriptor, Type>(
                instance: controllerDescriptor,
                propertyGetter: cd => cd.ControllerType,
                expectedDefaultValue: null,
                allowNull: false,
                roundTripTestValue: controllerType);
        }

        [Fact]
        public void GetFilters_InvokesGetCustomAttributesMethod()
        {
            var descriptorMock = new Mock<HttpControllerDescriptor> { CallBase = true };
            var filters = new Collection<IFilter>(new List<IFilter>());
            descriptorMock.Setup(d => d.GetCustomAttributes<IFilter>()).Returns(filters).Verifiable();

            var result = descriptorMock.Object.GetFilters();

            Assert.Same(filters, result);
            descriptorMock.Verify();
        }

        [Fact]
        public void Initialize_In_InheritenceHierarchy()
        {
            // Verifies that initialization is run in order with , and that they all mutate on the same descriptor object
            HttpConfiguration config = new HttpConfiguration();

            // Act.
            HttpControllerDescriptor desc = new HttpControllerDescriptor(config, "MyController", typeof(MyDerived2Controller));

            // Assert
            Assert.Same(MyDerived1Controller.SelectorBase, desc.Configuration.Services.GetActionSelector());
            Assert.Same(MyDerived1Controller.ActionValueBinderDerived1, desc.Configuration.Services.GetActionValueBinder());
            Assert.Same(config.Formatters, desc.Configuration.Formatters); // didn't override, stays the same
            Assert.Same(config.ParameterBindingRules, desc.Configuration.ParameterBindingRules); // didn't override, stays the same
        }

        [Fact]
        public void Initialize_In_InheritenceHierarchy_Branching()
        {
            // Verifies that initialization is run in order with, and that they all mutate on the same descriptor object
            HttpConfiguration config = new HttpConfiguration();

            // Act.
            HttpControllerDescriptor desc = new HttpControllerDescriptor(config, "MyController", typeof(MyDerived3Controller));

            // Assert
            Assert.Same(MyDerived1Controller.SelectorBase, desc.Configuration.Services.GetActionSelector());
            Assert.Same(MyDerived3Controller.ActionValueBinderDerived3, desc.Configuration.Services.GetActionValueBinder());
            Assert.Same(config.Formatters, desc.Configuration.Formatters); // didn't override, stays the same
            Assert.Same(config.ParameterBindingRules, desc.Configuration.ParameterBindingRules); // didn't override, stays the same
        }

        [Fact]
        public void Initialize_GetsTheActualControllerDescriptor_In_InheritenceHierarchy()
        {
            HttpConfiguration config = new HttpConfiguration();

            HttpControllerDescriptor desc = new HttpControllerDescriptor(config, "MyController", typeof(VerifyControllerDescriptorDerivedController));
            Assert.Equal(typeof(VerifyControllerDescriptorDerivedController), VerifyControllerDescriptorAttribute.ControllerType);

            desc = new HttpControllerDescriptor(config, "MyController", typeof(VerifyControllerDescriptorBaseController));
            Assert.Equal(typeof(VerifyControllerDescriptorBaseController), VerifyControllerDescriptorAttribute.ControllerType);
        }

        [Fact]
        public void EmptySetting_DoesNotChangeTheConfigurationInstance()
        {
            HttpConfiguration config = new HttpConfiguration();

            HttpControllerDescriptor desc = new HttpControllerDescriptor(config, "MyController", typeof(NoopControllerConfigController));
            Assert.Same(config, desc.Configuration); // didn't change anything, the config instance stays the same
        }

        [Fact]
        public void GetCustomAttributes_GetsInheritedAttributes()
        {
            HttpConfiguration config = new HttpConfiguration();
            HttpControllerDescriptor desc = new HttpControllerDescriptor(config, "MyController", typeof(MyDerived1Controller));

            var attributes = desc.GetCustomAttributes<MyConfigBaseAttribute>();

            Assert.Single(attributes);
        }

        [Fact]
        public void GetCustomAttributesInheritTrue_GetsInheritedAttributes()
        {
            HttpConfiguration config = new HttpConfiguration();
            HttpControllerDescriptor desc = new HttpControllerDescriptor(config, "MyController", typeof(MyDerived1Controller));

            var attributes = desc.GetCustomAttributes<MyConfigBaseAttribute>(inherit: true);

            Assert.Single(attributes);
        }

        [Fact]
        public void GetCustomAttributesInheritFalse_DoesNotGetInheritedAttributes()
        {
            HttpConfiguration config = new HttpConfiguration();
            HttpControllerDescriptor desc = new HttpControllerDescriptor(config, "MyController", typeof(MyDerived1Controller));

            var attributes = desc.GetCustomAttributes<MyConfigBaseAttribute>(inherit: false);

            Assert.Empty(attributes);
        }

        [Fact]
        public void GetCustomAttributesInheritFalse_GetsDeclaredAttributes()
        {
            HttpConfiguration config = new HttpConfiguration();
            HttpControllerDescriptor desc = new HttpControllerDescriptor(config, "MyController", typeof(MyDerived1Controller));

            var attributes = desc.GetCustomAttributes<MyConfigDerived1Attribute>(inherit: false);

            Assert.Single(attributes);
        }

        class NoopControllerConfigAttribute : Attribute, IControllerConfiguration
        {
            public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
            {
            }
        }

        [NoopControllerConfig]
        class NoopControllerConfigController : ApiController
        {
        }

        class VerifyControllerDescriptorAttribute : Attribute, IControllerConfiguration
        {
            public static Type ControllerType;
            public void Initialize(HttpControllerSettings settings, HttpControllerDescriptor controllerDescriptor)
            {
                ControllerType = controllerDescriptor.ControllerType;
            }
        }

        [VerifyControllerDescriptor]
        class VerifyControllerDescriptorBaseController : ApiController
        {
        }

        class VerifyControllerDescriptorDerivedController : VerifyControllerDescriptorBaseController
        {
        }

        class MyConfigBaseAttribute : Attribute, IControllerConfiguration
        {
            public void Initialize(HttpControllerSettings settings, HttpControllerDescriptor controllerDescriptor)
            {
                settings.Services.Replace(typeof(IActionValueBinder), MyBaseController.ActionValueBinderBase);
                settings.Services.Replace(typeof(IHttpActionSelector), MyBaseController.SelectorBase);
            }
        }

        [MyConfigBase]
        class MyBaseController : ApiController
        {
            public static IHttpActionSelector SelectorBase = new Mock<IHttpActionSelector>().Object;
            public static IActionValueBinder ActionValueBinderBase = new Mock<IActionValueBinder>().Object;
        }

        class MyConfigDerived1Attribute : Attribute, IControllerConfiguration
        {
            public void Initialize(HttpControllerSettings settings, HttpControllerDescriptor controllerDescriptor)
            {
                // Base runs first, so we should be able to see changes from the base.
                Assert.Same(MyBaseController.ActionValueBinderBase, settings.Services.GetActionValueBinder());

                // Also overwrite them
                settings.Services.Replace(typeof(IActionValueBinder), MyDerived1Controller.ActionValueBinderDerived1);
            }
        }

        class MyConfigDerived3Attribute : Attribute, IControllerConfiguration
        {
            public void Initialize(HttpControllerSettings settings, HttpControllerDescriptor controllerDescriptor)
            {
                // MyConfigDerived1 runs first, so we should be able to see changes from the MyConfigDerived1.
                Assert.Same(MyDerived1Controller.ActionValueBinderDerived1, settings.Services.GetActionValueBinder());

                // Also overwrite them
                settings.Services.Replace(typeof(IActionValueBinder), MyDerived3Controller.ActionValueBinderDerived3);
            }
        }

        [MyConfigDerived1]
        class MyDerived1Controller : MyBaseController
        {
            public static IActionValueBinder ActionValueBinderDerived1 = new Mock<IActionValueBinder>().Object;
        }

        class MyDerived2Controller : MyDerived1Controller
        {
        }

        [MyConfigDerived3]
        class MyDerived3Controller : MyDerived1Controller
        {
            public static IActionValueBinder ActionValueBinderDerived3 = new Mock<IActionValueBinder>().Object;
        }


        [Fact]
        public void Initialize_Append_A_Formatter()
        {
            // Verifies that controller inherit the formatter list from the global config, and can mutate it.
            HttpConfiguration config = new HttpConfiguration();

            MediaTypeFormatter globalFormatter = new Mock<MediaTypeFormatter>().Object;
            config.Formatters.Clear();
            config.Formatters.Add(globalFormatter);

            // Act.
            HttpControllerDescriptor desc = new HttpControllerDescriptor(config, "MyController", typeof(MyControllerWithCustomFormatter));

            // Assert
            Assert.Equal(2, desc.Configuration.Formatters.Count);
            Assert.Same(globalFormatter, desc.Configuration.Formatters[0]);
            Assert.Same(MyControllerWithCustomFormatter.CustomFormatter, desc.Configuration.Formatters[1]);
        }


        class MyControllerWithCustomFormatterConfigAttribute : Attribute, IControllerConfiguration
        {
            public void Initialize(HttpControllerSettings settings, HttpControllerDescriptor controllerDescriptor)
            {
                // Appends to existing list. Formatter list has copy-on-write semantics.
                Assert.Single(settings.Formatters); // the one we already set
                settings.Formatters.Add(MyControllerWithCustomFormatter.CustomFormatter);
            }
        }

        [MyControllerWithCustomFormatterConfig]
        class MyControllerWithCustomFormatter : ApiController
        {
            public static MediaTypeFormatter CustomFormatter = new Mock<MediaTypeFormatter>().Object;

        }

    }
}
