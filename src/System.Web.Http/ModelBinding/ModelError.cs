// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Http.ModelBinding
{
    [Serializable]
    public class ModelError
    {
        public ModelError(Exception exception)
            : this(exception, errorMessage: null)
        {
        }

        public ModelError(Exception exception, string errorMessage)
            : this(errorMessage)
        {
            if (exception == null)
            {
                throw Error.ArgumentNull("exception");
            }

            Exception = exception;
        }

        public ModelError(string errorMessage)
        {
            ErrorMessage = errorMessage ?? String.Empty;
        }

        public Exception Exception { get; private set; }

        public string ErrorMessage { get; private set; }
    }
}
