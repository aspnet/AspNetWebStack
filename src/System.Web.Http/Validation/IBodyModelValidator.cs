// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.ModelBinding;

namespace System.Web.Http.Validation
{
    /// <summary>
    /// Validates the body parameter of an action after the parameter has been read by the <see cref="MediaTypeFormatter"/>.
    /// </summary>
    public interface IBodyModelValidator
    {
        /// <summary>
        /// Determines whether the <paramref name="model"/> is valid and adds any validation errors to the <paramref name="actionContext"/>'s <see cref="ModelStateDictionary"/>
        /// </summary>
        /// <param name="model">The model to be validated.</param>
        /// <param name="type">The <see cref="Type"/> to use for validation.</param>
        /// <param name="metadataProvider">The <see cref="ModelMetadataProvider"/> used to provide the model metadata.</param>
        /// <param name="actionContext">The <see cref="HttpActionContext"/> within which the model is being validated.</param>
        /// <param name="keyPrefix">The <see cref="string"/> to append to the key for any validation errors.</param>
        /// <returns><c>true</c>if <paramref name="model"/> is valid, <c>false</c> otherwise.</returns>
        bool Validate(object model, Type type, ModelMetadataProvider metadataProvider, HttpActionContext actionContext, string keyPrefix);
    }
}
