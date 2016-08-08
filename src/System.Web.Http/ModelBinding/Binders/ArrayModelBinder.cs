// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Controllers;

namespace System.Web.Http.ModelBinding.Binders
{
    public class ArrayModelBinder<TElement> : CollectionModelBinder<TElement>
    {
        public override bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelMetadata.IsReadOnly)
            {
                return false;
            }

            return base.BindModel(actionContext, bindingContext);
        }

        protected override bool CreateOrReplaceCollection(HttpActionContext actionContext, ModelBindingContext bindingContext, IList<TElement> newCollection)
        {
            bindingContext.Model = newCollection.ToArray();
            return true;
        }
    }
}
