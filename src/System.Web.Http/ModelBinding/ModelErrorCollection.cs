// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.ObjectModel;

namespace System.Web.Http.ModelBinding
{
    [Serializable]
    public class ModelErrorCollection : Collection<ModelError>
    {
        public void Add(Exception exception)
        {
            Add(new ModelError(exception));
        }

        public void Add(string errorMessage)
        {
            Add(new ModelError(errorMessage));
        }
    }
}
