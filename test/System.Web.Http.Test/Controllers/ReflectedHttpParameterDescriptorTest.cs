// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http.Controllers;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http
{
    public class ReflectedHttpParameterDescriptorTest
    {
        [Fact]
        public void Parameter_Constructor()
        {
            UsersRpcController controller = new UsersRpcController();
            Func<string, string, User> echoUserMethod = controller.EchoUser;
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor { MethodInfo = echoUserMethod.Method };
            ParameterInfo parameterInfo = echoUserMethod.Method.GetParameters()[0];
            ReflectedHttpParameterDescriptor parameterDescriptor = new ReflectedHttpParameterDescriptor(actionDescriptor, parameterInfo);

            Assert.Equal(actionDescriptor, parameterDescriptor.ActionDescriptor);
            Assert.Null(parameterDescriptor.DefaultValue);
            Assert.Equal(parameterInfo, parameterDescriptor.ParameterInfo);
            Assert.Equal(parameterInfo.Name, parameterDescriptor.ParameterName);
            Assert.Equal(typeof(string), parameterDescriptor.ParameterType);
            Assert.Null(parameterDescriptor.Prefix);
            Assert.Null(parameterDescriptor.ParameterBinderAttribute);
            Assert.False(parameterDescriptor.IsOptional);
        }

        [Fact]
        public void Constructor_Throws_IfParameterInfoIsNull()
        {
            Assert.ThrowsArgumentNull(
                () => new ReflectedHttpParameterDescriptor(new Mock<HttpActionDescriptor>().Object, null),
                "parameterInfo");
        }

        [Fact]
        public void Constructor_Throws_IfActionDescriptorIsNull()
        {
            Assert.ThrowsArgumentNull(
                () => new ReflectedHttpParameterDescriptor(null, new Mock<ParameterInfo>().Object),
                "actionDescriptor");
        }

        [Fact]
        public void ParameterInfo_Property()
        {
            ParameterInfo referenceParameter = new Mock<ParameterInfo>().Object;
            Assert.Reflection.Property(new ReflectedHttpParameterDescriptor(), d => d.ParameterInfo, expectedDefaultValue: null, allowNull: false, roundTripTestValue: referenceParameter);
        }

        [Fact]
        public void ParameterBinderAttribute_NotNull_WhenParameterAttributeIsFound()
        {
            UsersRpcController controller = new UsersRpcController();
            Action<User> addUserMethod = controller.AddUser;
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor { MethodInfo = addUserMethod.Method };
            ParameterInfo parameterInfo = addUserMethod.Method.GetParameters()[0];
            ReflectedHttpParameterDescriptor parameterDescriptor = new ReflectedHttpParameterDescriptor(actionDescriptor, parameterInfo);
            Assert.NotNull(parameterDescriptor.ParameterBinderAttribute);
        }

        private static void MethodWithOptionalParam(int id = 7) { }

        [Fact]
        public void IsOptional_Returns_True_ForOptionalParameter()
        {
            UsersRpcController controller = new UsersRpcController();
            MethodInfo methodWithOptionalParam = GetType().GetMethod("MethodWithOptionalParam", BindingFlags.Static | BindingFlags.NonPublic);
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor { MethodInfo = methodWithOptionalParam };
            ParameterInfo parameterInfo = methodWithOptionalParam.GetParameters()[0];
            ReflectedHttpParameterDescriptor parameterDescriptor = new ReflectedHttpParameterDescriptor(actionDescriptor, parameterInfo);
            Assert.True(parameterDescriptor.IsOptional);
        }

        [Fact]
        public void GetCustomAttributes_Returns_ParameterAttributes()
        {
            UsersRpcController controller = new UsersRpcController();
            Action<User> addUserMethod = controller.AddUser;
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor { MethodInfo = addUserMethod.Method };
            ParameterInfo parameterInfo = addUserMethod.Method.GetParameters()[0];
            ReflectedHttpParameterDescriptor parameterDescriptor = new ReflectedHttpParameterDescriptor(actionDescriptor, parameterInfo);
            object[] attributes = parameterDescriptor.GetCustomAttributes<object>().ToArray();

            object attribute = Assert.Single(attributes);
            Assert.IsType<FromBodyAttribute>(attribute);
        }

        [Fact]
        public void GetCustomAttributes_AttributeType_Returns_ParameterAttributes()
        {
            UsersRpcController controller = new UsersRpcController();
            Action<User> addUserMethod = controller.AddUser;
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor { MethodInfo = addUserMethod.Method };
            ParameterInfo parameterInfo = addUserMethod.Method.GetParameters()[0];
            ReflectedHttpParameterDescriptor parameterDescriptor = new ReflectedHttpParameterDescriptor(actionDescriptor, parameterInfo);
            IEnumerable<FromBodyAttribute> attributes = parameterDescriptor.GetCustomAttributes<FromBodyAttribute>();

            Assert.Single(attributes);
        }
    }
}
