// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Web.Mvc
{
    [TypeForwardedFrom("System.Web.Mvc, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class ModelClientValidationRule
    {
        private readonly Dictionary<string, object> _validationParameters = new Dictionary<string, object>();
        private string _validationType;

        public string ErrorMessage { get; set; }

        public IDictionary<string, object> ValidationParameters
        {
            get { return _validationParameters; }
        }

        public string ValidationType
        {
            get { return _validationType ?? String.Empty; }
            set { _validationType = value; }
        }
    }
}
