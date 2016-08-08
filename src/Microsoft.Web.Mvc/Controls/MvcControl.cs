// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Mvc;
using System.Web.UI;

namespace Microsoft.Web.Mvc.Controls
{
    // TODO: Consider using custom HTML writer instead of the default one to get prettier rendering

    public abstract class MvcControl : Control, IAttributeAccessor
    {
        private IDictionary<string, string> _attributes;
        private IViewDataContainer _viewDataContainer;
        private ViewContext _viewContext;

        [Browsable(false)]
        public IDictionary<string, string> Attributes
        {
            get
            {
                EnsureAttributes();
                return _attributes;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override bool EnableViewState
        {
            get { return base.EnableViewState; }
            set { base.EnableViewState = value; }
        }

        public ViewContext ViewContext
        {
            get
            {
                if (_viewContext == null)
                {
                    // TODO: Is this logic correct? Why not just case Page to ViewPage?
                    Control parent = Parent;
                    while (parent != null)
                    {
                        ViewPage viewPage = parent as ViewPage;
                        if (viewPage != null)
                        {
                            _viewContext = viewPage.ViewContext;
                            break;
                        }
                        parent = parent.Parent;
                    }
                }
                return _viewContext;
            }
        }

        public IViewDataContainer ViewDataContainer
        {
            get
            {
                if (_viewDataContainer == null)
                {
                    Control parent = Parent;
                    while (parent != null)
                    {
                        _viewDataContainer = parent as IViewDataContainer;
                        if (_viewDataContainer != null)
                        {
                            break;
                        }
                        parent = parent.Parent;
                    }
                }
                return _viewDataContainer;
            }
        }

        public ViewDataDictionary ViewData
        {
            get
            {
                IViewDataContainer vdc = ViewDataContainer;
                return (vdc == null) ? null : vdc.ViewData;
            }
        }

        private void EnsureAttributes()
        {
            if (_attributes == null)
            {
                _attributes = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        protected virtual string GetAttribute(string key)
        {
            EnsureAttributes();
            string value;
            _attributes.TryGetValue(key, out value);
            return value;
        }

        protected virtual void SetAttribute(string key, string value)
        {
            EnsureAttributes();
            _attributes[key] = value;
        }

        #region IAttributeAccessor Members

        string IAttributeAccessor.GetAttribute(string key)
        {
            return GetAttribute(key);
        }

        void IAttributeAccessor.SetAttribute(string key, string value)
        {
            SetAttribute(key, value);
        }

        #endregion
    }
}
