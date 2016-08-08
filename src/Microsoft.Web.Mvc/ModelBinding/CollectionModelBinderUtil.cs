// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Web.Mvc;

namespace Microsoft.Web.Mvc.ModelBinding
{
    internal static class CollectionModelBinderUtil
    {
        public static void CreateOrReplaceCollection<TElement>(ExtensibleModelBindingContext bindingContext, IEnumerable<TElement> incomingElements, Func<ICollection<TElement>> creator)
        {
            ICollection<TElement> collection = bindingContext.Model as ICollection<TElement>;
            if (collection == null || collection.IsReadOnly)
            {
                collection = creator();
                bindingContext.Model = collection;
            }

            collection.Clear();
            foreach (TElement element in incomingElements)
            {
                collection.Add(element);
            }
        }

        public static void CreateOrReplaceDictionary<TKey, TValue>(ExtensibleModelBindingContext bindingContext, IEnumerable<KeyValuePair<TKey, TValue>> incomingElements, Func<IDictionary<TKey, TValue>> creator)
        {
            IDictionary<TKey, TValue> dictionary = bindingContext.Model as IDictionary<TKey, TValue>;
            if (dictionary == null || dictionary.IsReadOnly)
            {
                dictionary = creator();
                bindingContext.Model = dictionary;
            }

            dictionary.Clear();
            foreach (var element in incomingElements)
            {
                if (element.Key != null)
                {
                    dictionary[element.Key] = element.Value;
                }
            }
        }

        // supportedInterfaceType: type that is updatable by this binder
        // newInstanceType: type that will be created by the binder if necessary
        // openBinderType: model binder type
        // modelMetadata: metadata for the model to bind
        //
        // example: GetGenericBinder(typeof(IList<>), typeof(List<>), typeof(ListBinder<>), ...) means that the ListBinder<T>
        // type can update models that implement IList<T>, and if for some reason the existing model instance is not
        // updatable the binder will create a List<T> object and bind to that instead. This method will return a ListBinder<T>
        // or null, depending on whether the type and updatability checks succeed.
        public static IExtensibleModelBinder GetGenericBinder(Type supportedInterfaceType, Type newInstanceType, Type openBinderType, ModelMetadata modelMetadata)
        {
            Type[] typeArguments = GetTypeArgumentsForUpdatableGenericCollection(supportedInterfaceType, newInstanceType, modelMetadata);
            return (typeArguments != null) ? (IExtensibleModelBinder)Activator.CreateInstance(openBinderType.MakeGenericType(typeArguments)) : null;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.Web.Mvc.ValueProviderResult.ConvertTo(System.Type)", Justification = "This model binder binds collections, so it does not need culture.")]
        public static IEnumerable<string> GetIndexNamesFromValueProviderResult(ValueProviderResult valueProviderResultIndex)
        {
            IEnumerable<string> indexNames = null;
            if (valueProviderResultIndex != null)
            {
                string[] indexes = (string[])(valueProviderResultIndex.ConvertTo(typeof(string[])));
                if (indexes != null && indexes.Length > 0)
                {
                    indexNames = indexes;
                }
            }
            return indexNames;
        }

        public static IEnumerable<string> GetZeroBasedIndexes()
        {
            int i = 0;
            while (true)
            {
                yield return i.ToString(CultureInfo.InvariantCulture);
                i++;
            }
        }

        // Returns the generic type arguments for the model type if updatable, else null.
        // supportedInterfaceType: open type (like IList<>) of supported interface, must implement ICollection<>
        // newInstanceType: open type (like List<>) of object that will be created, must implement supportedInterfaceType
        public static Type[] GetTypeArgumentsForUpdatableGenericCollection(Type supportedInterfaceType, Type newInstanceType, ModelMetadata modelMetadata)
        {
            /*
             * Check that we can extract proper type arguments from the model.
             */

            if (!modelMetadata.ModelType.IsGenericType || modelMetadata.ModelType.IsGenericTypeDefinition)
            {
                // not a closed generic type
                return null;
            }

            Type[] modelTypeArguments = modelMetadata.ModelType.GetGenericArguments();
            if (modelTypeArguments.Length != supportedInterfaceType.GetGenericArguments().Length)
            {
                // wrong number of generic type arguments
                return null;
            }

            /*
             * Is it possible just to change the reference rather than update the collection in-place?
             */

            if (!modelMetadata.IsReadOnly)
            {
                Type closedNewInstanceType = newInstanceType.MakeGenericType(modelTypeArguments);
                if (modelMetadata.ModelType.IsAssignableFrom(closedNewInstanceType))
                {
                    return modelTypeArguments;
                }
            }

            /*
             * At this point, we know we can't change the reference, so we need to verify that
             * the model instance can be updated in-place.
             */

            Type closedSupportedInterfaceType = supportedInterfaceType.MakeGenericType(modelTypeArguments);
            if (!closedSupportedInterfaceType.IsInstanceOfType(modelMetadata.Model))
            {
                return null; // not instance of correct interface
            }

            Type closedCollectionType = TypeHelpers.ExtractGenericInterface(closedSupportedInterfaceType, typeof(ICollection<>));
            bool collectionInstanceIsReadOnly = (bool)closedCollectionType.GetProperty("IsReadOnly").GetValue(modelMetadata.Model, null);
            if (collectionInstanceIsReadOnly)
            {
                return null;
            }
            else
            {
                return modelTypeArguments;
            }
        }
    }
}
