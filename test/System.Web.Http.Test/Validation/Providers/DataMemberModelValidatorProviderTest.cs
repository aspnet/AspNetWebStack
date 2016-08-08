// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Web.Http.Metadata.Providers;
using Microsoft.TestCommon;

namespace System.Web.Http.Validation.Providers
{
    public class DataMemberModelValidatorProviderTest
    {
        private static DataAnnotationsModelMetadataProvider _metadataProvider = new DataAnnotationsModelMetadataProvider();

        [Fact]
        public void ClassWithoutAttributes_NoValidator()
        {
            // Arrange
            var provider = new DataMemberModelValidatorProvider();
            var metadata = _metadataProvider.GetMetadataForProperty(() => null, typeof(ClassWithoutAttributes), "TheProperty");

            // Act
            IEnumerable<ModelValidator> validators = provider.GetValidators(metadata, new[] { provider });

            // Assert
            Assert.Empty(validators);
        }

        class ClassWithoutAttributes
        {
            public int TheProperty { get; set; }
        }

        [Fact]
        public void ClassWithDataMemberIsRequiredTrue_Validator()
        {
            // Arrange
            var provider = new DataMemberModelValidatorProvider();
            var metadata = _metadataProvider.GetMetadataForProperty(() => null, typeof(ClassWithDataMemberIsRequiredTrue), "TheProperty");

            // Act
            IEnumerable<ModelValidator> validators = provider.GetValidators(metadata, new[] { provider });

            // Assert
            ModelValidator validator = Assert.Single(validators);
            Assert.True(validator.IsRequired);
        }

        [DataContract]
        class ClassWithDataMemberIsRequiredTrue
        {
            [DataMember(IsRequired = true)]
            public int TheProperty { get; set; }
        }

        [Fact]
        public void ClassWithDataMemberIsRequiredFalse_NoValidator()
        {
            // Arrange
            var provider = new DataMemberModelValidatorProvider();
            var metadata = _metadataProvider.GetMetadataForProperty(() => null, typeof(ClassWithDataMemberIsRequiredFalse), "TheProperty");

            // Act
            IEnumerable<ModelValidator> validators = provider.GetValidators(metadata, new[] { provider });

            // Assert
            Assert.Empty(validators);
        }

        [DataContract]
        class ClassWithDataMemberIsRequiredFalse
        {
            [DataMember(IsRequired = false)]
            public int TheProperty { get; set; }
        }

        [Fact]
        public void ClassWithDataMemberIsRequiredTrueWithoutDataContract_NoValidator()
        {
            // Arrange
            var provider = new DataMemberModelValidatorProvider();
            var metadata = _metadataProvider.GetMetadataForProperty(() => null, typeof(ClassWithDataMemberIsRequiredTrueWithoutDataContract), "TheProperty");

            // Act
            IEnumerable<ModelValidator> validators = provider.GetValidators(metadata, new[] { provider });

            // Assert
            Assert.Empty(validators);
        }

        class ClassWithDataMemberIsRequiredTrueWithoutDataContract
        {
            [DataMember(IsRequired = true)]
            public int TheProperty { get; set; }
        }
    }
}
