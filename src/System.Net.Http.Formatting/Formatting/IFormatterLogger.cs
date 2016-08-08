// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Net.Http.Formatting
{
    /// <summary>
    /// Interface to log events that occur during formatter reads.
    /// </summary>
    public interface IFormatterLogger
    {
        /// <summary>
        /// Logs an error.
        /// </summary>
        /// <param name="errorPath">The path to the member for which the error is being logged.</param>
        /// <param name="errorMessage">The error message to be logged.</param>
        void LogError(string errorPath, string errorMessage);

        /// <summary>
        /// Logs an error.
        /// </summary>
        /// <param name="errorPath">The path to the member for which the error is being logged.</param>
        /// <param name="exception">The exception to be logged.</param>
        void LogError(string errorPath, Exception exception);
    }
}
