// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.WebPages.Html;
using System.Web.WebPages.Scope;
using Microsoft.Internal.Web.Utils;

namespace System.Web.WebPages
{
    public sealed class ValidationHelper
    {
        private static readonly object _invalidCssClassKey = new object();
        private static readonly object _validCssClassKey = new object();
        private static IDictionary<object, object> _scopeOverride;

        private readonly Dictionary<string, List<IValidator>> _validators = new Dictionary<string, List<IValidator>>(StringComparer.OrdinalIgnoreCase);
        private readonly HttpContextBase _httpContext;
        private readonly ModelStateDictionary _modelStateDictionary;

        internal ValidationHelper(HttpContextBase httpContext, ModelStateDictionary modelStateDictionary)
        {
            Debug.Assert(httpContext != null);
            Debug.Assert(modelStateDictionary != null);

            _httpContext = httpContext;
            _modelStateDictionary = modelStateDictionary;
        }

        public static string ValidCssClass
        {
            get
            {
                object value;
                if (!Scope.TryGetValue(_validCssClassKey, out value))
                {
                    return null;
                }
                return value as string;
            }
            set { Scope[_validCssClassKey] = value; }
        }

        public static string InvalidCssClass
        {
            get
            {
                object value;
                if (!Scope.TryGetValue(_invalidCssClassKey, out value))
                {
                    return HtmlHelper.DefaultValidationInputErrorCssClass;
                }
                return value as string;
            }
            set { Scope[_invalidCssClassKey] = value; }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This makes it easier for a user to read this value without knowing of this type.")]
        public string FormField
        {
            get { return ModelStateDictionary.FormFieldKey; }
        }

        internal static IDictionary<object, object> Scope
        {
            get { return _scopeOverride ?? ScopeStorage.CurrentScope; }
        }

        public void RequireField(string field)
        {
            RequireField(field, errorMessage: null);
        }

        public void RequireField(string field, string errorMessage)
        {
            if (String.IsNullOrEmpty(field))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "field");
            }
            Add(field, Validator.Required(errorMessage: errorMessage));
        }

        public void RequireFields(params string[] fields)
        {
            if (fields == null)
            {
                throw new ArgumentNullException("fields");
            }
            foreach (var field in fields)
            {
                RequireField(field);
            }
        }

        public void Add(string field, params IValidator[] validators)
        {
            if (String.IsNullOrEmpty(field))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "field");
            }
            if ((validators == null) || validators.Any(v => v == null))
            {
                throw new ArgumentNullException("validators");
            }

            AddFieldValidators(field, validators);
        }

        public void Add(IEnumerable<string> fields, params IValidator[] validators)
        {
            if (fields == null)
            {
                throw new ArgumentNullException("fields");
            }
            if (validators == null)
            {
                throw new ArgumentNullException("validators");
            }
            foreach (var field in fields)
            {
                Add(field, validators);
            }
        }

        public void AddFormError(string errorMessage)
        {
            _modelStateDictionary.AddFormError(errorMessage);
        }

        public bool IsValid(params string[] fields)
        {
            // Don't need to validate fields as we treat empty fields as all in Validate.
            return !Validate(fields).Any();
        }

        public IEnumerable<ValidationResult> Validate(params string[] fields)
        {
            IEnumerable<string> keys = fields;
            if (fields == null || !fields.Any())
            {
                // If no fields are present, validate all of them.
                keys = _validators.Keys.Concat(new[] { FormField });
            }
            return ValidateFieldsAndUpdateModelState(keys);
        }

        public IEnumerable<string> GetErrors(params string[] fields)
        {
            // Don't need to validate fields as we treat empty fields as all in Validate.
            return Validate(fields).Select(r => r.ErrorMessage);
        }

        public HtmlString For(string field)
        {
            if (String.IsNullOrEmpty(field))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "field");
            }

            var clientRules = GetClientValidationRules(field);
            return GenerateHtmlFromClientValidationRules(clientRules);
        }

        public HtmlString ClassFor(string field)
        {
            if (_httpContext != null && String.Equals("POST", _httpContext.Request.HttpMethod, StringComparison.OrdinalIgnoreCase))
            {
                string cssClass = IsValid(field) ? ValidationHelper.ValidCssClass : ValidationHelper.InvalidCssClass;
                return cssClass == null ? null : new HtmlString(cssClass);
            }
            return null;
        }

        internal static IDisposable OverrideScope()
        {
            _scopeOverride = new Dictionary<object, object>();
            return new DisposableAction(() => _scopeOverride = null);
        }

        internal IDictionary<string, object> GetUnobtrusiveValidationAttributes(string field)
        {
            var clientRules = GetClientValidationRules(field);
            var attributes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            UnobtrusiveValidationAttributesGenerator.GetValidationAttributes(clientRules, attributes);
            return attributes;
        }

        private IEnumerable<ValidationResult> ValidateFieldsAndUpdateModelState(IEnumerable<string> fields)
        {
            var validationContext = new ValidationContext(_httpContext, serviceProvider: null, items: null);
            var validationResults = new List<ValidationResult>();
            foreach (var field in fields)
            {
                IEnumerable<ValidationResult> fieldResults = ValidateField(field, validationContext);
                IEnumerable<string> errors = fieldResults.Select(c => c.ErrorMessage);
                ModelState modelState = _modelStateDictionary[field];
                if (modelState != null && modelState.Errors.Any())
                {
                    errors = errors.Except(modelState.Errors, StringComparer.OrdinalIgnoreCase);

                    // If there were other validation errors that were added via ModelState, add them to the collection.
                    fieldResults = fieldResults.Concat(modelState.Errors.Select(e => new ValidationResult(e, new[] { field })));
                }

                foreach (var errorMessage in errors)
                {
                    // Only add errors that haven't been encountered before. This is to prevent from the same error message being duplicated 
                    // if a call is made multiple times
                    _modelStateDictionary.AddError(field, errorMessage);
                }

                validationResults.AddRange(fieldResults);
            }
            return validationResults;
        }

        private void AddFieldValidators(string field, params IValidator[] validators)
        {
            List<IValidator> fieldValidators = null;
            if (!_validators.TryGetValue(field, out fieldValidators))
            {
                fieldValidators = new List<IValidator>();
                _validators[field] = fieldValidators;
            }
            foreach (var validator in validators)
            {
                fieldValidators.Add(validator);
            }
        }

        private IEnumerable<ValidationResult> ValidateField(string field, ValidationContext context)
        {
            List<IValidator> fieldValidators;
            if (!_validators.TryGetValue(field, out fieldValidators))
            {
                return Enumerable.Empty<ValidationResult>();
            }
            context.MemberName = field;
            return fieldValidators.Select(f => f.Validate(context))
                .Where(result => result != ValidationResult.Success);
        }

        private IEnumerable<ModelClientValidationRule> GetClientValidationRules(string field)
        {
            List<IValidator> fieldValidators = null;
            if (!_validators.TryGetValue(field, out fieldValidators))
            {
                return Enumerable.Empty<ModelClientValidationRule>();
            }

            return from item in fieldValidators
                   let clientRule = item.ClientValidationRule
                   where clientRule != null
                   select clientRule;
        }

        internal static HtmlString GenerateHtmlFromClientValidationRules(IEnumerable<ModelClientValidationRule> clientRules)
        {
            if (!clientRules.Any())
            {
                return null;
            }

            var attributes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            UnobtrusiveValidationAttributesGenerator.GetValidationAttributes(clientRules, attributes);

            var stringBuilder = new StringBuilder();
            foreach (var attribute in attributes)
            {
                string key = attribute.Key;
                string value = HttpUtility.HtmlEncode(Convert.ToString(attribute.Value, CultureInfo.InvariantCulture));
                stringBuilder.Append(key)
                    .Append("=\"")
                    .Append(value)
                    .Append('"')
                    .Append(' ');
            }

            // Trim trailing whitespace
            if (stringBuilder.Length > 0)
            {
                stringBuilder.Length--;
            }

            return new HtmlString(stringBuilder.ToString());
        }
    }
}
