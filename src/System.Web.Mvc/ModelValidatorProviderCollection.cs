// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace System.Web.Mvc
{
    public class ModelValidatorProviderCollection : Collection<ModelValidatorProvider>
    {
        private ModelValidatorProvider[] _combinedItems;
        private IDependencyResolver _dependencyResolver;

        public ModelValidatorProviderCollection()
        {
        }

        public ModelValidatorProviderCollection(IList<ModelValidatorProvider> list)
            : base(list)
        {
        }

        internal ModelValidatorProviderCollection(IList<ModelValidatorProvider> list, IDependencyResolver dependencyResolver)
            : base(list)
        {
            _dependencyResolver = dependencyResolver;
        }

        internal ModelValidatorProvider[] CombinedItems
        {
            get 
            {
                ModelValidatorProvider[] combinedItems = _combinedItems;
                if (combinedItems == null)
                {
                    combinedItems = MultiServiceResolver.GetCombined<ModelValidatorProvider>(Items, _dependencyResolver);
                    _combinedItems = combinedItems;
                }
                return combinedItems;
            }
        }

        protected override void ClearItems()
        {
            _combinedItems = null;
            base.ClearItems();
        }

        protected override void InsertItem(int index, ModelValidatorProvider item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            _combinedItems = null;
            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            _combinedItems = null;
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, ModelValidatorProvider item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            _combinedItems = null;
            base.SetItem(index, item);
        }

        public IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, ControllerContext context)
        {
            ModelValidatorProvider[] combined = CombinedItems;
            for (int i = 0; i < combined.Length; i++)
            {
                ModelValidatorProvider provider = combined[i];
                foreach (ModelValidator validator in provider.GetValidators(metadata, context))
                {
                    yield return validator;
                }
            }
        }
    }
}
