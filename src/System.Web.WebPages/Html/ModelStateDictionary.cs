// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace System.Web.WebPages.Html
{
    // Most of the code for this class is copied from MVC 2.0
    public class ModelStateDictionary : IDictionary<string, ModelState>
    {
        internal const string FormFieldKey = "_FORM";
        private readonly Dictionary<string, ModelState> _innerDictionary = new Dictionary<string, ModelState>(StringComparer.OrdinalIgnoreCase);

        public ModelStateDictionary()
        {
        }

        public ModelStateDictionary(ModelStateDictionary dictionary)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException("dictionary");
            }

            foreach (var entry in dictionary)
            {
                _innerDictionary.Add(entry.Key, entry.Value);
            }
        }

        public int Count
        {
            get { return _innerDictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return ((IDictionary<string, ModelState>)_innerDictionary).IsReadOnly; }
        }

        public bool IsValid
        {
            get { return !Values.SelectMany(modelState => modelState.Errors).Any(); }
        }

        public ICollection<string> Keys
        {
            get { return _innerDictionary.Keys; }
        }

        public ICollection<ModelState> Values
        {
            get { return _innerDictionary.Values; }
        }

        public ModelState this[string key]
        {
            get
            {
                ModelState value;
                _innerDictionary.TryGetValue(key, out value);
                return value;
            }
            set { _innerDictionary[key] = value; }
        }

        public void Add(KeyValuePair<string, ModelState> item)
        {
            ((IDictionary<string, ModelState>)_innerDictionary).Add(item);
        }

        public void Add(string key, ModelState value)
        {
            _innerDictionary.Add(key, value);
        }

        public void AddError(string key, string errorMessage)
        {
            GetModelStateForKey(key).Errors.Add(errorMessage);
        }

        public void AddFormError(string errorMessage)
        {
            GetModelStateForKey(FormFieldKey).Errors.Add(errorMessage);
        }

        public void Clear()
        {
            _innerDictionary.Clear();
        }

        public bool Contains(KeyValuePair<string, ModelState> item)
        {
            return ((IDictionary<string, ModelState>)_innerDictionary).Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return _innerDictionary.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, ModelState>[] array, int arrayIndex)
        {
            ((IDictionary<string, ModelState>)_innerDictionary).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, ModelState>> GetEnumerator()
        {
            return _innerDictionary.GetEnumerator();
        }

        private ModelState GetModelStateForKey(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            ModelState modelState;
            if (!TryGetValue(key, out modelState))
            {
                modelState = new ModelState();
                _innerDictionary[key] = modelState;
            }

            return modelState;
        }

        public bool IsValidField(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            // if the key is not found in the dictionary, we just say that it's valid (since there are no errors)
            ModelState modelState = this[key];
            return (modelState == null) || !modelState.Errors.Any();
        }

        public void Merge(ModelStateDictionary dictionary)
        {
            if (dictionary == null)
            {
                return;
            }

            foreach (var entry in dictionary)
            {
                this[entry.Key] = entry.Value;
            }
        }

        public bool Remove(KeyValuePair<string, ModelState> item)
        {
            return ((IDictionary<string, ModelState>)_innerDictionary).Remove(item);
        }

        public bool Remove(string key)
        {
            return _innerDictionary.Remove(key);
        }

        public bool TryGetValue(string key, out ModelState value)
        {
            return _innerDictionary.TryGetValue(key, out value);
        }

        public void SetModelValue(string key, object value)
        {
            ModelState state = GetModelStateForKey(key);
            state.Value = value;
        }

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_innerDictionary).GetEnumerator();
        }

        #endregion
    }
}
