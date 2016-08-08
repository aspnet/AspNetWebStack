// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.ModelBinding.Binders;
using System.Web.Http.Properties;
using System.Web.Http.Routing;
using System.Web.Http.ValueProviders;
using System.Web.Http.ValueProviders.Providers;

namespace System.Web.Http.Tracing
{
    /// <summary>
    /// General purpose utilities to format strings used in tracing.
    /// </summary>
    internal static class FormattingUtilities
    {
        public static readonly string NullMessage = "null";

        public static string ActionArgumentsToString(IDictionary<string, object> actionArguments)
        {
            Contract.Assert(actionArguments != null);
            return String.Join(", ",
                               actionArguments.Keys.Select<string, string>(
                                   (k) => k + "=" + ValueToString(actionArguments[k], CultureInfo.CurrentCulture)));
        }

        public static string ActionDescriptorToString(HttpActionDescriptor actionDescriptor)
        {
            Contract.Assert(actionDescriptor != null);

            string parameterList = String.Join(", ",
                                               actionDescriptor.GetParameters().Select<HttpParameterDescriptor, string>(
                                                   (p) => p.ParameterType.Name + " " + p.ParameterName));

            return actionDescriptor.ActionName + "(" + parameterList + ")";
        }

        public static string ActionInvokeToString(HttpActionContext actionContext)
        {
            Contract.Assert(actionContext != null);
            return ActionInvokeToString(actionContext.ActionDescriptor.ActionName, actionContext.ActionArguments);
        }

        public static string ActionInvokeToString(string actionName, IDictionary<string, object> arguments)
        {
            Contract.Assert(actionName != null);
            Contract.Assert(arguments != null);

            return actionName + "(" + ActionArgumentsToString(arguments) + ")";
        }

        public static string FormattersToString(IEnumerable<MediaTypeFormatter> formatters)
        {
            Contract.Assert(formatters != null);

            return String.Join(", ", formatters.Select<MediaTypeFormatter, string>((f) => f.GetType().Name));
        }

        public static string ModelBinderToString(ModelBinderProvider provider)
        {
            Contract.Assert(provider != null);

            CompositeModelBinderProvider composite = provider as CompositeModelBinderProvider;
            if (composite == null)
            {
                return provider.GetType().Name;
            }

            string modelBinderList = String.Join(", ", composite.Providers.Select<ModelBinderProvider, string>(ModelBinderToString));

            return provider.GetType().Name + "(" + modelBinderList + ")";
        }

        public static string ModelStateToString(ModelStateDictionary modelState)
        {
            Contract.Assert(modelState != null);

            if (modelState.IsValid)
            {
                return String.Empty;
            }

            StringBuilder modelStateBuilder = new StringBuilder();
            foreach (string key in modelState.Keys)
            {
                ModelState state = modelState[key];
                if (state.Errors.Count > 0)
                {
                    foreach (ModelError error in state.Errors)
                    {
                        string errorString = Error.Format(SRResources.TraceModelStateErrorMessage, 
                                                           key,
                                                           error.ErrorMessage);
                        if (modelStateBuilder.Length > 0)
                        {
                            modelStateBuilder.Append(',');
                        }

                        modelStateBuilder.Append(errorString);
                    }
                }
            }

            return modelStateBuilder.ToString();
        }

        public static string RouteToString(IHttpRouteData routeData)
        {
            Contract.Assert(routeData != null);

            return String.Join(",", routeData.Values.Select((pair) => Error.Format("{0}:{1}", pair.Key, pair.Value)));
        }

        public static string ValueProviderToString(IValueProvider provider)
        {
            Contract.Assert(provider != null);

            CompositeValueProvider composite = provider as CompositeValueProvider;
            if (composite == null)
            {
                return provider.GetType().Name;
            }

            string providerList = String.Join(", ", composite.Select<IValueProvider, string>(ValueProviderToString));
            return provider.GetType().Name + "(" + providerList + ")";
        }

        public static string ValueToString(object value, CultureInfo cultureInfo)
        {
            Contract.Assert(cultureInfo != null);

            if (value == null)
            {
                return NullMessage;
            }

            return Convert.ToString(value, cultureInfo);
        }
    }
}
