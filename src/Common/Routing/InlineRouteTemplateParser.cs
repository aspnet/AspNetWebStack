// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;
#if ASPNETWEBAPI
using System.Web.Http.Routing.Constraints;
using ErrorResources = System.Web.Http.Properties.SRResources;
#else
using System.Web.Mvc.Routing.Constraints;
using System.Web.Routing;
using ErrorResources = System.Web.Mvc.Properties.MvcResources;
#endif

#if ASPNETWEBAPI
namespace System.Web.Http.Routing
#else
namespace System.Web.Mvc.Routing
#endif
{
    /// <summary>
    /// This class is used by the AttributeRouting mapper, during configuration,
    /// to parse and strip defaults and constraints from the template.
    /// </summary>
    internal class InlineRouteTemplateParser
    {
        // One or more characters, matches "id"
        private const string ParameterNameRegex = @"(?<parameterName>.+?)";

        // Zero or more inline constraints that start with a colon followed by zero or more characters
        // Optionally the constraint can have arguments within parentheses - necessary to capture characters like ":" and "}"
        // Matches ":int", ":length(2)", ":regex(\})", ":regex(:)" zero or more times
        private const string ConstraintRegex = @"(:(?<constraint>.*?(\(.*?\))?))*";

        // Optional "?" for optional parameters or a default value with an equal sign followed by zero or more characters
        // Matches "?", "=", "=abc"
        private const string DefaultValueRegex = @"(?<defaultValue>\?|(=.*?))?";

        private static readonly Regex _parameterRegex = new Regex(
            "{" + ParameterNameRegex + ConstraintRegex + DefaultValueRegex + "}",
            RegexOptions.Compiled);

        public static string ParseRouteTemplate(string routeTemplate, IDictionary<string, object> defaults, IDictionary<string, object> constraints, IInlineConstraintResolver constraintResolver)
        {
            Contract.Assert(defaults != null);
            Contract.Assert(constraints != null);

            MatchCollection parameterMatches = _parameterRegex.Matches(routeTemplate);

            foreach (Match parameterMatch in parameterMatches)
            {
                string parameterName = parameterMatch.Groups["parameterName"].Value;
                // We may need to strip out the initial wildcard used for wildcard parameters
                if (parameterName.StartsWith("*", StringComparison.OrdinalIgnoreCase))
                {
                    parameterName = parameterName.Substring(1);
                }

                // Add the default value if present
                Group defaultValueGroup = parameterMatch.Groups["defaultValue"];
                object defaultValue = GetDefaultValue(defaultValueGroup);
                if (defaultValue != null)
                {
                    defaults.Add(parameterName, defaultValue);
                }

                // Register inline constraints if present
                Group constraintGroup = parameterMatch.Groups["constraint"];
#if ASPNETWEBAPI
                bool isOptional = defaultValue == RouteParameter.Optional;
#else
                bool isOptional = defaultValue == UrlParameter.Optional;
#endif
                var constraint = GetInlineConstraint(constraintGroup, isOptional, constraintResolver);
                if (constraint != null)
                {
                    constraints.Add(parameterName, constraint);
                }
            }

            // Replaces parameter matches with just the parameter name in braces
            // Strips out the optional '?', default value, inline constraints
            return _parameterRegex.Replace(routeTemplate, @"{${parameterName}}");
        }

        private static object GetDefaultValue(Group defaultValueGroup)
        {
            if (defaultValueGroup.Success)
            {
                string defaultValueMatch = defaultValueGroup.Value;
                if (defaultValueMatch == "?")
                {
#if ASPNETWEBAPI
                    return RouteParameter.Optional;
#else
                    return UrlParameter.Optional;
#endif
                }
                else
                {
                    // Strip out the equal sign at the beginning
                    Contract.Assert(defaultValueMatch.StartsWith("=", StringComparison.Ordinal));
                    return defaultValueMatch.Substring(1);
                }
            }
            return null;
        }

#if ASPNETWEBAPI
        private static IHttpRouteConstraint GetInlineConstraint(Group constraintGroup, bool isOptional, IInlineConstraintResolver constraintResolver)
#else
        private static IRouteConstraint GetInlineConstraint(Group constraintGroup, bool isOptional, IInlineConstraintResolver constraintResolver)
#endif
        {
#if ASPNETWEBAPI
            List<IHttpRouteConstraint> parameterConstraints = new List<IHttpRouteConstraint>();
#else
            List<IRouteConstraint> parameterConstraints = new List<IRouteConstraint>();
#endif
            foreach (Capture constraintCapture in constraintGroup.Captures)
            {
                string inlineConstraint = constraintCapture.Value;
                var constraint = constraintResolver.ResolveConstraint(inlineConstraint);
                if (constraint == null)
                {
                    throw Error.InvalidOperation(ErrorResources.HttpRouteBuilder_CouldNotResolveConstraint, constraintResolver.GetType().Name, inlineConstraint);
                }
                parameterConstraints.Add(constraint);
            }

            if (parameterConstraints.Count > 0)
            {
                var constraint = parameterConstraints.Count == 1 ?
                    parameterConstraints[0] :
                    new CompoundRouteConstraint(parameterConstraints);

                if (isOptional)
                {
                    // Constraints should match RouteParameter.Optional if the parameter is optional
                    // This prevents contraining when there's no value specified
                    constraint = new OptionalRouteConstraint(constraint);
                }

                return constraint;
            }
            return null;
        }
    }
}