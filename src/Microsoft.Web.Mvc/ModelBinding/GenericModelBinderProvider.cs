// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Web.Mvc;

namespace Microsoft.Web.Mvc.ModelBinding
{
    // Returns a user-specified binder for a given open generic type.
    public sealed class GenericModelBinderProvider : ModelBinderProvider
    {
        private readonly Func<Type[], IExtensibleModelBinder> _modelBinderFactory;
        private readonly Type _modelType;

        public GenericModelBinderProvider(Type modelType, IExtensibleModelBinder modelBinder)
        {
            if (modelType == null)
            {
                throw new ArgumentNullException("modelType");
            }
            if (modelBinder == null)
            {
                throw new ArgumentNullException("modelBinder");
            }

            ValidateParameters(modelType, null /* modelBinderType */);

            _modelType = modelType;
            _modelBinderFactory = _ => modelBinder;
        }

        public GenericModelBinderProvider(Type modelType, Type modelBinderType)
        {
            // The binder can be a closed type, in which case it will be instantiated directly. If the binder
            // is an open type, the type arguments will be determined at runtime and the corresponding closed
            // type instantiated.

            if (modelType == null)
            {
                throw new ArgumentNullException("modelType");
            }
            if (modelBinderType == null)
            {
                throw new ArgumentNullException("modelBinderType");
            }

            ValidateParameters(modelType, modelBinderType);
            bool modelBinderTypeIsOpenGeneric = modelBinderType.IsGenericTypeDefinition;

            _modelType = modelType;
            _modelBinderFactory = typeArguments =>
            {
                Type closedModelBinderType = (modelBinderTypeIsOpenGeneric) ? modelBinderType.MakeGenericType(typeArguments) : modelBinderType;
                try
                {
                    return (IExtensibleModelBinder)Activator.CreateInstance(closedModelBinderType);
                }
                catch (MissingMethodException exception)
                {
                    // Ensure thrown exception contains the type name.  Might be down a few levels.
                    MissingMethodException replacementException =
                        ModelBinderUtil.EnsureDebuggableException(exception, closedModelBinderType.FullName);
                    if (replacementException != null)
                    {
                        throw replacementException;
                    }

                    throw;
                }
            };
        }

        public GenericModelBinderProvider(Type modelType, Func<Type[], IExtensibleModelBinder> modelBinderFactory)
        {
            if (modelType == null)
            {
                throw new ArgumentNullException("modelType");
            }
            if (modelBinderFactory == null)
            {
                throw new ArgumentNullException("modelBinderFactory");
            }

            ValidateParameters(modelType, null /* modelBinderType */);

            _modelType = modelType;
            _modelBinderFactory = modelBinderFactory;
        }

        public Type ModelType
        {
            get { return _modelType; }
        }

        public bool SuppressPrefixCheck { get; set; }

        public override IExtensibleModelBinder GetBinder(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext)
        {
            ModelBinderUtil.ValidateBindingContext(bindingContext);

            Type[] typeArguments = null;
            if (ModelType.IsInterface)
            {
                Type matchingClosedInterface = TypeHelpers.ExtractGenericInterface(bindingContext.ModelType, ModelType);
                if (matchingClosedInterface != null)
                {
                    typeArguments = matchingClosedInterface.GetGenericArguments();
                }
            }
            else
            {
                typeArguments = TypeHelpers.GetTypeArgumentsIfMatch(bindingContext.ModelType, ModelType);
            }

            if (typeArguments != null)
            {
                if (SuppressPrefixCheck || bindingContext.ValueProvider.ContainsPrefix(bindingContext.ModelName))
                {
                    return _modelBinderFactory(typeArguments);
                }
            }

            return null;
        }

        private static void ValidateParameters(Type modelType, Type modelBinderType)
        {
            if (!modelType.IsGenericTypeDefinition)
            {
                throw Error.GenericModelBinderProvider_ParameterMustSpecifyOpenGenericType(modelType, "modelType");
            }
            if (modelBinderType != null)
            {
                if (!typeof(IExtensibleModelBinder).IsAssignableFrom(modelBinderType))
                {
                    throw Error.Common_TypeMustImplementInterface(modelBinderType, typeof(IExtensibleModelBinder), "modelBinderType");
                }
                if (modelBinderType.IsGenericTypeDefinition)
                {
                    if (modelType.GetGenericArguments().Length != modelBinderType.GetGenericArguments().Length)
                    {
                        throw Error.GenericModelBinderProvider_TypeArgumentCountMismatch(modelType, modelBinderType);
                    }
                }
            }
        }
    }
}
