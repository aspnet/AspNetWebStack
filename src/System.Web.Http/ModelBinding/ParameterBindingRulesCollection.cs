// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Web.Http.Controllers;

namespace System.Web.Http.ModelBinding
{    
    /// <summary>
    /// Collection of functions that can produce a parameter binding for a given parameter.   
    /// </summary>
    public class ParameterBindingRulesCollection : Collection<Func<HttpParameterDescriptor, HttpParameterBinding>>
    {
        // Helper to wrap the lambda in a type-check
        // This is for convenience overloads that want to register by type, which should be a common case.
        private static Func<HttpParameterDescriptor, HttpParameterBinding> TypeCheck(Type type, Func<HttpParameterDescriptor, HttpParameterBinding> func)
        {
            return (param => (param.ParameterType == type) ? func(param) : null);
        }
                
        /// <summary>
        /// Adds function to the end of the collection. 
        /// The function added is a wrapper around funcInner that checks that parameterType matches typeMatch.
        /// </summary>
        /// <param name="typeMatch">type to match against HttpParameterDescriptor.ParameterType</param>
        /// <param name="funcInner">inner function that is invoked if type match succeeds</param>
        public void Add(Type typeMatch, Func<HttpParameterDescriptor, HttpParameterBinding> funcInner)
        {
            Add(TypeCheck(typeMatch, funcInner));
        }

        /// <summary>
        /// Insert a function at the specified index in the collection.
        /// /// The function added is a wrapper around funcInner that checks that parameterType matches typeMatch.
        /// </summary>
        /// <param name="index">index to insert at.</param>
        /// <param name="typeMatch">type to match against HttpParameterDescriptor.ParameterType</param>
        /// <param name="funcInner">inner function that is invoked if type match succeeds</param>
        public void Insert(int index, Type typeMatch, Func<HttpParameterDescriptor, HttpParameterBinding> funcInner)
        {
            Insert(index, TypeCheck(typeMatch, funcInner));
        }
                
        /// <summary>
        /// Execute each binding function in order until one of them returns a non-null binding. 
        /// </summary>
        /// <param name="parameter">parameter to bind.</param>
        /// <returns>the first non-null binding produced for the parameter. Of null if no binding is produced.</returns>
        public HttpParameterBinding LookupBinding(HttpParameterDescriptor parameter)
        {
            foreach (Func<HttpParameterDescriptor, HttpParameterBinding> func in this)
            {
                HttpParameterBinding binding = func(parameter);
                if (binding != null)
                {
                    return binding;
                }
            }
            return null;
        }
    }
}
