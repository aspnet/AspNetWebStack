// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Facebook;

namespace Microsoft.AspNet.Facebook.Test.Helpers
{
    public class LocalFacebookClient : FacebookClient
    {
        public string Path { get; set; }

        public override Task<TResult> GetTaskAsync<TResult>(string path)
        {
            Path = path;
            return Task.FromResult<TResult>(Activator.CreateInstance<TResult>());
        }

        public override Task<object> GetTaskAsync(string path)
        {
            Path = path;
            return Task.FromResult(new object());
        }

        public override object Get(string path)
        {
            Path = path;
            return new object();
        }

        public override TResult Get<TResult>(string path)
        {
            Path = path;
            return Activator.CreateInstance<TResult>();
        }
    }
}