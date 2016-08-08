// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Web.Mvc.Properties;

namespace System.Web.Mvc
{
    [Obsolete("The recommended alternative is to use the System.ComponentModel.DataAnnotations.CompareAttribute type, which has the same functionality as this type.")]
    [AttributeUsage(AttributeTargets.Property)]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "This attribute is designed to be a base class for other attributes.")]
    public class CompareAttribute : ValidationAttribute, IClientValidatable
    {
        public CompareAttribute(string otherProperty)
            : base(MvcResources.CompareAttribute_MustMatch)
        {
            if (otherProperty == null)
            {
                throw new ArgumentNullException("otherProperty");
            }
            OtherProperty = otherProperty;
        }

        public string OtherProperty { get; private set; }

        public string OtherPropertyDisplayName { get; internal set; }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, OtherPropertyDisplayName ?? OtherProperty);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            PropertyInfo otherPropertyInfo = validationContext.ObjectType.GetProperty(OtherProperty);
            if (otherPropertyInfo == null)
            {
                return new ValidationResult(String.Format(CultureInfo.CurrentCulture, MvcResources.CompareAttribute_UnknownProperty, OtherProperty));
            }

            object otherPropertyValue = otherPropertyInfo.GetValue(validationContext.ObjectInstance, null);
            if (!Equals(value, otherPropertyValue))
            {
                if (OtherPropertyDisplayName == null)
                {
                    OtherPropertyDisplayName = ModelMetadataProviders.Current.GetMetadataForProperty(() => validationContext.ObjectInstance, validationContext.ObjectType, OtherProperty).GetDisplayName();
                }
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
            }
            return null;
        }

        public static string FormatPropertyForClientValidation(string property)
        {
            if (property == null)
            {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "property");
            }
            return "*." + property;
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            if (metadata.ContainerType != null)
            {
                if (OtherPropertyDisplayName == null)
                {
                    OtherPropertyDisplayName = ModelMetadataProviders.Current.GetMetadataForProperty(() => metadata.Model, metadata.ContainerType, OtherProperty).GetDisplayName();
                }
            }
            yield return new ModelClientValidationEqualToRule(FormatErrorMessage(metadata.GetDisplayName()), FormatPropertyForClientValidation(OtherProperty));
        }
    }
}
