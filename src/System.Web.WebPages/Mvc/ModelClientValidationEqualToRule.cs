// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Runtime.CompilerServices;

namespace System.Web.Mvc
{
    [TypeForwardedFrom("System.Web.Mvc, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class ModelClientValidationEqualToRule : ModelClientValidationRule
    {
        public ModelClientValidationEqualToRule(string errorMessage, object other)
        {
            ErrorMessage = errorMessage;
            ValidationType = "equalto";
            ValidationParameters["other"] = other;
        }
    }
}
