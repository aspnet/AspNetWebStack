// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Dispatcher
{
    public class DefaultHttpControllerTypeResolverTest
    {
        public static TheoryDataSet<Type> ValidControllerTypes
        {
            get
            {
                return new TheoryDataSet<Type>
                {
                    typeof(validcontroller),
                    typeof(ValidController),
                    typeof(VALIDController),
                    typeof(VALIDCONTROLLER),
                    typeof(ValidSealedController),
                    typeof(ValidPartialController),
                    typeof(ValidInheritedController),
                    typeof(ControllerWrapper.ValidNestedController),
                    typeof(ControllerWrapper.ValidInheritedNestedController),
                    typeof(ControllerWrapper.ControllerNestedWrapper.ValidNestedNestedController),
                    typeof(InheritedControllerHidingWrapper.ValidNestedController)
                };
            }
        }

        public static TheoryDataSet<Type> InvalidControllerTypesWithValidNames
        {
            get
            {
                return new TheoryDataSet<Type>
                {
                    typeof(InvalidAbstractController),
                    typeof(ControllerWrapper.InvalidAbstractNestedController),
                    ControllerWrapper.TypeOfInvalidProtectedNestedController(), // NOTE: Unable to get through typeof(ControllerWrapper.InvalidProtectedNestedController),
                    ControllerWrapper.TypeOfInvalidPrivateNestedController(), // NOTE: Unable to get through typeof(ControllerWrapper.InvalidPrivateNestedController),
                    typeof(ControllerWrapper.InvalidInternalNestedController)
                };
            }
        }

        public static TheoryDataSet<Type> InvalidControllerTypesWithInvalidNames
        {
            get
            {
                return new TheoryDataSet<Type>
                {
                    typeof(Ctrl),
                    typeof(Controller),
                    typeof(ControllerPrefix),
                    typeof(InvalidControllerStruct),
                    typeof(InvalidControllerWithInconsistentName),
                    typeof(InvalidControllerWithNoBaseType)
                };
            }
        }

        [Fact]
        public void DefaultHttpControllerTypeResolver_GuardClauses()
        {
            Assert.ThrowsArgumentNull(() => new DefaultHttpControllerTypeResolver(null), "predicate");
        }

        [Theory]
        [PropertyData("ValidControllerTypes")]
        public void IsControllerType_AcceptsValidControllerTypes(Type validControllerType)
        {
            Assert.True(DefaultHttpControllerTypeResolver.IsControllerType(validControllerType));
        }

        [Theory]
        [PropertyData("InvalidControllerTypesWithValidNames"), PropertyData("InvalidControllerTypesWithInvalidNames")]
        public void IsControllerType_RejectsInvalidControllerTypes(Type invalidControllerType)
        {
            Assert.False(DefaultHttpControllerTypeResolver.IsControllerType(invalidControllerType));
        }

        [Theory]
        [PropertyData("ValidControllerTypes")]
        public void HasValidControllerName_AcceptsValidControllerNames(Type validControllerType)
        {
            Assert.True(DefaultHttpControllerTypeResolver.HasValidControllerName(validControllerType));
        }

        [Theory]
        [PropertyData("InvalidControllerTypesWithInvalidNames")]
        public void HasValidControllerName_RejectsInvalidControllerNames(Type invalidControllerType)
        {
            Assert.False(DefaultHttpControllerTypeResolver.HasValidControllerName(invalidControllerType));
        }

        [Fact]
        public void GetControllerTypes_ThrowsOnNull()
        {
            DefaultHttpControllerTypeResolver resolver = new DefaultHttpControllerTypeResolver();
            Assert.ThrowsArgumentNull(() => resolver.GetControllerTypes(null), "assembliesResolver");
        }

        [Fact]
        public void GetControllerTypes_FindsTypes()
        {
            // Arrange
            DefaultHttpControllerTypeResolver resolver = new DefaultHttpControllerTypeResolver();
            Mock<IAssembliesResolver> mockAssemblyResolver = new Mock<IAssembliesResolver>();

            mockAssemblyResolver.Setup(a => a.GetAssemblies()).Returns(new List<Assembly> 
            {
                null,
                new MockExportedTypesAssembly(ThrowException.ReflectionTypeLoadException,
                    typeof(ControllerWrapper.ValidNestedController),
                    typeof(ControllerWrapper.ValidInheritedNestedController),
                    typeof(ControllerWrapper.ControllerNestedWrapper.ValidNestedNestedController),
                    typeof(InheritedControllerHidingWrapper.ValidNestedController)),
                new MockExportedTypesAssembly(ThrowException.Exception,
                    typeof(ControllerWrapper.InvalidAbstractNestedController),
                    ControllerWrapper.TypeOfInvalidProtectedNestedController(), // NOTE: Unable to get through typeof(ControllerWrapper.InvalidProtectedNestedController),
                    ControllerWrapper.TypeOfInvalidPrivateNestedController(), // NOTE: Unable to get through typeof(ControllerWrapper.InvalidPrivateNestedController),
                    typeof(ControllerWrapper.InvalidInternalNestedController)),
                new MockExportedTypesAssembly(ThrowException.None,
                    typeof(ValidSealedController),
                    typeof(ValidPartialController),
                    typeof(ValidInheritedController),
                    typeof(ValidController),
                    typeof(VALIDController),
                    typeof(VALIDCONTROLLER),
                    typeof(InvalidAbstractController),
                    typeof(InvalidControllerStruct),
                    typeof(InvalidControllerWithInconsistentName),
                    typeof(InvalidControllerWithNoBaseType))
            });

            // Act
            ICollection<Type> actualControllerTypes = resolver.GetControllerTypes(mockAssemblyResolver.Object);

            // Assert
            Assert.Equal(10, actualControllerTypes.Count);
            Assert.True(actualControllerTypes.Contains(typeof(ControllerWrapper.ValidNestedController)));
            Assert.True(actualControllerTypes.Contains(typeof(ControllerWrapper.ValidInheritedNestedController)));
            Assert.True(actualControllerTypes.Contains(typeof(ControllerWrapper.ControllerNestedWrapper.ValidNestedNestedController)));
            Assert.True(actualControllerTypes.Contains(typeof(ValidSealedController)));
            Assert.True(actualControllerTypes.Contains(typeof(ValidPartialController)));
            Assert.True(actualControllerTypes.Contains(typeof(ValidInheritedController)));
            Assert.True(actualControllerTypes.Contains(typeof(ValidController)));
            Assert.True(actualControllerTypes.Contains(typeof(VALIDController)));
            Assert.True(actualControllerTypes.Contains(typeof(VALIDCONTROLLER)));
            Assert.True(actualControllerTypes.Contains(typeof(InheritedControllerHidingWrapper.ValidNestedController)));
        }

        private static string GetDefaultControllerRouteName(Type controllerType)
        {
            return controllerType.Name.Substring(0, controllerType.Name.Length - "Controller".Length);
        }
    }

    public enum ThrowException
    {
        None = 0,
        Exception,
        ReflectionTypeLoadException,
    }

    public class MockExportedTypesAssembly : Assembly
    {
        private ThrowException _throwException;
        private Type[] _exportedTypes;

        public MockExportedTypesAssembly(ThrowException throwException, params Type[] exportedTypes)
        {
            _throwException = throwException;
            _exportedTypes = exportedTypes;
        }

        public override bool IsDynamic
        {
            get { return false; }
        }

        public override Type[] GetTypes()
        {
            switch (_throwException)
            {
                case ThrowException.Exception:
                    throw new Exception("GetTypes exception");

                case ThrowException.ReflectionTypeLoadException:
                    throw new ReflectionTypeLoadException(_exportedTypes, null, "GetTypes exception");
            }

            return _exportedTypes;
        }
    }

    public class ControllerWrapper
    {
        public static Type TypeOfInvalidPrivateNestedController()
        {
            return typeof(InvalidPrivateNestedController);
        }

        public static Type TypeOfInvalidProtectedNestedController()
        {
            return typeof(InvalidProtectedNestedController);
        }

        public class ValidNestedController : ApiController
        {
        }

        public abstract class InvalidAbstractNestedController : ApiController
        {
        }

        public class ValidInheritedNestedController : InvalidAbstractNestedController
        {
        }

        protected class InvalidProtectedNestedController : ApiController
        {
        }

        private class InvalidPrivateNestedController : ApiController
        {
        }

        internal class InvalidInternalNestedController : ApiController
        {
        }

        public class ControllerNestedWrapper
        {
            public class ValidNestedNestedController : ApiController
            {
            }
        }
    }

    public abstract class InvalidAbstractController : ApiController
    {
    }

    public struct InvalidControllerStruct
    {
    }

    public class ControllerPrefix
    {
    }

    public class InvalidControllerWithInconsistentName : ApiController
    {
    }

    public class InvalidControllerWithNoBaseType
    {
    }

    public sealed class ValidSealedController : ApiController
    {
    }

    public partial class ValidPartialController : ApiController
    {
    }

    public partial class ValidPartialController
    {
    }

    public class ValidInheritedController : InvalidAbstractController
    {
    }

    public class Controller : ApiController
    {
    }

    public class Ctrl : ApiController
    {
    }

    public class validcontroller : ApiController
    {
    }

    public class ValidController : ApiController
    {
    }

    public class VALIDController : ApiController
    {
    }

    public class VALIDCONTROLLER : ApiController
    {
    }

    public class InheritedControllerHidingWrapper : ControllerWrapper
    {
        public new class ValidNestedController : ControllerWrapper.ValidNestedController
        {
        }
    }
}
