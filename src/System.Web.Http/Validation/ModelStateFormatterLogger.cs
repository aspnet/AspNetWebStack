// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http.Formatting;
using System.Web.Http.ModelBinding;

namespace System.Web.Http.Validation
{
    /// <summary>
    /// This <see cref="IFormatterLogger"/> logs formatter errors to the provided <see cref="ModelStateDictionary"/>.
    /// </summary>
    public class ModelStateFormatterLogger : IFormatterLogger
    {
        private readonly ModelStateDictionary _modelState;
        private readonly string _prefix;

        public ModelStateFormatterLogger(ModelStateDictionary modelState, string prefix)
        {
            if (modelState == null)
            {
                throw Error.ArgumentNull("modelState");
            }

            _modelState = modelState;
            _prefix = prefix;
        }

        public void LogError(string errorPath, string errorMessage)
        {
            if (errorPath == null)
            {
                throw Error.ArgumentNull("errorPath");
            }
            if (errorMessage == null)
            {
                throw Error.ArgumentNull("errorMessage");
            }

            string key = ModelBindingHelper.ConcatenateKeys(_prefix, errorPath);
            _modelState.AddModelError(key, errorMessage);
        }

        public void LogError(string errorPath, Exception exception)
        {
            if (errorPath == null)
            {
                throw Error.ArgumentNull("errorPath");
            }
            if (exception == null)
            {
                throw Error.ArgumentNull("exception");
            }

            string key = ModelBindingHelper.ConcatenateKeys(_prefix, errorPath);
            _modelState.AddModelError(key, exception);
        }
    }
}
