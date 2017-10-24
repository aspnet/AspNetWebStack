// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web.Http.Metadata.Providers;
using Microsoft.TestCommon;

namespace System.Web.Http.Validation.Providers
{
    public class InvalidModelValidatorProviderTest
    {
        private static DataAnnotationsModelMetadataProvider _metadataProvider = new DataAnnotationsModelMetadataProvider();
        private static IEnumerable<ModelValidatorProvider> _noValidatorProviders = Enumerable.Empty<ModelValidatorProvider>();

        [Fact]
        public void GetValidatorsReturnsNothingForValidModel()
        {
            InvalidModelValidatorProvider validatorProvider = new InvalidModelValidatorProvider();

            IEnumerable<ModelValidator> validators = validatorProvider.GetValidators(_metadataProvider.GetMetadataForType(null, typeof(ValidModel)), _noValidatorProviders);

            Assert.Empty(validators);
        }

        [Fact]
        public void GetValidatorsReturnsInvalidModelValidatorsForInvalidModelType()
        {
            InvalidModelValidatorProvider validatorProvider = new InvalidModelValidatorProvider();

            IEnumerable<ModelValidator> validators = validatorProvider.GetValidators(_metadataProvider.GetMetadataForType(null, typeof(InvalidModel)), _noValidatorProviders);

            Assert.Equal(2, validators.Count());
            Assert.Throws<InvalidOperationException>(() => validators.ElementAt(0).Validate(null, null),
                "Non-public property 'Internal' on type 'System.Web.Http.Validation.Providers.InvalidModelValidatorProviderTest+InvalidModel' is attributed with one or more validation attributes. Validation attributes on non-public properties are not supported. Consider using a public property for validation instead.");
            Assert.Throws<InvalidOperationException>(() => validators.ElementAt(1).Validate(null, null),
                "Field 'Field' on type 'System.Web.Http.Validation.Providers.InvalidModelValidatorProviderTest+InvalidModel' is attributed with one or more validation attributes. Validation attributes on fields are not supported. Consider using a public property for validation instead.");
        }

        [Fact]
        public void GetValidatorsReturnsInvalidModelValidatorsForInvalidModelProperty()
        {
            InvalidModelValidatorProvider validatorProvider = new InvalidModelValidatorProvider();

            IEnumerable<ModelValidator> validators = validatorProvider.GetValidators(_metadataProvider.GetMetadataForProperty(null, typeof(InvalidModel), "Value"), _noValidatorProviders);

            ModelValidator validator = Assert.Single(validators);
            Assert.Throws<InvalidOperationException>(() => validator.Validate(null, null),
                "Property 'Value' on type 'System.Web.Http.Validation.Providers.InvalidModelValidatorProviderTest+InvalidModel' is invalid. Value-typed properties marked as [Required] must also be marked with [DataMember(IsRequired=true)] to be recognized as required. Consider attributing the declaring type with [DataContract] and the property with [DataMember(IsRequired=true)].");
        }

        [DataContract]
        public class ValidModel
        {
            [Required]
            [DataMember]
            [StringLength(10)]
            public string Ref { get; set; }

            [DataMember]
            internal string Internal { get; set; }

            [Required]
            [DataMember(IsRequired=true)]
            public int Value { get; set; }

            public string Field;
        }

        public class InvalidModel
        {
            [Required]
            public string Ref { get; set; }

            [StringLength(10)]
            [RegularExpression("pattern")]
            internal string Internal { get; set; }

            [Required]
            public int Value { get; set; }

            [StringLength(10)]
            public string Field;
        }
    }
}