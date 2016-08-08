// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Dynamic;
using System.Web.Mvc;
using Microsoft.Web.Mvc.Properties;

namespace Microsoft.Web.Mvc
{
    internal class DynamicViewDataDictionary : DynamicObject
    {
        private readonly ViewDataDictionary _dictionary;

        private DynamicViewDataDictionary(ViewDataDictionary dictionary)
        {
            _dictionary = dictionary;
        }

        public object Model
        {
            get { return _dictionary.Model; }
            set { _dictionary.Model = value; }
        }

        public ModelMetadata ModelMetadata
        {
            get { return _dictionary.ModelMetadata; }
            set { _dictionary.ModelMetadata = value; }
        }

        public ModelStateDictionary ModelState
        {
            get { return _dictionary.ModelState; }
        }

        public TemplateInfo TemplateInfo
        {
            get { return _dictionary.TemplateInfo; }
            set { _dictionary.TemplateInfo = value; }
        }

        private bool GetValue(string name, out object result)
        {
            result = DynamicReflectionObject.Wrap(_dictionary.Eval(name)) ?? String.Empty;
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (indexes.Length != 1)
            {
                throw new ArgumentException(MvcResources.DynamicViewDataDictionary_SingleIndexerOnly);
            }

            string name = indexes[0] as string;
            if (name == null)
            {
                throw new ArgumentException(MvcResources.DynamicViewDataDictionary_StringIndexerOnly);
            }

            return GetValue(name, out result);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return GetValue(binder.Name, out result);
        }

        public static dynamic Wrap(ViewDataDictionary dictionary)
        {
            return new DynamicViewDataDictionary(dictionary);
        }
    }
}
