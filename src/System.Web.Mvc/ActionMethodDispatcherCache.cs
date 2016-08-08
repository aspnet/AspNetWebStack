// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;

namespace System.Web.Mvc
{
    internal sealed class ActionMethodDispatcherCache : ReaderWriterCache<MethodInfo, ActionMethodDispatcher>
    {
        public ActionMethodDispatcherCache()
        {
        }

        public ActionMethodDispatcher GetDispatcher(MethodInfo methodInfo)
        {
            // Frequently called, so ensure delegate remains static
            return FetchOrCreateItem(methodInfo, (MethodInfo methodInfoInner) => new ActionMethodDispatcher(methodInfoInner), methodInfo);
        }
    }
}
