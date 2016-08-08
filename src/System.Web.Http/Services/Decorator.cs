// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Http.Services
{
    /// <summary>
    /// Provides a method for retrieving the innermost object of an object that might be wrapped by an <see cref="IDecorator{T}"/>.
    /// </summary>
    public static class Decorator
    {
        /// <summary>
        /// Gets the innermost object which does not implement <see cref="IDecorator{T}"/>.
        /// </summary>
        /// <param name="outer">Object which needs to be unwrapped.</param>
        /// <returns>The innermost object of Type T which does not implement <see cref="IDecorator{T}"/>.</returns>
        public static T GetInner<T>(T outer)
        {
            T inner = outer;
            IDecorator<T> decorator = inner as IDecorator<T>;
            while (decorator != null)
            {
                inner = decorator.Inner;
                IDecorator<T> innerDecorator = inner as IDecorator<T>;
                if (decorator == innerDecorator)
                {
                    break;
                }
                decorator = innerDecorator;
            }

            return inner;
        }
    }
}
