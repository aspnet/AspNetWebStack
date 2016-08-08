// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

#if ASPNETWEBAPI
using System.Web.Http.Controllers;
using System.Web.Http.Properties;
using TActionDescriptor = System.Web.Http.Controllers.HttpActionDescriptor;
using TParsedRoute = System.Web.Http.Routing.HttpParsedRoute;
using TRouteDictionary = System.Web.Http.Routing.HttpRouteValueDictionary;
#else
using System.Text;
using System.Web.Mvc.Properties;
using System.Web.Routing;
using TActionDescriptor = System.Web.Mvc.ActionDescriptor;
using TParsedRoute = System.Web.Mvc.Routing.ParsedRoute;
using TRouteDictionary = System.Web.Routing.RouteValueDictionary;
#endif

#if ASPNETWEBAPI
namespace System.Web.Http.Routing
#else
namespace System.Web.Mvc.Routing
#endif
{
    /// <summary>Represents a context that supports creating a direct route.</summary>
    public class DirectRouteFactoryContext
    {
        private readonly string _actionName;

#if !ASPNETWEBAPI
        private readonly string _controllerName;
#endif

#if ASPNETWEBAPI
        private readonly string _prefix;
#else
        private readonly string _areaPrefix;
        private readonly string _controllerPrefix;
#endif

        private readonly IReadOnlyCollection<TActionDescriptor> _actions;
        private readonly IInlineConstraintResolver _inlineConstraintResolver;
        private readonly bool _targetIsAction;

#if ASPNETWEBAPI
        /// <summary>Initializes a new instance of the <see cref="DirectRouteFactoryContext"/></summary>
        /// <param name="prefix">The route prefix, if any, defined by the controller.</param>
        /// <param name="actions">The action descriptors to which to create a route.</param>
        /// <param name="inlineConstraintResolver">The inline constraint resolver.</param>
        /// <param name="targetIsAction">
        /// A value indicating whether the route is configured at the action or controller level.
        /// </param>
        public DirectRouteFactoryContext(string prefix, IReadOnlyCollection<HttpActionDescriptor> actions,
            IInlineConstraintResolver inlineConstraintResolver, bool targetIsAction)
#else
        /// <summary>Initializes a new instance of the <see cref="DirectRouteFactoryContext"/></summary>
        /// <param name="areaPrefix">The route prefix, if any, defined by the area.</param>
        /// <param name="controllerPrefix">The route prefix, if any, defined by the controller.</param>
        /// <param name="actions">The action descriptors to which to create a route.</param>
        /// <param name="inlineConstraintResolver">The inline constraint resolver.</param>
        /// <param name="targetIsAction">
        /// A value indicating whether the route is configured at the action or controller level.
        /// </param>
        public DirectRouteFactoryContext(string areaPrefix, string controllerPrefix,
            IReadOnlyCollection<ActionDescriptor> actions, IInlineConstraintResolver inlineConstraintResolver,
            bool targetIsAction)
#endif
        {
            if (actions == null)
            {
                throw new ArgumentNullException("actions");
            }

            if (inlineConstraintResolver == null)
            {
                throw new ArgumentNullException("inlineConstraintResolver");
            }

#if ASPNETWEBAPI
            _prefix = prefix;
#else
            _areaPrefix = areaPrefix;
            _controllerPrefix = controllerPrefix;
#endif
            _actions = actions;
            _inlineConstraintResolver = inlineConstraintResolver;

            TActionDescriptor firstDescriptor = actions.FirstOrDefault();

            if (firstDescriptor != null)
            {
                _actionName = firstDescriptor.ActionName;
#if !ASPNETWEBAPI
                ControllerDescriptor controllerDescriptor = firstDescriptor.ControllerDescriptor;

                if (controllerDescriptor != null)
                {
                    _controllerName = controllerDescriptor.ControllerName;
                }
#endif
            }

            _targetIsAction = targetIsAction;
        }

#if ASPNETWEBAPI
        /// <summary>Gets the route prefix, if any, defined by the controller.</summary>
        public string Prefix
        {
            get { return _prefix; }
        }
#else
        /// <summary>Gets the route prefix, if any, defined by the area.</summary>
        public string AreaPrefix
        {
            get { return _areaPrefix; }
        }

        /// <summary>Gets the route prefix, if any, defined by the controller.</summary>
        public string ControllerPrefix
        {
            get { return _controllerPrefix; }
        }
#endif

        /// <summary>Gets the action descriptors to which to create a route.</summary>
        public IReadOnlyCollection<TActionDescriptor> Actions
        {
            get { return _actions; }
        }

        /// <summary>Gets the inline constraint resolver.</summary>
        public IInlineConstraintResolver InlineConstraintResolver
        {
            get { return _inlineConstraintResolver; }
        }

        /// <summary>
        /// Gets a value indicating whether the route is configured at the action or controller level.
        /// </summary>
        /// <remarks>
        /// <see langword="true"/> when the route is configured at the action level; otherwise <see langword="false"/>
        /// (if the route is configured at the controller level).
        /// </remarks>
        public bool TargetIsAction
        {
            get { return _targetIsAction; }
        }

        /// <summary>Creates a route builder that can build a route matching this context.</summary>
        /// <param name="template">The route template.</param>
        /// <returns>A route builder that can build a route matching this context.</returns>
        public IDirectRouteBuilder CreateBuilder(string template)
        {
            return CreateBuilderInternal(template);
        }

        internal virtual IDirectRouteBuilder CreateBuilderInternal(string template)
        {
            return CreateBuilder(template, _inlineConstraintResolver);
        }

        /// <summary>Creates a route builder that can build a route matching this context.</summary>
        /// <param name="template">The route template.</param>
        /// <param name="constraintResolver">
        /// The inline constraint resolver to use, if any; otherwise, <see langword="null"/>.
        /// </param>
        /// <returns>A route builder that can build a route matching this context.</returns>
        public IDirectRouteBuilder CreateBuilder(string template, IInlineConstraintResolver constraintResolver)
        {
            DirectRouteBuilder builder = new DirectRouteBuilder(_actions, _targetIsAction);

#if ASPNETWEBAPI
            string prefixedTemplate = BuildRouteTemplate(_prefix, template);
#else
            string prefixedTemplate = BuildRouteTemplate(_areaPrefix, _controllerPrefix, template ?? String.Empty);
#endif
            ValidateTemplate(prefixedTemplate);

            if (constraintResolver != null)
            {
                TRouteDictionary defaults = new TRouteDictionary();
                TRouteDictionary constraints = new TRouteDictionary();

                string detokenizedTemplate = InlineRouteTemplateParser.ParseRouteTemplate(prefixedTemplate, defaults,
                    constraints, constraintResolver);
                TParsedRoute parsedRoute = RouteParser.Parse(detokenizedTemplate);
                decimal precedence = RoutePrecedence.Compute(parsedRoute, constraints);

                builder.Defaults = defaults;
                builder.Constraints = constraints;
                builder.Template = detokenizedTemplate;
                builder.Precedence = precedence;
                builder.ParsedRoute = parsedRoute;
            }
            else
            {
                builder.Template = prefixedTemplate;
            }

            return builder;
        }

#if ASPNETWEBAPI
        private static string BuildRouteTemplate(string routePrefix, string routeTemplate)
        {
            if (String.IsNullOrEmpty(routeTemplate))
            {
                return routePrefix ?? String.Empty;
            }

            // If the provider's template starts with '~/', ignore the route prefix
            if (routeTemplate.StartsWith("~/", StringComparison.Ordinal))
            {
                return routeTemplate.Substring(2);
            }
            else if (String.IsNullOrEmpty(routePrefix))
            {
                return routeTemplate;
            }
            else
            {
                // template and prefix both not null - combine them
                return routePrefix + '/' + routeTemplate;
            }
        }
#else
        internal static string BuildRouteTemplate(string areaPrefix, string prefix, string template)
        {
            // If the attribute's template starts with '~/', ignore the area and controller prefixes
            if (template != null && template.StartsWith("~/", StringComparison.Ordinal))
            {
                return template.Substring(2);
            }

            if (prefix == null && areaPrefix == null)
            {
                return template;
            }

            StringBuilder templateBuilder = new StringBuilder();

            if (areaPrefix != null)
            {
                templateBuilder.Append(areaPrefix);
            }

            if (!String.IsNullOrEmpty(prefix))
            {
                if (templateBuilder.Length > 0)
                {
                    templateBuilder.Append('/');
                }
                templateBuilder.Append(prefix);
            }

            if (!String.IsNullOrEmpty(template))
            {
                if (templateBuilder.Length > 0)
                {
                    templateBuilder.Append('/');
                }
                templateBuilder.Append(template);
            }

            return templateBuilder.ToString();
        }
#endif

        private void ValidateTemplate(string template)
        {
            if (template != null && template.StartsWith("/", StringComparison.Ordinal))
            {
#if ASPNETWEBAPI
                string errorMessage = Error.Format(SRResources.AttributeRoutes_InvalidTemplate, template, _actionName);
#else
                string errorMessage = Error.Format(MvcResources.RouteTemplate_CannotStart_WithForwardSlash, template,
                    _actionName, _controllerName);
#endif
                throw new InvalidOperationException(errorMessage);
            }
        }
    }
}
