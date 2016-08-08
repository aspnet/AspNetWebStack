// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Web.Mvc.Properties;

namespace Microsoft.Web.Mvc
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class AcceptAttribute : DataTypeAttribute, IClientValidatable
    {
        public AcceptAttribute()
            : base("upload")
        {
            ErrorMessage = MvcResources.FileExtensionsAttribute_Invalid;
            ErrorMessage = MvcResources.AcceptAttribute_Invalid;
        }

        public string MimeTypes { get; set; }

        private string MimeTypesFormatted
        {
            get { return MimeTypesParsed.Aggregate((left, right) => left + ", " + right); }
        }

        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "These strings are normalized to lowercase because they are presented to the user in lowercase format")]
        private string MimeTypesNormalized
        {
            get { return MimeTypes.Replace(" ", String.Empty).ToLowerInvariant(); }
        }

        private IEnumerable<string> MimeTypesParsed
        {
            get { return MimeTypesNormalized.Split(','); }
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, MimeTypesFormatted);
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var rule = new ModelClientValidationRule
            {
                ValidationType = "accept",
                ErrorMessage = FormatErrorMessage(metadata.GetDisplayName())
            };
            rule.ValidationParameters["mimetype"] = MimeTypesNormalized;
            yield return rule;
        }

        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return true;
            }

            HttpPostedFileBase valueAsFileBase = value as HttpPostedFileBase;
            if (valueAsFileBase != null)
            {
                return ValidateMimeTypes(valueAsFileBase.ContentType);
            }

            string valueAsString = value as string;
            if (valueAsString != null)
            {
                return ValidateMimeTypes(valueAsString);
            }

            return false;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "These strings are normalized to lowercase because they are presented to the user in lowercase format")]
        private bool ValidateMimeTypes(string mimeType)
        {
            return MimeTypesParsed.Contains(mimeType.ToLowerInvariant());
        }
    }
}
