// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Microsoft.AspNet.Facebook.Test.Types
{
    public class UserTypeWithRenamedProperties
    {
        [JsonProperty("Id")]
        public string FacebookId { get; set; }

        public string Name { get; set; }

        [JsonProperty("picture")]
        public FacebookConnection<FacebookPicture> PictureConnection { get; set; }
    }
}
