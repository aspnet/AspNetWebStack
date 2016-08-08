// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.WebPages.Scope;
using Moq;

namespace System.Web.Mvc.Html.Test
{
    /// <summary>
    /// A scope within which it is safe to invoke <see cref="TemplateHelpers"/> methods. For example
    /// <see cref="TemplateHelpers.ExecuteTemplate()"/> invokes <code>ViewEngines.Engines.FindPartialView()</code> and
    /// clones the current <see cref="ViewContext"/>.
    /// </summary>
    /// <remarks>Similar to TemplateHelpersTest.MockViewEngine but FindPartialView() succeed there and fail here. In
    /// addition TemplateHelpersTest tests do not continue far enough to need the transient scope.
    /// </remarks>
    public class TemplateHelpersSafeScope : IDisposable
    {
        private readonly List<IViewEngine> _oldEngines;
        private IDisposable _transientScope;

        public TemplateHelpersSafeScope()
        {
            // Copying an HtmlHelper instance reads and writes the current StorageScope.
            // Ensure that's not the global scope.
            _transientScope = ScopeStorage.CreateTransientScope();

            // Do not want templates to check disk for anything.
            var engine = new Mock<IViewEngine>(MockBehavior.Strict);
            engine
                .Setup(e => e.FindPartialView(It.IsAny<ControllerContext>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(new ViewEngineResult(Enumerable.Empty<string>()));

            _oldEngines = ViewEngines.Engines.ToList();
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(engine.Object);
        }

        public void Dispose()
        {
            ViewEngines.Engines.Clear();
            foreach (var oldEngine in _oldEngines)
            {
                ViewEngines.Engines.Add(oldEngine);
            }

            using (_transientScope)
            {
                _transientScope = null;
            }
        }
    }
}
