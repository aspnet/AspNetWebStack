// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Metadata;

namespace System.Web.Http.Validation.Validators
{
    /// <summary>
    /// <see cref="ModelValidator"/> for required members.
    /// </summary>
    public class RequiredMemberModelValidator : ModelValidator
    {
        public RequiredMemberModelValidator(IEnumerable<ModelValidatorProvider> validatorProviders)
            : base(validatorProviders)
        {
        }

        public override bool IsRequired
        {
            get { return true; }
        }

        public override IEnumerable<ModelValidationResult> Validate(ModelMetadata metadata, object container)
        {
            return Enumerable.Empty<ModelValidationResult>();
        }
    }
}
