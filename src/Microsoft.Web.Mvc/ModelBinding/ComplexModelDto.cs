// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Mvc;

namespace Microsoft.Web.Mvc.ModelBinding
{
    // Describes a complex model, but uses a collection rather than individual properties as the data store.
    public class ComplexModelDto
    {
        public ComplexModelDto(ModelMetadata modelMetadata, IEnumerable<ModelMetadata> propertyMetadata)
        {
            if (modelMetadata == null)
            {
                throw new ArgumentNullException("modelMetadata");
            }
            if (propertyMetadata == null)
            {
                throw new ArgumentNullException("propertyMetadata");
            }

            ModelMetadata = modelMetadata;
            PropertyMetadata = new ReadOnlyCollection<ModelMetadata>(propertyMetadata.ToList());
            Results = new Dictionary<ModelMetadata, ComplexModelDtoResult>();
        }

        public ModelMetadata ModelMetadata { get; private set; }

        public ReadOnlyCollection<ModelMetadata> PropertyMetadata { get; private set; }

        // Contains entries corresponding to each property against which binding was
        // attempted. If binding failed, the entry's value will be null. If binding
        // was never attempted, this dictionary will not contain a corresponding
        // entry.
        public IDictionary<ModelMetadata, ComplexModelDtoResult> Results { get; private set; }
    }
}
