// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Web.Routing;
using System.Web.WebPages.Scope;

namespace System.Web.WebPages.Html
{
    public partial class HtmlHelper
    {
        internal const string DefaultValidationInputErrorCssClass = "input-validation-error";
        private const string DefaultValidationInputValidCssClass = "input-validation-valid";
        private const string DefaultValidationMessageErrorCssClass = "field-validation-error";
        private const string DefaultValidationMessageValidCssClass = "field-validation-valid";
        private const string DefaultValidationSummaryErrorCssClass = "validation-summary-errors";
        private const string DefaultValidationSummaryValidCssClassName = "validation-summary-valid";
        private static readonly object _validationMesssageErrorClassKey = new object();
        private static readonly object _validationMessageValidClassKey = new object();
        private static readonly object _validationInputErrorClassKey = new object();
        private static readonly object _validationInputValidClassKey = new object();
        private static readonly object _validationSummaryClassKey = new object();
        private static readonly object _validationSummaryValidClassKey = new object();
        private static readonly object _unobtrusiveValidationKey = new object();
        private static string _idAttributeDotReplacement;
        private readonly ValidationHelper _validationHelper;

        internal HtmlHelper(ModelStateDictionary modelState, ValidationHelper validationHelper)
        {
            ModelState = modelState;
            _validationHelper = validationHelper;
        }

        // This property got copied from MVC's HtmlHelper along with TagBuilder.
        // It was a global property in MVC so it should not have scoped semantics here either.
        public static string IdAttributeDotReplacement
        {
            get
            {
                if (String.IsNullOrEmpty(_idAttributeDotReplacement))
                {
                    _idAttributeDotReplacement = "_";
                }
                return _idAttributeDotReplacement;
            }
            set { _idAttributeDotReplacement = value; }
        }

        public static string ValidationInputValidCssClassName
        {
            get { return ScopeStorage.CurrentScope[_validationInputValidClassKey] as string ?? DefaultValidationInputValidCssClass; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                ScopeStorage.CurrentScope[_validationInputValidClassKey] = value;
            }
        }

        public static string ValidationInputCssClassName
        {
            get { return ScopeStorage.CurrentScope[_validationInputErrorClassKey] as string ?? DefaultValidationInputErrorCssClass; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                ScopeStorage.CurrentScope[_validationInputErrorClassKey] = value;
            }
        }

        public static string ValidationMessageValidCssClassName
        {
            get { return ScopeStorage.CurrentScope[_validationMessageValidClassKey] as string ?? DefaultValidationMessageValidCssClass; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                ScopeStorage.CurrentScope[_validationMessageValidClassKey] = value;
            }
        }

        public static string ValidationMessageCssClassName
        {
            get { return ScopeStorage.CurrentScope[_validationMesssageErrorClassKey] as string ?? DefaultValidationMessageErrorCssClass; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                ScopeStorage.CurrentScope[_validationMesssageErrorClassKey] = value;
            }
        }

        public static string ValidationSummaryClass
        {
            get { return ScopeStorage.CurrentScope[_validationSummaryClassKey] as string ?? DefaultValidationSummaryErrorCssClass; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                ScopeStorage.CurrentScope[_validationSummaryClassKey] = value;
            }
        }

        public static string ValidationSummaryValidClass
        {
            get { return ScopeStorage.CurrentScope[_validationSummaryValidClassKey] as string ?? DefaultValidationSummaryValidCssClassName; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                ScopeStorage.CurrentScope[_validationSummaryValidClassKey] = value;
            }
        }

        public static bool UnobtrusiveJavaScriptEnabled
        {
            get
            {
                bool? value = (bool?)ScopeStorage.CurrentScope[_unobtrusiveValidationKey];
                return value ?? true;
            }
            set { ScopeStorage.CurrentScope[_unobtrusiveValidationKey] = value; }
        }

        private ModelStateDictionary ModelState { get; set; }

        public string AttributeEncode(object value)
        {
            return AttributeEncode(Convert.ToString(value, CultureInfo.InvariantCulture));
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "For consistency, all helpers are instance methods.")]
        public string AttributeEncode(string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return String.Empty;
            }
            else
            {
                return HttpUtility.HtmlAttributeEncode(value);
            }
        }

        public string Encode(object value)
        {
            return Encode(Convert.ToString(value, CultureInfo.InvariantCulture));
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "For consistency, all helpers are instance methods.")]
        public string Encode(string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return String.Empty;
            }
            else
            {
                return HttpUtility.HtmlEncode(value);
            }
        }

        /// <summary>
        /// Wraps HTML markup in an IHtmlString, which will enable HTML markup to be
        /// rendered to the output without getting HTML encoded.
        /// </summary>
        /// <param name="value">HTML markup string.</param>
        /// <returns>An IHtmlString that represents HTML markup.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "For consistency, all helpers are instance methods.")]
        public IHtmlString Raw(string value)
        {
            return new HtmlString(value);
        }

        /// <summary>
        /// Wraps HTML markup from the string representation of an object in an IHtmlString,
        /// which will enable HTML markup to be rendered to the output without getting HTML encoded.
        /// </summary>
        /// <param name="value">object with string representation as HTML markup</param>
        /// <returns>An IHtmlString that represents HTML markup.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "For consistency, all helpers are instance methods.")]
        public IHtmlString Raw(object value)
        {
            return new HtmlString(value == null ? null : value.ToString());
        }

        /// <summary>
        /// Creates a dictionary of HTML attributes from the input object, 
        /// translating underscores to dashes.
        /// </summary>
        /// <example>
        /// new <c>{ data_name="value" }</c> will translate to the entry <c>{ "data-name" , "value" }</c>
        /// in the resulting dictionary.
        /// </example>
        /// <param name="htmlAttributes">Anonymous object describing HTML attributes.</param>
        /// <returns>A dictionary that represents HTML attributes.</returns>
        public static RouteValueDictionary AnonymousObjectToHtmlAttributes(object htmlAttributes)
        {
            RouteValueDictionary result = new RouteValueDictionary();

            if (htmlAttributes != null)
            {
                foreach (PropertyHelper property in HtmlAttributePropertyHelper.GetProperties(htmlAttributes))
                {
                    result.Add(property.Name, property.GetValue(htmlAttributes));
                }
            }

            return result;
        }

        /// <summary>
        /// Creates a dictionary from an object, by adding each public instance property as a key with its associated 
        /// value to the dictionary. It will expose public properties from derived types as well. This is typically used
        /// with objects of an anonymous type.
        /// </summary>
        /// <example>
        /// <c>new { property_name = "value" }</c> will translate to the entry <c>{ "property_name" , "value" }</c>
        /// in the resulting dictionary.
        /// </example>
        /// <param name="value">The object to be converted.</param>
        /// <returns>The created dictionary of property names and property values.</returns>
        public static IDictionary<string, object> ObjectToDictionary(object value)
        {
            return TypeHelper.ObjectToDictionary(value);
        }
    }
}
