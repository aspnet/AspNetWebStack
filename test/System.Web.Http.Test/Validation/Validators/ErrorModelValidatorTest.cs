// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Metadata.Providers;
using Microsoft.TestCommon;

namespace System.Web.Http.Validation.Validators
{
    public class ErrorModelValidatorTest
    {
        private static DataAnnotationsModelMetadataProvider _metadataProvider = new DataAnnotationsModelMetadataProvider();
        private static IEnumerable<ModelValidatorProvider> _noValidatorProviders = Enumerable.Empty<ModelValidatorProvider>();

        [Fact]
        public void ConstructorGuards()
        {
            Assert.ThrowsArgumentNull(
                () => new ErrorModelValidator(validatorProviders: null, errorMessage: "error"),
                "validatorProviders");
            Assert.ThrowsArgumentNull(
                () => new ErrorModelValidator(validatorProviders: _noValidatorProviders, errorMessage: null),
                "errorMessage");
        }

        [Fact]
        public void ValidateThrowsException()
        {
            ErrorModelValidator validator = new ErrorModelValidator(_noValidatorProviders, "error");

            Assert.Throws<InvalidOperationException>(() => validator.Validate(null, null), "error");
        }
    }
}
