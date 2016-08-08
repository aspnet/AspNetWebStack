// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Web.Mvc
{
    public class FieldValidationMetadata
    {
        private readonly Collection<ModelClientValidationRule> _validationRules = new Collection<ModelClientValidationRule>();
        private string _fieldName;

        public string FieldName
        {
            get { return _fieldName ?? String.Empty; }
            set { _fieldName = value; }
        }

        public bool ReplaceValidationMessageContents { get; set; }

        public string ValidationMessageId { get; set; }

        public ICollection<ModelClientValidationRule> ValidationRules
        {
            get { return _validationRules; }
        }
    }
}
