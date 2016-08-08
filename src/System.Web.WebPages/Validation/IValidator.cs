// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace System.Web.WebPages
{
    public interface IValidator
    {
        ModelClientValidationRule ClientValidationRule { get; }
        ValidationResult Validate(ValidationContext validationContext);
    }
}
