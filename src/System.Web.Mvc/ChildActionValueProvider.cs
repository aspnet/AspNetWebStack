// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;

namespace System.Web.Mvc
{
    public sealed class ChildActionValueProvider : DictionaryValueProvider<object>
    {
        private static string _childActionValuesKey = Guid.NewGuid().ToString();

        public ChildActionValueProvider(ControllerContext controllerContext)
            : base(controllerContext.RouteData.Values, CultureInfo.InvariantCulture)
        {
        }

        internal static string ChildActionValuesKey
        {
            get { return _childActionValuesKey; }
        }

        public override ValueProviderResult GetValue(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            ValueProviderResult explicitValues = base.GetValue(ChildActionValuesKey);
            if (explicitValues != null)
            {
                DictionaryValueProvider<object> rawExplicitValues = explicitValues.RawValue as DictionaryValueProvider<object>;
                if (rawExplicitValues != null)
                {
                    return rawExplicitValues.GetValue(key);
                }
            }

            return null;
        }
    }
}
