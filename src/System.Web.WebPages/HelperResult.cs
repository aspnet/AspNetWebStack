// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using System.IO;

namespace System.Web.WebPages
{
    public class HelperResult : IHtmlString
    {
        private readonly Action<TextWriter> _action;

        public HelperResult(Action<TextWriter> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            _action = action;
        }

        public string ToHtmlString()
        {
            return ToString();
        }

        public override string ToString()
        {
            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                _action(writer);
                return writer.ToString();
            }
        }

        public void WriteTo(TextWriter writer)
        {
            _action(writer);
        }
    }
}
