// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    public static class ModelValidatorProviders
    {
        private static readonly ModelValidatorProviderCollection _providers = new ModelValidatorProviderCollection()
        {
            new DataAnnotationsModelValidatorProvider(),
            new DataErrorInfoModelValidatorProvider(),
            new ClientDataTypeModelValidatorProvider()
        };

        public static ModelValidatorProviderCollection Providers
        {
            get { return _providers; }
        }
    }
}
