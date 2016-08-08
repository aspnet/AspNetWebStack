// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Mvc.Async.Test
{
    public class OperationCounterTest
    {
        [Fact]
        public void CompletedEvent()
        {
            // Arrange
            bool premature = true;
            bool eventFired = false;
            OperationCounter ops = new OperationCounter();
            ops.Completed += (sender, eventArgs) =>
            {
                if (premature)
                {
                    Assert.True(false, "Event fired too early!");
                }
                if (eventFired)
                {
                    Assert.True(false, "Event fired multiple times.");
                }

                Assert.Equal(ops, sender);
                Assert.Equal(eventArgs, EventArgs.Empty);
                eventFired = true;
            };

            // Act & assert
            ops.Increment(); // should not fire event (will throw exception)
            premature = false;

            ops.Decrement(); // should fire event
            Assert.True(eventFired);

            ops.Increment(); // should not fire event (will throw exception)
        }

        [Fact]
        public void CountStartsAtZero()
        {
            // Arrange
            OperationCounter ops = new OperationCounter();

            // Act & assert
            Assert.Equal(0, ops.Count);
        }

        [Fact]
        public void DecrementWithIntegerArgument()
        {
            // Arrange
            OperationCounter ops = new OperationCounter();

            // Act
            int returned = ops.Decrement(3);
            int newCount = ops.Count;

            // Assert
            Assert.Equal(-3, returned);
            Assert.Equal(-3, newCount);
        }

        [Fact]
        public void DecrementWithNoArguments()
        {
            // Arrange
            OperationCounter ops = new OperationCounter();

            // Act
            int returned = ops.Decrement();
            int newCount = ops.Count;

            // Assert
            Assert.Equal(-1, returned);
            Assert.Equal(-1, newCount);
        }

        [Fact]
        public void IncrementWithIntegerArgument()
        {
            // Arrange
            OperationCounter ops = new OperationCounter();

            // Act
            int returned = ops.Increment(3);
            int newCount = ops.Count;

            // Assert
            Assert.Equal(3, returned);
            Assert.Equal(3, newCount);
        }

        [Fact]
        public void IncrementWithNoArguments()
        {
            // Arrange
            OperationCounter ops = new OperationCounter();

            // Act
            int returned = ops.Increment();
            int newCount = ops.Count;

            // Assert
            Assert.Equal(1, returned);
            Assert.Equal(1, newCount);
        }
    }
}
