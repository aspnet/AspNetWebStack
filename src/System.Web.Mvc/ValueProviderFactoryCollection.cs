// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace System.Web.Mvc
{
    public class ValueProviderFactoryCollection : Collection<ValueProviderFactory>
    {
        private ValueProviderFactory[] _combinedItems;
        private IDependencyResolver _dependencyResolver;

        public ValueProviderFactoryCollection()
        {
        }

        public ValueProviderFactoryCollection(IList<ValueProviderFactory> list)
            : base(list)
        {
        }

        internal ValueProviderFactoryCollection(IList<ValueProviderFactory> list, IDependencyResolver dependencyResolver)
            : base(list)
        {
            _dependencyResolver = dependencyResolver;
        }

        internal ValueProviderFactory[] CombinedItems
        {
            get
            {
                ValueProviderFactory[] combinedItems = _combinedItems;
                if (combinedItems == null)
                {
                    combinedItems = MultiServiceResolver.GetCombined<ValueProviderFactory>(Items, _dependencyResolver);
                    _combinedItems = combinedItems;
                }
                return combinedItems;
            }
        }

        public IValueProvider GetValueProvider(ControllerContext controllerContext)
        {
            ValueProviderFactory[] current = CombinedItems;
            List<IValueProvider> providers = new List<IValueProvider>(current.Length);
            for (int i = 0; i < current.Length; i++)
            {
                ValueProviderFactory factory = current[i];
                IValueProvider provider = factory.GetValueProvider(controllerContext);
                if (provider != null)
                {
                    providers.Add(provider);
                }
            }
            return new ValueProviderCollection(providers);
        }

        protected override void ClearItems()
        {
            _combinedItems = null;
            base.ClearItems();
        }

        protected override void InsertItem(int index, ValueProviderFactory item)
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

        protected override void SetItem(int index, ValueProviderFactory item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            _combinedItems = null;
            base.SetItem(index, item);
        }
    }
}
