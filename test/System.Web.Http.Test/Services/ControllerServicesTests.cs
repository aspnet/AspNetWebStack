// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using System.Web.Http.ValueProviders;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Services
{
    public class ControllerServicesTests
    {
        [Fact]
        public void Get_Falls_Through_To_Global()
        {
            HttpConfiguration config = new HttpConfiguration();
            ControllerServices cs = new ControllerServices(config.Services);

            // Act
            IActionValueBinder localVal = (IActionValueBinder) cs.GetService(typeof(IActionValueBinder));
            IActionValueBinder globalVal = (IActionValueBinder) config.Services.GetService(typeof(IActionValueBinder));
            
            // Assert
            // Local controller didn't override, should get same value as global case.
            Assert.Same(localVal, globalVal);
        }

        [Fact]
        public void Controller_Overrides_Global()
        {
            HttpConfiguration config = new HttpConfiguration();
            ControllerServices cs = new ControllerServices(config.Services);
                        
            IActionValueBinder newLocalService = new Mock<IActionValueBinder>().Object;
            cs.Replace(typeof(IActionValueBinder), newLocalService);

            // Act            
            IActionValueBinder localVal = (IActionValueBinder)cs.GetService(typeof(IActionValueBinder));
            IActionValueBinder globalVal = (IActionValueBinder)config.Services.GetService(typeof(IActionValueBinder));

            // Assert
            // Local controller didn't override, should get same value as global case.
            Assert.Same(localVal, newLocalService);
            Assert.NotSame(localVal, globalVal);
        }

        [Fact]
        public void Controller_Overrides_DependencyInjection()
        {
            // Setting on Controller config overrides the DI container. 
            HttpConfiguration config = new HttpConfiguration();

            IActionValueBinder newDIService = new Mock<IActionValueBinder>().Object;
            var mockDependencyResolver = new Mock<IDependencyResolver>();
            mockDependencyResolver.Setup(dr => dr.GetService(typeof(IActionValueBinder))).Returns(newDIService);
            config.DependencyResolver = mockDependencyResolver.Object;
            
            ControllerServices cs = new ControllerServices(config.Services);

            IActionValueBinder newLocalService = new Mock<IActionValueBinder>().Object;
            cs.Replace(typeof(IActionValueBinder), newLocalService);

            // Act            
            IActionValueBinder localVal = (IActionValueBinder)cs.GetService(typeof(IActionValueBinder));
            IActionValueBinder globalVal = (IActionValueBinder)config.Services.GetService(typeof(IActionValueBinder));

            // Assert
            // Local controller didn't override, should get same value as global case.            
            Assert.Same(newDIService, globalVal); // asking the config will give back the DI service
            Assert.Same(newLocalService, localVal); // but asking locally will get back the local service.
        }

        [Fact]
        public void Controller_Appends_Inherited_List()
        {
            // Controller Services has "copy on write" semantics for inherited list. 
            // It can get the inherited list and mutate it. 

            HttpConfiguration config = new HttpConfiguration();
            ServicesContainer global = config.Services;

            ControllerServices cs = new ControllerServices(config.Services);

            ValueProviderFactory vpf = new Mock<ValueProviderFactory>().Object;

            // Act
            cs.Add(typeof(ValueProviderFactory), vpf); // appends to end

            // Assert
            IEnumerable<object> original = global.GetServices(typeof(ValueProviderFactory));
            object[] modified = cs.GetServices(typeof(ValueProviderFactory)).ToArray();

            Assert.True(original.Count() > 1);
            object[] expected = original.Concat(new object[] { vpf }).ToArray();
            Assert.Equal(expected, modified);
        }

        [Fact]
        public void Controller_Clear_Single_Item()
        {
            HttpConfiguration config = new HttpConfiguration();
            ServicesContainer global = config.Services;

            ControllerServices cs = new ControllerServices(config.Services);
            IActionValueBinder newLocalService = new Mock<IActionValueBinder>().Object;
            cs.Replace(typeof(IActionValueBinder), newLocalService);

            // Act
            cs.Clear(typeof(IActionValueBinder));

            // Assert
            IActionValueBinder localVal = (IActionValueBinder)cs.GetService(typeof(IActionValueBinder));
            IActionValueBinder globalVal = (IActionValueBinder)config.Services.GetService(typeof(IActionValueBinder));

            Assert.Same(globalVal, localVal);
        }

        [Fact]
        public void Controller_Set_Null()
        {
            HttpConfiguration config = new HttpConfiguration();
            ServicesContainer global = config.Services;

            ControllerServices cs = new ControllerServices(config.Services);

            // Act
            // Setting to null is not the same as clear. Clear() means fall through to global config. 
            cs.Replace(typeof(IActionValueBinder), null);

            // Assert
            IActionValueBinder localVal = (IActionValueBinder)cs.GetService(typeof(IActionValueBinder));
            
            Assert.Null(localVal);
        }
    }
}