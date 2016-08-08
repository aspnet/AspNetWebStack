// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Dynamic;
using System.Web.WebPages.Resources;

namespace System.Web.WebPages
{
    internal class DynamicHttpApplicationState : DynamicObject
    {
        private HttpApplicationStateBase _state;

        public DynamicHttpApplicationState(HttpApplicationStateBase state)
        {
            _state = state;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = _state[binder.Name];
            // We return true here because HttpApplicationState returns null if the key is not
            // in the dictionary, so we simply pass on the returned value.
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _state[binder.Name] = value;
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (indexes == null || indexes.Length != 1)
            {
                throw new ArgumentException(WebPageResources.DynamicDictionary_InvalidNumberOfIndexes);
            }

            result = null;
            string key = indexes[0] as string;
            if (key != null)
            {
                result = _state[key];
            }
            else if (indexes[0] is int)
            {
                result = _state[(int)indexes[0]];
            }
            else
            {
                // HttpApplicationState only supports keys of type string and int when getting values, so any attempt 
                // to use other types will result in an error. We throw an exception here to explain to the user what is wrong.
                // Returning false will instead cause a runtime binder exception which might be confusing to the user.
                throw new ArgumentException(WebPageResources.DynamicHttpApplicationState_UseOnlyStringOrIntToGet);
            }
            return true;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            if (indexes == null || indexes.Length != 1)
            {
                throw new ArgumentException(WebPageResources.DynamicDictionary_InvalidNumberOfIndexes);
            }

            string key = indexes[0] as string;
            if (key != null)
            {
                _state[key] = value;
                return true;
            }
            else
            {
                // HttpApplicationState only supports keys of type string when setting values, so any attempt 
                // to use other types will result in an error. We throw an exception here to explain to the user what is wrong.
                // Returning false will instead cause a runtime binder error which might be confusing to the user.
                throw new ArgumentException(WebPageResources.DynamicHttpApplicationState_UseOnlyStringToSet);
            }
        }
    }
}
