// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Web.TestUtil;
using System.Web.WebPages;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Helpers.Test
{
    public class WebGridTest
    {
        [Fact]
        public void AjaxCheckedOnlyOnce()
        {
            var grid = new WebGrid(GetContext(), ajaxUpdateContainerId: "grid")
                .Bind(new[] { new { First = "First", Second = "Second" } });
            string html = grid.Table().ToString();
            Assert.Contains("<script", html);
            html = grid.Table().ToString();
            Assert.DoesNotContain("<script", html);
            html = grid.Pager().ToString();
            Assert.DoesNotContain("<script", html);
        }

        [Fact]
        public void AjaxCallbackIgnoredIfAjaxUpdateContainerIdIsNotSet()
        {
            var grid = new WebGrid(GetContext(), ajaxUpdateCallback: "myCallback")
                .Bind(new[] { new { First = "First", Second = "Second" } });
            string html = grid.Table().ToString();
            Assert.DoesNotContain("<script", html);
            Assert.DoesNotContain("myCallback", html);
        }

        [Fact]
        public void ColumnNameDefaultsExcludesIndexedProperties()
        {
            var grid = new WebGrid(GetContext()).Bind(new[] { "First", "Second" });
            Assert.Single(grid.ColumnNames);
            Assert.Contains("Length", grid.ColumnNames);
        }

        [Fact]
        public void ColumnNameDefaultsForDynamics()
        {
            var grid = new WebGrid(GetContext()).Bind(Dynamics(new { First = "First", Second = "Second" }));
            Assert.Equal(2, grid.ColumnNames.Count());
            Assert.Contains("First", grid.ColumnNames);
            Assert.Contains("Second", grid.ColumnNames);
        }

        [Fact]
        public void ColumnNameDefaultsForNonDynamics()
        {
            var grid = new WebGrid(GetContext()).Bind(new[] { new { First = "First", Second = "Second" } });
            Assert.Equal(2, grid.ColumnNames.Count());
            Assert.Contains("First", grid.ColumnNames);
            Assert.Contains("Second", grid.ColumnNames);
        }

        [Fact]
        public void ColumnNameDefaultsSupportsBindableTypes()
        {
            var grid = new WebGrid(GetContext()).Bind(new[]
            {
                new
                {
                    DateTime = DateTime.MinValue,
                    DateTimeOffset = DateTimeOffset.MinValue,
                    Decimal = Decimal.MinValue,
                    Guid = Guid.Empty,
                    Int32 = 1,
                    NullableInt32 = (int?)1,
                    Object = new object(),
                    Projection = new { Foo = "Bar" },
                    TimeSpan = TimeSpan.MinValue
                }
            });
            Assert.Equal(7, grid.ColumnNames.Count());
            Assert.Contains("DateTime", grid.ColumnNames);
            Assert.Contains("DateTimeOffset", grid.ColumnNames);
            Assert.Contains("Decimal", grid.ColumnNames);
            Assert.Contains("Guid", grid.ColumnNames);
            Assert.Contains("Int32", grid.ColumnNames);
            Assert.Contains("NullableInt32", grid.ColumnNames);
            Assert.Contains("TimeSpan", grid.ColumnNames);
            Assert.DoesNotContain("Object", grid.ColumnNames);
            Assert.DoesNotContain("Projection", grid.ColumnNames);
        }

        [Fact]
        public void ColumnsIsNoOp()
        {
            var grid = new WebGrid(GetContext()).Bind(new[]
            {
                new { First = "First", Second = "Second" }
            });
            var columns = new[]
            {
                grid.Column("First"), grid.Column("Second")
            };
            Assert.Equal(columns, grid.Columns(columns));
        }

        [Fact]
        public void ColumnThrowsIfColumnNameIsEmptyAndNoFormat()
        {
            var grid = new WebGrid(GetContext()).Bind(new object[0]);
            Assert.ThrowsArgument(() => { grid.Column(columnName: String.Empty, format: null); }, "columnName", "The column name cannot be null or an empty string unless a custom format is specified.");
        }

        [Fact]
        public void ColumnThrowsIfColumnNameIsNullAndNoFormat()
        {
            var grid = new WebGrid(GetContext()).Bind(new object[0]);
            Assert.ThrowsArgument(() => { grid.Column(columnName: null, format: null); }, "columnName", "The column name cannot be null or an empty string unless a custom format is specified.");
        }

        [Fact]
        public void BindThrowsIfSourceIsNull()
        {
            Assert.ThrowsArgumentNull(() => { new WebGrid(GetContext()).Bind(null); }, "source");
        }

        [Fact]
        public void ConstructorThrowsIfRowsPerPageIsLessThanOne()
        {
            Assert.ThrowsArgumentOutOfRange(() => { new WebGrid(GetContext(), rowsPerPage: 0); }, "rowsPerPage", "Value must be greater than or equal to 1.", allowDerivedExceptions: true);
        }

        [Fact]
        public void GetHtmlDefaults()
        {
            var grid = new WebGrid(GetContext(), rowsPerPage: 1)
                .Bind(new[]
                {
                    new { P1 = 1, P2 = '2', P3 = "3" },
                    new { P1 = 4, P2 = '5', P3 = "6" }
                });
            var html = grid.GetHtml();
            UnitTestHelper.AssertEqualsIgnoreWhitespace(
                "<table><thead><tr>" +
                "<th scope=\"col\"><a href=\"?sort=P1&amp;sortdir=ASC\">P1</a></th>" +
                "<th scope=\"col\"><a href=\"?sort=P2&amp;sortdir=ASC\">P2</a></th>" +
                "<th scope=\"col\"><a href=\"?sort=P3&amp;sortdir=ASC\">P3</a></th>" +
                "</tr></thead>" +
                "<tfoot><tr>" +
                "<td colspan=\"3\">1 <a href=\"?page=2\">2</a> <a href=\"?page=2\">&gt;</a> </td>" +
                "</tr></tfoot>" +
                "<tbody><tr><td>1</td><td>2</td><td>3</td></tr></tbody>" +
                "</table>", html.ToString());
            XhtmlAssert.Validate1_1(html);
        }

        [Fact]
        public void WebGridProducesValidHtmlWhenSummaryIsSpecified()
        {
            var grid = new WebGrid(GetContext(), rowsPerPage: 1)
                .Bind(new[]
                {
                    new { P1 = 1, P2 = '2', P3 = "3" },
                    new { P1 = 4, P2 = '5', P3 = "6" }
                });
            var caption = "WebGrid With Caption";
            var html = grid.GetHtml(caption: caption);
            UnitTestHelper.AssertEqualsIgnoreWhitespace(
                "<table>" +
                "<caption>" + caption + "</caption>" +
                "<thead><tr>" +
                "<th scope=\"col\"><a href=\"?sort=P1&amp;sortdir=ASC\">P1</a></th>" +
                "<th scope=\"col\"><a href=\"?sort=P2&amp;sortdir=ASC\">P2</a></th>" +
                "<th scope=\"col\"><a href=\"?sort=P3&amp;sortdir=ASC\">P3</a></th>" +
                "</tr></thead>" +
                "<tfoot><tr>" +
                "<td colspan=\"3\">1 <a href=\"?page=2\">2</a> <a href=\"?page=2\">&gt;</a> </td>" +
                "</tr></tfoot>" +
                "<tbody><tr><td>1</td><td>2</td><td>3</td></tr></tbody>" +
                "</table>", html.ToString());
            XhtmlAssert.Validate1_1(html);
        }

        [Fact]
        public void WebGridEncodesCaptionText()
        {
            var grid = new WebGrid(GetContext(), rowsPerPage: 1)
                .Bind(new[]
                {
                    new { P1 = 1, P2 = '2', P3 = "3" },
                    new { P1 = 4, P2 = '5', P3 = "6" }
                });
            var caption = "WebGrid <> With Caption";
            var html = grid.GetHtml(caption: caption);
            UnitTestHelper.AssertEqualsIgnoreWhitespace(
                "<table>" +
                "<caption>WebGrid &lt;&gt; With Caption</caption>" +
                "<thead><tr>" +
                "<th scope=\"col\"><a href=\"?sort=P1&amp;sortdir=ASC\">P1</a></th>" +
                "<th scope=\"col\"><a href=\"?sort=P2&amp;sortdir=ASC\">P2</a></th>" +
                "<th scope=\"col\"><a href=\"?sort=P3&amp;sortdir=ASC\">P3</a></th>" +
                "</tr></thead>" +
                "<tfoot><tr>" +
                "<td colspan=\"3\">1 <a href=\"?page=2\">2</a> <a href=\"?page=2\">&gt;</a> </td>" +
                "</tr></tfoot>" +
                "<tbody><tr><td>1</td><td>2</td><td>3</td></tr></tbody>" +
                "</table>", html.ToString());
            XhtmlAssert.Validate1_1(html);
        }

        [Fact]
        public void GetHtmlWhenPageCountIsOne()
        {
            var grid = new WebGrid(GetContext())
                .Bind(new[]
                {
                    new { P1 = 1, P2 = '2', P3 = "3" }
                });
            var html = grid.GetHtml();
            UnitTestHelper.AssertEqualsIgnoreWhitespace(
                "<table><thead><tr>" +
                "<th scope=\"col\"><a href=\"?sort=P1&amp;sortdir=ASC\">P1</a></th>" +
                "<th scope=\"col\"><a href=\"?sort=P2&amp;sortdir=ASC\">P2</a></th>" +
                "<th scope=\"col\"><a href=\"?sort=P3&amp;sortdir=ASC\">P3</a></th>" +
                "</tr></thead>" +
                "<tbody><tr><td>1</td><td>2</td><td>3</td></tr></tbody>" +
                "</table>", html.ToString());
            XhtmlAssert.Validate1_1(html);
        }

        [Fact]
        public void GetHtmlWhenPagingAndSortingAreDisabled()
        {
            var grid = new WebGrid(GetContext(), rowsPerPage: 1, canPage: false, canSort: false)
                .Bind(new[]
                {
                    new { P1 = 1, P2 = '2', P3 = "3" },
                    new { P1 = 4, P2 = '5', P3 = "6" }
                });
            var html = grid.GetHtml();
            UnitTestHelper.AssertEqualsIgnoreWhitespace(
                "<table><thead><tr>" +
                "<th scope=\"col\">P1</th>" +
                "<th scope=\"col\">P2</th>" +
                "<th scope=\"col\">P3</th>" +
                "</tr></thead>" +
                "<tbody>" +
                "<tr><td>1</td><td>2</td><td>3</td></tr>" +
                "<tr><td>4</td><td>5</td><td>6</td></tr>" +
                "</tbody>" +
                "</table>", html.ToString());
            XhtmlAssert.Validate1_1(html);
        }

        [Fact]
        public void PageIndexCanBeReset()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["page"] = "2";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 1)
                .Bind(new[]
                {
                    new { P1 = 1, P2 = '2', P3 = "3" },
                    new { P1 = 4, P2 = '5', P3 = "6" }
                });
            Assert.Equal(1, grid.PageIndex);
            grid.PageIndex = 0;
            Assert.Equal(0, grid.PageIndex);
            // verify that selection link has updated page
            Assert.Equal("?page=1&row=1", grid.Rows.FirstOrDefault().GetSelectUrl());
        }

        [Fact]
        public void PageIndexCanBeResetToSameValue()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["page"] = "2";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 1)
                .Bind(new[]
                {
                    new { P1 = 1, P2 = '2', P3 = "3" },
                    new { P1 = 4, P2 = '5', P3 = "6" }
                });
            grid.PageIndex = 0;
            Assert.Equal(0, grid.PageIndex);
        }

        [Fact]
        public void PageIndexDefaultsToZero()
        {
            var grid = new WebGrid(GetContext(), rowsPerPage: 1)
                .Bind(new[]
                {
                    new { P1 = 1, P2 = '2', P3 = "3" },
                    new { P1 = 4, P2 = '5', P3 = "6" }
                });
            Assert.Equal(0, grid.PageIndex);
            Assert.Equal(1, grid.Rows.Count);
            Assert.Equal(1, grid.Rows.First()["P1"]);
        }

        [Fact]
        public void SetPageIndexThrowsExceptionWhenValueIsNegative()
        {
            var grid = new WebGrid(GetContext(), rowsPerPage: 1)
                .Bind(new[]
                {
                    new { P1 = 1, P2 = '2', P3 = "3" },
                    new { P1 = 4, P2 = '5', P3 = "6" }
                });
            Assert.ThrowsArgumentOutOfRange(() => { grid.PageIndex = -1; }, "value", "Value must be between 0 and 1.");
        }

        [Fact]
        public void SetPageIndexThrowsExceptionWhenValueIsEqualToPageCount()
        {
            var grid = new WebGrid(GetContext(), rowsPerPage: 1)
                .Bind(new[]
                {
                    new { P1 = 1, P2 = '2', P3 = "3" },
                    new { P1 = 4, P2 = '5', P3 = "6" }
                });
            Assert.ThrowsArgumentOutOfRange(() => { grid.PageIndex = grid.PageCount; }, "value", "Value must be between 0 and 1.");
        }

        [Fact]
        public void SetPageIndexThrowsExceptionWhenValueIsGreaterToPageCount()
        {
            var grid = new WebGrid(GetContext(), rowsPerPage: 1)
                .Bind(new[]
                {
                    new { P1 = 1, P2 = '2', P3 = "3" },
                    new { P1 = 4, P2 = '5', P3 = "6" }
                });
            Assert.ThrowsArgumentOutOfRange(() => { grid.PageIndex = grid.PageCount + 1; }, "value", "Value must be between 0 and 1.");
        }

        [Fact]
        public void SetPageIndexThrowsExceptionWhenPagingIsDisabled()
        {
            var grid = new WebGrid(GetContext(), canPage: false)
                .Bind(new[]
                {
                    new { P1 = 1, P2 = '2', P3 = "3" },
                    new { P1 = 4, P2 = '5', P3 = "6" }
                });
            Assert.Throws<NotSupportedException>(() => { grid.PageIndex = 1; }, "This operation is not supported when paging is disabled for the \"WebGrid\" object.");
        }

        [Fact]
        public void PageIndexResetsToLastPageWhenQueryStringValueGreaterThanPageCount()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["page"] = "3";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 1)
                .Bind(new[]
                {
                    new { P1 = 1, P2 = '2', P3 = "3" },
                    new { P1 = 4, P2 = '5', P3 = "6" }
                });
            Assert.Equal(1, grid.PageIndex);
            Assert.Equal(1, grid.Rows.Count);
            Assert.Equal(4, grid.Rows.First()["P1"]);
        }

        [Fact]
        public void PageIndexResetWhenQueryStringValueIsInvalid()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["page"] = "NotAnInt";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 1)
                .Bind(new[]
                {
                    new { P1 = 1, P2 = '2', P3 = "3" },
                    new { P1 = 4, P2 = '5', P3 = "6" }
                });
            Assert.Equal(0, grid.PageIndex);
            Assert.Equal(1, grid.Rows.Count);
            Assert.Equal(1, grid.Rows.First()["P1"]);
        }

        [Fact]
        public void PageIndexResetWhenQueryStringValueLessThanOne()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["page"] = "0";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 1)
                .Bind(new[]
                {
                    new { P1 = 1, P2 = '2', P3 = "3" },
                    new { P1 = 4, P2 = '5', P3 = "6" }
                });
            Assert.Equal(0, grid.PageIndex);
            Assert.Equal(1, grid.Rows.Count);
            Assert.Equal(1, grid.Rows.First()["P1"]);
        }

        [Fact]
        public void PageIndexUsesCustomQueryString()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["g_pg"] = "2";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 1, fieldNamePrefix: "g_", pageFieldName: "pg")
                .Bind(new[]
                {
                    new { P1 = 1, P2 = '2', P3 = "3" },
                    new { P1 = 4, P2 = '5', P3 = "6" }
                });
            Assert.Equal(1, grid.PageIndex);
            Assert.Equal(1, grid.Rows.Count);
            Assert.Equal(4, grid.Rows.First()["P1"]);
        }

        [Fact]
        public void PageIndexUsesQueryString()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["page"] = "2";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 1)
                .Bind(new[]
                {
                    new { P1 = 1, P2 = '2', P3 = "3" },
                    new { P1 = 4, P2 = '5', P3 = "6" }
                });
            Assert.Equal(1, grid.PageIndex);
            Assert.Equal(1, grid.Rows.Count);
            Assert.Equal(4, grid.Rows.First()["P1"]);
        }

        [Fact]
        public void GetPageCountWhenPagingIsTurnedOn()
        {
            var grid = new WebGrid(GetContext(), canPage: true, rowsPerPage: 1)
                .Bind(new[]
                {
                    new { P1 = 1, P2 = '2', P3 = "3" },
                    new { P1 = 4, P2 = '5', P3 = "6" }
                });
            Assert.Equal(2, grid.PageCount);
        }

        [Fact]
        public void GetPageIndexWhenPagingIsTurnedOn()
        {
            var grid = new WebGrid(GetContext(), canPage: true, rowsPerPage: 1)
                .Bind(new[]
                {
                    new { P1 = 1, P2 = '2', P3 = "3" },
                    new { P1 = 4, P2 = '5', P3 = "6" },
                    new { P1 = 4, P2 = '5', P3 = "6" },
                });
            grid.PageIndex = 1;
            Assert.Equal(1, grid.PageIndex);
            Assert.Equal(3, grid.PageCount);
            grid.PageIndex = 2;
            Assert.Equal(2, grid.PageIndex);
        }

        [Fact]
        public void GetPageCountWhenPagingIsTurnedOff()
        {
            var grid = new WebGrid(GetContext(), canPage: false, rowsPerPage: 1)
                .Bind(new[]
                {
                    new { P1 = 1, P2 = '2', P3 = "3" },
                    new { P1 = 4, P2 = '5', P3 = "6" }
                });
            Assert.Equal(1, grid.PageCount);
        }

        [Fact]
        public void GetPageIndexWhenPagingIsTurnedOff()
        {
            var grid = new WebGrid(GetContext(), canPage: false, rowsPerPage: 1)
                .Bind(new[]
                {
                    new { P1 = 1, P2 = '2', P3 = "3" },
                    new { P1 = 4, P2 = '5', P3 = "6" },
                    new { P1 = 4, P2 = '5', P3 = "6" },
                });
            Assert.Equal(0, grid.PageIndex);
            Assert.Equal(1, grid.PageCount);
        }

        [Fact]
        public void PageUrlResetsSelection()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["page"] = "0";
            queryString["row"] = "0";
            queryString["sort"] = "P1";
            queryString["sortdir"] = "DESC";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 1)
                .Bind(new[]
                {
                    new { P1 = 1, P2 = '2', P3 = "3" },
                    new { P1 = 4, P2 = '5', P3 = "6" }
                });
            string url = grid.GetPageUrl(1);
            Assert.Equal("?page=2&sort=P1&sortdir=DESC", url);
        }

        [Fact]
        public void PageUrlResetsSelectionForAjax()
        {
            string expected40 = "$(&quot;#grid-container&quot;).swhgLoad(&quot;?page=2&amp;sort=P1&amp;sortdir=DESC&quot;,&quot;#grid-container&quot;);";
            string expected45 = "$(&quot;#grid-container&quot;).swhgLoad(&quot;?page=2\\u0026sort=P1\\u0026sortdir=DESC&quot;,&quot;#grid-container&quot;);";
            NameValueCollection queryString = new NameValueCollection();
            queryString["page"] = "0";
            queryString["row"] = "0";
            queryString["sort"] = "P1";
            queryString["sortdir"] = "DESC";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 1, ajaxUpdateContainerId: "grid-container")
                .Bind(new[]
                {
                    new { P1 = 1, P2 = '2', P3 = "3" },
                    new { P1 = 4, P2 = '5', P3 = "6" }
                });
            string html = grid.GetContainerUpdateScript(grid.GetPageUrl(1)).ToString();

            // Assert
            Assert.Equal(RuntimeEnvironment.IsVersion45Installed ? expected45 : expected40, html);
        }

        [Fact]
        public void PageUrlResetsSelectionForAjaxWithCallback()
        {
            string expected40 = "$(&quot;#grid&quot;).swhgLoad(&quot;?page=2&amp;sort=P1&amp;sortdir=DESC&quot;,&quot;#grid&quot;,myCallback);";
            string expected45 = "$(&quot;#grid&quot;).swhgLoad(&quot;?page=2\\u0026sort=P1\\u0026sortdir=DESC&quot;,&quot;#grid&quot;,myCallback);";
            NameValueCollection queryString = new NameValueCollection();
            queryString["page"] = "0";
            queryString["row"] = "0";
            queryString["sort"] = "P1";
            queryString["sortdir"] = "DESC";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 1, ajaxUpdateContainerId: "grid", ajaxUpdateCallback: "myCallback")
                .Bind(new[]
                {
                    new { P1 = 1, P2 = '2', P3 = "3" },
                    new { P1 = 4, P2 = '5', P3 = "6" }
                });
            string html = grid.GetContainerUpdateScript(grid.GetPageUrl(1)).ToString();

            // Assert
            Assert.Equal(RuntimeEnvironment.IsVersion45Installed ? expected45 : expected40, html);
        }

        [Fact]
        public void PageUrlThrowsIfIndexGreaterThanOrEqualToPageCount()
        {
            var grid = new WebGrid(GetContext(), rowsPerPage: 1).Bind(new[] { new { }, new { } });
            Assert.ThrowsArgumentOutOfRange(() => { grid.GetPageUrl(2); }, "pageIndex", "Value must be between 0 and 1.");
        }

        [Fact]
        public void PageUrlThrowsIfIndexLessThanZero()
        {
            var grid = new WebGrid(GetContext(), rowsPerPage: 1).Bind(new[] { new { }, new { } });
            Assert.ThrowsArgumentOutOfRange(() => { grid.GetPageUrl(-1); }, "pageIndex", "Value must be between 0 and 1.");
        }

        [Fact]
        public void PageUrlThrowsIfPagingIsDisabled()
        {
            var grid = new WebGrid(GetContext(), rowsPerPage: 1, canPage: false).Bind(new[] { new { }, new { } });
            Assert.Throws<NotSupportedException>(() => { grid.GetPageUrl(2); }, "This operation is not supported when paging is disabled for the \"WebGrid\" object.");
        }

        [Fact]
        public void PagerRenderingDefaults()
        {
            var grid = new WebGrid(GetContext(), rowsPerPage: 1).Bind(new[] { new { }, new { }, new { }, new { } });
            var html = grid.Pager();
            Assert.Equal(
                "1 " +
                "<a href=\"?page=2\">2</a> " +
                "<a href=\"?page=3\">3</a> " +
                "<a href=\"?page=4\">4</a> " +
                "<a href=\"?page=2\">&gt;</a> ",
                html.ToString());
            XhtmlAssert.Validate1_1(html, wrapper: "div");
        }

        [Fact]
        public void PagerRenderingOnFirstShowingAll()
        {
            var grid = new WebGrid(GetContext(), rowsPerPage: 1).Bind(new[] { new { }, new { }, new { }, new { } });
            var html = grid.Pager(WebGridPagerModes.All, numericLinksCount: 5);
            Assert.Equal(
                "1 " +
                "<a href=\"?page=2\">2</a> " +
                "<a href=\"?page=3\">3</a> " +
                "<a href=\"?page=4\">4</a> " +
                "<a href=\"?page=2\">&gt;</a> " +
                "<a href=\"?page=4\">&gt;&gt;</a>",
                html.ToString());
            XhtmlAssert.Validate1_1(html, wrapper: "div");
        }

        [Fact]
        public void PagerRenderingOnNextToLastShowingAll()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["page"] = "3";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 1).Bind(new[]
            {
                new { }, new { }, new { }, new { }
            });
            var html = grid.Pager(WebGridPagerModes.All, numericLinksCount: 4);
            Assert.Equal(
                "<a href=\"?page=1\">&lt;&lt;</a> " +
                "<a href=\"?page=2\">&lt;</a> " +
                "<a href=\"?page=1\">1</a> " +
                "<a href=\"?page=2\">2</a> " +
                "3 " +
                "<a href=\"?page=4\">4</a> " +
                "<a href=\"?page=4\">&gt;</a> ",
                html.ToString());
            XhtmlAssert.Validate1_1(html, wrapper: "div");
        }

        [Fact]
        public void PagerRenderingOnMiddleShowingAll()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["page"] = "3";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 1).Bind(new[]
            {
                new { }, new { }, new { }, new { }
            });
            var html = grid.Pager(WebGridPagerModes.All, numericLinksCount: 3);
            Assert.Equal(
                "<a href=\"?page=1\">&lt;&lt;</a> " +
                "<a href=\"?page=2\">&lt;</a> " +
                "<a href=\"?page=2\">2</a> " +
                "3 " +
                "<a href=\"?page=4\">4</a> " +
                "<a href=\"?page=4\">&gt;</a> ",
                html.ToString());
            XhtmlAssert.Validate1_1(html, wrapper: "div");
        }

        [Fact]
        public void PagerRenderingOnSecondHidingFirstLast()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["page"] = "2";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 1).Bind(new[]
            {
                new { }, new { }, new { }, new { }
            });
            var html = grid.Pager(WebGridPagerModes.NextPrevious | WebGridPagerModes.Numeric, numericLinksCount: 2);
            Assert.Equal(
                "<a href=\"?page=1\">&lt;</a> " +
                "2 " +
                "<a href=\"?page=3\">3</a> " +
                "<a href=\"?page=3\">&gt;</a> ",
                html.ToString());
            XhtmlAssert.Validate1_1(html, wrapper: "div");
        }

        [Fact]
        public void PagerRenderingOnLastHidingFirstLast()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["page"] = "4";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 1).Bind(new[]
            {
                new { }, new { }, new { }, new { }
            });
            var html = grid.Pager(WebGridPagerModes.NextPrevious | WebGridPagerModes.Numeric, numericLinksCount: 1);
            Assert.Equal(
                "<a href=\"?page=3\">&lt;</a> " +
                "4 ",
                html.ToString());
            XhtmlAssert.Validate1_1(html, wrapper: "div");
        }

        [Fact]
        public void PagerRenderingOnMiddleHidingNextPrevious()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["page"] = "3";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 1).Bind(new[]
            {
                new { }, new { }, new { }, new { }
            });
            var html = grid.Pager(WebGridPagerModes.FirstLast | WebGridPagerModes.Numeric, numericLinksCount: 0);
            Assert.Equal(
                "<a href=\"?page=1\">&lt;&lt;</a> ",
                html.ToString());
            XhtmlAssert.Validate1_1(html, wrapper: "div");
        }

        [Fact]
        public void PagerRenderingOnMiddleWithLinksCountGreaterThanPageCount()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["page"] = "3";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 1).Bind(new[]
            {
                new { }, new { }, new { }, new { }
            });
            var html = grid.Pager(WebGridPagerModes.Numeric, numericLinksCount: 6);
            Assert.Equal(
                "<a href=\"?page=1\">1</a> " +
                "<a href=\"?page=2\">2</a> " +
                "3 " +
                "<a href=\"?page=4\">4</a> ",
                html.ToString());
            XhtmlAssert.Validate1_1(html, wrapper: "div");
        }

        [Fact]
        public void PagerRenderingHidingAll()
        {
            var grid = new WebGrid(GetContext(), rowsPerPage: 2).Bind(new[]
            {
                new { }, new { }, new { }, new { }
            });
            var html = grid.Pager(WebGridPagerModes.Numeric, numericLinksCount: 0);
            Assert.Equal("", html.ToString());
        }

        [Fact]
        public void PagerRenderingTextOverrides()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["page"] = "3";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 1).Bind(new[]
            {
                new { }, new { }, new { }, new { }, new { }
            });
            var html = grid.Pager(WebGridPagerModes.FirstLast | WebGridPagerModes.NextPrevious,
                                  firstText: "first", previousText: "previous", nextText: "next", lastText: "last");
            Assert.Equal(
                "<a href=\"?page=1\">first</a> " +
                "<a href=\"?page=2\">previous</a> " +
                "<a href=\"?page=4\">next</a> " +
                "<a href=\"?page=5\">last</a>",
                html.ToString());
            XhtmlAssert.Validate1_1(html, wrapper: "div");
        }

        [Fact]
        public void PagerThrowsIfTextSetAndModeNotEnabled()
        {
            var grid = new WebGrid(GetContext(), rowsPerPage: 1).Bind(new[] { new { }, new { } });
            Assert.ThrowsArgument(() => { grid.Pager(firstText: "first"); }, "firstText", "To use this argument, pager mode \"FirstLast\" must be enabled.");
            Assert.ThrowsArgument(() => { grid.Pager(mode: WebGridPagerModes.Numeric, previousText: "previous"); }, "previousText", "To use this argument, pager mode \"NextPrevious\" must be enabled.");
            Assert.ThrowsArgument(() => { grid.Pager(mode: WebGridPagerModes.Numeric, nextText: "next"); }, "nextText", "To use this argument, pager mode \"NextPrevious\" must be enabled.");
            Assert.ThrowsArgument(() => { grid.Pager(lastText: "last"); }, "lastText", "To use this argument, pager mode \"FirstLast\" must be enabled.");
        }

        [Fact]
        public void PagerThrowsIfNumericLinkCountIsLessThanZero()
        {
            var grid = new WebGrid(GetContext(), rowsPerPage: 1).Bind(new[] { new { }, new { } });
            Assert.ThrowsArgumentOutOfRange(() => { grid.Pager(numericLinksCount: -1); }, "numericLinksCount", "Value must be greater than or equal to 0.");
        }

        [Fact]
        public void PagerThrowsIfPagingIsDisabled()
        {
            var grid = new WebGrid(GetContext(), rowsPerPage: 1, canPage: false).Bind(new[] { new { }, new { } });
            Assert.Throws<NotSupportedException>(() => { grid.Pager(); }, "This operation is not supported when paging is disabled for the \"WebGrid\" object.");
        }

        [Fact]
        public void PagerWithAjax()
        {
            var grid = new WebGrid(GetContext(), rowsPerPage: 1, ajaxUpdateContainerId: "grid")
                .Bind(new[]
                {
                    new { P1 = 1, P2 = '2', P3 = "3" },
                    new { P1 = 4, P2 = '5', P3 = "6" }
                });
            string html = grid.Pager().ToString();
            Assert.Contains("<script", html);
        }

        [Fact]
        public void PagerWithAjaxAndCallback()
        {
            var grid = new WebGrid(GetContext(), rowsPerPage: 1, ajaxUpdateContainerId: "grid", ajaxUpdateCallback: "myCallback")
                .Bind(new[]
                {
                    new { P1 = 1, P2 = '2', P3 = "3" },
                    new { P1 = 4, P2 = '5', P3 = "6" }
                });
            string html = grid.Pager().ToString();
            Assert.Contains("<script", html);
            Assert.Contains("data-swhgcallback=\"myCallback\"", html);
        }

        [Fact]
        public void PropertySettersDoNotThrowBeforePagingAndSorting()
        {
            // test with selection because SelectedIndex getter used to do range checking that caused paging and sorting
            NameValueCollection queryString = new NameValueCollection();
            queryString["row"] = "1";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 2).Bind(new[]
            {
                new { P1 = 1 }, new { P1 = 2 }, new { P1 = 3 }
            });

            // invoke other WebGrid properties to ensure they don't cause sorting and paging
            foreach (var prop in typeof(WebGrid).GetProperties())
            {
                // exceptions: these do cause sorting and paging
                if (prop.Name.Equals("Rows") || prop.Name.Equals("SelectedRow") || prop.Name.Equals("ElementType"))
                {
                    continue;
                }
                prop.GetValue(grid, null);
            }

            grid.PageIndex = 1;
            grid.SelectedIndex = 0;
            grid.SortColumn = "P1";
            grid.SortDirection = SortDirection.Descending;
        }

        [Fact]
        public void PropertySettersDoNotThrowAfterPagingAndSortingIfValuesHaveNotChanged()
        {
            var grid = new WebGrid(GetContext(), rowsPerPage: 2).Bind(new[]
            {
                new { P1 = 1 }, new { P1 = 2 }, new { P1 = 3 }
            });
            // calling Rows will sort and page the data
            Assert.Equal(2, grid.Rows.Count());

            grid.PageIndex = 0;
            grid.SelectedIndex = -1;
            grid.SortColumn = String.Empty;
            grid.SortDirection = SortDirection.Ascending;
        }

        [Fact]
        public void PropertySettersThrowAfterPagingAndSorting()
        {
            var grid = new WebGrid(GetContext(), rowsPerPage: 2).Bind(new[]
            {
                new { P1 = 1 }, new { P1 = 2 }, new { P1 = 3 }
            });
            // calling Rows will sort and page the data
            Assert.Equal(2, grid.Rows.Count());

            var message = "This property cannot be set after the \"WebGrid\" object has been sorted or paged. Make sure that this property is set prior to invoking the \"Rows\" property directly or indirectly through other methods such as \"GetHtml\", \"Pager\", \"Table\", etc.";
            Assert.Throws<InvalidOperationException>(() => { grid.PageIndex = 1; }, message);
            Assert.Throws<InvalidOperationException>(() => { grid.SelectedIndex = 0; }, message);
            Assert.Throws<InvalidOperationException>(() => { grid.SortColumn = "P1"; }, message);
            Assert.Throws<InvalidOperationException>(() => { grid.SortDirection = SortDirection.Descending; }, message);
        }

        [Fact]
        public void RowColumnsAreDynamicMembersForDynamics()
        {
            var grid = new WebGrid(GetContext()).Bind(Dynamics(
                new { P1 = 1, P2 = '2', P3 = "3" }
                                                          ));
            dynamic row = grid.Rows.First();
            Assert.Equal(1, row.P1);
            Assert.Equal('2', row.P2);
            Assert.Equal("3", row.P3);
        }

        [Fact]
        public void RowColumnsAreDynamicMembersForNonDynamics()
        {
            var grid = new WebGrid(GetContext()).Bind(new[]
            {
                new { P1 = 1, P2 = '2', P3 = "3" }
            });
            dynamic row = grid.Rows.First();
            Assert.Equal(1, row.P1);
            Assert.Equal('2', row.P2);
            Assert.Equal("3", row.P3);
        }

        [Fact]
        public void RowExposesRowIndex()
        {
            var grid = new WebGrid(GetContext()).Bind(new[]
            {
                new { }, new { }, new { }
            });
            dynamic row = grid.Rows.First();
            Assert.Equal(0, row["ROW"]);
            row = grid.Rows.Skip(1).First();
            Assert.Equal(1, row.ROW);
            row = grid.Rows.Skip(2).First();
            Assert.Equal(2, row.ROW);
        }

        [Fact]
        public void RowExposesUnderlyingValue()
        {
            var sb = new StringBuilder("Foo");
            sb.Append("Bar");
            var grid = new WebGrid(GetContext()).Bind(new[] { sb });
            var row = grid.Rows.First();
            Assert.Equal(sb, row.Value);
            Assert.Equal("FooBar", row.ToString());
            Assert.Equal(grid, row.WebGrid);
        }

        [Fact]
        public void RowIndexerThrowsWhenColumnNameIsEmpty()
        {
            var grid = new WebGrid(GetContext()).Bind(new[] { new { } });
            var row = grid.Rows.First();
            Assert.ThrowsArgumentNullOrEmptyString(() => { var value = row[String.Empty]; }, "name");
        }

        [Fact]
        public void RowIndexerThrowsWhenColumnNameIsNull()
        {
            var grid = new WebGrid(GetContext()).Bind(new[] { new { } });
            var row = grid.Rows.First();
            Assert.ThrowsArgumentNullOrEmptyString(() => { var value = row[null]; }, "name");
        }

        [Fact] // todo - should throw ArgumentException?
        public void RowIndexerThrowsWhenColumnNotFound()
        {
            var grid = new WebGrid(GetContext()).Bind(new[] { new { } });
            var row = grid.Rows.First();
            Assert.Throws<InvalidOperationException>(() => { var value = row["NotAColumn"]; });
        }

        [Fact]
        public void RowIndexerThrowsWhenGreaterThanColumnCount()
        {
            var grid = new WebGrid(GetContext()).Bind(new[]
            {
                new { P1 = 1, P2 = '2', P3 = "3" }
            });
            var row = grid.Rows.First();
            Assert.Throws<ArgumentOutOfRangeException>(() => { var value = row[4]; });
        }

        [Fact]
        public void RowIndexerThrowsWhenLessThanZero()
        {
            var grid = new WebGrid(GetContext()).Bind(new[] { new { } });
            var row = grid.Rows.First();
            Assert.Throws<ArgumentOutOfRangeException>(() => { var value = row[-1]; });
        }

        [Fact]
        public void RowIsEnumerableForDynamics()
        {
            var grid = new WebGrid(GetContext()).Bind(Dynamics(
                new { P1 = 1, P2 = '2', P3 = "3" }
                                                          ));
            int i = 0;
            foreach (var col in (IEnumerable)grid.Rows.First())
            {
                i++;
            }
            Assert.Equal(3, i);
        }

        [Fact]
        public void RowIsEnumerableForNonDynamics()
        {
            var grid = new WebGrid(GetContext()).Bind(new[]
            {
                new { P1 = 1, P2 = '2', P3 = "3" }
            });
            int i = 0;
            foreach (var col in grid.Rows.First())
            {
                i++;
            }
            Assert.Equal(3, i);
        }

        [Fact]
        public void RowIsIndexableByColumnForDynamics()
        {
            var grid = new WebGrid(GetContext()).Bind(Dynamics(
                new { P1 = 1, P2 = '2', P3 = "3" }
                                                          ));
            var row = grid.Rows.First();
            Assert.Equal(1, row["P1"]);
            Assert.Equal('2', row["P2"]);
            Assert.Equal("3", row["P3"]);
        }

        [Fact]
        public void RowIsIndexableByColumnForNonDynamics()
        {
            var grid = new WebGrid(GetContext()).Bind(new[]
            {
                new { P1 = 1, P2 = '2', P3 = "3" }
            });
            var row = grid.Rows.First();
            Assert.Equal(1, row["P1"]);
            Assert.Equal('2', row["P2"]);
            Assert.Equal("3", row["P3"]);
        }

        [Fact]
        public void RowIsIndexableByIndexForDynamics()
        {
            var grid = new WebGrid(GetContext()).Bind(Dynamics(
                new { P1 = 1, P2 = '2', P3 = "3" }
                                                          ));
            var row = grid.Rows.First();
            Assert.Equal(1, row[0]);
            Assert.Equal('2', row[1]);
            Assert.Equal("3", row[2]);
        }

        [Fact]
        public void RowIsIndexableByIndexForNonDynamics()
        {
            var grid = new WebGrid(GetContext()).Bind(new[]
            {
                new { P1 = 1, P2 = '2', P3 = "3" }
            });
            var row = grid.Rows.First();
            Assert.Equal(1, row[0]);
            Assert.Equal('2', row[1]);
            Assert.Equal("3", row[2]);
        }

        [Fact]
        public void RowsNotPagedWhenPagingIsDisabled()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["page"] = "2";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 1, canPage: false)
                .Bind(new[]
                {
                    new { P1 = 1, P2 = '2', P3 = "3" },
                    new { P1 = 4, P2 = '5', P3 = "6" }
                });
            // review: should we reset PageIndex or Sort when operation disabled?
            Assert.Equal(0, grid.PageIndex);
            Assert.Equal(2, grid.Rows.Count);
            Assert.Equal(1, grid.Rows.First()["P1"]);
            Assert.Equal(4, grid.Rows.Skip(1).First()["P1"]);
        }

        [Fact] // todo - should throw ArgumentException?
        public void RowTryGetMemberReturnsFalseWhenColumnNotFound()
        {
            var grid = new WebGrid(GetContext()).Bind(new[] { new { } });
            var row = grid.Rows.First();
            object value = null;
            Assert.False(row.TryGetMember("NotAColumn", out value));
        }

        [Fact]
        public void SelectedIndexCanBeReset()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["row"] = "2";
            var grid = new WebGrid(GetContext(queryString)).Bind(new[]
            {
                new { P1 = 1, P2 = '2', P3 = "3" },
                new { P1 = 4, P2 = '5', P3 = "6" }
            });
            Assert.Equal(1, grid.SelectedIndex);
            grid.SelectedIndex = 0;
            Assert.Equal(0, grid.SelectedIndex);
        }

        [Fact]
        public void SelectedIndexCanBeResetToSameValue()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["row"] = "2";
            var grid = new WebGrid(GetContext(queryString)).Bind(new[]
            {
                new { P1 = 1, P2 = '2', P3 = "3" },
                new { P1 = 4, P2 = '5', P3 = "6" }
            });
            grid.SelectedIndex = -1;
            Assert.Equal(-1, grid.SelectedIndex);
        }

        [Fact]
        public void SelectedIndexDefaultsToNegative()
        {
            var grid = new WebGrid(GetContext()).Bind(new[]
            {
                new { P1 = 1, P2 = '2', P3 = "3" },
                new { P1 = 4, P2 = '5', P3 = "6" }
            });
            Assert.False(grid.HasSelection);
            Assert.Equal(-1, grid.SelectedIndex);
            Assert.Null(grid.SelectedRow);
        }

        [Fact]
        public void SelectedIndexResetWhenQueryStringValueGreaterThanRowsPerPage()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["row"] = "3";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 2).Bind(new[]
            {
                new { P1 = 1, P2 = '2', P3 = "3" },
                new { P1 = 4, P2 = '5', P3 = "6" }
            });
            Assert.False(grid.HasSelection);
            Assert.Equal(-1, grid.SelectedIndex);
            Assert.Null(grid.SelectedRow);
        }

        [Fact]
        public void SelectedIndexPersistsWhenPagingTurnedOff()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["row"] = "3";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 2, canPage: false).Bind(new[]
            {
                new { }, new { }, new { }, new { }
            });
            grid.SelectedIndex = 3;
            Assert.Equal(3, grid.SelectedIndex);
        }

        [Fact]
        public void SelectedIndexResetWhenQueryStringValueIsInvalid()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["row"] = "NotAnInt";
            var grid = new WebGrid(GetContext(queryString)).Bind(new[]
            {
                new { P1 = 1, P2 = '2', P3 = "3" },
                new { P1 = 4, P2 = '5', P3 = "6" }
            });
            Assert.False(grid.HasSelection);
            Assert.Equal(-1, grid.SelectedIndex);
            Assert.Null(grid.SelectedRow);
        }

        [Fact]
        public void SelectedIndexResetWhenQueryStringValueLessThanOne()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["row"] = "0";
            var grid = new WebGrid(GetContext(queryString)).Bind(new[]
            {
                new { P1 = 1, P2 = '2', P3 = "3" },
                new { P1 = 4, P2 = '5', P3 = "6" }
            });
            Assert.False(grid.HasSelection);
            Assert.Equal(-1, grid.SelectedIndex);
            Assert.Null(grid.SelectedRow);
        }

        [Fact]
        public void SelectedIndexUsesCustomQueryString()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["g_sel"] = "2";
            var grid = new WebGrid(GetContext(queryString), fieldNamePrefix: "g_", selectionFieldName: "sel").Bind(new[]
            {
                new { P1 = 1, P2 = '2', P3 = "3" },
                new { P1 = 4, P2 = '5', P3 = "6" }
            });
            Assert.True(grid.HasSelection);
            Assert.Equal(1, grid.SelectedIndex);
            Assert.NotNull(grid.SelectedRow);
            Assert.Equal(4, grid.SelectedRow["P1"]);
        }

        [Fact]
        public void SelectedIndexUsesQueryString()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["row"] = "2";
            var grid = new WebGrid(GetContext(queryString)).Bind(new[]
            {
                new { P1 = 1, P2 = '2', P3 = "3" },
                new { P1 = 4, P2 = '5', P3 = "6" }
            });
            Assert.True(grid.HasSelection);
            Assert.Equal(1, grid.SelectedIndex);
            Assert.NotNull(grid.SelectedRow);
            Assert.Equal(4, grid.SelectedRow["P1"]);
        }

        [Fact]
        public void SelectLink()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["page"] = "1";
            queryString["row"] = "1";
            queryString["sort"] = "P1";
            queryString["sortdir"] = "DESC";
            var grid = new WebGrid(GetContext(queryString)).Bind(new[]
            {
                new { P1 = 1, P2 = '2', P3 = "3" },
                new { P1 = 4, P2 = '5', P3 = "6" }
            });
            string html = grid.Rows[1].GetSelectLink().ToString();
            Assert.Equal("<a href=\"?page=1&amp;row=2&amp;sort=P1&amp;sortdir=DESC\">Select</a>", html.ToString());
        }

        [Fact]
        public void SortCanBeReset()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["sort"] = "P1";
            var grid = new WebGrid(GetContext(queryString)).Bind(new[]
            {
                new { P1 = 1, P2 = '2', P3 = "3" },
                new { P1 = 4, P2 = '5', P3 = "6" }
            });
            Assert.Equal("P1", grid.SortColumn);
            grid.SortColumn = "P2";
            Assert.Equal("P2", grid.SortColumn);
            // verify that selection and page links have updated sort
            Assert.Equal("?sort=P2&row=1", grid.Rows.FirstOrDefault().GetSelectUrl());
            Assert.Equal("?sort=P2&page=1", grid.GetPageUrl(0));
        }

        [Fact]
        public void SortCanBeResetToNull()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["sort"] = "P1";
            var grid = new WebGrid(GetContext(queryString)).Bind(new[]
            {
                new { P1 = 1, P2 = '2', P3 = "3" },
                new { P1 = 4, P2 = '5', P3 = "6" }
            });
            Assert.Equal("P1", grid.SortColumn);
            grid.SortColumn = null;
            Assert.Equal(String.Empty, grid.SortColumn);
            // verify that selection and page links have updated sort
            Assert.Equal("?row=1", grid.Rows.FirstOrDefault().GetSelectUrl());
            Assert.Equal("?page=1", grid.GetPageUrl(0));
        }

        [Fact]
        public void SortCanBeResetToSameValue()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["sort"] = "P1";
            var grid = new WebGrid(GetContext(queryString)).Bind(new[]
            {
                new { P1 = 1, P2 = '2', P3 = "3" },
                new { P1 = 4, P2 = '5', P3 = "6" }
            });
            grid.SortColumn = String.Empty;
            Assert.Equal(String.Empty, grid.SortColumn);
        }

        [Fact]
        public void SortColumnDefaultsToEmpty()
        {
            var grid = new WebGrid(GetContext()).Bind(new[]
            {
                new { P1 = 1, P2 = '2', P3 = "3" }
            });
            Assert.Equal(String.Empty, grid.SortColumn);
        }

        [Fact]
        public void SortColumnResetWhenQueryStringValueIsInvalid()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["sort"] = "P4";
            var grid = new WebGrid(GetContext(queryString)).Bind(new[]
            {
                new { P1 = 1, P2 = '2', P3 = "3" }
            });
            Assert.Equal("", grid.SortColumn);
        }

        [Fact]
        public void SortColumnUsesCustomQueryString()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["g_st"] = "P2";
            var grid = new WebGrid(GetContext(queryString), fieldNamePrefix: "g_", sortFieldName: "st").Bind(new[]
            {
                new { P1 = 1, P2 = '2', P3 = "3" }
            });
            Assert.Equal("P2", grid.SortColumn);
        }

        [Fact]
        public void SortColumnUsesQueryString()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["sort"] = "P2";
            var grid = new WebGrid(GetContext(queryString)).Bind(new[]
            {
                new { P1 = 1, P2 = '2', P3 = "3" }
            });
            Assert.Equal("P2", grid.SortColumn);
        }

        [Fact]
        public void SortDirectionCanBeReset()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["sortdir"] = "DESC";
            var grid = new WebGrid(GetContext(queryString)).Bind(new[]
            {
                new { P1 = 1, P2 = '2', P3 = "3" },
                new { P1 = 4, P2 = '5', P3 = "6" }
            });
            Assert.Equal(SortDirection.Descending, grid.SortDirection);
            grid.SortDirection = SortDirection.Ascending;
            Assert.Equal(SortDirection.Ascending, grid.SortDirection);
            // verify that selection and page links have updated sort
            Assert.Equal("?sortdir=ASC&row=1", grid.Rows.FirstOrDefault().GetSelectUrl());
            Assert.Equal("?sortdir=ASC&page=1", grid.GetPageUrl(0));
        }

        [Fact]
        public void SortDirectionDefaultsToAscending()
        {
            var grid = new WebGrid(GetContext()).Bind(new object[0]);
            Assert.Equal(SortDirection.Ascending, grid.SortDirection);
        }

        [Fact]
        public void SortDirectionResetWhenQueryStringValueIsInvalid()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["sortdir"] = "NotASortDir";
            var grid = new WebGrid(GetContext(queryString)).Bind(new object[0]);
            Assert.Equal(SortDirection.Ascending, grid.SortDirection);
        }

        [Fact]
        public void SortDirectionUsesQueryStringOfAsc()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["sortdir"] = "aSc";
            var grid = new WebGrid(GetContext(queryString)).Bind(new object[0]);
            Assert.Equal(SortDirection.Ascending, grid.SortDirection);
        }

        [Fact]
        public void SortDirectionUsesQueryStringOfAscending()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["sortdir"] = "AScendING";
            var grid = new WebGrid(GetContext(queryString)).Bind(new object[0]);
            Assert.Equal(SortDirection.Ascending, grid.SortDirection);
        }

        [Fact]
        public void SortDirectionUsesQueryStringOfDesc()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["sortdir"] = "DeSc";
            var grid = new WebGrid(GetContext(queryString)).Bind(new object[0]);
            Assert.Equal(SortDirection.Descending, grid.SortDirection);
        }

        [Fact]
        public void SortDirectionUsesQueryStringOfDescending()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["g_sd"] = "DeScendING";
            var grid = new WebGrid(GetContext(queryString), fieldNamePrefix: "g_", sortDirectionFieldName: "sd").Bind(new object[0]);
            Assert.Equal(SortDirection.Descending, grid.SortDirection);
        }

        [Fact]
        public void SortDisabledIfSortIsEmpty()
        {
            var grid = new WebGrid(GetContext(), defaultSort: String.Empty).Bind(Dynamics(
                new { FirstName = "Joe", LastName = "Smith" },
                new { FirstName = "Bob", LastName = "Johnson" },
                new { FirstName = "Sam", LastName = "Jones" },
                new { FirstName = "Tom", LastName = "Anderson" }
                                                                                     ));
            Assert.Equal("Joe", grid.Rows[0]["FirstName"]);
            Assert.Equal("Bob", grid.Rows[1]["FirstName"]);
            Assert.Equal("Sam", grid.Rows[2]["FirstName"]);
            Assert.Equal("Tom", grid.Rows[3]["FirstName"]);
        }

        [Fact]
        public void SortDisabledIfSortIsNull()
        {
            var grid = new WebGrid(GetContext(), defaultSort: null).Bind(Dynamics(
                new { FirstName = "Joe", LastName = "Smith" },
                new { FirstName = "Bob", LastName = "Johnson" },
                new { FirstName = "Sam", LastName = "Jones" },
                new { FirstName = "Tom", LastName = "Anderson" }
                                                                             ));
            Assert.Equal("Joe", grid.Rows[0]["FirstName"]);
            Assert.Equal("Bob", grid.Rows[1]["FirstName"]);
            Assert.Equal("Sam", grid.Rows[2]["FirstName"]);
            Assert.Equal("Tom", grid.Rows[3]["FirstName"]);
        }

        [Fact]
        public void SortForDynamics()
        {
            var grid = new WebGrid(GetContext(), defaultSort: "FirstName").Bind(Dynamics(
                new { FirstName = "Joe", LastName = "Smith" },
                new { FirstName = "Bob", LastName = "Johnson" },
                new { FirstName = "Sam", LastName = "Jones" },
                new { FirstName = "Tom", LastName = "Anderson" }
                                                                                    ));
            Assert.Equal("Bob", grid.Rows[0]["FirstName"]);
            Assert.Equal("Joe", grid.Rows[1]["FirstName"]);
            Assert.Equal("Sam", grid.Rows[2]["FirstName"]);
            Assert.Equal("Tom", grid.Rows[3]["FirstName"]);
        }

        [Fact]
        public void SortForDynamicsDescending()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["sort"] = "LastName";
            queryString["sortdir"] = "DESCENDING";
            var grid = new WebGrid(GetContext(queryString), defaultSort: "FirstName").Bind(Dynamics(
                new { FirstName = "Joe", LastName = "Smith" },
                new { FirstName = "Bob", LastName = "Johnson" },
                new { FirstName = "Sam", LastName = "Jones" },
                new { FirstName = "Tom", LastName = "Anderson" }
                                                                                               ));
            Assert.Equal("Smith", grid.Rows[0]["LastName"]);
            Assert.Equal("Jones", grid.Rows[1]["LastName"]);
            Assert.Equal("Johnson", grid.Rows[2]["LastName"]);
            Assert.Equal("Anderson", grid.Rows[3]["LastName"]);
        }

        [Fact]
        public void SortForNonDynamicNavigationColumn()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["sort"] = "Not.A.Column";
            var grid = new WebGrid(GetContext(queryString), defaultSort: "Person.FirstName").Bind(new[]
            {
                new { Person = new { FirstName = "Joe", LastName = "Smith" } },
                new { Person = new { FirstName = "Bob", LastName = "Johnson" } },
                new { Person = new { FirstName = "Sam", LastName = "Jones" } },
                new { Person = new { FirstName = "Tom", LastName = "Anderson" } }
            });
            Assert.Equal("Not.A.Column", grid.SortColumn); // navigation columns are validated during sort
            Assert.Equal("Bob", grid.Rows[0]["Person.FirstName"]);
            Assert.Equal("Joe", grid.Rows[1]["Person.FirstName"]);
            Assert.Equal("Sam", grid.Rows[2]["Person.FirstName"]);
            Assert.Equal("Tom", grid.Rows[3]["Person.FirstName"]);
        }

        [Fact]
        public void SortForNonDynamics()
        {
            var grid = new WebGrid(GetContext(), defaultSort: "FirstName").Bind(new[]
            {
                new { FirstName = "Joe", LastName = "Smith" },
                new { FirstName = "Bob", LastName = "Johnson" },
                new { FirstName = "Sam", LastName = "Jones" },
                new { FirstName = "Tom", LastName = "Anderson" }
            });
            Assert.Equal("Bob", grid.Rows[0]["FirstName"]);
            Assert.Equal("Joe", grid.Rows[1]["FirstName"]);
            Assert.Equal("Sam", grid.Rows[2]["FirstName"]);
            Assert.Equal("Tom", grid.Rows[3]["FirstName"]);
        }

        [Fact]
        public void SortForNonDynamicsDescending()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["sort"] = "LastName";
            queryString["sortdir"] = "DESCENDING";
            var grid = new WebGrid(GetContext(queryString), defaultSort: "FirstName").Bind(new[]
            {
                new { FirstName = "Joe", LastName = "Smith" },
                new { FirstName = "Bob", LastName = "Johnson" },
                new { FirstName = "Sam", LastName = "Jones" },
                new { FirstName = "Tom", LastName = "Anderson" }
            });
            Assert.Equal("Smith", grid.Rows[0]["LastName"]);
            Assert.Equal("Jones", grid.Rows[1]["LastName"]);
            Assert.Equal("Johnson", grid.Rows[2]["LastName"]);
            Assert.Equal("Anderson", grid.Rows[3]["LastName"]);
        }

        [Fact]
        public void SortForNonDynamicsEnumerable()
        {
            var grid = new WebGrid(GetContext(), defaultSort: "FirstName").Bind(new[]
            {
                new { FirstName = "Joe", LastName = "Smith" },
                new { FirstName = "Bob", LastName = "Johnson" },
                new { FirstName = "Sam", LastName = "Jones" },
                new { FirstName = "Tom", LastName = "Anderson" }
            }.ToList());
            Assert.Equal("Bob", grid.Rows[0]["FirstName"]);
            Assert.Equal("Joe", grid.Rows[1]["FirstName"]);
            Assert.Equal("Sam", grid.Rows[2]["FirstName"]);
            Assert.Equal("Tom", grid.Rows[3]["FirstName"]);
        }

        [Fact]
        public void SortForNonDynamicsEnumerableDescending()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["sort"] = "LastName";
            queryString["sortdir"] = "DESCENDING";
            var grid = new WebGrid(GetContext(queryString), defaultSort: "FirstName").Bind(new[]
            {
                new { FirstName = "Joe", LastName = "Smith" },
                new { FirstName = "Bob", LastName = "Johnson" },
                new { FirstName = "Sam", LastName = "Jones" },
                new { FirstName = "Tom", LastName = "Anderson" }
            }.ToList());
            Assert.Equal("Smith", grid.Rows[0]["LastName"]);
            Assert.Equal("Jones", grid.Rows[1]["LastName"]);
            Assert.Equal("Johnson", grid.Rows[2]["LastName"]);
            Assert.Equal("Anderson", grid.Rows[3]["LastName"]);
        }

        [Fact]
        public void SortForNonGenericEnumerable()
        {
            var grid = new WebGrid(GetContext(), defaultSort: "FirstName").Bind(new NonGenericEnumerable(new[]
            {
                new Person { FirstName = "Joe", LastName = "Smith" },
                new Person { FirstName = "Bob", LastName = "Johnson" },
                new Person { FirstName = "Sam", LastName = "Jones" },
                new Person { FirstName = "Tom", LastName = "Anderson" }
            }));
            Assert.Equal("Bob", grid.Rows[0]["FirstName"]);
            Assert.Equal("Joe", grid.Rows[1]["FirstName"]);
            Assert.Equal("Sam", grid.Rows[2]["FirstName"]);
            Assert.Equal("Tom", grid.Rows[3]["FirstName"]);
        }

        [Fact]
        public void SortForNonGenericEnumerableDescending()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["sort"] = "LastName";
            queryString["sortdir"] = "DESCENDING";
            var grid = new WebGrid(GetContext(queryString), defaultSort: "FirstName").Bind(new NonGenericEnumerable(new[]
            {
                new Person { FirstName = "Joe", LastName = "Smith" },
                new Person { FirstName = "Bob", LastName = "Johnson" },
                new Person { FirstName = "Sam", LastName = "Jones" },
                new Person { FirstName = "Tom", LastName = "Anderson" }
            }));
            Assert.Equal("Smith", grid.Rows[0]["LastName"]);
            Assert.Equal("Jones", grid.Rows[1]["LastName"]);
            Assert.Equal("Johnson", grid.Rows[2]["LastName"]);
            Assert.Equal("Anderson", grid.Rows[3]["LastName"]);
        }

        [Fact]
        public void SortUrlDefaults()
        {
            var grid = new WebGrid(GetContext()).Bind(new[]
            {
                new { FirstName = "Bob" }
            });
            string html = grid.GetSortUrl("FirstName");
            Assert.Equal("?sort=FirstName&sortdir=ASC", html.ToString());
        }

        [Fact]
        public void SortUrlThrowsIfColumnNameIsEmpty()
        {
            var grid = new WebGrid(GetContext()).Bind(new[]
            {
                new { }, new { }
            });
            Assert.ThrowsArgumentNullOrEmptyString(() => { grid.GetSortUrl(String.Empty); }, "column");
        }

        [Fact]
        public void SortUrlThrowsIfColumnNameIsNull()
        {
            var grid = new WebGrid(GetContext()).Bind(new[]
            {
                new { }, new { }
            });
            Assert.ThrowsArgumentNullOrEmptyString(() => { grid.GetSortUrl(null); }, "column");
        }

        [Fact]
        public void SortUrlThrowsIfSortingIsDisabled()
        {
            var grid = new WebGrid(GetContext(), canSort: false).Bind(new[]
            {
                new { P1 = 1 }, new { P1 = 2 }
            });
            Assert.Throws<NotSupportedException>(() => { grid.GetSortUrl("P1"); }, "This operation is not supported when sorting is disabled for the \"WebGrid\" object.");
        }

        [Fact]
        public void SortWhenSortIsDisabled()
        {
            var grid = new WebGrid(GetContext(), defaultSort: "FirstName", canSort: false).Bind(new[]
            {
                new { FirstName = "Joe", LastName = "Smith" },
                new { FirstName = "Bob", LastName = "Johnson" },
                new { FirstName = "Sam", LastName = "Jones" },
                new { FirstName = "Tom", LastName = "Anderson" }
            });
            Assert.Equal("Joe", grid.Rows[0]["FirstName"]);
            Assert.Equal("Bob", grid.Rows[1]["FirstName"]);
            Assert.Equal("Sam", grid.Rows[2]["FirstName"]);
            Assert.Equal("Tom", grid.Rows[3]["FirstName"]);
        }

        [Fact]
        public void SortWithNullValues()
        {
            var data = new[]
            {
                new { FirstName = (object)"Joe", LastName = "Smith" },
                new { FirstName = (object)"Bob", LastName = "Johnson" },
                new { FirstName = (object)null, LastName = "Jones" }
            };
            var grid = new WebGrid(GetContext(), defaultSort: "FirstName").Bind(data);

            Assert.Equal("Jones", grid.Rows[0]["LastName"]);
            Assert.Equal("Bob", grid.Rows[1]["FirstName"]);
            Assert.Equal("Joe", grid.Rows[2]["FirstName"]);

            grid = new WebGrid(GetContext(), defaultSort: "FirstName desc").Bind(data);

            Assert.Equal("Joe", grid.Rows[0]["FirstName"]);
            Assert.Equal("Bob", grid.Rows[1]["FirstName"]);
            Assert.Equal("Jones", grid.Rows[2]["LastName"]);
        }

        [Fact]
        public void SortWithMultipleNullValues()
        {
            var data = new[]
            {
                new { FirstName = (object)"Joe", LastName = "Smith" },
                new { FirstName = (object)"Bob", LastName = "Johnson" },
                new { FirstName = (object)null, LastName = "Hughes" },
                new { FirstName = (object)null, LastName = "Jones" }
            };
            var grid = new WebGrid(GetContext(), defaultSort: "FirstName").Bind(data);

            Assert.Equal("Hughes", grid.Rows[0]["LastName"]);
            Assert.Equal("Jones", grid.Rows[1]["LastName"]);
            Assert.Equal("Bob", grid.Rows[2]["FirstName"]);
            Assert.Equal("Joe", grid.Rows[3]["FirstName"]);

            grid = new WebGrid(GetContext(), defaultSort: "FirstName desc").Bind(data);

            Assert.Equal("Joe", grid.Rows[0]["FirstName"]);
            Assert.Equal("Bob", grid.Rows[1]["FirstName"]);
            Assert.Equal("Hughes", grid.Rows[2]["LastName"]);
            Assert.Equal("Jones", grid.Rows[3]["LastName"]);
        }

        [Fact]
        public void SortWithMixedValuesDoesNotThrow()
        {
            var data = new[]
            {
                new { FirstName = (object)1, LastName = "Smith" },
                new { FirstName = (object)"Bob", LastName = "Johnson" },
                new { FirstName = (object)DBNull.Value, LastName = "Jones" }
            };
            var grid = new WebGrid(GetContext(), defaultSort: "FirstName").Bind(data);

            Assert.NotNull(grid.Rows);

            Assert.Equal("Smith", grid.Rows[0]["LastName"]);
            Assert.Equal("Johnson", grid.Rows[1]["LastName"]);
            Assert.Equal("Jones", grid.Rows[2]["LastName"]);
        }

        [Fact]
        public void SortWithUnsortableDoesNotThrow()
        {
            var object1 = new object();
            var object2 = new object();
            var data = new[]
            {
                new { Value = object1 },
                new { Value = object2 }
            };
            var grid = new WebGrid(GetContext(), defaultSort: "Value").Bind(data);

            Assert.NotNull(grid.Rows);

            Assert.Equal(object1, grid.Rows[0]["Value"]);
            Assert.Equal(object2, grid.Rows[1]["Value"]);
        }

        [Fact]
        public void TableRenderingWithColumnTemplates()
        {
            var grid = new WebGrid(GetContext(), rowsPerPage: 3).Bind(new[]
            {
                new { P1 = 1, P2 = '2', P3 = "3" }
            });
            var html = grid.Table(displayHeader: false,
                                  columns: new[]
                                  {
                                      grid.Column("P1", format: item => { return "<span>P1: " + item.P1 + "</span>"; }),
                                      grid.Column("P2", format: item => { return new HtmlString("<span>P2: " + item.P2 + "</span>"); }),
                                      grid.Column("P3", format: item => { return new HelperResult(tw => { tw.Write("<span>P3: " + item.P3 + "</span>"); }); })
                                  });
            UnitTestHelper.AssertEqualsIgnoreWhitespace(
                "<table><tbody><tr>" +
                "<td>&lt;span&gt;P1: 1&lt;/span&gt;</td>" +
                "<td><span>P2: 2</span></td>" +
                "<td><span>P3: 3</span></td>" +
                "</tr></tbody></table>", html.ToString());
            XhtmlAssert.Validate1_1(html);
        }

        [Fact]
        public void TableRenderingWithDefaultCellValueOfCustom()
        {
            var grid = new WebGrid(GetContext(), rowsPerPage: 3).Bind(new[]
            {
                new { P1 = String.Empty, P2 = (string)null },
            });
            var html = grid.Table(fillEmptyRows: true, emptyRowCellValue: "N/A", displayHeader: false);
            UnitTestHelper.AssertEqualsIgnoreWhitespace(
                "<table><tbody>" +
                "<tr><td></td><td></td></tr>" +
                "<tr><td>N/A</td><td>N/A</td></tr>" +
                "<tr><td>N/A</td><td>N/A</td></tr>" +
                "</tbody></table>", html.ToString());
            XhtmlAssert.Validate1_1(html);
        }

        [Fact]
        public void TableRenderingWithDefaultCellValueOfEmpty()
        {
            var grid = new WebGrid(GetContext(), rowsPerPage: 3).Bind(new[]
            {
                new { P1 = String.Empty, P2 = (string)null }
            });
            var html = grid.Table(fillEmptyRows: true, emptyRowCellValue: "", displayHeader: false);
            UnitTestHelper.AssertEqualsIgnoreWhitespace(
                "<table><tbody>" +
                "<tr><td></td><td></td></tr>" +
                "<tr><td></td><td></td></tr>" +
                "<tr><td></td><td></td></tr>" +
                "</tbody></table>", html.ToString());
            XhtmlAssert.Validate1_1(html);
        }

        [Fact]
        public void TableRenderingWithDefaultCellValueOfNbsp()
        {
            var grid = new WebGrid(GetContext(), rowsPerPage: 3).Bind(new[]
            {
                new { P1 = String.Empty, P2 = (string)null }
            });
            var html = grid.Table(fillEmptyRows: true, displayHeader: false);
            UnitTestHelper.AssertEqualsIgnoreWhitespace(
                "<table><tbody>" +
                "<tr><td></td><td></td></tr>" +
                "<tr><td>&nbsp;</td><td>&nbsp;</td></tr>" +
                "<tr><td>&nbsp;</td><td>&nbsp;</td></tr>" +
                "</tbody></table>", html.ToString());
            XhtmlAssert.Validate1_1(html);
        }

        [Fact]
        public void TableRenderingWithExclusions()
        {
            var grid = new WebGrid(GetContext()).Bind(new[]
            {
                new { P1 = 1, P2 = '2', P3 = "3" }
            });
            var html = grid.Table(exclusions: new string[] { "P2" });
            UnitTestHelper.AssertEqualsIgnoreWhitespace(
                "<table><thead><tr>" +
                "<th scope=\"col\"><a href=\"?sort=P1&amp;sortdir=ASC\">P1</a></th>" +
                "<th scope=\"col\"><a href=\"?sort=P3&amp;sortdir=ASC\">P3</a></th>" +
                "</tr></thead>" +
                "<tbody>" +
                "<tr><td>1</td><td>3</td></tr>" +
                "</tbody></table>", html.ToString());
            XhtmlAssert.Validate1_1(html);
        }

        [Fact]
        public void TableRenderingWithNoStylesAndFillEmptyRows()
        {
            var grid = new WebGrid(GetContext(), rowsPerPage: 3).Bind(new[]
            {
                new { FirstName = "Joe", LastName = "Smith" }
            });
            var html = grid.Table(fillEmptyRows: true);
            UnitTestHelper.AssertEqualsIgnoreWhitespace(
                "<table><thead><tr>" +
                "<th scope=\"col\"><a href=\"?sort=FirstName&amp;sortdir=ASC\">FirstName</a></th>" +
                "<th scope=\"col\"><a href=\"?sort=LastName&amp;sortdir=ASC\">LastName</a></th>" +
                "</tr></thead>" +
                "<tbody>" +
                "<tr><td>Joe</td><td>Smith</td></tr>" +
                "<tr><td>&nbsp;</td><td>&nbsp;</td></tr>" +
                "<tr><td>&nbsp;</td><td>&nbsp;</td></tr>" +
                "</tbody></table>", html.ToString());
            XhtmlAssert.Validate1_1(html);
        }

        [Fact]
        public void TableRenderingWithSortingDisabled()
        {
            var grid = new WebGrid(GetContext(), canSort: false).Bind(new[]
            {
                new { FirstName = "Joe", LastName = "Smith" }
            });
            var html = grid.Table();
            UnitTestHelper.AssertEqualsIgnoreWhitespace(
                "<table><thead><tr>" +
                "<th scope=\"col\">FirstName</th>" +
                "<th scope=\"col\">LastName</th>" +
                "</tr></thead>" +
                "<tbody>" +
                "<tr><td>Joe</td><td>Smith</td></tr>" +
                "</tbody></table>", html.ToString());
            XhtmlAssert.Validate1_1(html);
        }

        [Fact]
        public void TableRenderingWithAttributes()
        {
            var grid = new WebGrid(GetContext()).Bind(new[]
            {
                new { FirstName = "Joe", LastName = "Smith" }
            });
            var html = grid.Table(htmlAttributes: new { id = "my-table-id", summary = "Table summary" });
            UnitTestHelper.AssertEqualsIgnoreWhitespace(
                "<table id=\"my-table-id\" summary=\"Table summary\"><thead><tr>" +
                "<th scope=\"col\"><a href=\"?sort=FirstName&amp;sortdir=ASC\">FirstName</a></th>" +
                "<th scope=\"col\"><a href=\"?sort=LastName&amp;sortdir=ASC\">LastName</a></th>" +
                "</tr></thead>" +
                "<tbody>" +
                "<tr><td>Joe</td><td>Smith</td></tr>" +
                "</tbody></table>", html.ToString());
            XhtmlAssert.Validate1_1(html);
        }

        [Fact]
        public void TableRenderingEncodesAttributes()
        {
            var grid = new WebGrid(GetContext()).Bind(new[]
            {
                new { FirstName = "Joe", LastName = "Smith" }
            });
            var html = grid.Table(htmlAttributes: new { summary = "\"<Table summary" });
            UnitTestHelper.AssertEqualsIgnoreWhitespace(
                "<table summary=\"&quot;&lt;Table summary\"><thead><tr>" +
                "<th scope=\"col\"><a href=\"?sort=FirstName&amp;sortdir=ASC\">FirstName</a></th>" +
                "<th scope=\"col\"><a href=\"?sort=LastName&amp;sortdir=ASC\">LastName</a></th>" +
                "</tr></thead>" +
                "<tbody>" +
                "<tr><td>Joe</td><td>Smith</td></tr>" +
                "</tbody></table>", html.ToString());
            XhtmlAssert.Validate1_1(html);
        }

        [Fact]
        public void TableRenderingIsNotAffectedWhenAttributesIsNull()
        {
            var grid = new WebGrid(GetContext()).Bind(new[]
            {
                new { FirstName = "Joe", LastName = "Smith" }
            });
            var html = grid.Table(htmlAttributes: null);
            UnitTestHelper.AssertEqualsIgnoreWhitespace(
                "<table><thead><tr>" +
                "<th scope=\"col\"><a href=\"?sort=FirstName&amp;sortdir=ASC\">FirstName</a></th>" +
                "<th scope=\"col\"><a href=\"?sort=LastName&amp;sortdir=ASC\">LastName</a></th>" +
                "</tr></thead>" +
                "<tbody>" +
                "<tr><td>Joe</td><td>Smith</td></tr>" +
                "</tbody></table>", html.ToString());
            XhtmlAssert.Validate1_1(html);
        }

        [Fact]
        public void TableRenderingIsNotAffectedWhenAttributesIsEmpty()
        {
            var grid = new WebGrid(GetContext()).Bind(new[]
            {
                new { FirstName = "Joe", LastName = "Smith" }
            });
            var html = grid.Table(htmlAttributes: new { });
            UnitTestHelper.AssertEqualsIgnoreWhitespace(
                "<table><thead><tr>" +
                "<th scope=\"col\"><a href=\"?sort=FirstName&amp;sortdir=ASC\">FirstName</a></th>" +
                "<th scope=\"col\"><a href=\"?sort=LastName&amp;sortdir=ASC\">LastName</a></th>" +
                "</tr></thead>" +
                "<tbody>" +
                "<tr><td>Joe</td><td>Smith</td></tr>" +
                "</tbody></table>", html.ToString());
            XhtmlAssert.Validate1_1(html);
        }

        [Fact]
        public void TableRenderingWithStyles()
        {
            NameValueCollection queryString = new NameValueCollection();
            queryString["row"] = "1";
            var grid = new WebGrid(GetContext(queryString), rowsPerPage: 4).Bind(new[]
            {
                new { FirstName = "Joe", LastName = "Smith" },
                new { FirstName = "Bob", LastName = "Johnson" }
            });
            var html = grid.Table(tableStyle: "tbl", headerStyle: "hdr", footerStyle: "ftr",
                                  rowStyle: "row", alternatingRowStyle: "arow", selectedRowStyle: "sel", fillEmptyRows: true,
                                  footer: item => "footer text",
                                  columns: new[]
                                  {
                                      grid.Column("firstName", style: "c1", canSort: false),
                                      grid.Column("lastName", style: "c2", canSort: false)
                                  });
            UnitTestHelper.AssertEqualsIgnoreWhitespace(
                "<table class=\"tbl\"><thead><tr class=\"hdr\">" +
                "<th scope=\"col\">firstName</th><th scope=\"col\">lastName</th>" +
                "</tr></thead>" +
                "<tfoot>" +
                "<tr class=\"ftr\"><td colspan=\"2\">footer text</td></tr>" +
                "</tfoot>" +
                "<tbody>" +
                "<tr class=\"row sel\"><td class=\"c1\">Joe</td><td class=\"c2\">Smith</td></tr>" +
                "<tr class=\"arow\"><td class=\"c1\">Bob</td><td class=\"c2\">Johnson</td></tr>" +
                "<tr class=\"row\"><td class=\"c1\">&nbsp;</td><td class=\"c2\">&nbsp;</td></tr>" +
                "<tr class=\"arow\"><td class=\"c1\">&nbsp;</td><td class=\"c2\">&nbsp;</td></tr>" +
                "</tbody></table>", html.ToString());
            XhtmlAssert.Validate1_1(html);
        }

        [Fact]
        public void TableWithAjax()
        {
            var grid = new WebGrid(GetContext(), ajaxUpdateContainerId: "grid").Bind(new[]
            {
                new { First = "First", Second = "Second" }
            });
            string html = grid.Table().ToString();
            Assert.Contains("<script", html);
            Assert.Contains("swhgajax=\"true\"", html);
        }

        [Fact]
        public void TableWithAjaxAndCallback()
        {
            var grid = new WebGrid(GetContext(), ajaxUpdateContainerId: "grid", ajaxUpdateCallback: "myCallback").Bind(new[]
            {
                new { First = "First", Second = "Second" }
            });
            string html = grid.Table().ToString();
            Assert.Contains("<script", html);
            Assert.Contains("myCallback", html);
        }

        [Fact]
        public void WebGridEncodesAjaxDataStrings()
        {
            var grid = new WebGrid(GetContext(), ajaxUpdateContainerId: "'grid'", ajaxUpdateCallback: "'myCallback'").Bind(new[]
            {
                new { First = "First", Second = "Second" }
            });
            string html = grid.Table().ToString();
            Assert.Contains(@"&#39;grid&#39;", html);
            Assert.Contains(@"&#39;myCallback&#39;", html);
        }

        [Fact]
        public void WebGridThrowsIfOperationsArePerformedBeforeBinding()
        {
            // Arrange
            string errorMessage = "A data source must be bound before this operation can be performed.";
            var grid = new WebGrid(GetContext());

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => { var rows = grid.Rows; }, errorMessage);
            Assert.Throws<InvalidOperationException>(() => { int count = grid.TotalRowCount; }, errorMessage);
            Assert.Throws<InvalidOperationException>(() => grid.GetHtml().ToString(), errorMessage);
            Assert.Throws<InvalidOperationException>(() => grid.Pager().ToString(), errorMessage);
            Assert.Throws<InvalidOperationException>(() => grid.Table().ToString(), errorMessage);
            Assert.Throws<InvalidOperationException>(() =>
            {
                grid.SelectedIndex = 1;
                var row = grid.SelectedRow;
            }, errorMessage);
        }

        [Fact]
        public void WebGridThrowsIfBindingIsPerformedWhenAlreadyBound()
        {
            // Arrange
            var grid = new WebGrid(GetContext());
            var values = Enumerable.Range(0, 10).Cast<dynamic>();

            // Act
            grid.Bind(values);

            // Assert
            Assert.Throws<InvalidOperationException>(() => grid.Bind(values), "The WebGrid instance is already bound to a data source.");
        }

        [Fact]
        public void GetElementTypeReturnsDynamicTypeIfElementIsDynamic()
        {
            // Arrange
            IEnumerable<dynamic> elements = Dynamics(new[] { new Person { FirstName = "Foo", LastName = "Bar" } });

            // Act
            Type type = WebGrid.GetElementType(elements);

            // Assert
            Assert.Equal(typeof(IDynamicMetaObjectProvider), type);
        }

        [Fact]
        public void GetElementTypeReturnsEnumerableTypeIfFirstInstanceIsNotDynamic()
        {
            // Arrange
            IEnumerable<dynamic> elements = Iterator();

            // Act
            Type type = WebGrid.GetElementType(elements);

            // Assert
            Assert.Equal(typeof(Person), type);
        }

        [Fact]
        public void TableThrowsIfQueryStringDerivedSortColumnIsExcluded()
        {
            // Arrange
            NameValueCollection collection = new NameValueCollection();
            collection["sort"] = "Salary";
            var context = GetContext(collection);
            IList<Employee> employees = new List<Employee>();
            employees.Add(new Employee { Name = "A", Salary = 5, Manager = new Employee { Name = "-" } });
            employees.Add(new Employee { Name = "B", Salary = 20, Manager = employees[0] });
            employees.Add(new Employee { Name = "C", Salary = 15, Manager = employees[0] });
            employees.Add(new Employee { Name = "D", Salary = 5, Manager = employees[1] });

            var grid = new WebGrid(context, defaultSort: "Name").Bind(employees);

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => grid.GetHtml(exclusions: new[] { "Salary" }), "Column \"Salary\" does not exist.");
        }

        [Fact]
        public void TableThrowsIfQueryStringDerivedSortColumnDoesNotExistInColumnsArgument()
        {
            // Arrange
            NameValueCollection collection = new NameValueCollection();
            collection["sort"] = "Salary";
            var context = GetContext(collection);
            IList<Employee> employees = new List<Employee>();
            employees.Add(new Employee { Name = "A", Salary = 5, Manager = new Employee { Name = "-" } });
            employees.Add(new Employee { Name = "B", Salary = 20, Manager = employees[0] });
            employees.Add(new Employee { Name = "C", Salary = 15, Manager = employees[0] });
            employees.Add(new Employee { Name = "D", Salary = 5, Manager = employees[1] });

            var grid = new WebGrid(context, canSort: true, defaultSort: "Name").Bind(employees);

            // Act and Assert
            Assert.Throws<InvalidOperationException>(
                () => grid.Table(columns: new[] { new WebGridColumn { ColumnName = "Name" }, new WebGridColumn { ColumnName = "Manager.Name" } }),
                "Column \"Salary\" does not exist.");
        }

        [Fact]
        public void TableDoesNotThrowIfQueryStringDerivedSortColumnIsVisibleButNotSortable()
        {
            // Arrange
            NameValueCollection collection = new NameValueCollection();
            collection["sort"] = "Salary";
            collection["sortDir"] = "Desc";
            var context = GetContext(collection);
            IList<Employee> employees = new List<Employee>();
            employees.Add(new Employee { Name = "A", Salary = 5, Manager = new Employee { Name = "-" } });
            employees.Add(new Employee { Name = "B", Salary = 20, Manager = employees[0] });
            employees.Add(new Employee { Name = "C", Salary = 15, Manager = employees[0] });
            employees.Add(new Employee { Name = "D", Salary = 10, Manager = employees[1] });

            var grid = new WebGrid(context, canSort: true).Bind(employees);

            // Act
            var html = grid.Table(columns: new[] { new WebGridColumn { ColumnName = "Salary", CanSort = false } });

            // Assert
            Assert.NotNull(html);
            Assert.Equal(20, grid.Rows[0]["Salary"]);
            Assert.Equal(15, grid.Rows[1]["Salary"]);
            Assert.Equal(10, grid.Rows[2]["Salary"]);
            Assert.Equal(5, grid.Rows[3]["Salary"]);
        }

        [Fact]
        public void TableThrowsIfComplexPropertyIsUnsortable()
        {
            // Arrange
            NameValueCollection collection = new NameValueCollection();
            collection["sort"] = "Manager.Salary";
            var context = GetContext(collection);
            IList<Employee> employees = new List<Employee>();
            employees.Add(new Employee { Name = "A", Salary = 5, Manager = new Employee { Name = "-" } });
            employees.Add(new Employee { Name = "B", Salary = 20, Manager = employees[0] });
            employees.Add(new Employee { Name = "C", Salary = 15, Manager = employees[0] });
            employees.Add(new Employee { Name = "D", Salary = 5, Manager = employees[1] });
            var grid = new WebGrid(context).Bind(employees, columnNames: new[] { "Name", "Manager.Name" });

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => grid.GetHtml(),
                                                              "Column \"Manager.Salary\" does not exist.");
        }

        [Fact]
        public void TableDoesNotThrowIfUnsortableColumnIsExplicitlySpecifiedByUser()
        {
            // Arrange
            var context = GetContext();
            IList<Employee> employees = new List<Employee>();
            employees.Add(new Employee { Name = "A", Salary = 5, Manager = new Employee { Name = "-" } });
            employees.Add(new Employee { Name = "C", Salary = 15, Manager = employees[0] });
            employees.Add(new Employee { Name = "D", Salary = 10, Manager = employees[1] });

            // Act
            var grid = new WebGrid(context).Bind(employees, columnNames: new[] { "Name", "Manager.Name" });
            grid.SortColumn = "Salary";
            var html = grid.Table();

            // Assert
            Assert.Equal(5, grid.Rows[0]["Salary"]);
            Assert.Equal(10, grid.Rows[1]["Salary"]);
            Assert.Equal(15, grid.Rows[2]["Salary"]);

            UnitTestHelper.AssertEqualsIgnoreWhitespace(
                "<table><thead><tr>"
                + "<th scope=\"col\"><a href=\"?sort=Name&amp;sortdir=ASC\">Name</a></th>"
                + "<th scope=\"col\"><a href=\"?sort=Manager.Name&amp;sortdir=ASC\">Manager.Name</a></th>"
                + "</tr></thead><tbody>"
                + "<tr><td>A</td><td>-</td></tr>"
                + "<tr><td>D</td><td>C</td></tr>"
                + "<tr><td>C</td><td>A</td></tr>"
                + "</tbody></table>", html.ToString());
        }

        [Fact]
        public void TableDoesNotThrowIfUnsortableColumnIsDefaultSortColumn()
        {
            // Arrange
            var context = GetContext();
            IList<Employee> employees = new List<Employee>();
            employees.Add(new Employee { Name = "A", Salary = 5, Manager = new Employee { Name = "-" } });
            employees.Add(new Employee { Name = "C", Salary = 15, Manager = employees[0] });
            employees.Add(new Employee { Name = "D", Salary = 10, Manager = employees[1] });

            // Act
            var grid = new WebGrid(context, defaultSort: "Salary").Bind(employees, columnNames: new[] { "Name", "Manager.Name" });
            var html = grid.Table();

            // Assert
            Assert.Equal(5, grid.Rows[0]["Salary"]);
            Assert.Equal(10, grid.Rows[1]["Salary"]);
            Assert.Equal(15, grid.Rows[2]["Salary"]);

            UnitTestHelper.AssertEqualsIgnoreWhitespace(
                "<table><thead><tr>"
                + "<th scope=\"col\"><a href=\"?sort=Name&amp;sortdir=ASC\">Name</a></th>"
                + "<th scope=\"col\"><a href=\"?sort=Manager.Name&amp;sortdir=ASC\">Manager.Name</a></th>"
                + "</tr></thead><tbody>"
                + "<tr><td>A</td><td>-</td></tr>"
                + "<tr><td>D</td><td>C</td></tr>"
                + "<tr><td>C</td><td>A</td></tr>"
                + "</tbody></table>", html.ToString());
        }

        [Fact]
        public void CustomSorter()
        {
            // Arrange
            var context = GetContext();
            IList<Employee> employees = new List<Employee>();
            employees.Add(new Employee { Name = "A", Salary = 10, Manager = new Employee { Name = "C" } });
            employees.Add(new Employee { Name = "B", Salary = 20, Manager = null });
            employees.Add(new Employee { Name = "C", Salary = 30, Manager = new Employee { Name = "A" } });

            // Act
            var grid = new WebGrid(context, defaultSort: "Salary")
                .AddSorter("Manager.Name", (Employee x) => (x == null || x.Manager == null) ? null : x.Manager.Name);

            grid.Bind(employees);

            grid.SortColumn = "Manager.Name";
            grid.SortDirection = SortDirection.Ascending;

            var html = grid.Table(columns: grid.Columns(
                grid.Column("Name"),
                grid.Column("Manager.Name",
                header: "Manager",
                format: item => item.Manager == null ? "" : item.Manager.Name)));

            // Assert
            Assert.Equal(20, grid.Rows[0]["Salary"]);
            Assert.Equal(30, grid.Rows[1]["Salary"]);
            Assert.Equal(10, grid.Rows[2]["Salary"]);

            UnitTestHelper.AssertEqualsIgnoreWhitespace(
                "<table><thead><tr>"
                + "<th scope=\"col\"><a href=\"?sort=Name&amp;sortdir=ASC\">Name</a></th>"
                + "<th scope=\"col\"><a href=\"?sort=Manager.Name&amp;sortdir=DESC\">Manager</a></th>"
                + "</tr></thead><tbody>"
                + "<tr><td>B</td><td></td></tr>"
                + "<tr><td>C</td><td>A</td></tr>"
                + "<tr><td>A</td><td>C</td></tr>"
                + "</tbody></table>", html.ToString());
        }

        private static IEnumerable<Person> Iterator()
        {
            yield return new Person { FirstName = "Foo", LastName = "Bar" };
        }

        [Fact]
        public void GetElementTypeReturnsEnumerableTypeIfCollectionPassedImplementsEnumerable()
        {
            // Arrange
            IList<Person> listElements = new List<Person> { new Person { FirstName = "Foo", LastName = "Bar" } };
            HashSet<dynamic> setElements = new HashSet<dynamic> { new DynamicWrapper(new Person { FirstName = "Foo", LastName = "Bar" }) };

            // Act
            Type listType = WebGrid.GetElementType(listElements);
            Type setType = WebGrid.GetElementType(setElements);

            // Assert
            Assert.Equal(typeof(Person), listType);
            Assert.Equal(typeof(IDynamicMetaObjectProvider), setType);
        }

        [Fact]
        public void GetElementTypeReturnsEnumerableTypeIfCollectionImplementsEnumerable()
        {
            // Arrange
            IEnumerable<Person> elements = new NonGenericEnumerable(new[] { new Person { FirstName = "Foo", LastName = "Bar" } });

            // Act
            Type type = WebGrid.GetElementType(elements);

            // Assert
            Assert.Equal(typeof(Person), type);
        }

        [Fact]
        public void GetElementTypeReturnsEnumerableTypeIfCollectionIsIEnumerable()
        {
            // Arrange
            IEnumerable<Person> elements = new GenericEnumerable<Person>(new[] { new Person { FirstName = "Foo", LastName = "Bar" } });

            // Act
            Type type = WebGrid.GetElementType(elements);

            // Assert
            Assert.Equal(typeof(Person), type);
        }

        [Fact]
        public void GetElementTypeDoesNotThrowIfTypeIsNotGeneric()
        {
            // Arrange
            IEnumerable<dynamic> elements = new[] { new Person { FirstName = "Foo", LastName = "Bar" } };

            // Act
            Type type = WebGrid.GetElementType(elements);

            // Assert
            Assert.Equal(typeof(Person), type);
        }

        [Fact]
        public void WebGridWithAttributesFromAnonymousObject_WithUnderscoreInName_TransformsUnderscoresToDashs()
        {
            // Arrange
            const string expected = @"data-name=""value""";
            const string unexpected = @"data_name=""value""";
            var attributes = new { data_name = "value" };

            var grid = new WebGrid(GetContext(), ajaxUpdateContainerId: "grid")
                    .Bind(new[]
            {
                new { P1 = 1, P2 = '2', P3 = "3" },
                new { P1 = 4, P2 = '5', P3 = "6" }
            });

            // Act
            var htmlString = grid.GetHtml(htmlAttributes: attributes).ToHtmlString();

            // Assert
            Assert.DoesNotContain(unexpected, htmlString);
            Assert.Contains(expected, htmlString);
        }


        private static IEnumerable<dynamic> Dynamics(params object[] objects)
        {
            return (from o in objects
                    select new DynamicWrapper(o)).ToArray();
        }

        private static HttpContextBase GetContext(NameValueCollection queryString = null)
        {
            Mock<HttpRequestBase> requestMock = new Mock<HttpRequestBase>();
            requestMock.Setup(request => request.QueryString).Returns(queryString ?? new NameValueCollection());

            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>();
            contextMock.Setup(context => context.Request).Returns(requestMock.Object);
            contextMock.Setup(context => context.Items).Returns(new Hashtable());
            return contextMock.Object;
        }

        class Person
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        private class Employee
        {
            public string Name { get; set; }
            public int Salary { get; set; }
            public Employee Manager { get; set; }
        }

        class NonGenericEnumerable : IEnumerable<Person>
        {
            private IEnumerable<Person> _source;

            public NonGenericEnumerable(IEnumerable<Person> source)
            {
                _source = source;
            }

            public IEnumerator<Person> GetEnumerator()
            {
                return _source.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        class GenericEnumerable<T> : IEnumerable<T>
        {
            private IEnumerable<T> _source;

            public GenericEnumerable(IEnumerable<T> source)
            {
                _source = source;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _source.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
