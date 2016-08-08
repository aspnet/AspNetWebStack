// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Microsoft.Web.Mvc.Properties;

namespace Microsoft.Web.Mvc.ModelBinding
{
    public sealed class ModelBinderProviderCollection : Collection<ModelBinderProvider>
    {
        public ModelBinderProviderCollection()
        {
        }

        public ModelBinderProviderCollection(IList<ModelBinderProvider> list)
            : base(list)
        {
        }

        private static void EnsureNoBindAttribute(Type modelType)
        {
            if (TypeDescriptorHelper.Get(modelType).GetAttributes().OfType<BindAttribute>().Any())
            {
                string errorMessage = String.Format(CultureInfo.CurrentCulture, MvcResources.ModelBinderProviderCollection_TypeCannotHaveBindAttribute,
                                                    modelType);
                throw new InvalidOperationException(errorMessage);
            }
        }

        public IExtensibleModelBinder GetBinder(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }
            if (bindingContext == null)
            {
                throw new ArgumentNullException("bindingContext");
            }

            EnsureNoBindAttribute(bindingContext.ModelType);

            ModelBinderProvider providerFromAttr;
            if (TryGetProviderFromAttributes(bindingContext.ModelType, out providerFromAttr))
            {
                return providerFromAttr.GetBinder(controllerContext, bindingContext);
            }

            return (from provider in this
                    let binder = provider.GetBinder(controllerContext, bindingContext)
                    where binder != null
                    select binder).FirstOrDefault();
        }

        internal IExtensibleModelBinder GetRequiredBinder(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext)
        {
            IExtensibleModelBinder binder = GetBinder(controllerContext, bindingContext);
            if (binder == null)
            {
                throw Error.ModelBinderProviderCollection_BinderForTypeNotFound(bindingContext.ModelType);
            }
            return binder;
        }

        protected override void InsertItem(int index, ModelBinderProvider item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            base.InsertItem(index, item);
        }

        private void InsertSimpleProviderAtFront(ModelBinderProvider provider)
        {
            // Don't want to insert simple providers before any that are marked as "should go first,"
            // as that might throw off other providers like the exact type match provider.

            int i = 0;
            for (; i < Count; i++)
            {
                if (!ShouldProviderGoFirst(this[i]))
                {
                    break;
                }
            }

            base.InsertItem(i, provider);
        }

        public void RegisterBinderForGenericType(Type modelType, IExtensibleModelBinder modelBinder)
        {
            InsertSimpleProviderAtFront(new GenericModelBinderProvider(modelType, modelBinder));
        }

        public void RegisterBinderForGenericType(Type modelType, Func<Type[], IExtensibleModelBinder> modelBinderFactory)
        {
            InsertSimpleProviderAtFront(new GenericModelBinderProvider(modelType, modelBinderFactory));
        }

        public void RegisterBinderForGenericType(Type modelType, Type modelBinderType)
        {
            InsertSimpleProviderAtFront(new GenericModelBinderProvider(modelType, modelBinderType));
        }

        public void RegisterBinderForType(Type modelType, IExtensibleModelBinder modelBinder)
        {
            RegisterBinderForType(modelType, modelBinder, false /* suppressPrefixCheck */);
        }

        internal void RegisterBinderForType(Type modelType, IExtensibleModelBinder modelBinder, bool suppressPrefixCheck)
        {
            SimpleModelBinderProvider provider = new SimpleModelBinderProvider(modelType, modelBinder)
            {
                SuppressPrefixCheck = suppressPrefixCheck
            };
            InsertSimpleProviderAtFront(provider);
        }

        public void RegisterBinderForType(Type modelType, Func<IExtensibleModelBinder> modelBinderFactory)
        {
            InsertSimpleProviderAtFront(new SimpleModelBinderProvider(modelType, modelBinderFactory));
        }

        protected override void SetItem(int index, ModelBinderProvider item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            base.SetItem(index, item);
        }

        private static bool ShouldProviderGoFirst(ModelBinderProvider provider)
        {
            ModelBinderProviderOptionsAttribute options = provider.GetType()
                .GetCustomAttributes(typeof(ModelBinderProviderOptionsAttribute), true /* inherit */)
                .OfType<ModelBinderProviderOptionsAttribute>()
                .FirstOrDefault();

            return (options != null) ? options.FrontOfList : false;
        }

        private static bool TryGetProviderFromAttributes(Type modelType, out ModelBinderProvider provider)
        {
            ExtensibleModelBinderAttribute attr = TypeDescriptorHelper.Get(modelType).GetAttributes().OfType<ExtensibleModelBinderAttribute>().FirstOrDefault();
            if (attr == null)
            {
                provider = null;
                return false;
            }

            if (typeof(ModelBinderProvider).IsAssignableFrom(attr.BinderType))
            {
                provider = (ModelBinderProvider)CreateInstance(attr.BinderType);
            }
            else if (typeof(IExtensibleModelBinder).IsAssignableFrom(attr.BinderType))
            {
                Type closedBinderType = (attr.BinderType.IsGenericTypeDefinition) ? attr.BinderType.MakeGenericType(modelType.GetGenericArguments()) : attr.BinderType;
                IExtensibleModelBinder binderInstance = (IExtensibleModelBinder)CreateInstance(closedBinderType);
                provider = new SimpleModelBinderProvider(modelType, binderInstance) { SuppressPrefixCheck = attr.SuppressPrefixCheck };
            }
            else
            {
                string errorMessage = String.Format(CultureInfo.CurrentCulture, MvcResources.ModelBinderProviderCollection_InvalidBinderType,
                                                    attr.BinderType, typeof(ModelBinderProvider), typeof(IExtensibleModelBinder));
                throw new InvalidOperationException(errorMessage);
            }

            return true;
        }

        private static object CreateInstance(Type type)
        {
            try
            {
                return Activator.CreateInstance(type);
            }
            catch (MissingMethodException exception)
            {
                // Ensure thrown exception contains the type name.  Might be down a few levels.
                MissingMethodException replacementException =
                    ModelBinderUtil.EnsureDebuggableException(exception, type.FullName);
                if (replacementException != null)
                {
                    throw replacementException;
                }

                throw;
            }
        }
    }
}
