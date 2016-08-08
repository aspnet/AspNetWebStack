// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Http.ValueProviders;

namespace System.Web.Http.ModelBinding
{
    [Serializable]
    public class ModelState
    {
        private ModelErrorCollection _errors = new ModelErrorCollection();

        public ValueProviderResult Value { get; set; }

        public ModelErrorCollection Errors
        {
            get { return _errors; }
        }
    }
}
