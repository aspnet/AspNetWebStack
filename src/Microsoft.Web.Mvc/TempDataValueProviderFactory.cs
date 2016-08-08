// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Mvc;

namespace Microsoft.Web.Mvc
{
    public class TempDataValueProviderFactory : ValueProviderFactory
    {
        public override IValueProvider GetValueProvider(ControllerContext controllerContext)
        {
            TempDataDictionary tempData = controllerContext.Controller.TempData;
            if (tempData.Count == 0)
            {
                // fast-track empty TempData
                return null;
            }

            return new TempDataValueProvider(tempData);
        }

        /// <summary>
        /// dummy struct that resembles Void but can be used in a generic context
        /// </summary>
        private struct TempDataVoid
        {
        }

        private sealed class TempDataValueProvider : DictionaryValueProvider<TempDataVoid>
        {
            private readonly TempDataDictionary _tempData;

            // use invariant culture since TempData should contain objects
            public TempDataValueProvider(TempDataDictionary tempData)
                : base(GetVoidDictionary(tempData), CultureInfo.InvariantCulture)
            {
                _tempData = tempData;
            }

            public override ValueProviderResult GetValue(string key)
            {
                object rawValue;

                // TryGetValue will mark the entry for removal.
                if (_tempData.TryGetValue(key, out rawValue))
                {
                    string attemptedValue = Convert.ToString(rawValue, CultureInfo.InvariantCulture);
                    return new ValueProviderResult(rawValue, attemptedValue, CultureInfo.InvariantCulture);
                }
                else
                {
                    // no value found
                    return null;
                }
            }

            private static Dictionary<string, TempDataVoid> GetVoidDictionary(TempDataDictionary tempData)
            {
                // Create a special backing store that doesn't directly hold the values, since the DictionaryValueProvider
                // enumerates over the backing store but enumerating over TempData marks everything for removal.
                Dictionary<string, TempDataVoid> d = new Dictionary<string, TempDataVoid>(StringComparer.OrdinalIgnoreCase);

                // Enumerating over TempDataDictionary.Keys doesn't mark them for removal.
                foreach (string key in tempData.Keys)
                {
                    d[key] = default(TempDataVoid);
                }

                return d;
            }
        }
    }
}
