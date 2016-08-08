// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Internal;
using System.Web.Http.ModelBinding.Binders;
using System.Web.Http.Routing;
using System.Web.Http.Services;

namespace System.Web.Http.Description
{
    /// <summary>
    /// Explores the URI space of the service based on routes, controllers and actions available in the system.
    /// </summary>
    public class ApiExplorer : IApiExplorer
    {
        private Lazy<Collection<ApiDescription>> _apiDescriptions;
        private readonly HttpConfiguration _config;
        private static readonly Regex _actionVariableRegex = new Regex(String.Format(CultureInfo.CurrentCulture, "{{{0}}}", RouteValueKeys.Action), RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex _controllerVariableRegex = new Regex(String.Format(CultureInfo.CurrentCulture, "{{{0}}}", RouteValueKeys.Controller), RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiExplorer"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public ApiExplorer(HttpConfiguration configuration)
        {
            _config = configuration;
            _apiDescriptions = new Lazy<Collection<ApiDescription>>(InitializeApiDescriptions);
        }

        /// <summary>
        /// Gets the API descriptions. The descriptions are initialized on the first access.
        /// </summary>
        public Collection<ApiDescription> ApiDescriptions
        {
            get
            {
                return _apiDescriptions.Value;
            }
        }

        /// <summary>
        /// Gets or sets the documentation provider. The provider will be responsible for documenting the API.
        /// </summary>
        /// <value>
        /// The documentation provider.
        /// </value>
        public IDocumentationProvider DocumentationProvider { get; set; }

        /// <summary>
        /// Determines whether the controller should be considered for <see cref="ApiExplorer.ApiDescriptions"/> generation. Called when initializing the <see cref="ApiExplorer.ApiDescriptions"/>.
        /// </summary>
        /// <param name="controllerVariableValue">The controller variable value from the route.</param>
        /// <param name="controllerDescriptor">The controller descriptor.</param>
        /// <param name="route">The route.</param>
        /// <returns><c>true</c> if the controller should be considered for <see cref="ApiExplorer.ApiDescriptions"/> generation, <c>false</c> otherwise.</returns>
        public virtual bool ShouldExploreController(string controllerVariableValue, HttpControllerDescriptor controllerDescriptor, IHttpRoute route)
        {
            if (controllerDescriptor == null)
            {
                throw Error.ArgumentNull("controllerDescriptor");
            }

            if (route == null)
            {
                throw Error.ArgumentNull("route");
            }

            ApiExplorerSettingsAttribute setting = controllerDescriptor.GetCustomAttributes<ApiExplorerSettingsAttribute>().FirstOrDefault();
            return (setting == null || !setting.IgnoreApi) &&
                MatchRegexConstraint(route, RouteValueKeys.Controller, controllerVariableValue);
        }

        /// <summary>
        /// Determines whether the action should be considered for <see cref="ApiExplorer.ApiDescriptions"/> generation. Called when initializing the <see cref="ApiExplorer.ApiDescriptions"/>.
        /// </summary>
        /// <param name="actionVariableValue">The action variable value from the route.</param>
        /// <param name="actionDescriptor">The action descriptor.</param>
        /// <param name="route">The route.</param>
        /// <returns><c>true</c> if the action should be considered for <see cref="ApiExplorer.ApiDescriptions"/> generation, <c>false</c> otherwise.</returns>
        public virtual bool ShouldExploreAction(string actionVariableValue, HttpActionDescriptor actionDescriptor, IHttpRoute route)
        {
            if (actionDescriptor == null)
            {
                throw Error.ArgumentNull("actionDescriptor");
            }

            if (route == null)
            {
                throw Error.ArgumentNull("route");
            }

            ApiExplorerSettingsAttribute setting = actionDescriptor.GetCustomAttributes<ApiExplorerSettingsAttribute>().FirstOrDefault();
            return (setting == null || !setting.IgnoreApi) &&
                MatchRegexConstraint(route, RouteValueKeys.Action, actionVariableValue);
        }

        /// <summary>
        /// Gets a collection of HttpMethods supported by the action. Called when initializing the <see cref="ApiExplorer.ApiDescriptions"/>.
        /// </summary>
        /// <param name="route">The route.</param>
        /// <param name="actionDescriptor">The action descriptor.</param>
        /// <returns>A collection of HttpMethods supported by the action.</returns>
        public virtual Collection<HttpMethod> GetHttpMethodsSupportedByAction(IHttpRoute route, HttpActionDescriptor actionDescriptor)
        {
            if (route == null)
            {
                throw Error.ArgumentNull("route");
            }

            if (actionDescriptor == null)
            {
                throw Error.ArgumentNull("actionDescriptor");
            }

            IList<HttpMethod> supportedMethods = new List<HttpMethod>();
            IList<HttpMethod> actionHttpMethods = actionDescriptor.SupportedHttpMethods;
            HttpMethodConstraint httpMethodConstraint = route.Constraints.Values.FirstOrDefault(c => typeof(HttpMethodConstraint).IsAssignableFrom(c.GetType())) as HttpMethodConstraint;

            if (httpMethodConstraint == null)
            {
                supportedMethods = actionHttpMethods;
            }
            else
            {
                supportedMethods = httpMethodConstraint.AllowedMethods.Intersect(actionHttpMethods).ToList();
            }

            return new Collection<HttpMethod>(supportedMethods);
        }

        private IEnumerable<IHttpRoute> FlattenRoutes(IEnumerable<IHttpRoute> routes)
        {
            foreach (IHttpRoute route in routes)
            {
                IEnumerable<IHttpRoute> nested = route as IEnumerable<IHttpRoute>;
                if (nested != null)
                {
                    foreach (var subRoute in FlattenRoutes(nested))
                    {
                        yield return subRoute;
                    }
                }
                else
                {
                    yield return route;
                }
            }
        }

        private static HttpControllerDescriptor GetDirectRouteController(CandidateAction[] directRouteCandidates)
        {
            if (directRouteCandidates != null)
            {
                // Set the controller descriptor for the first action descriptor
                HttpControllerDescriptor controllerDescriptor = directRouteCandidates[0].ActionDescriptor.ControllerDescriptor;

                // Check that all other action descriptors share the same controller descriptor
                for (int i = 1; i < directRouteCandidates.Length; i++)
                {
                    if (directRouteCandidates[i].ActionDescriptor.ControllerDescriptor != controllerDescriptor)
                    {
                        // This can happen if a developer puts the same route template on different actions
                        // in different controllers.
                        return null;
                    }
                }

                return controllerDescriptor;
            }

            return null;
        }

        private Collection<ApiDescription> InitializeApiDescriptions()
        {
            Collection<ApiDescription> apiDescriptions = new Collection<ApiDescription>();
            IHttpControllerSelector controllerSelector = _config.Services.GetHttpControllerSelector();
            IDictionary<string, HttpControllerDescriptor> controllerMappings = controllerSelector.GetControllerMapping();
            if (controllerMappings != null)
            {
                ApiDescriptionComparer descriptionComparer = new ApiDescriptionComparer();
                foreach (IHttpRoute route in FlattenRoutes(_config.Routes))
                {
                    CandidateAction[] directRouteCandidates = route.GetDirectRouteCandidates();

                    HttpControllerDescriptor directRouteController = GetDirectRouteController(directRouteCandidates);
                    Collection<ApiDescription> descriptionsFromRoute =
                        (directRouteController != null && directRouteCandidates != null) ?
                            ExploreDirectRoute(directRouteController, directRouteCandidates, route) :
                            ExploreRouteControllers(controllerMappings, route);

                    // Remove ApiDescription that will lead to ambiguous action matching.
                    // E.g. a controller with Post() and PostComment(). When the route template is {controller}, it produces POST /controller and POST /controller.
                    descriptionsFromRoute = RemoveInvalidApiDescriptions(descriptionsFromRoute);

                    foreach (ApiDescription description in descriptionsFromRoute)
                    {
                        // Do not add the description if the previous route has a matching description with the same HTTP method and relative path.
                        // E.g. having two routes with the templates "api/Values/{id}" and "api/{controller}/{id}" can potentially produce the same
                        // relative path "api/Values/{id}" but only the first one matters.
                        if (!apiDescriptions.Contains(description, descriptionComparer))
                        {
                            apiDescriptions.Add(description);
                        }
                    }
                }
            }

            return apiDescriptions;
        }

        private Collection<ApiDescription> ExploreDirectRoute(HttpControllerDescriptor controllerDescriptor, CandidateAction[] candidates, IHttpRoute route)
        {
            Collection<ApiDescription> descriptions = new Collection<ApiDescription>();

            if (ShouldExploreController(controllerDescriptor.ControllerName, controllerDescriptor, route))
            {
                foreach (CandidateAction action in candidates)
                {
                    HttpActionDescriptor actionDescriptor = action.ActionDescriptor;
                    string actionName = actionDescriptor.ActionName;

                    if (ShouldExploreAction(actionName, actionDescriptor, route))
                    {
                        string routeTemplate = route.RouteTemplate;
                        if (_actionVariableRegex.IsMatch(routeTemplate))
                        {
                            // expand {action} variable
                            routeTemplate = _actionVariableRegex.Replace(routeTemplate, actionName);
                        }

                        PopulateActionDescriptions(actionDescriptor, route, routeTemplate, descriptions);
                    }
                }
            }

            return descriptions;
        }

        private Collection<ApiDescription> ExploreRouteControllers(IDictionary<string, HttpControllerDescriptor> controllerMappings, IHttpRoute route)
        {
            Collection<ApiDescription> apiDescriptions = new Collection<ApiDescription>();
            string routeTemplate = route.RouteTemplate;
            string controllerVariableValue;
            if (_controllerVariableRegex.IsMatch(routeTemplate))
            {
                // unbound controller variable, {controller}
                foreach (KeyValuePair<string, HttpControllerDescriptor> controllerMapping in controllerMappings)
                {
                    controllerVariableValue = controllerMapping.Key;
                    HttpControllerDescriptor controllerDescriptor = controllerMapping.Value;
                    if (ShouldExploreController(controllerVariableValue, controllerDescriptor, route))
                    {
                        // expand {controller} variable
                        string expandedRouteTemplate = _controllerVariableRegex.Replace(routeTemplate, controllerVariableValue);
                        ExploreRouteActions(route, expandedRouteTemplate, controllerDescriptor, apiDescriptions);
                    }
                }
            }
            else if (route.Defaults.TryGetValue(RouteValueKeys.Controller, out controllerVariableValue))
            {
                // bound controller variable, {controller = "controllerName"}
                HttpControllerDescriptor controllerDescriptor;
                if (controllerMappings.TryGetValue(controllerVariableValue, out controllerDescriptor) && ShouldExploreController(controllerVariableValue, controllerDescriptor, route))
                {
                    ExploreRouteActions(route, routeTemplate, controllerDescriptor, apiDescriptions);
                }
            }

            return apiDescriptions;
        }

        private void ExploreRouteActions(IHttpRoute route, string localPath, HttpControllerDescriptor controllerDescriptor, Collection<ApiDescription> apiDescriptions)
        {
            // exclude controllers that are marked with route attributes.
            if (!controllerDescriptor.IsAttributeRouted())
            {
                ServicesContainer controllerServices = controllerDescriptor.Configuration.Services;
                ILookup<string, HttpActionDescriptor> actionMappings = controllerServices.GetActionSelector().GetActionMapping(controllerDescriptor);
                string actionVariableValue;
                if (actionMappings != null)
                {
                    if (_actionVariableRegex.IsMatch(localPath))
                    {
                        // unbound action variable, {action}
                        foreach (IGrouping<string, HttpActionDescriptor> actionMapping in actionMappings)
                        {
                            // expand {action} variable
                            actionVariableValue = actionMapping.Key;
                            string expandedLocalPath = _actionVariableRegex.Replace(localPath, actionVariableValue);
                            PopulateActionDescriptions(actionMapping, actionVariableValue, route, expandedLocalPath, apiDescriptions);
                        }
                    }
                    else if (route.Defaults.TryGetValue(RouteValueKeys.Action, out actionVariableValue))
                    {
                        // bound action variable, { action = "actionName" }
                        PopulateActionDescriptions(actionMappings[actionVariableValue], actionVariableValue, route, localPath, apiDescriptions);
                    }
                    else
                    {
                        // no {action} specified, e.g. {controller}/{id}
                        foreach (IGrouping<string, HttpActionDescriptor> actionMapping in actionMappings)
                        {
                            PopulateActionDescriptions(actionMapping, null, route, localPath, apiDescriptions);
                        }
                    }
                }
            }
        }

        private void PopulateActionDescriptions(IEnumerable<HttpActionDescriptor> actionDescriptors, string actionVariableValue, IHttpRoute route, string localPath, Collection<ApiDescription> apiDescriptions)
        {
            foreach (HttpActionDescriptor actionDescriptor in actionDescriptors)
            {
                if (ShouldExploreAction(actionVariableValue, actionDescriptor, route))
                {
                    // exclude actions that are marked with route attributes except for the inherited actions.
                    if (!actionDescriptor.IsAttributeRouted())
                    {
                        PopulateActionDescriptions(actionDescriptor, route, localPath, apiDescriptions);
                    }
                }
            }
        }

        private void PopulateActionDescriptions(HttpActionDescriptor actionDescriptor, IHttpRoute route, string localPath, Collection<ApiDescription> apiDescriptions)
        {
            string apiDocumentation = GetApiDocumentation(actionDescriptor);

            HttpParsedRoute parsedRoute = RouteParser.Parse(localPath);

            // parameters
            IList<ApiParameterDescription> parameterDescriptions = CreateParameterDescriptions(actionDescriptor, parsedRoute, route.Defaults);

            // expand all parameter variables
            string finalPath;

            if (!TryExpandUriParameters(route, parsedRoute, parameterDescriptions, out finalPath))
            {
                // the action cannot be reached due to parameter mismatch, e.g. routeTemplate = "/users/{name}" and GetUsers(id)
                return;
            }

            // request formatters
            ApiParameterDescription bodyParameter = parameterDescriptions.FirstOrDefault(description => description.Source == ApiParameterSource.FromBody);
            IEnumerable<MediaTypeFormatter> supportedRequestBodyFormatters = bodyParameter != null ?
                actionDescriptor.Configuration.Formatters.Where(f => f.CanReadType(bodyParameter.ParameterDescriptor.ParameterType)) :
                Enumerable.Empty<MediaTypeFormatter>();

            // response formatters
            ResponseDescription responseDescription = CreateResponseDescription(actionDescriptor);
            Type returnType = responseDescription.ResponseType ?? responseDescription.DeclaredType;
            IEnumerable<MediaTypeFormatter> supportedResponseFormatters = (returnType != null && returnType != typeof(void)) ?
                actionDescriptor.Configuration.Formatters.Where(f => f.CanWriteType(returnType)) :
                Enumerable.Empty<MediaTypeFormatter>();

            // Replacing the formatter tracers with formatters if tracers are present.
            supportedRequestBodyFormatters = GetInnerFormatters(supportedRequestBodyFormatters);
            supportedResponseFormatters = GetInnerFormatters(supportedResponseFormatters);

            // get HttpMethods supported by an action. Usually there is one HttpMethod per action but we allow multiple of them per action as well.
            IList<HttpMethod> supportedMethods = GetHttpMethodsSupportedByAction(route, actionDescriptor);

            foreach (HttpMethod method in supportedMethods)
            {
                apiDescriptions.Add(new ApiDescription
                {
                    Documentation = apiDocumentation,
                    HttpMethod = method,
                    RelativePath = finalPath,
                    ActionDescriptor = actionDescriptor,
                    Route = route,
                    SupportedResponseFormatters = new Collection<MediaTypeFormatter>(supportedResponseFormatters.ToList()),
                    SupportedRequestBodyFormatters = new Collection<MediaTypeFormatter>(supportedRequestBodyFormatters.ToList()),
                    ParameterDescriptions = new Collection<ApiParameterDescription>(parameterDescriptions),
                    ResponseDescription = responseDescription
                });
            }
        }

        private ResponseDescription CreateResponseDescription(HttpActionDescriptor actionDescriptor)
        {
            Collection<ResponseTypeAttribute> responseTypeAttribute = actionDescriptor.GetCustomAttributes<ResponseTypeAttribute>();
            Type responseType = responseTypeAttribute.Select(attribute => attribute.ResponseType).FirstOrDefault();

            return new ResponseDescription
            {
                DeclaredType = actionDescriptor.ReturnType,
                ResponseType = responseType,
                Documentation = GetApiResponseDocumentation(actionDescriptor)
            };
        }

        private static IEnumerable<MediaTypeFormatter> GetInnerFormatters(IEnumerable<MediaTypeFormatter> mediaTypeFormatters)
        {
            foreach (MediaTypeFormatter formatter in mediaTypeFormatters)
            {
                yield return Decorator.GetInner(formatter);
            }
        }

        private static bool ShouldEmitPrefixes(ICollection<ApiParameterDescription> parameterDescriptions)
        {
            // Determine if there are two or more complex objects from the Uri so TryExpandUriParameters needs to emit prefixes.
            return parameterDescriptions.Count(parameter =>
                        parameter.Source == ApiParameterSource.FromUri &&
                        parameter.ParameterDescriptor != null &&
                        !TypeHelper.CanConvertFromString(parameter.ParameterDescriptor.ParameterType) &&
                        parameter.CanConvertPropertiesFromString()) > 1;
        }

        // Set as internal for the unit test.
        internal static bool TryExpandUriParameters(IHttpRoute route, HttpParsedRoute parsedRoute, ICollection<ApiParameterDescription> parameterDescriptions, out string expandedRouteTemplate)
        {
            Dictionary<string, object> parameterValuesForRoute = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            bool emitPrefixes = ShouldEmitPrefixes(parameterDescriptions);
            string prefix = String.Empty;
            foreach (ApiParameterDescription parameterDescription in parameterDescriptions)
            {
                if (parameterDescription.Source == ApiParameterSource.FromUri)
                {
                    if (parameterDescription.ParameterDescriptor == null)
                    {
                        // Undeclared route parameter handling generates query string like
                        // "?name={name}"
                        AddPlaceholder(parameterValuesForRoute, parameterDescription.Name);
                    }
                    else if (TypeHelper.CanConvertFromString(parameterDescription.ParameterDescriptor.ParameterType))
                    {
                        // Simple type generates query string like
                        // "?name={name}"
                        AddPlaceholder(parameterValuesForRoute, parameterDescription.Name);
                    }
                    else if (IsBindableCollection(parameterDescription.ParameterDescriptor.ParameterType))
                    {
                        string parameterName = parameterDescription.ParameterDescriptor.ParameterName;
                        Type innerType =
                            GetCollectionElementType(parameterDescription.ParameterDescriptor.ParameterType);
                        PropertyInfo[] innerTypeProperties =
                            ApiParameterDescription.GetBindableProperties(innerType).ToArray();
                        if (innerTypeProperties.Any())
                        {
                            // Complex array and collection generate query string like
                            // "?name[0].foo={name[0].foo}&name[0].bar={name[0].bar}
                            //  &name[1].foo={name[1].foo}&name[1].bar={name[1].bar}"
                            AddPlaceholderForProperties(parameterValuesForRoute,
                                                        innerTypeProperties,
                                                        parameterName + "[0].");
                            AddPlaceholderForProperties(parameterValuesForRoute,
                                                        innerTypeProperties,
                                                        parameterName + "[1].");
                        }
                        else
                        {
                            // Simple array and collection generate query string like
                            // "?name[0]={name[0]}&name[1]={name[1]}".
                            AddPlaceholder(parameterValuesForRoute, parameterName + "[0]");
                            AddPlaceholder(parameterValuesForRoute, parameterName + "[1]");
                        }
                    }
                    else if (IsBindableKeyValuePair(parameterDescription.ParameterDescriptor.ParameterType))
                    {
                        // KeyValuePair generates query string like
                        // "?key={key}&value={value}"
                        AddPlaceholder(parameterValuesForRoute, "key");
                        AddPlaceholder(parameterValuesForRoute, "value");
                    }
                    else if (IsBindableDictionry(parameterDescription.ParameterDescriptor.ParameterType))
                    {
                        // Dictionary generates query string like
                        // "?dict[0].key={dict[0].key}&dict[0].value={dict[0].value}
                        //  &dict[1].key={dict[1].key}&dict[1].value={dict[1].value}"
                        string parameterName = parameterDescription.ParameterDescriptor.ParameterName;
                        AddPlaceholder(parameterValuesForRoute, parameterName + "[0].key");
                        AddPlaceholder(parameterValuesForRoute, parameterName + "[0].value");
                        AddPlaceholder(parameterValuesForRoute, parameterName + "[1].key");
                        AddPlaceholder(parameterValuesForRoute, parameterName + "[1].value");
                    }
                    else if (parameterDescription.CanConvertPropertiesFromString())
                    {
                        if (emitPrefixes)
                        {
                            prefix = parameterDescription.Name + ".";
                        }

                        // Inserting the individual properties of the object in the query string
                        // as all the complex object can not be converted from string, but all its
                        // individual properties can.
                        AddPlaceholderForProperties(parameterValuesForRoute,
                                                    parameterDescription.GetBindableProperties(),
                                                    prefix);
                    }
                }
            }

            BoundRouteTemplate boundRouteTemplate = parsedRoute.Bind(null, parameterValuesForRoute, new HttpRouteValueDictionary(route.Defaults), new HttpRouteValueDictionary(route.Constraints));
            if (boundRouteTemplate == null)
            {
                expandedRouteTemplate = null;
                return false;
            }

            expandedRouteTemplate = Uri.UnescapeDataString(boundRouteTemplate.BoundTemplate);
            return true;
        }

        private static Type GetCollectionElementType(Type collectionType)
        {
            Contract.Assert(!typeof(IDictionary).IsAssignableFrom(collectionType));

            Type elementType = collectionType.GetElementType();
            if (elementType == null)
            {
                elementType = CollectionModelBinderUtil
                    .GetGenericBinderTypeArgs(typeof(ICollection<>), collectionType)
                    .First();
            }
            return elementType;
        }

        private static void AddPlaceholderForProperties(Dictionary<string, object> parameterValuesForRoute,
                                                        IEnumerable<PropertyInfo> properties,
                                                        string prefix)
        {
            foreach (PropertyInfo property in properties)
            {
                string queryParameterName = prefix + property.Name;
                AddPlaceholder(parameterValuesForRoute, queryParameterName);
            }
        }

        private static bool IsBindableCollection(Type type)
        {
            Contract.Assert(type != null);

            return type.IsArray || new CollectionModelBinderProvider().GetBinder(null, type) != null;
        }

        private static bool IsBindableDictionry(Type type)
        {
            Contract.Assert(type != null);

            return new DictionaryModelBinderProvider().GetBinder(null, type) != null;
        }

        private static bool IsBindableKeyValuePair(Type type)
        {
            Contract.Assert(type != null);

            return TypeHelper.GetTypeArgumentsIfMatch(type, typeof(KeyValuePair<,>)) != null;
        }

        private static void AddPlaceholder(Dictionary<string, object> parameterValuesForRoute,
                                          string queryParameterName)
        {
            if (!parameterValuesForRoute.ContainsKey(queryParameterName))
            {
                parameterValuesForRoute.Add(queryParameterName, "{" + queryParameterName + "}");
            }
        }

        private IList<ApiParameterDescription> CreateParameterDescriptions(HttpActionDescriptor actionDescriptor, HttpParsedRoute parsedRoute, IDictionary<string, object> routeDefaults)
        {
            IList<ApiParameterDescription> parameterDescriptions = new List<ApiParameterDescription>();
            HttpActionBinding actionBinding = GetActionBinding(actionDescriptor);

            // try get parameter binding information if available
            if (actionBinding != null)
            {
                HttpParameterBinding[] parameterBindings = actionBinding.ParameterBindings;
                if (parameterBindings != null)
                {
                    foreach (HttpParameterBinding parameter in parameterBindings)
                    {
                        parameterDescriptions.Add(CreateParameterDescriptionFromBinding(parameter));
                    }
                }
            }
            else
            {
                Collection<HttpParameterDescriptor> parameters = actionDescriptor.GetParameters();
                if (parameters != null)
                {
                    foreach (HttpParameterDescriptor parameter in parameters)
                    {
                        parameterDescriptions.Add(CreateParameterDescriptionFromDescriptor(parameter));
                    }
                }
            }

            // Adding route parameters not declared on the action. We're doing this because route parameters may or
            // may not be part of the action parameters and we want to have them in the description.
            AddUndeclaredRouteParameters(parsedRoute, routeDefaults, parameterDescriptions);

            return parameterDescriptions;
        }

        private static void AddUndeclaredRouteParameters(HttpParsedRoute parsedRoute, IDictionary<string, object> routeDefaults, IList<ApiParameterDescription> parameterDescriptions)
        {
            foreach (PathSegment path in parsedRoute.PathSegments)
            {
                PathContentSegment content = path as PathContentSegment;
                if (content != null)
                {
                    foreach (PathSubsegment subSegment in content.Subsegments)
                    {
                        PathParameterSubsegment parameter = subSegment as PathParameterSubsegment;
                        if (parameter != null)
                        {
                            object parameterValue;
                            string parameterName = parameter.ParameterName;
                            if (!parameterDescriptions.Any(p => String.Equals(p.Name, parameterName, StringComparison.OrdinalIgnoreCase)) &&
                                (!routeDefaults.TryGetValue(parameterName, out parameterValue) ||
                                parameterValue != RouteParameter.Optional))
                            {
                                parameterDescriptions.Add(
                                    new ApiParameterDescription
                                    {
                                        Name = parameterName,
                                        Source = ApiParameterSource.FromUri
                                    });
                            }
                        }
                    }
                }
            }
        }

        private ApiParameterDescription CreateParameterDescriptionFromDescriptor(HttpParameterDescriptor parameter)
        {
            Contract.Assert(parameter != null);
            return new ApiParameterDescription
            {
                ParameterDescriptor = parameter,
                Name = parameter.Prefix ?? parameter.ParameterName,
                Documentation = GetApiParameterDocumentation(parameter),
                Source = ApiParameterSource.Unknown,
            };
        }

        private ApiParameterDescription CreateParameterDescriptionFromBinding(HttpParameterBinding parameterBinding)
        {
            ApiParameterDescription parameterDescription = CreateParameterDescriptionFromDescriptor(parameterBinding.Descriptor);
            if (parameterBinding.WillReadBody)
            {
                parameterDescription.Source = ApiParameterSource.FromBody;
            }
            else if (parameterBinding.WillReadUri())
            {
                parameterDescription.Source = ApiParameterSource.FromUri;
            }

            return parameterDescription;
        }

        private string GetApiDocumentation(HttpActionDescriptor actionDescriptor)
        {
            IDocumentationProvider documentationProvider = DocumentationProvider ?? actionDescriptor.Configuration.Services.GetDocumentationProvider();
            if (documentationProvider != null)
            {
                return documentationProvider.GetDocumentation(actionDescriptor);
            }

            return null;
        }

        private string GetApiParameterDocumentation(HttpParameterDescriptor parameterDescriptor)
        {
            IDocumentationProvider documentationProvider = DocumentationProvider ?? parameterDescriptor.Configuration.Services.GetDocumentationProvider();
            if (documentationProvider != null)
            {
                return documentationProvider.GetDocumentation(parameterDescriptor);
            }

            return null;
        }

        private string GetApiResponseDocumentation(HttpActionDescriptor actionDescriptor)
        {
            IDocumentationProvider documentationProvider = DocumentationProvider ?? actionDescriptor.Configuration.Services.GetDocumentationProvider();
            if (documentationProvider != null)
            {
                return documentationProvider.GetResponseDocumentation(actionDescriptor);
            }

            return null;
        }

        // remove ApiDescription that will lead to ambiguous action matching.
        private static Collection<ApiDescription> RemoveInvalidApiDescriptions(Collection<ApiDescription> apiDescriptions)
        {
            HashSet<string> duplicateApiDescriptionIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> visitedApiDescriptionIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (ApiDescription description in apiDescriptions)
            {
                string apiDescriptionId = description.ID;
                if (visitedApiDescriptionIds.Contains(apiDescriptionId))
                {
                    duplicateApiDescriptionIds.Add(apiDescriptionId);
                }
                else
                {
                    visitedApiDescriptionIds.Add(apiDescriptionId);
                }
            }

            Collection<ApiDescription> filteredApiDescriptions = new Collection<ApiDescription>();
            foreach (ApiDescription apiDescription in apiDescriptions)
            {
                string apiDescriptionId = apiDescription.ID;
                if (!duplicateApiDescriptionIds.Contains(apiDescriptionId))
                {
                    filteredApiDescriptions.Add(apiDescription);
                }
            }

            return filteredApiDescriptions;
        }

        private static bool MatchRegexConstraint(IHttpRoute route, string parameterName, string parameterValue)
        {
            IDictionary<string, object> constraints = route.Constraints;
            if (constraints != null)
            {
                object constraint;
                if (constraints.TryGetValue(parameterName, out constraint))
                {
                    // treat the constraint as a string which represents a Regex.
                    // note that we don't support custom constraint (IHttpRouteConstraint) because it might rely on the request and some runtime states
                    string constraintsRule = constraint as string;
                    if (constraintsRule != null)
                    {
                        string constraintsRegEx = "^(" + constraintsRule + ")$";
                        return parameterValue != null && Regex.IsMatch(parameterValue, constraintsRegEx, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                    }
                }
            }

            return true;
        }

        private static HttpActionBinding GetActionBinding(HttpActionDescriptor actionDescriptor)
        {
            HttpControllerDescriptor controllerDescriptor = actionDescriptor.ControllerDescriptor;
            if (controllerDescriptor == null)
            {
                return null;
            }

            ServicesContainer controllerServices = controllerDescriptor.Configuration.Services;
            IActionValueBinder actionValueBinder = controllerServices.GetActionValueBinder();
            HttpActionBinding actionBinding = actionValueBinder != null ? actionValueBinder.GetBinding(actionDescriptor) : null;
            return actionBinding;
        }

        private sealed class ApiDescriptionComparer : IEqualityComparer<ApiDescription>
        {
            public bool Equals(ApiDescription x, ApiDescription y)
            {
                return String.Equals(x.ID, y.ID, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode(ApiDescription obj)
            {
                return obj.ID.ToUpperInvariant().GetHashCode();
            }
        }
    }
}
