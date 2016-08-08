// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.TestCommon;

namespace System.Threading.Tasks
{
    public class TaskHelpersTest
    {
        // -----------------------------------------------------------------
        //  TaskHelpers.Canceled

        [Fact]
        public void Canceled_ReturnsCanceledTask()
        {
            Task result = TaskHelpers.Canceled();

            Assert.NotNull(result);
            Assert.True(result.IsCanceled);
        }

        // -----------------------------------------------------------------
        //  TaskHelpers.Canceled<T>

        [Fact]
        public void Canceled_Generic_ReturnsCanceledTask()
        {
            Task<string> result = TaskHelpers.Canceled<string>();

            Assert.NotNull(result);
            Assert.True(result.IsCanceled);
        }

        // -----------------------------------------------------------------
        //  TaskHelpers.Completed

        [Fact]
        public void Completed_ReturnsCompletedTask()
        {
            Task result = TaskHelpers.Completed();

            Assert.NotNull(result);
            Assert.Equal(TaskStatus.RanToCompletion, result.Status);
        }

        // -----------------------------------------------------------------
        //  TaskHelpers.FromError

        [Fact]
        public void FromError_ReturnsFaultedTaskWithGivenException()
        {
            var exception = new Exception();

            Task result = TaskHelpers.FromError(exception);

            Assert.NotNull(result);
            Assert.True(result.IsFaulted);
            Assert.Same(exception, result.Exception.InnerException);
        }

        // -----------------------------------------------------------------
        //  TaskHelpers.FromError<T>

        [Fact]
        public void FromError_Generic_ReturnsFaultedTaskWithGivenException()
        {
            var exception = new Exception();

            Task<string> result = TaskHelpers.FromError<string>(exception);

            Assert.NotNull(result);
            Assert.True(result.IsFaulted);
            Assert.Same(exception, result.Exception.InnerException);
        }
    }
}
