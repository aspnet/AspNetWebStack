// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;

namespace System.Net.Http.Internal
{
    /// <summary>
    /// Stream that doesn't close the inner stream when closed. This is to work around a limitation
    /// in the <see cref="System.Xml.XmlDictionaryReader"/> insisting of closing the inner stream.
    /// The regular <see cref="System.Xml.XmlReader"/> does allow for not closing the inner stream but that
    /// doesn't have the quota that we need for security reasons. Implementations of
    /// <see cref="System.Net.Http.Formatting.MediaTypeFormatter"/>
    /// should not close the input stream when reading or writing so hence this workaround.
    /// </summary>
    internal class NonClosingDelegatingStream : DelegatingStream
    {
        public NonClosingDelegatingStream(Stream innerStream)
            : base(innerStream)
        {
        }

#if NETSTANDARD1_3
        protected override void Dispose(bool disposing)
        {
        }
#else
        public override void Close()
        {
        }
#endif
    }
}
