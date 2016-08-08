// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;
namespace Microsoft.AspNet.Facebook.Test.Types
{
    public class UserTypeWithIgnoredProperties
    {
        public string Id { get; set; }

        [JsonIgnore]
        public string Name { get; set; }

        [JsonIgnore]
        public FacebookConnection<FacebookPicture> Picture { get; set; }
    }
}
