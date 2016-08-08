// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Http.Filters
{
    public sealed class FilterInfo
    {
        public FilterInfo(IFilter instance, FilterScope scope)
        {
            if (instance == null)
            {
                throw Error.ArgumentNull("instance");
            }

            Instance = instance;
            Scope = scope;
        }

        public IFilter Instance { get; private set; }

        public FilterScope Scope { get; private set; }
    }
}
