// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.WebPages.Resources;

namespace System.Web.Mvc
{
    public static class UnobtrusiveValidationAttributesGenerator
    {
        public static void GetValidationAttributes(IEnumerable<ModelClientValidationRule> clientRules, IDictionary<string, object> results)
        {
            if (clientRules == null)
            {
                throw new ArgumentNullException("clientRules");
            }
            if (results == null)
            {
                throw new ArgumentNullException("results");
            }

            bool renderedRules = false;

            foreach (ModelClientValidationRule rule in clientRules)
            {
                renderedRules = true;
                string ruleName = "data-val-" + rule.ValidationType;

                ValidateUnobtrusiveValidationRule(rule, results, ruleName);

                results.Add(ruleName, rule.ErrorMessage ?? String.Empty);
                ruleName += "-";

                foreach (var kvp in rule.ValidationParameters)
                {
                    results.Add(ruleName + kvp.Key, kvp.Value ?? String.Empty);
                }
            }

            if (renderedRules)
            {
                results.Add("data-val", "true");
            }
        }

        private static void ValidateUnobtrusiveValidationRule(ModelClientValidationRule rule, IDictionary<string, object> resultsDictionary, string dictionaryKey)
        {
            if (String.IsNullOrWhiteSpace(rule.ValidationType))
            {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        WebPageResources.UnobtrusiveJavascript_ValidationTypeCannotBeEmpty,
                        rule.GetType().FullName));
            }

            if (resultsDictionary.ContainsKey(dictionaryKey))
            {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        WebPageResources.UnobtrusiveJavascript_ValidationTypeMustBeUnique,
                        rule.ValidationType));
            }

            if (rule.ValidationType.Any(c => !Char.IsLower(c)))
            {
                throw new InvalidOperationException(
                    String.Format(CultureInfo.CurrentCulture, WebPageResources.UnobtrusiveJavascript_ValidationTypeMustBeLegal,
                                  rule.ValidationType,
                                  rule.GetType().FullName));
            }

            foreach (var key in rule.ValidationParameters.Keys)
            {
                if (String.IsNullOrWhiteSpace(key))
                {
                    throw new InvalidOperationException(
                        String.Format(
                            CultureInfo.CurrentCulture,
                            WebPageResources.UnobtrusiveJavascript_ValidationParameterCannotBeEmpty,
                            rule.GetType().FullName));
                }

                if (!Char.IsLower(key.First()) || key.Any(c => !Char.IsLower(c) && !Char.IsDigit(c)))
                {
                    throw new InvalidOperationException(
                        String.Format(
                            CultureInfo.CurrentCulture,
                            WebPageResources.UnobtrusiveJavascript_ValidationParameterMustBeLegal,
                            key,
                            rule.GetType().FullName));
                }
            }
        }
    }
}
