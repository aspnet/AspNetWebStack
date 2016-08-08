// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Web.Mvc.Filters;
using System.Web.Mvc.Properties;
using System.Web.Mvc.Routing;
using System.Web.Routing;
using Microsoft.Web.Infrastructure.DynamicValidationHelper;

namespace System.Web.Mvc
{
    [SuppressMessage(
        "Microsoft.Maintainability", 
        "CA1506:AvoidExcessiveClassCoupling",
        Justification = "This class has to work with both traditional and direct routing, which is the cause of the high" +
        "number of classes it uses.")]
    public class ControllerActionInvoker : IActionInvoker
    {
        private static readonly ControllerDescriptorCache _staticDescriptorCache = new ControllerDescriptorCache();

        private ModelBinderDictionary _binders;
        private Func<ControllerContext, ActionDescriptor, IEnumerable<Filter>> _getFiltersThunk = FilterProviders.Providers.GetFilters;
        private ControllerDescriptorCache _instanceDescriptorCache;

        public ControllerActionInvoker()
        {
        }

        internal ControllerActionInvoker(params object[] filters)
            : this()
        {
            if (filters != null)
            {
                _getFiltersThunk = (cc, ad) => filters.Select(f => new Filter(f, FilterScope.Action, null));
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Property is settable so that the dictionary can be provided for unit testing purposes.")]
        protected internal ModelBinderDictionary Binders
        {
            get
            {
                if (_binders == null)
                {
                    _binders = ModelBinders.Binders;
                }
                return _binders;
            }
            set { _binders = value; }
        }

        internal ControllerDescriptorCache DescriptorCache
        {
            get
            {
                if (_instanceDescriptorCache == null)
                {
                    _instanceDescriptorCache = _staticDescriptorCache;
                }
                return _instanceDescriptorCache;
            }
            set { _instanceDescriptorCache = value; }
        }

        protected virtual ActionResult CreateActionResult(ControllerContext controllerContext, ActionDescriptor actionDescriptor, object actionReturnValue)
        {
            if (actionReturnValue == null)
            {
                return new EmptyResult();
            }

            ActionResult actionResult = (actionReturnValue as ActionResult) ??
                                        new ContentResult { Content = Convert.ToString(actionReturnValue, CultureInfo.InvariantCulture) };
            return actionResult;
        }

        protected virtual ControllerDescriptor GetControllerDescriptor(ControllerContext controllerContext)
        {
            // Frequently called, so ensure delegate is static
            Type controllerType = controllerContext.Controller.GetType();
            ControllerDescriptor controllerDescriptor = DescriptorCache.GetDescriptor(
                controllerType: controllerType,
                creator: (Type innerType) => new ReflectedControllerDescriptor(innerType),
                state: controllerType);
            return controllerDescriptor;
        }

        protected virtual ActionDescriptor FindAction(ControllerContext controllerContext, ControllerDescriptor controllerDescriptor, string actionName)
        {
            Contract.Assert(controllerContext != null);
            Contract.Assert(controllerContext.RouteData != null);
            Contract.Assert(controllerDescriptor != null);

            if (controllerContext.RouteData.HasDirectRouteMatch())
            {
                List<DirectRouteCandidate> candidates = GetDirectRouteCandidates(controllerContext);

                DirectRouteCandidate bestCandidate = DirectRouteCandidate.SelectBestCandidate(candidates, controllerContext);
                if (bestCandidate == null)
                {
                    return null;
                }
                else
                {
                    // We need to stash the RouteData of the matched route into the context, so it can be
                    // used for binding.
                    controllerContext.RouteData = bestCandidate.RouteData;
                    controllerContext.RequestContext.RouteData = bestCandidate.RouteData;

                    // We need to remove any optional parameters that haven't gotten a value (See MvcHandler)
                    bestCandidate.RouteData.Values.RemoveFromDictionary((entry) => entry.Value == UrlParameter.Optional);

                    return bestCandidate.ActionDescriptor;
                }
            }
            else
            {
                ActionDescriptor actionDescriptor = controllerDescriptor.FindAction(controllerContext, actionName);
                return actionDescriptor;
            }
        }

        private static List<DirectRouteCandidate> GetDirectRouteCandidates(ControllerContext controllerContext)
        {
            Debug.Assert(controllerContext != null);
            Debug.Assert(controllerContext.RouteData != null);

            List<DirectRouteCandidate> candiates = new List<DirectRouteCandidate>();

            RouteData routeData = controllerContext.RouteData;
            foreach (var directRoute in routeData.GetDirectRouteMatches())
            {
                if (directRoute == null)
                {
                    continue;
                }

                ControllerDescriptor controllerDescriptor = directRoute.GetTargetControllerDescriptor();
                if (controllerDescriptor == null)
                {
                    throw new InvalidOperationException(MvcResources.DirectRoute_MissingControllerDescriptor);
                }

                ActionDescriptor[] actionDescriptors = directRoute.GetTargetActionDescriptors();
                if (actionDescriptors == null || actionDescriptors.Length == 0)
                {
                    throw new InvalidOperationException(MvcResources.DirectRoute_MissingActionDescriptors);
                }

                foreach (var actionDescriptor in actionDescriptors)
                {
                    if (actionDescriptor != null)
                    {
                        candiates.Add(new DirectRouteCandidate()
                        {
                            ActionDescriptor = actionDescriptor,
                            ActionNameSelectors = actionDescriptor.GetNameSelectors(),
                            ActionSelectors = actionDescriptor.GetSelectors(),
                            Order = directRoute.GetOrder(),
                            Precedence = directRoute.GetPrecedence(),
                            RouteData = directRoute,
                        });
                    }
                }
            }

            return candiates;
        }

        protected virtual FilterInfo GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            return new FilterInfo(_getFiltersThunk(controllerContext, actionDescriptor));
        }

        private IModelBinder GetModelBinder(ParameterDescriptor parameterDescriptor)
        {
            // look on the parameter itself, then look in the global table
            return parameterDescriptor.BindingInfo.Binder ?? Binders.GetBinder(parameterDescriptor.ParameterType);
        }

        protected virtual object GetParameterValue(ControllerContext controllerContext, ParameterDescriptor parameterDescriptor)
        {
            // collect all of the necessary binding properties
            Type parameterType = parameterDescriptor.ParameterType;
            IModelBinder binder = GetModelBinder(parameterDescriptor);
            IValueProvider valueProvider = controllerContext.Controller.ValueProvider;
            string parameterName = parameterDescriptor.BindingInfo.Prefix ?? parameterDescriptor.ParameterName;
            Predicate<string> propertyFilter = GetPropertyFilter(parameterDescriptor);

            // finally, call into the binder
            ModelBindingContext bindingContext = new ModelBindingContext()
            {
                FallbackToEmptyPrefix = (parameterDescriptor.BindingInfo.Prefix == null), // only fall back if prefix not specified
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, parameterType),
                ModelName = parameterName,
                ModelState = controllerContext.Controller.ViewData.ModelState,
                PropertyFilter = propertyFilter,
                ValueProvider = valueProvider
            };

            object result = binder.BindModel(controllerContext, bindingContext);
            return result ?? parameterDescriptor.DefaultValue;
        }

        protected virtual IDictionary<string, object> GetParameterValues(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            Dictionary<string, object> parametersDict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            ParameterDescriptor[] parameterDescriptors = actionDescriptor.GetParameters();

            foreach (ParameterDescriptor parameterDescriptor in parameterDescriptors)
            {
                parametersDict[parameterDescriptor.ParameterName] = GetParameterValue(controllerContext, parameterDescriptor);
            }
            return parametersDict;
        }

        private static Predicate<string> GetPropertyFilter(ParameterDescriptor parameterDescriptor)
        {
            ParameterBindingInfo bindingInfo = parameterDescriptor.BindingInfo;
            return propertyName => BindAttribute.IsPropertyAllowed(propertyName, bindingInfo.Include, bindingInfo.Exclude);
        }

        public virtual bool InvokeAction(ControllerContext controllerContext, string actionName)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }

            Contract.Assert(controllerContext.RouteData != null);
            if (String.IsNullOrEmpty(actionName) && !controllerContext.RouteData.HasDirectRouteMatch())
            {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "actionName");
            }

            ControllerDescriptor controllerDescriptor = GetControllerDescriptor(controllerContext);
            ActionDescriptor actionDescriptor = FindAction(controllerContext, controllerDescriptor, actionName);

            if (actionDescriptor != null)
            {
                FilterInfo filterInfo = GetFilters(controllerContext, actionDescriptor);

                try
                {
                    AuthenticationContext authenticationContext = InvokeAuthenticationFilters(controllerContext, filterInfo.AuthenticationFilters, actionDescriptor);

                    if (authenticationContext.Result != null)
                    {
                        // An authentication filter signaled that we should short-circuit the request. Let all
                        // authentication filters contribute to an action result (to combine authentication
                        // challenges). Then, run this action result.
                        AuthenticationChallengeContext challengeContext = InvokeAuthenticationFiltersChallenge(
                            controllerContext, filterInfo.AuthenticationFilters, actionDescriptor,
                            authenticationContext.Result);
                        InvokeActionResult(controllerContext, challengeContext.Result ?? authenticationContext.Result);
                    }
                    else
                    {
                        AuthorizationContext authorizationContext = InvokeAuthorizationFilters(controllerContext, filterInfo.AuthorizationFilters, actionDescriptor);
                        if (authorizationContext.Result != null)
                        {
                            // An authorization filter signaled that we should short-circuit the request. Let all
                            // authentication filters contribute to an action result (to combine authentication
                            // challenges). Then, run this action result.
                            AuthenticationChallengeContext challengeContext = InvokeAuthenticationFiltersChallenge(
                                controllerContext, filterInfo.AuthenticationFilters, actionDescriptor,
                                authorizationContext.Result);
                            InvokeActionResult(controllerContext, challengeContext.Result ?? authorizationContext.Result);
                        }
                        else
                        {
                            if (controllerContext.Controller.ValidateRequest)
                            {
                                ValidateRequest(controllerContext);
                            }

                            IDictionary<string, object> parameters = GetParameterValues(controllerContext, actionDescriptor);
                            ActionExecutedContext postActionContext = InvokeActionMethodWithFilters(controllerContext, filterInfo.ActionFilters, actionDescriptor, parameters);

                            // The action succeeded. Let all authentication filters contribute to an action result (to
                            // combine authentication challenges; some authentication filters need to do negotiation
                            // even on a successful result). Then, run this action result.
                            AuthenticationChallengeContext challengeContext = InvokeAuthenticationFiltersChallenge(
                                controllerContext, filterInfo.AuthenticationFilters, actionDescriptor,
                                postActionContext.Result);
                            InvokeActionResultWithFilters(controllerContext, filterInfo.ResultFilters,
                                challengeContext.Result ?? postActionContext.Result);
                        }
                    }
                }
                catch (ThreadAbortException)
                {
                    // This type of exception occurs as a result of Response.Redirect(), but we special-case so that
                    // the filters don't see this as an error.
                    throw;
                }
                catch (Exception ex)
                {
                    // something blew up, so execute the exception filters
                    ExceptionContext exceptionContext = InvokeExceptionFilters(controllerContext, filterInfo.ExceptionFilters, ex);
                    if (!exceptionContext.ExceptionHandled)
                    {
                        throw;
                    }
                    InvokeActionResult(controllerContext, exceptionContext.Result);
                }

                return true;
            }

            // notify controller that no method matched
            return false;
        }

        protected virtual ActionResult InvokeActionMethod(ControllerContext controllerContext, ActionDescriptor actionDescriptor, IDictionary<string, object> parameters)
        {
            object returnValue = actionDescriptor.Execute(controllerContext, parameters);
            ActionResult result = CreateActionResult(controllerContext, actionDescriptor, returnValue);
            return result;
        }

        internal static ActionExecutedContext InvokeActionMethodFilter(IActionFilter filter, ActionExecutingContext preContext, Func<ActionExecutedContext> continuation)
        {
            filter.OnActionExecuting(preContext);
            if (preContext.Result != null)
            {
                return new ActionExecutedContext(preContext, preContext.ActionDescriptor, true /* canceled */, null /* exception */)
                {
                    Result = preContext.Result
                };
            }

            bool wasError = false;
            ActionExecutedContext postContext = null;
            try
            {
                postContext = continuation();
            }
            catch (ThreadAbortException)
            {
                // This type of exception occurs as a result of Response.Redirect(), but we special-case so that
                // the filters don't see this as an error.
                postContext = new ActionExecutedContext(preContext, preContext.ActionDescriptor, false /* canceled */, null /* exception */);
                filter.OnActionExecuted(postContext);
                throw;
            }
            catch (Exception ex)
            {
                wasError = true;
                postContext = new ActionExecutedContext(preContext, preContext.ActionDescriptor, false /* canceled */, ex);
                filter.OnActionExecuted(postContext);
                if (!postContext.ExceptionHandled)
                {
                    throw;
                }
            }
            if (!wasError)
            {
                filter.OnActionExecuted(postContext);
            }
            return postContext;
        }

        protected virtual ActionExecutedContext InvokeActionMethodWithFilters(ControllerContext controllerContext, IList<IActionFilter> filters, ActionDescriptor actionDescriptor, IDictionary<string, object> parameters)
        {
            ActionExecutingContext preContext = new ActionExecutingContext(controllerContext, actionDescriptor, parameters);
            Func<ActionExecutedContext> continuation = () =>
                                                       new ActionExecutedContext(controllerContext, actionDescriptor, false /* canceled */, null /* exception */)
                                                       {
                                                           Result = InvokeActionMethod(controllerContext, actionDescriptor, parameters)
                                                       };

            // need to reverse the filter list because the continuations are built up backward
            Func<ActionExecutedContext> thunk = filters.Reverse().Aggregate(continuation,
                                                                            (next, filter) => () => InvokeActionMethodFilter(filter, preContext, next));
            return thunk();
        }

        protected virtual void InvokeActionResult(ControllerContext controllerContext, ActionResult actionResult)
        {
            actionResult.ExecuteResult(controllerContext);
        }

        private ResultExecutedContext InvokeActionResultFilterRecursive(IList<IResultFilter> filters, int filterIndex, ResultExecutingContext preContext, ControllerContext controllerContext, ActionResult actionResult)
        {
            // Performance-sensitive

            // For compatbility, the following behavior must be maintained
            //   The OnResultExecuting events must fire in forward order
            //   The InvokeActionResult must then fire
            //   The OnResultExecuted events must fire in reverse order
            //   Earlier filters can process the results and exceptions from the handling of later filters
            // This is achieved by calling recursively and moving through the filter list forwards

            // If there are no more filters to recurse over, create the main result
            if (filterIndex > filters.Count - 1)
            {
                InvokeActionResult(controllerContext, actionResult);
                return new ResultExecutedContext(controllerContext, actionResult, canceled: false, exception: null);
            }

            // Otherwise process the filters recursively
            IResultFilter filter = filters[filterIndex];
            filter.OnResultExecuting(preContext);
            if (preContext.Cancel)
            {
                return new ResultExecutedContext(preContext, preContext.Result, canceled: true, exception: null);
            }

            bool wasError = false;
            ResultExecutedContext postContext = null;
            try
            {
                // Use the filters in forward direction
                int nextFilterIndex = filterIndex + 1;
                postContext = InvokeActionResultFilterRecursive(filters, nextFilterIndex, preContext, controllerContext, actionResult);
            }
            catch (ThreadAbortException)
            {
                // This type of exception occurs as a result of Response.Redirect(), but we special-case so that
                // the filters don't see this as an error.
                postContext = new ResultExecutedContext(preContext, preContext.Result, canceled: false, exception: null);
                filter.OnResultExecuted(postContext);
                throw;
            }
            catch (Exception ex)
            {
                wasError = true;
                postContext = new ResultExecutedContext(preContext, preContext.Result, canceled: false, exception: ex);
                filter.OnResultExecuted(postContext);
                if (!postContext.ExceptionHandled)
                {
                    throw;
                }
            }
            if (!wasError)
            {
                filter.OnResultExecuted(postContext);
            }
            return postContext;
        }

        protected virtual ResultExecutedContext InvokeActionResultWithFilters(ControllerContext controllerContext, IList<IResultFilter> filters, ActionResult actionResult)
        {
            ResultExecutingContext preContext = new ResultExecutingContext(controllerContext, actionResult);

            int startingFilterIndex = 0;
            return InvokeActionResultFilterRecursive(filters, startingFilterIndex, preContext, controllerContext, actionResult);
        }

        protected virtual AuthenticationContext InvokeAuthenticationFilters(ControllerContext controllerContext,
            IList<IAuthenticationFilter> filters, ActionDescriptor actionDescriptor)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }

            Contract.Assert(controllerContext.HttpContext != null);
            IPrincipal originalPrincipal = controllerContext.HttpContext.User;
            AuthenticationContext context = new AuthenticationContext(controllerContext, actionDescriptor,
                originalPrincipal);
            foreach (IAuthenticationFilter filter in filters)
            {
                filter.OnAuthentication(context);
                // short-circuit evaluation when an error occurs
                if (context.Result != null)
                {
                    break;
                }
            }

            IPrincipal newPrincipal = context.Principal;

            if (newPrincipal != originalPrincipal)
            {
                Contract.Assert(context.HttpContext != null);
                context.HttpContext.User = newPrincipal;
                Thread.CurrentPrincipal = newPrincipal;
            }

            return context;
        }

        protected virtual AuthenticationChallengeContext InvokeAuthenticationFiltersChallenge(
            ControllerContext controllerContext, IList<IAuthenticationFilter> filters,
            ActionDescriptor actionDescriptor, ActionResult result)
        {
            AuthenticationChallengeContext context = new AuthenticationChallengeContext(controllerContext,
                actionDescriptor, result);
            foreach (IAuthenticationFilter filter in filters)
            {
                filter.OnAuthenticationChallenge(context);
                // unlike other filter types, don't short-circuit evaluation when context.Result != null (since it
                // starts out that way, and multiple filters may add challenges to the result)
            }

            return context;
        }

        protected virtual AuthorizationContext InvokeAuthorizationFilters(ControllerContext controllerContext, IList<IAuthorizationFilter> filters, ActionDescriptor actionDescriptor)
        {
            AuthorizationContext context = new AuthorizationContext(controllerContext, actionDescriptor);
            foreach (IAuthorizationFilter filter in filters)
            {
                filter.OnAuthorization(context);
                // short-circuit evaluation when an error occurs
                if (context.Result != null)
                {
                    break;
                }
            }

            return context;
        }

        protected virtual ExceptionContext InvokeExceptionFilters(ControllerContext controllerContext, IList<IExceptionFilter> filters, Exception exception)
        {
            ExceptionContext context = new ExceptionContext(controllerContext, exception);
            foreach (IExceptionFilter filter in filters.Reverse())
            {
                filter.OnException(context);
            }

            return context;
        }

        internal static void ValidateRequest(ControllerContext controllerContext)
        {
            if (controllerContext.IsChildAction)
            {
                return;
            }

            // DevDiv 214040: Enable Request Validation by default for all controller requests
            // 
            // Earlier versions of this method dereferenced Request.RawUrl to force validation of
            // that field. This was necessary for Routing before ASP.NET v4, which read the incoming
            // path from RawUrl. Request validation has been moved earlier in the pipeline by default and
            // routing no longer consumes this property, so we don't have to either.

            // Tolerate null HttpContext for testing
            HttpContext currentContext = HttpContext.Current;
            if (currentContext != null)
            {
                ValidationUtility.EnableDynamicValidation(currentContext);
            }

            controllerContext.HttpContext.Request.ValidateInput();
        }
    }
}
