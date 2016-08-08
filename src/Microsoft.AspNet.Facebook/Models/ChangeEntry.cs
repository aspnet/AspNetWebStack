// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.AspNet.Facebook.Models
{
    /// <summary>
    /// Change entry from Facebook as part of Realtime Updates.
    /// </summary>
    public class ChangeEntry
    {
        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        [JsonProperty("UId")]
        public long UserId { get; set; }

        /// <summary>
        /// Gets or sets the changed fields.
        /// </summary>
        [JsonProperty("Changed_Fields")]
        public IEnumerable<string> ChangedFields { get; set; }

        /// <summary>
        /// Gets or sets the time.
        /// </summary>
        public long Time { get; set; }
    }
}