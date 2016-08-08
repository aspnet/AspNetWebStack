// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    public static class ModelBinderProviders
    {
        private static readonly ModelBinderProviderCollection _binderProviders = new ModelBinderProviderCollection
        {
        };

        public static ModelBinderProviderCollection BinderProviders
        {
            get { return _binderProviders; }
        }
    }
}
