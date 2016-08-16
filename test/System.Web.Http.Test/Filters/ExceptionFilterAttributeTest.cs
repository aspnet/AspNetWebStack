// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Filters
{
    public class ExceptionFilterAttributeTest
    {
        private readonly HttpActionExecutedContext _context = new HttpActionExecutedContext(ContextUtil.CreateActionContext(), new Exception());

        [Fact]
        public void AllowsMultiple_DefaultReturnsTrue()
        {
            ExceptionFilterAttribute actionFilter = new TestableExceptionFilter();

            Assert.True(actionFilter.AllowMultiple);
        }

        [Fact]
        public void ExecuteExceptionFilterAsync_IfContextParameterIsNull_ThrowsException()
        {
            IExceptionFilter filter = new Mock<ExceptionFilterAttribute>() { CallBase = true }.Object;

            Assert.ThrowsArgumentNull(() =>
            {
                filter.ExecuteExceptionFilterAsync(actionExecutedContext: null, cancellationToken: CancellationToken.None);
            }, "actionExecutedContext");
        }

        [Fact]
        public async Task ExecuteExceptionFilterAsync_IfOnExceptionThrowsException_RethrowsTheSameException()
        {
            // Arrange
            var mockFilter = new Mock<ExceptionFilterAttribute>() { CallBase = true };

            Exception exception = new Exception();
            mockFilter.Setup(f => f.OnException(_context)).Throws(exception);
            IExceptionFilter filter = mockFilter.Object;

            // Act & Assert
            var thrownException = await Assert.ThrowsAsync<Exception>(() =>
                filter.ExecuteExceptionFilterAsync(_context, CancellationToken.None));
            Assert.Same(exception, thrownException);
        }

        [Fact]
        public async Task ExecuteExceptionFilterAsync_InvokesOnExceptionMethod()
        {
            // Arrange
            var mockFilter = new Mock<ExceptionFilterAttribute>()
            {
                CallBase = true,
            };

            IExceptionFilter filter = mockFilter.Object;

            // Act
            await filter.ExecuteExceptionFilterAsync(_context, CancellationToken.None);

            // Assert
            mockFilter.Verify(f => f.OnException(_context));
        }

        [Fact]
        public void ExecuteExceptionFilterAsync_ReturnsCompletedTask()
        {
            // Arrange
            var mockFilter = new Mock<ExceptionFilterAttribute>() { CallBase = true };
            IExceptionFilter filter = mockFilter.Object;

            // Act
            var result = filter.ExecuteExceptionFilterAsync(_context, CancellationToken.None);

            // Assert
            Assert.True(result.Status == TaskStatus.RanToCompletion);
        }

        public sealed class TestableExceptionFilter : ExceptionFilterAttribute
        {
        }
    }
}
