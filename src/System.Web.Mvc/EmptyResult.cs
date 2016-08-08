// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    // represents a result that doesn't do anything, like a controller action returning null
    public class EmptyResult : ActionResult
    {
        private static readonly EmptyResult _singleton = new EmptyResult();

        internal static EmptyResult Instance
        {
            get { return _singleton; }
        }

        public override void ExecuteResult(ControllerContext context)
        {
        }
    }
}
