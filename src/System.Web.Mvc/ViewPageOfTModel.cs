// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace System.Web.Mvc
{
    public class ViewPage<TModel> : ViewPage
    {
        private ViewDataDictionary<TModel> _viewData;

        public new AjaxHelper<TModel> Ajax { get; set; }

        public new HtmlHelper<TModel> Html { get; set; }

        public new TModel Model
        {
            get { return ViewData.Model; }
        }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "This is settable for unit testing purposes")]
        public new ViewDataDictionary<TModel> ViewData
        {
            get
            {
                if (_viewData == null)
                {
                    SetViewData(new ViewDataDictionary<TModel>());
                }
                return _viewData;
            }
            set { SetViewData(value); }
        }

        public override void InitHelpers()
        {
            base.InitHelpers();

            Ajax = new AjaxHelper<TModel>(ViewContext, this);
            Html = new HtmlHelper<TModel>(ViewContext, this);
        }

        protected override void SetViewData(ViewDataDictionary viewData)
        {
            _viewData = new ViewDataDictionary<TModel>(viewData);

            base.SetViewData(_viewData);
        }
    }
}
