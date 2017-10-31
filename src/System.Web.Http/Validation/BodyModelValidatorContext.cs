// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.ModelBinding;

namespace System.Web.Http.Validation
{
    /// <summary>
    /// Context passed between <see cref="DefaultBodyModelValidator"/> methods.
    /// </summary>
    public class BodyModelValidatorContext
    {
        public BodyModelValidatorContext(ModelStateDictionary modelState)
        {
            if (modelState == null)
            {
                throw new ArgumentNullException("modelState");
            }

            KeyBuilders = new Stack<IBodyModelValidatorKeyBuilder>();
            ModelState = modelState;
            Visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
        }

        /// <summary>
        /// Gets or sets the <see cref="ModelMetadataProvider"/> used to provide the model metadata.
        /// </summary>
        public ModelMetadataProvider MetadataProvider { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="HttpActionContext"/> within which the model is being validated.
        /// </summary>
        public HttpActionContext ActionContext { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IModelValidatorCache"/>.
        /// </summary>
        public IModelValidatorCache ValidatorCache { get; set; }

        /// <summary>
        /// Gets the current <see cref="ModelStateDictionary"/>.
        /// </summary>
        public ModelStateDictionary ModelState { get; private set; }

        /// <summary>
        /// Gets the set of model objects visited in this validation. Includes the model being validated in the
        /// current scope.
        /// </summary>
        public HashSet<object> Visited { get; private set; }

        /// <summary>
        /// Gets the stack of <see cref="IBodyModelValidatorKeyBuilder"/>s used in this validation. Includes
        /// the <see cref="IBodyModelValidatorKeyBuilder"/> to generate model state keys for the current scope.
        /// </summary>
        public Stack<IBodyModelValidatorKeyBuilder> KeyBuilders { get; private set; }

        /// <summary>
        /// Gets or sets the model state prefix for the root scope of this validation.
        /// </summary>
        public string RootPrefix { get; set; }
    }
}
