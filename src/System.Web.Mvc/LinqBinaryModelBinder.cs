// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Linq;

namespace System.Web.Mvc
{
    public class LinqBinaryModelBinder : ByteArrayModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            byte[] byteValue = (byte[])base.BindModel(controllerContext, bindingContext);
            if (byteValue == null)
            {
                return null;
            }

            return new Binary(byteValue);
        }
    }
}
