// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class ReflectedActionDescriptorTest
    {
        private static readonly MethodInfo _int32EqualsIntMethod = typeof(int).GetMethod("Equals", new Type[] { typeof(int) });

        [Fact]
        public void ConstructorSetsActionNameProperty()
        {
            // Arrange
            string name = "someName";

            // Act
            ReflectedActionDescriptor ad = new ReflectedActionDescriptor(new Mock<MethodInfo>().Object, "someName", new Mock<ControllerDescriptor>().Object, false /* validateMethod */);

            // Assert
            Assert.Equal(name, ad.ActionName);
        }

        [Fact]
        public void ConstructorSetsControllerDescriptorProperty()
        {
            // Arrange
            ControllerDescriptor cd = new Mock<ControllerDescriptor>().Object;

            // Act
            ReflectedActionDescriptor ad = new ReflectedActionDescriptor(new Mock<MethodInfo>().Object, "someName", cd, false /* validateMethod */);

            // Assert
            Assert.Same(cd, ad.ControllerDescriptor);
        }

        [Fact]
        public void ConstructorSetsMethodInfoProperty()
        {
            // Arrange
            MethodInfo methodInfo = new Mock<MethodInfo>().Object;

            // Act
            ReflectedActionDescriptor ad = new ReflectedActionDescriptor(methodInfo, "someName", new Mock<ControllerDescriptor>().Object, false /* validateMethod */);

            // Assert
            Assert.Same(methodInfo, ad.MethodInfo);
        }

        [Fact]
        public void ConstructorThrowsIfActionNameIsEmpty()
        {
            // Act & assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { new ReflectedActionDescriptor(new Mock<MethodInfo>().Object, "", new Mock<ControllerDescriptor>().Object); }, "actionName");
        }

        [Fact]
        public void ConstructorThrowsIfActionNameIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { new ReflectedActionDescriptor(new Mock<MethodInfo>().Object, null, new Mock<ControllerDescriptor>().Object); }, "actionName");
        }

        [Fact]
        public void ConstructorThrowsIfControllerDescriptorIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { new ReflectedActionDescriptor(new Mock<MethodInfo>().Object, "someName", null); }, "controllerDescriptor");
        }

        [Fact]
        public void ConstructorThrowsIfMethodInfoHasRefParameters()
        {
            // Arrange
            MethodInfo methodInfo = typeof(MyController).GetMethod("MethodHasRefParameter");

            // Act & assert
            Assert.Throws<ArgumentException>(
                delegate { new ReflectedActionDescriptor(methodInfo, "someName", new Mock<ControllerDescriptor>().Object); },
                "Cannot call action method 'Void MethodHasRefParameter(Int32 ByRef)' on controller 'System.Web.Mvc.Test.ReflectedActionDescriptorTest+MyController' because the parameter 'Int32& i' is passed by reference." + Environment.NewLine
              + "Parameter name: methodInfo");
        }

        [Fact]
        public void ConstructorThrowsIfMethodInfoHasOutParameters()
        {
            // Arrange
            MethodInfo methodInfo = typeof(MyController).GetMethod("MethodHasOutParameter");

            // Act & assert
            Assert.Throws<ArgumentException>(
                delegate { new ReflectedActionDescriptor(methodInfo, "someName", new Mock<ControllerDescriptor>().Object); },
                "Cannot call action method 'Void MethodHasOutParameter(Int32 ByRef)' on controller 'System.Web.Mvc.Test.ReflectedActionDescriptorTest+MyController' because the parameter 'Int32& i' is passed by reference." + Environment.NewLine
              + "Parameter name: methodInfo");
        }

        [Fact]
        public void ConstructorThrowsIfMethodInfoIsInstanceMethodOnNonControllerBaseType()
        {
            // Arrange
            MethodInfo methodInfo = typeof(string).GetMethod("Clone");

            // Act & assert
            Assert.Throws<ArgumentException>(
                delegate { new ReflectedActionDescriptor(methodInfo, "someName", new Mock<ControllerDescriptor>().Object); },
                "Cannot create a descriptor for instance method 'System.Object Clone()' on type 'System.String' because the type does not derive from ControllerBase." + Environment.NewLine
              + "Parameter name: methodInfo");
        }

        [Fact]
        public void ConstructorThrowsIfMethodIsStatic()
        {
            // Arrange
            MethodInfo methodInfo = typeof(MyController).GetMethod("StaticMethod");

            // Act & assert
            Assert.Throws<ArgumentException>(
                delegate { new ReflectedActionDescriptor(methodInfo, "someName", new Mock<ControllerDescriptor>().Object); },
                "Cannot call action method 'Void StaticMethod()' on controller 'System.Web.Mvc.Test.ReflectedActionDescriptorTest+MyController' because the action method is a static method." + Environment.NewLine
              + "Parameter name: methodInfo");
        }

        [Fact]
        public void ConstructorThrowsIfMethodInfoIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { new ReflectedActionDescriptor(null, "someName", new Mock<ControllerDescriptor>().Object); }, "methodInfo");
        }

        [Fact]
        public void ConstructorThrowsIfMethodInfoIsOpenGenericType()
        {
            // Arrange
            MethodInfo methodInfo = typeof(MyController).GetMethod("OpenGenericMethod");

            // Act & assert
            Assert.Throws<ArgumentException>(
                delegate { new ReflectedActionDescriptor(methodInfo, "someName", new Mock<ControllerDescriptor>().Object); },
                "Cannot call action method 'Void OpenGenericMethod[T]()' on controller 'System.Web.Mvc.Test.ReflectedActionDescriptorTest+MyController' because the action method is a generic method." + Environment.NewLine
              + "Parameter name: methodInfo");
        }

        [Fact]
        public void ExecuteCallsMethodInfoOnSuccess()
        {
            // Arrange
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(c => c.Controller).Returns(new ConcatController());
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "a", "hello " },
                { "b", "world" }
            };

            ReflectedActionDescriptor ad = GetActionDescriptor(typeof(ConcatController).GetMethod("Concat"));

            // Act
            object result = ad.Execute(mockControllerContext.Object, parameters);

            // Assert
            Assert.Equal("hello world", result);
        }

        [Fact]
        public void ExecuteThrowsIfControllerContextIsNull()
        {
            // Arrange
            ReflectedActionDescriptor ad = GetActionDescriptor();

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { ad.Execute(null, new Dictionary<string, object>()); }, "controllerContext");
        }

        [Fact]
        public void ExecuteThrowsIfParametersContainsNullForNonNullableParameter()
        {
            // Arrange
            ReflectedActionDescriptor ad = GetActionDescriptor(_int32EqualsIntMethod);
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "obj", null } };

            // Act & assert
            Assert.Throws<ArgumentException>(
                delegate { ad.Execute(new Mock<ControllerContext>().Object, parameters); },
                "The parameters dictionary contains a null entry for parameter 'obj' of non-nullable type 'System.Int32' for method 'Boolean Equals(Int32)' in 'System.Int32'. An optional parameter must be a reference type, a nullable type, or be declared as an optional parameter." + Environment.NewLine
              + "Parameter name: parameters");
        }

        [Fact]
        public void ExecuteThrowsIfParametersContainsValueOfWrongTypeForParameter()
        {
            // Arrange
            ReflectedActionDescriptor ad = GetActionDescriptor(_int32EqualsIntMethod);
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "obj", "notAnInteger" } };

            // Act & assert
            Assert.Throws<ArgumentException>(
                delegate { ad.Execute(new Mock<ControllerContext>().Object, parameters); },
                "The parameters dictionary contains an invalid entry for parameter 'obj' for method 'Boolean Equals(Int32)' in 'System.Int32'. The dictionary contains a value of type 'System.String', but the parameter requires a value of type 'System.Int32'." + Environment.NewLine
              + "Parameter name: parameters");
        }

        [Fact]
        public void ExecuteThrowsIfParametersIsMissingAValue()
        {
            // Arrange
            ReflectedActionDescriptor ad = GetActionDescriptor(_int32EqualsIntMethod);
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            // Act & assert
            Assert.Throws<ArgumentException>(
                delegate { ad.Execute(new Mock<ControllerContext>().Object, parameters); },
                "The parameters dictionary does not contain an entry for parameter 'obj' of type 'System.Int32' for method 'Boolean Equals(Int32)' in 'System.Int32'. The dictionary must contain an entry for each parameter, including parameters that have null values." + Environment.NewLine
              + "Parameter name: parameters");
        }

        [Fact]
        public void ExecuteThrowsIfParametersIsNull()
        {
            // Arrange
            ReflectedActionDescriptor ad = GetActionDescriptor();

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { ad.Execute(new Mock<ControllerContext>().Object, null); }, "parameters");
        }

        [Fact]
        public void GetCustomAttributesCallsMethodInfoGetCustomAttributes()
        {
            // Arrange
            object[] expected = new object[0];
            Mock<MethodInfo> mockMethod = new Mock<MethodInfo>();
            mockMethod.Setup(mi => mi.GetCustomAttributes(true)).Returns(expected);
            ReflectedActionDescriptor ad = GetActionDescriptor(mockMethod.Object);

            // Act
            object[] returned = ad.GetCustomAttributes(true);

            // Assert
            Assert.Same(expected, returned);
        }

        [Fact]
        public void GetCustomAttributesWithAttributeTypeCallsMethodInfoGetCustomAttributes()
        {
            // Arrange
            object[] expected = new object[0];
            Mock<MethodInfo> mockMethod = new Mock<MethodInfo>();
            mockMethod.Setup(mi => mi.GetCustomAttributes(typeof(ObsoleteAttribute), true)).Returns(expected);
            ReflectedActionDescriptor ad = GetActionDescriptor(mockMethod.Object);

            // Act
            object[] returned = ad.GetCustomAttributes(typeof(ObsoleteAttribute), true);

            // Assert
            Assert.Same(expected, returned);
        }

        [Fact]
        public void GetParametersWrapsParameterInfos()
        {
            // Arrange
            ParameterInfo pInfo0 = typeof(ConcatController).GetMethod("Concat").GetParameters()[0];
            ParameterInfo pInfo1 = typeof(ConcatController).GetMethod("Concat").GetParameters()[1];
            ReflectedActionDescriptor ad = GetActionDescriptor(typeof(ConcatController).GetMethod("Concat"));

            // Act
            ParameterDescriptor[] pDescsFirstCall = ad.GetParameters();
            ParameterDescriptor[] pDescsSecondCall = ad.GetParameters();

            // Assert
            Assert.NotSame(pDescsFirstCall, pDescsSecondCall);
            Assert.True(pDescsFirstCall.SequenceEqual(pDescsSecondCall));
            Assert.Equal(2, pDescsFirstCall.Length);

            ReflectedParameterDescriptor pDesc0 = pDescsFirstCall[0] as ReflectedParameterDescriptor;
            ReflectedParameterDescriptor pDesc1 = pDescsFirstCall[1] as ReflectedParameterDescriptor;

            Assert.NotNull(pDesc0);
            Assert.Same(ad, pDesc0.ActionDescriptor);
            Assert.Same(pInfo0, pDesc0.ParameterInfo);
            Assert.NotNull(pDesc1);
            Assert.Same(ad, pDesc1.ActionDescriptor);
            Assert.Same(pInfo1, pDesc1.ParameterInfo);
        }

        [Fact]
        public void GetSelectorsWrapsSelectorAttributes()
        {
            // Arrange
            ControllerContext controllerContext = new Mock<ControllerContext>().Object;
            Mock<MethodInfo> mockMethod = new Mock<MethodInfo>();

            Mock<ActionMethodSelectorAttribute> mockAttr = new Mock<ActionMethodSelectorAttribute>();
            mockAttr.Setup(attr => attr.IsValidForRequest(controllerContext, mockMethod.Object)).Returns(true).Verifiable();
            mockMethod.Setup(m => m.GetCustomAttributes(typeof(ActionMethodSelectorAttribute), true)).Returns(new ActionMethodSelectorAttribute[] { mockAttr.Object });

            ReflectedActionDescriptor ad = GetActionDescriptor(mockMethod.Object);

            // Act
            ICollection<ActionSelector> selectors = ad.GetSelectors();
            bool executedSuccessfully = selectors.All(s => s(controllerContext));

            // Assert
            Assert.Single(selectors);
            Assert.True(executedSuccessfully);
            mockAttr.Verify();
        }

        [Fact]
        public void IsDefinedCallsMethodInfoIsDefined()
        {
            // Arrange
            Mock<MethodInfo> mockMethod = new Mock<MethodInfo>();
            mockMethod.Setup(mi => mi.IsDefined(typeof(ObsoleteAttribute), true)).Returns(true);
            ReflectedActionDescriptor ad = GetActionDescriptor(mockMethod.Object);

            // Act
            bool isDefined = ad.IsDefined(typeof(ObsoleteAttribute), true);

            // Assert
            Assert.True(isDefined);
        }

        [Fact]
        public void TryCreateDescriptorReturnsDescriptorOnSuccess()
        {
            // Arrange
            MethodInfo methodInfo = typeof(MyController).GetMethod("GoodActionMethod");
            ControllerDescriptor cd = new Mock<ControllerDescriptor>().Object;

            // Act
            ReflectedActionDescriptor ad = ReflectedActionDescriptor.TryCreateDescriptor(methodInfo, "someName", cd);

            // Assert
            Assert.NotNull(ad);
            Assert.Same(methodInfo, ad.MethodInfo);
            Assert.Equal("someName", ad.ActionName);
            Assert.Same(cd, ad.ControllerDescriptor);
        }

        [Fact]
        public void TryCreateDescriptorReturnsNullOnFailure()
        {
            // Arrange
            MethodInfo methodInfo = typeof(MyController).GetMethod("OpenGenericMethod");
            ControllerDescriptor cd = new Mock<ControllerDescriptor>().Object;

            // Act
            ReflectedActionDescriptor ad = ReflectedActionDescriptor.TryCreateDescriptor(methodInfo, "someName", cd);

            // Assert
            Assert.Null(ad);
        }

        private static ReflectedActionDescriptor GetActionDescriptor()
        {
            return GetActionDescriptor(new Mock<MethodInfo>().Object);
        }

        private static ReflectedActionDescriptor GetActionDescriptor(MethodInfo methodInfo)
        {
            return new ReflectedActionDescriptor(methodInfo, "someName", new Mock<ControllerDescriptor>().Object, false /* validateMethod */)
            {
                DispatcherCache = new ActionMethodDispatcherCache()
            };
        }

        private class ConcatController : Controller
        {
            public string Concat(string a, string b)
            {
                return a + b;
            }
        }

        [OutputCache(VaryByParam = "Class")]
        private class OverriddenAttributeController : Controller
        {
            [OutputCache(VaryByParam = "Method")]
            public void SomeMethod()
            {
            }
        }

        [KeyedActionFilter(Key = "BaseClass", Order = 0)]
        [KeyedAuthorizationFilter(Key = "BaseClass", Order = 0)]
        [KeyedExceptionFilter(Key = "BaseClass", Order = 0)]
        private class GetMemberChainController : Controller
        {
            [KeyedActionFilter(Key = "BaseMethod", Order = 0)]
            [KeyedAuthorizationFilter(Key = "BaseMethod", Order = 0)]
            public virtual void SomeVirtual()
            {
            }
        }

        [KeyedActionFilter(Key = "DerivedClass", Order = 1)]
        private class GetMemberChainDerivedController : GetMemberChainController
        {
        }

        [KeyedActionFilter(Key = "SubderivedClass", Order = 2)]
        private class GetMemberChainSubderivedController : GetMemberChainDerivedController
        {
            [KeyedActionFilter(Key = "SubderivedMethod", Order = 2)]
            public override void SomeVirtual()
            {
            }
        }

        private abstract class KeyedFilterAttribute : FilterAttribute
        {
            public string Key { get; set; }
        }

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
        private class KeyedAuthorizationFilterAttribute : KeyedFilterAttribute, IAuthorizationFilter
        {
            public void OnAuthorization(AuthorizationContext filterContext)
            {
                throw new NotImplementedException();
            }
        }

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
        private class KeyedExceptionFilterAttribute : KeyedFilterAttribute, IExceptionFilter
        {
            public void OnException(ExceptionContext filterContext)
            {
                throw new NotImplementedException();
            }
        }

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
        private class KeyedActionFilterAttribute : KeyedFilterAttribute, IActionFilter, IResultFilter
        {
            public void OnActionExecuting(ActionExecutingContext filterContext)
            {
                throw new NotImplementedException();
            }

            public void OnActionExecuted(ActionExecutedContext filterContext)
            {
                throw new NotImplementedException();
            }

            public void OnResultExecuting(ResultExecutingContext filterContext)
            {
                throw new NotImplementedException();
            }

            public void OnResultExecuted(ResultExecutedContext filterContext)
            {
                throw new NotImplementedException();
            }
        }

        private class MyController : Controller
        {
            public void GoodActionMethod()
            {
            }

            public static void StaticMethod()
            {
            }

            public void OpenGenericMethod<T>()
            {
            }

            public void MethodHasOutParameter(out int i)
            {
                i = 0;
            }

            public void MethodHasRefParameter(ref int i)
            {
            }
        }
    }
}
