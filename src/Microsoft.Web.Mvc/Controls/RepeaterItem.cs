// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;
using System.Web.UI;

namespace Microsoft.Web.Mvc.Controls
{
    public class RepeaterItem : Control, IDataItemContainer, IViewDataContainer
    {
        private object _dataItem;
        private int _itemIndex;

        public RepeaterItem(int itemIndex, object dataItem)
        {
            _itemIndex = itemIndex;
            _dataItem = dataItem;
        }

        public object DataItem
        {
            get { return _dataItem; }
        }

        public int DataItemIndex
        {
            get { return _itemIndex; }
        }

        public int DisplayIndex
        {
            get { return _itemIndex; }
        }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "This is intended to be settable for unit testing purposes.")]
        public ViewDataDictionary ViewData { get; set; }
    }
}
