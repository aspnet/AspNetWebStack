// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Microsoft.Internal.Web.Utils;

namespace System.Web.WebPages.Html
{
    public partial class HtmlHelper
    {
        public IHtmlString ValidationMessage(string name)
        {
            return ValidationMessage(name, null, null);
        }

        public IHtmlString ValidationMessage(string name, string message)
        {
            return ValidationMessage(name, message, (IDictionary<string, object>)null);
        }

        public IHtmlString ValidationMessage(string name, object htmlAttributes)
        {
            return ValidationMessage(name, null, AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public IHtmlString ValidationMessage(string name, IDictionary<string, object> htmlAttributes)
        {
            return ValidationMessage(name, null, htmlAttributes);
        }

        public IHtmlString ValidationMessage(string name, string message, object htmlAttributes)
        {
            return ValidationMessage(name, message, AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public IHtmlString ValidationMessage(string name, string message, IDictionary<string, object> htmlAttributes)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "name");
            }
            return BuildValidationMessage(name, message, htmlAttributes);
        }

        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase",
            Justification = "Normalization to lowercase is a common requirement for JavaScript and HTML values")]
        private IHtmlString BuildValidationMessage(string name, string message, IDictionary<string, object> htmlAttributes)
        {
            var modelState = ModelState[name];
            IEnumerable<string> errors = null;
            if (modelState != null)
            {
                errors = modelState.Errors;
            }
            bool hasError = errors != null && errors.Any();
            if (!hasError && !UnobtrusiveJavaScriptEnabled)
            {
                // If unobtrusive validation is enabled, we need to generate an empty span with the "val-for" attribute"
                return null;
            }
            else
            {
                string error = null;
                if (hasError)
                {
                    error = message ?? errors.First();
                }

                TagBuilder tagBuilder = new TagBuilder("span") { InnerHtml = Encode(error) };
                tagBuilder.MergeAttributes(htmlAttributes);
                if (UnobtrusiveJavaScriptEnabled)
                {
                    bool replaceValidationMessageContents = String.IsNullOrEmpty(message);
                    tagBuilder.MergeAttribute("data-valmsg-for", name);
                    tagBuilder.MergeAttribute("data-valmsg-replace", replaceValidationMessageContents.ToString().ToLowerInvariant());
                }
                tagBuilder.AddCssClass(hasError ? ValidationMessageCssClassName : ValidationMessageValidCssClassName);
                return tagBuilder.ToHtmlString(TagRenderMode.Normal);
            }
        }

        public IHtmlString ValidationSummary()
        {
            return BuildValidationSummary(message: null, excludeFieldErrors: false, htmlAttributes: (IDictionary<string, object>)null);
        }

        public IHtmlString ValidationSummary(string message)
        {
            return BuildValidationSummary(message: message, excludeFieldErrors: false, htmlAttributes: (IDictionary<string, object>)null);
        }

        public IHtmlString ValidationSummary(bool excludeFieldErrors)
        {
            return ValidationSummary(message: null, excludeFieldErrors: excludeFieldErrors, htmlAttributes: (IDictionary<string, object>)null);
        }

        public IHtmlString ValidationSummary(object htmlAttributes)
        {
            return ValidationSummary(message: null, excludeFieldErrors: false, htmlAttributes: htmlAttributes);
        }

        public IHtmlString ValidationSummary(IDictionary<string, object> htmlAttributes)
        {
            return ValidationSummary(message: null, excludeFieldErrors: false, htmlAttributes: htmlAttributes);
        }

        public IHtmlString ValidationSummary(string message, object htmlAttributes)
        {
            return ValidationSummary(message, excludeFieldErrors: false, htmlAttributes: htmlAttributes);
        }

        public IHtmlString ValidationSummary(string message, IDictionary<string, object> htmlAttributes)
        {
            return ValidationSummary(message, excludeFieldErrors: false, htmlAttributes: htmlAttributes);
        }

        public IHtmlString ValidationSummary(string message, bool excludeFieldErrors, object htmlAttributes)
        {
            return ValidationSummary(message, excludeFieldErrors, AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public IHtmlString ValidationSummary(string message, bool excludeFieldErrors, IDictionary<string, object> htmlAttributes)
        {
            return BuildValidationSummary(message, excludeFieldErrors, htmlAttributes);
        }

        private IHtmlString BuildValidationSummary(string message, bool excludeFieldErrors, IDictionary<string, object> htmlAttributes)
        {
            IEnumerable<string> errors = null;
            if (excludeFieldErrors)
            {
                // Review: Is there a better way to share the form field name between this and ModelStateDictionary?
                var formModelState = ModelState[ModelStateDictionary.FormFieldKey];
                if (formModelState != null)
                {
                    errors = formModelState.Errors;
                }
            }
            else
            {
                errors = ModelState.SelectMany(c => c.Value.Errors);
            }

            bool hasErrors = errors != null && errors.Any();
            if (!hasErrors && (!UnobtrusiveJavaScriptEnabled || excludeFieldErrors))
            {
                // If no errors are found and we do not have unobtrusive validation enabled or if the summary is not meant to display field errors, don't generate the summary.
                return null;
            }
            else
            {
                TagBuilder tagBuilder = new TagBuilder("div");
                tagBuilder.MergeAttributes(htmlAttributes);
                tagBuilder.AddCssClass(hasErrors ? ValidationSummaryClass : ValidationSummaryValidClass);
                if (UnobtrusiveJavaScriptEnabled && !excludeFieldErrors)
                {
                    tagBuilder.MergeAttribute("data-valmsg-summary", "true");
                }

                StringBuilder builder = new StringBuilder();
                if (message != null)
                {
                    builder.Append("<span>");
                    builder.Append(Encode(message));
                    builder.AppendLine("</span>");
                }
                builder.AppendLine("<ul>");
                foreach (var error in errors)
                {
                    builder.Append("<li>");
                    builder.Append(Encode(error));
                    builder.AppendLine("</li>");
                }
                builder.Append("</ul>");

                tagBuilder.InnerHtml = builder.ToString();
                return tagBuilder.ToHtmlString(TagRenderMode.Normal);
            }
        }
    }
}
