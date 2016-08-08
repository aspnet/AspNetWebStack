// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Internal;
using System.Web.Http.Properties;

namespace System.Web.Http.ModelBinding
{
    public class DefaultActionValueBinder : IActionValueBinder
    {
        /// <summary>
        /// Implementation of <see cref="IActionValueBinder"/>, Primary entry point for binding parameters for an action.
        /// </summary>
        public virtual HttpActionBinding GetBinding(HttpActionDescriptor actionDescriptor)
        {
            if (actionDescriptor == null)
            {
                throw Error.ArgumentNull("actionDescriptor");
            }

            HttpParameterDescriptor[] parameters = actionDescriptor.GetParameters().ToArray();
            HttpParameterBinding[] binders = Array.ConvertAll(parameters, GetParameterBinding);

            HttpActionBinding actionBinding = new HttpActionBinding(actionDescriptor, binders);

            EnsureOneBodyParameter(actionBinding);

            return actionBinding;
        }

        /// <summary>
        /// Update actionBinding to enforce there is at most 1 body parameter.
        /// If there are multiple, convert them all to <see cref="ErrorParameterBinding"/>
        /// </summary>
        private static void EnsureOneBodyParameter(HttpActionBinding actionBinding)
        {
            IList<HttpParameterDescriptor> parameters = actionBinding.ActionDescriptor.GetParameters();

            int idxFromBody = -1;
            for (int i = 0; i < actionBinding.ParameterBindings.Length; i++)
            {
                if (actionBinding.ParameterBindings[i].WillReadBody)
                {
                    if (idxFromBody >= 0)
                    {
                        // This is the 2nd parameter to read from the body. Flag an error.
                        string name1 = parameters[idxFromBody].ParameterName;
                        string name2 = parameters[i].ParameterName;

                        string message = Error.Format(SRResources.ParameterBindingCantHaveMultipleBodyParameters, name1, name2);
                        actionBinding.ParameterBindings[i] = new ErrorParameterBinding(parameters[i], message);
                        actionBinding.ParameterBindings[idxFromBody] = new ErrorParameterBinding(parameters[idxFromBody], message);
                    }
                    else
                    {
                        idxFromBody = i;
                    }
                }
            }
        }

        // Determine how a single parameter will get bound.
        // This is all sync. We don't need to actually read the body just to determine that we'll bind to the body.
        protected virtual HttpParameterBinding GetParameterBinding(HttpParameterDescriptor parameter)
        {
            // Attribute has the highest precedence
            // Presence of a model binder attribute overrides.
            ParameterBindingAttribute attr = parameter.ParameterBinderAttribute;
            if (attr != null)
            {
                return attr.GetBinding(parameter);
            }

            // No attribute, so lookup in global map.
            ParameterBindingRulesCollection pb = parameter.Configuration.ParameterBindingRules;
            if (pb != null)
            {
                HttpParameterBinding binding = pb.LookupBinding(parameter);
                if (binding != null)
                {
                    return binding;
                }
            }

            // Not explicitly specified in global map or attribute.
            // Use a default policy to determine it. These are catch-all policies.
            Type type = parameter.ParameterType;
            if (TypeHelper.CanConvertFromString(type))
            {
                // For simple types, the default is to look in URI. Exactly as if the parameter had a [FromUri] attribute.
                return parameter.BindWithAttribute(new FromUriAttribute());
            }

            // Fallback. Must be a complex type. Default is to look in body. Exactly as if this type had a [FromBody] attribute.
            attr = new FromBodyAttribute();
            return attr.GetBinding(parameter);
        }

        // Create an instance and add some default binders
        internal static ParameterBindingRulesCollection GetDefaultParameterBinders()
        {
            ParameterBindingRulesCollection pb = new ParameterBindingRulesCollection();

            pb.Add(typeof(CancellationToken), parameter => new CancellationTokenParameterBinding(parameter));
            pb.Add(typeof(HttpRequestMessage), parameter => new HttpRequestParameterBinding(parameter));

            // Warning binder for HttpContent.
            pb.Add(parameter => typeof(HttpContent).IsAssignableFrom(parameter.ParameterType) ?
                                    parameter.BindAsError(Error.Format(SRResources.ParameterBindingIllegalType, parameter.ParameterType.Name, parameter.ParameterName))
                                    : null);

            return pb;
        }
    }
}