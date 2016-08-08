// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.TestCommon;

namespace System.Web.Http.Validation
{
    public class ModelValidationRequiredMemberSelectorTest
    {
        [Theory]
        [InlineData("CustomerID", true)]
        [InlineData("ID", true)]
        [InlineData("ItemID", true)]
        [InlineData("UselessInfo", false)]
        public void IsRequiredMember_RecognizesRequiredMembers(string propertyName, bool isRequired)
        {
            HttpConfiguration config = new HttpConfiguration();
            IRequiredMemberSelector selector = new ModelValidationRequiredMemberSelector(config.Services.GetModelMetadataProvider(), config.Services.GetModelValidatorProviders());

            Assert.Equal(isRequired, selector.IsRequiredMember(typeof(PurchaseOrder).GetProperty(propertyName)));
        }

        [Theory]
        [InlineData("StringProperty")]
        [InlineData("NullableProperty")]
        [InlineData("CollectionProperty")]
        public void IsRequiredMember_ReturnsFalse_ForNullableProperties(string propertyName)
        {
            HttpConfiguration config = new HttpConfiguration();
            IRequiredMemberSelector selector = new ModelValidationRequiredMemberSelector(config.Services.GetModelMetadataProvider(), config.Services.GetModelValidatorProviders());

            Assert.False(selector.IsRequiredMember(typeof(NullableProperties).GetProperty(propertyName)));
        }


        [Theory]
        [InlineData("ProtectedGet")]
        [InlineData("NoGet")]
        [InlineData("Internal")]
        public void IsRequiredMember_ReturnsFalse_ForInvalidProperties(string propertyName)
        {
            HttpConfiguration config = new HttpConfiguration();
            IRequiredMemberSelector selector = new ModelValidationRequiredMemberSelector(config.Services.GetModelMetadataProvider(), config.Services.GetModelValidatorProviders());
            PropertyInfo propertyInfo = typeof(BadProperties).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            Assert.False(selector.IsRequiredMember(propertyInfo));
        }

        [DataContract]
        public class PurchaseOrder
        {
            [DataMember(IsRequired = true)]
            public int ID { get; set; }

            [Required]
            public int CustomerID { get; set; }

            [Required]
            [DataMember(IsRequired = true)]
            public int ItemID { get; set; }

            public int UselessInfo { get; set; }
        }

        public class NullableProperties
        {
            [Required]
            public string StringProperty { get; set; }

            [Required]
            public Nullable<int> NullableProperty { get; set; }

            [Required]
            public List<int> CollectionProperty { get; set; }
        }

        public class BadProperties
        {
            public int ProtectedGet
            {
                protected get;
                set;
            }

            public int NoGet
            {
                set { }
            }

            internal int Internal
            {
                get;
                set;
            }
        }
    }
}