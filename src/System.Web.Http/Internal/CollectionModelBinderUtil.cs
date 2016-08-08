// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Web.Http.Metadata;
using System.Web.Http.ModelBinding;
using System.Web.Http.ValueProviders;

namespace System.Web.Http.Internal
{
    internal static class CollectionModelBinderUtil
    {
        internal static void CreateOrReplaceCollection<TElement>(ModelBindingContext bindingContext, IEnumerable<TElement> incomingElements, Func<ICollection<TElement>> creator)
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

        internal static void CreateOrReplaceDictionary<TKey, TValue>(ModelBindingContext bindingContext, IEnumerable<KeyValuePair<TKey, TValue>> incomingElements, Func<IDictionary<TKey, TValue>> creator)
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

        /// <summary>
        /// Instantiate a generic binder.
        /// </summary>
        /// <param name="supportedInterfaceType">Type that is updatable by this binder.</param>
        /// <param name="newInstanceType">Type that will be created by the binder if necessary.</param>
        /// <param name="openBinderType">Model binder type.</param>
        /// <param name="modelType">Model type.</param>
        /// <returns></returns>
        // Example: GetGenericBinder(typeof(IList<>), typeof(List<>), typeof(ListBinder<>), ...) means that the ListBinder<T>
        // type can update models that implement IList<T>, and if for some reason the existing model instance is not
        // updatable the binder will create a List<T> object and bind to that instead. This method will return a ListBinder<T>
        // or null, depending on whether the type and updatability checks succeed.
        internal static IModelBinder GetGenericBinder(Type supportedInterfaceType, Type newInstanceType, Type openBinderType, Type modelType)
        {
            Contract.Assert(supportedInterfaceType != null);
            Contract.Assert(openBinderType != null);
            Contract.Assert(modelType != null);

            Type[] modelTypeArguments = GetGenericBinderTypeArgs(supportedInterfaceType, modelType);

            if (modelTypeArguments == null)
            {
                return null;
            }

            Type closedNewInstanceType = newInstanceType.MakeGenericType(modelTypeArguments);
            if (!modelType.IsAssignableFrom(closedNewInstanceType))
            {
                return null;
            }

            Type closedBinderType = openBinderType.MakeGenericType(modelTypeArguments);
            var binder = (IModelBinder)Activator.CreateInstance(closedBinderType);
            return binder;
        }

        // Get the generic arguments for the binder, based on the model type. Or null if not compatible.
        internal static Type[] GetGenericBinderTypeArgs(Type supportedInterfaceType, Type modelType)
        {
            if (!modelType.IsGenericType || modelType.IsGenericTypeDefinition)
            {
                // not a closed generic type
                return null;
            }

            Type[] modelTypeArguments = modelType.GetGenericArguments();
            if (modelTypeArguments.Length != supportedInterfaceType.GetGenericArguments().Length)
            {
                // wrong number of generic type arguments
                return null;
            }

            return modelTypeArguments;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.Web.Http.ValueProviders.ValueProviderResult.ConvertTo(System.Type)", Justification = "The ValueProviderResult already has the necessary context to perform a culture-aware conversion.")]
        internal static IEnumerable<string> GetIndexNamesFromValueProviderResult(ValueProviderResult valueProviderResultIndex)
        {
            IEnumerable<string> indexNames = null;
            if (valueProviderResultIndex != null)
            {
                string[] indexes = (string[])valueProviderResultIndex.ConvertTo(typeof(string[]));
                if (indexes != null && indexes.Length > 0)
                {
                    indexNames = indexes;
                }
            }
            return indexNames;
        }

        internal static IEnumerable<string> GetZeroBasedIndexes()
        {
            int i = 0;
            while (true)
            {
                yield return i.ToString(CultureInfo.InvariantCulture);
                i++;
            }
        }
    }
}
