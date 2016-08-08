// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using WebMatrix.Data.Resources;

namespace WebMatrix.Data
{
    public sealed class DynamicRecord : DynamicObject, ICustomTypeDescriptor
    {
        internal DynamicRecord(IEnumerable<string> columnNames, IDataRecord record)
        {
            Debug.Assert(record != null, "record should not be null");
            Debug.Assert(columnNames != null, "columnNames should not be null");

            Columns = columnNames.ToList();
            Record = record;
        }

        public IList<string> Columns { get; private set; }

        private IDataRecord Record { get; set; }

        public object this[string name]
        {
            get
            {
                VerifyColumn(name);
                return GetValue(Record[name]);
            }
        }

        public object this[int index]
        {
            get { return GetValue(Record[index]); }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = this[binder.Name];
            return true;
        }

        private static object GetValue(object value)
        {
            return DBNull.Value == value ? null : value;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return Columns;
        }

        private void VerifyColumn(string name)
        {
            // REVIEW: Perf
            if (!Columns.Contains(name, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    String.Format(CultureInfo.CurrentCulture,
                                  DataResources.InvalidColumnName, name));
            }
        }

        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return AttributeCollection.Empty;
        }

        string ICustomTypeDescriptor.GetClassName()
        {
            return null;
        }

        string ICustomTypeDescriptor.GetComponentName()
        {
            return null;
        }

        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return null;
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            return null;
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            return null;
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            return null;
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            return EventDescriptorCollection.Empty;
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return EventDescriptorCollection.Empty;
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            return ((ICustomTypeDescriptor)this).GetProperties();
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            // Get the name and type for each column name
            var properties = from columnName in Columns
                             let columnIndex = Record.GetOrdinal(columnName)
                             let type = Record.GetFieldType(columnIndex)
                             select new DynamicPropertyDescriptor(columnName, type);

            return new PropertyDescriptorCollection(properties.ToArray(), readOnly: true);
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        private class DynamicPropertyDescriptor : PropertyDescriptor
        {
            private static readonly Attribute[] _empty = new Attribute[0];
            private readonly Type _type;

            public DynamicPropertyDescriptor(string name, Type type)
                : base(name, _empty)
            {
                _type = type;
            }

            public override Type ComponentType
            {
                get { return typeof(DynamicRecord); }
            }

            public override bool IsReadOnly
            {
                get { return true; }
            }

            public override Type PropertyType
            {
                get { return _type; }
            }

            public override bool CanResetValue(object component)
            {
                return false;
            }

            public override object GetValue(object component)
            {
                DynamicRecord record = component as DynamicRecord;
                // REVIEW: Should we throw if the wrong object was passed in?
                if (record != null)
                {
                    return record[Name];
                }
                return null;
            }

            public override void ResetValue(object component)
            {
                throw new InvalidOperationException(
                    String.Format(CultureInfo.CurrentCulture,
                                  DataResources.RecordIsReadOnly, Name));
            }

            public override void SetValue(object component, object value)
            {
                throw new InvalidOperationException(
                    String.Format(CultureInfo.CurrentCulture,
                                  DataResources.RecordIsReadOnly, Name));
            }

            public override bool ShouldSerializeValue(object component)
            {
                return false;
            }
        }
    }
}
