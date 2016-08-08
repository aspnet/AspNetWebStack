// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Http.Batch
{
    /// <summary>
    /// Defines the order of execution for batch requests.
    /// </summary>
    public enum BatchExecutionOrder
    {
        /// <summary>
        /// Executes the batch requests sequentially.
        /// </summary>
        Sequential = 0,

        /// <summary>
        /// Executes the batch requests non-sequentially.
        /// </summary>
        NonSequential
    }
}