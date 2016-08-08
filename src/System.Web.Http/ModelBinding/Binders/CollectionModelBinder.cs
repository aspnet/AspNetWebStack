// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Web.Http.Controllers;
using System.Web.Http.Internal;
using System.Web.Http.ValueProviders;
using System.Web.Http.ValueProviders.Providers;

namespace System.Web.Http.ModelBinding.Binders
{
    public class CollectionModelBinder<TElement> : IModelBinder
    {
        // Used when the ValueProvider contains the collection to be bound as multiple elements, e.g. foo[0], foo[1].
        private static List<TElement> BindComplexCollection(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            string indexPropertyName = ModelBindingHelper.CreatePropertyModelName(bindingContext.ModelName, "index");
            ValueProviderResult valueProviderResultIndex = bindingContext.ValueProvider.GetValue(indexPropertyName);
            IEnumerable<string> indexNames = CollectionModelBinderUtil.GetIndexNamesFromValueProviderResult(valueProviderResultIndex);
            return BindComplexCollectionFromIndexes(actionContext, bindingContext, indexNames);
        }

        internal static List<TElement> BindComplexCollectionFromIndexes(HttpActionContext actionContext, ModelBindingContext bindingContext, IEnumerable<string> indexNames)
        {
            bool indexNamesIsFinite;
            if (indexNames != null)
            {
                indexNamesIsFinite = true;
            }
            else
            {
                indexNamesIsFinite = false;
                indexNames = CollectionModelBinderUtil.GetZeroBasedIndexes();
            }

            List<TElement> boundCollection = new List<TElement>();
            foreach (string indexName in indexNames)
            {
                string fullChildName = ModelBindingHelper.CreateIndexModelName(bindingContext.ModelName, indexName);
                ModelBindingContext childBindingContext = new ModelBindingContext(bindingContext)
                {
                    ModelMetadata = actionContext.GetMetadataProvider().GetMetadataForType(null, typeof(TElement)),
                    ModelName = fullChildName
                };

                bool didBind = false;
                object boundValue = null;                
                if (actionContext.Bind(childBindingContext))                
                {
                    didBind = true;
                    boundValue = childBindingContext.Model;

                    // merge validation up
                    bindingContext.ValidationNode.ChildNodes.Add(childBindingContext.ValidationNode);
                }                

                // infinite size collection stops on first bind failure
                if (!didBind && !indexNamesIsFinite)
                {
                    break;
                }

                boundCollection.Add(ModelBindingHelper.CastOrDefault<TElement>(boundValue));
            }

            return boundCollection;
        }

        public virtual bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            ModelBindingHelper.ValidateBindingContext(bindingContext);

            if (!bindingContext.ValueProvider.ContainsPrefix(bindingContext.ModelName))
            {
                return false;
            }

            ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            List<TElement> boundCollection = (valueProviderResult != null)
                                                 ? BindSimpleCollection(actionContext, bindingContext, valueProviderResult.RawValue, valueProviderResult.Culture)
                                                 : BindComplexCollection(actionContext, bindingContext);

            bool retVal = CreateOrReplaceCollection(actionContext, bindingContext, boundCollection);
            return retVal;
        }

        // Used when the ValueProvider contains the collection to be bound as a single element, e.g. the raw value
        // is [ "1", "2" ] and needs to be converted to an int[].
        internal static List<TElement> BindSimpleCollection(HttpActionContext actionContext, ModelBindingContext bindingContext, object rawValue, CultureInfo culture)
        {
            if (rawValue == null)
            {
                return null; // nothing to do
            }

            List<TElement> boundCollection = new List<TElement>();

            object[] rawValueArray = ModelBindingHelper.RawValueToObjectArray(rawValue);
            foreach (object rawValueElement in rawValueArray)
            {
                ModelBindingContext innerBindingContext = new ModelBindingContext(bindingContext)
                {
                    ModelMetadata = actionContext.GetMetadataProvider().GetMetadataForType(null, typeof(TElement)),
                    ModelName = bindingContext.ModelName,
                    ValueProvider = new CompositeValueProvider
                    {
                        new ElementalValueProvider(bindingContext.ModelName, rawValueElement, culture), // our temporary provider goes at the front of the list
                        bindingContext.ValueProvider
                    }
                };

                object boundValue = null;
                if (actionContext.Bind(innerBindingContext))                
                {
                    boundValue = innerBindingContext.Model;
                    bindingContext.ValidationNode.ChildNodes.Add(innerBindingContext.ValidationNode);
                }                
                boundCollection.Add(ModelBindingHelper.CastOrDefault<TElement>(boundValue));
            }

            return boundCollection;
        }

        // Extensibility point that allows the bound collection to be manipulated or transformed before
        // being returned from the binder.
        protected virtual bool CreateOrReplaceCollection(HttpActionContext actionContext, ModelBindingContext bindingContext, IList<TElement> newCollection)
        {
            CollectionModelBinderUtil.CreateOrReplaceCollection(bindingContext, newCollection, () => new List<TElement>());
            return true;
        }
    }
}
