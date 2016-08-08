// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.TestCommon;

namespace System.Web.Http.ExceptionHandling
{
    public class EmptyExceptionHandlerTests
    {
        [Fact]
        public void HandleAsync_ReturnsCompletedTask()
        {
            // Arrange
            IExceptionHandler product = CreateProductUnderTest();

            ExceptionHandlerContext context = CreateContext();
            CancellationToken cancellationToken = CancellationToken.None;

            // Act
            Task task = product.HandleAsync(context, cancellationToken);

            // Assert
            Assert.NotNull(task);
            Assert.Equal(TaskStatus.RanToCompletion, task.Status);
        }

        private static ExceptionHandlerContext CreateContext()
        {
            return new ExceptionHandlerContext(new ExceptionContext(new Exception(), ExceptionCatchBlocks.HttpServer));
        }

        private static EmptyExceptionHandler CreateProductUnderTest()
        {
            return new EmptyExceptionHandler();
        }
    }
}
