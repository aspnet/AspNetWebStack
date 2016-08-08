// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Description;

namespace System.Web.Http.ApiExplorer
{
    public class AttributeDocumentationProvider : IDocumentationProvider
    {
        public string GetDocumentation(HttpActionDescriptor actionDescriptor)
        {
            var apiDocumentation = actionDescriptor.GetCustomAttributes<ApiDocumentationAttribute>().FirstOrDefault();
            if (apiDocumentation != null)
            {
                return apiDocumentation.Description;
            }

            return String.Empty;
        }

        public string GetDocumentation(HttpParameterDescriptor parameterDescriptor)
        {
            var parameterDocumentation = parameterDescriptor.ActionDescriptor.GetCustomAttributes<ApiParameterDocumentationAttribute>().FirstOrDefault(param => param.ParameterName == parameterDescriptor.ParameterName);
            if (parameterDocumentation != null)
            {
                return parameterDocumentation.Description;
            }

            return String.Empty;
        }

        public string GetDocumentation(HttpControllerDescriptor controllerDescriptor)
        {
            var apiDocumentation = controllerDescriptor.GetCustomAttributes<ApiDocumentationAttribute>().FirstOrDefault();
            if (apiDocumentation != null)
            {
                return apiDocumentation.Description;
            }

            return String.Empty;
        }
        
        public string GetResponseDocumentation(HttpActionDescriptor actionDescriptor)
        {
            var apiDocumentation = actionDescriptor.GetCustomAttributes<ApiResponseDocumentationAttribute>().FirstOrDefault();
            if (apiDocumentation != null)
            {
                return apiDocumentation.Description;
            }

            return String.Empty;
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ApiDocumentationAttribute : Attribute
    {
        public ApiDocumentationAttribute(string description)
        {
            Description = description;
        }

        public string Description { get; private set; }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ApiResponseDocumentationAttribute : Attribute
    {
        public ApiResponseDocumentationAttribute(string description)
        {
            Description = description;
        }

        public string Description { get; private set; }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class ApiParameterDocumentationAttribute : Attribute
    {
        public ApiParameterDocumentationAttribute(string parameterName, string description)
        {
            ParameterName = parameterName;
            Description = description;
        }

        public string ParameterName { get; private set; }

        public string Description { get; private set; }
    }
}
