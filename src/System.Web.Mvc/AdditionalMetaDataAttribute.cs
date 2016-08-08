// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Property, AllowMultiple = true)]
    public sealed class AdditionalMetadataAttribute : Attribute, IMetadataAware
    {
        private object _typeId = new object();

        public AdditionalMetadataAttribute(string name, object value)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            Name = name;
            Value = value;
        }

        public override object TypeId
        {
            get { return _typeId; }
        }

        public string Name { get; private set; }

        public object Value { get; private set; }

        public void OnMetadataCreated(ModelMetadata metadata)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException("metadata");
            }

            metadata.AdditionalValues[Name] = Value;
        }
    }
}
