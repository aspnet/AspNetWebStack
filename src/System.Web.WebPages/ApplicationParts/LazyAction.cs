// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;

namespace System.Web.WebPages.ApplicationParts
{
    internal class LazyAction
    {
        private Lazy<object> _lazyAction;

        public LazyAction(Action action)
        {
            Debug.Assert(action != null, "action should not be null");
            // Setup the lazy object to run our action and just return null 
            // since we don't care about the value
            _lazyAction = new Lazy<object>(() =>
            {
                action();
                return null;
            });
        }

        public object EnsurePerformed()
        {
            // REVIEW: This isn't used we're just exploiting the use of Lazy<T> to execute 
            // our action once in a thread safe way
            // It would be nice if the framework had Unit (i.e a better void type so we could type Func<Unit> -> Action) 
            return _lazyAction.Value;
        }
    }
}
