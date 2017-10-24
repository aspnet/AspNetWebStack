// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Drawing;
using System.IO;
using System.Web.Hosting;
using System.Web.UI.DataVisualization.Charting;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Helpers.Test
{
    public class ChartTest
    {
        private byte[] _writeData;

        public ChartTest()
        {
            _writeData = null;
        }

        [Fact]
        public void BuildChartAddsDefaultArea()
        {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            AssertBuiltChartAction(chart, c =>
            {
                ChartArea chartArea = Assert.Single(c.ChartAreas);
                Assert.Equal("Default", chartArea.Name);
            });
        }

        [Fact]
        public void XAxisOverrides()
        {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100)
                .SetXAxis("AxisX", 1, 100);
            AssertBuiltChartAction(chart, c =>
            {
                ChartArea chartArea = Assert.Single(c.ChartAreas);
                Assert.Equal("AxisX", chartArea.AxisX.Title);
                Assert.Equal(1, chartArea.AxisX.Minimum);
                Assert.Equal(100, chartArea.AxisX.Maximum);
            });
        }

        [Fact]
        public void YAxisOverrides()
        {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100)
                .SetYAxis("AxisY", 1, 100);
            AssertBuiltChartAction(chart, c =>
            {
                ChartArea chartArea = Assert.Single(c.ChartAreas);
                Assert.Equal("AxisY", chartArea.AxisY.Title);
                Assert.Equal(1, chartArea.AxisY.Minimum);
                Assert.Equal(100, chartArea.AxisY.Maximum);
            });
        }

        [Fact]
        public void ConstructorLoadsTemplate()
        {
            var template = WriteTemplate(@"<Chart BorderWidth=""2""></Chart>");
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100, themePath: template);
            AssertBuiltChartAction(chart, c => { Assert.Equal(2, c.BorderWidth); });
        }

        [Fact]
        public void ConstructorLoadsTheme()
        {
            //Vanilla theme
            /*
             * <Chart Palette="SemiTransparent" BorderColor="#000" BorderWidth="2" BorderlineDashStyle="Solid">
                <ChartAreas>
                    <ChartArea _Template_="All" Name="Default">
                            <AxisX>
                                <MinorGrid Enabled="False" />
                                <MajorGrid Enabled="False" />
                            </AxisX>
                            <AxisY>
                                <MajorGrid Enabled="False" />
                                <MinorGrid Enabled="False" />
                            </AxisY>
                    </ChartArea>
                </ChartAreas>
                </Chart>
             */
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100, theme: ChartTheme.Vanilla);
            AssertBuiltChartAction(chart, c =>
            {
                Assert.Equal(ChartColorPalette.SemiTransparent, c.Palette);
                Assert.Equal(c.BorderColor, Color.FromArgb(0, Color.Black));
                ChartArea chartArea = Assert.Single(c.ChartAreas);
                Assert.False(chartArea.AxisX.MajorGrid.Enabled);
                Assert.False(chartArea.AxisY.MinorGrid.Enabled);
            });
        }

        [Fact]
        public void ConstructorLoadsThemeAndTemplate()
        {
            //Vanilla theme
            /*
             * <Chart Palette="SemiTransparent" BorderColor="#000" BorderWidth="2" BorderlineDashStyle="Solid">
                <ChartAreas>
                    <ChartArea _Template_="All" Name="Default">
                            <AxisX>
                                <MinorGrid Enabled="False" />
                                <MajorGrid Enabled="False" />
                            </AxisX>
                            <AxisY>
                                <MajorGrid Enabled="False" />
                                <MinorGrid Enabled="False" />
                            </AxisY>
                    </ChartArea>
                </ChartAreas>
                </Chart>
             */
            var template = WriteTemplate(@"<Chart BorderlineDashStyle=""DashDot""><Legends><Legend BackColor=""Red"" /></Legends></Chart>");
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100, theme: ChartTheme.Vanilla, themePath: template);
            AssertBuiltChartAction(chart, c =>
            {
                Assert.Equal(ChartColorPalette.SemiTransparent, c.Palette);
                Assert.Equal(c.BorderColor, Color.FromArgb(0, Color.Black));
                Assert.Equal(ChartDashStyle.DashDot, c.BorderlineDashStyle);
                Legend legend = Assert.Single(c.Legends);
                Assert.Equal(legend.BackColor, Color.Red);
                ChartArea chartArea = Assert.Single(c.ChartAreas);
                Assert.False(chartArea.AxisX.MajorGrid.Enabled);
                Assert.False(chartArea.AxisY.MinorGrid.Enabled);
            });
        }

        [Fact]
        public void ConstructorLoadsThemeAndTemplate_VPPRegistrationChanging()
        {
            // Arrange
            // Vanilla theme, as in ConstructorLoadsThemeAndTemplate()
            string template = WriteTemplate(@"<Chart BorderlineDashStyle=""DashDot""><Legends><Legend BackColor=""Red"" /></Legends></Chart>");
            HttpContextBase context = GetContext();
            string templatePath = VirtualPathUtility.Combine(context.Request.AppRelativeCurrentExecutionFilePath, template);

            MockVirtualPathProvider provider1 = new MockVirtualPathProvider();
            MockVirtualPathProvider provider2 = new MockVirtualPathProvider();
            VirtualPathProvider provider = provider1;

            // Act; use one provider in constructor and another in ExecuteChartAction()
            Chart chart = new Chart(context, () => provider, 100, 100, theme: ChartTheme.Vanilla, themePath: template);

            // The moral equivalent of HostingEnvironment.RegisterVirtualPathProvider(provider2)
            provider = provider2;

            // Assert
            AssertBuiltChartAction(chart, c =>
            {
                Assert.Equal(ChartColorPalette.SemiTransparent, c.Palette);
                Assert.Equal(c.BorderColor, Color.FromArgb(0, Color.Black));
                Assert.Equal(ChartDashStyle.DashDot, c.BorderlineDashStyle);
                Legend legend = Assert.Single(c.Legends);
                Assert.Equal(legend.BackColor, Color.Red);
                ChartArea chartArea = Assert.Single(c.ChartAreas);
                Assert.False(chartArea.AxisX.MajorGrid.Enabled);
                Assert.False(chartArea.AxisY.MinorGrid.Enabled);
            });

            Assert.Equal(1, provider1.FileExistsCalls);
            Assert.Equal(0, provider1.GetFileCalls);
            Assert.Equal(0, provider2.FileExistsCalls);
            Assert.Equal(1, provider2.GetFileCalls);
        }

        [Fact]
        public void ConstructorSetsWidthAndHeight()
        {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 101, 102);
            Assert.Equal(101, chart.Width);
            Assert.Equal(102, chart.Height);
            AssertBuiltChartAction(chart, c =>
            {
                Assert.Equal(101, c.Width);
                Assert.Equal(102, c.Height);
            });
        }

        [Fact]
        public void ConstructorThrowsWhenHeightIsLessThanZero()
        {
            Assert.ThrowsArgumentOutOfRange(() => { new Chart(GetContext(), GetVirtualPathProvider(), 100, -1); }, "height", "Value must be greater than or equal to 0.");
        }

        [Fact]
        public void ConstructorThrowsWhenTemplateNotFound()
        {
            var templateFile = @"FileNotFound.xml";
            Assert.ThrowsArgument(() => { new Chart(GetContext(), GetVirtualPathProvider(), 100, 100, themePath: templateFile); },
                                                    "themePath",
                                                    String.Format("The theme file \"{0}\" could not be found.", VirtualPathUtility.Combine(GetContext().Request.AppRelativeCurrentExecutionFilePath, templateFile)));
        }

        [Fact]
        public void ConstructorThrowsWhenWidthIsLessThanZero()
        {
            Assert.ThrowsArgumentOutOfRange(() => { new Chart(GetContext(), GetVirtualPathProvider(), -1, 100); }, "width", "Value must be greater than or equal to 0.");
        }

        [Fact]
        public void DataBindCrossTable()
        {
            var data = new[]
            {
                new { GroupBy = "1", YValue = 1 },
                new { GroupBy = "1", YValue = 2 },
                new { GroupBy = "2", YValue = 1 }
            };
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100)
                .DataBindCrossTable(data, "GroupBy", xField: null, yFields: "YValue");
            // todo - anything else to verify here?
            AssertBuiltChartAction(chart, c =>
            {
                Assert.Equal(2, c.Series.Count);
                Assert.Equal(2, c.Series[0].Points.Count);
                Assert.Single(c.Series[1].Points);
            });
        }

        [Fact]
        public void DataBindCrossTableThrowsWhenDataSourceIsNull()
        {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            Assert.ThrowsArgumentNull(() => { chart.DataBindCrossTable(null, "GroupBy", xField: null, yFields: "yFields"); }, "dataSource");
        }

        [Fact]
        public void DataBindCrossTableThrowsWhenDataSourceIsString()
        {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            Assert.ThrowsArgument(() => { chart.DataBindCrossTable("DataSource", "GroupBy", xField: null, yFields: "yFields"); }, "dataSource", "A series cannot be data-bound to a string object.");
        }

        [Fact]
        public void DataBindCrossTableThrowsWhenGroupByIsNull()
        {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            Assert.ThrowsArgumentNullOrEmptyString(() => { chart.DataBindCrossTable(new object[0], null, xField: null, yFields: "yFields"); }, "groupByField");
        }

        [Fact]
        public void DataBindCrossTableThrowsWhenGroupByIsEmpty()
        {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            Assert.ThrowsArgumentNullOrEmptyString(() => { chart.DataBindCrossTable(new object[0], "", xField: null, yFields: "yFields"); }, "groupByField");
        }

        [Fact]
        public void DataBindCrossTableThrowsWhenYFieldsIsNull()
        {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            Assert.ThrowsArgumentNullOrEmptyString(() => { chart.DataBindCrossTable(new object[0], "GroupBy", xField: null, yFields: null); }, "yFields");
        }

        [Fact]
        public void DataBindCrossTableThrowsWhenYFieldsIsEmpty()
        {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            Assert.ThrowsArgumentNullOrEmptyString(() => { chart.DataBindCrossTable(new object[0], "GroupBy", xField: null, yFields: ""); }, "yFields");
        }

        [Fact]
        public void DataBindTable()
        {
            var data = new[]
            {
                new { XValue = "1", YValue = 1 },
                new { XValue = "2", YValue = 2 },
                new { XValue = "3", YValue = 3 }
            };
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100)
                .DataBindTable(data, xField: "XValue");
            // todo - anything else to verify here?
            AssertBuiltChartAction(chart, c =>
            {
                var series = Assert.Single(c.Series);
                Assert.Equal(3, series.Points.Count);
            });
        }

        [Fact]
        public void DataBindTableWhenXFieldIsNull()
        {
            var data = new[]
            {
                new { YValue = 1 },
                new { YValue = 2 },
                new { YValue = 3 }
            };
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100)
                .DataBindTable(data, xField: null);
            // todo - anything else to verify here?
            AssertBuiltChartAction(chart, c =>
            {
                var series = Assert.Single(c.Series);
                Assert.Equal(3, series.Points.Count);
            });
        }

        [Fact]
        public void DataBindTableThrowsWhenDataSourceIsNull()
        {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            Assert.ThrowsArgumentNull(() => { chart.DataBindTable(null); }, "dataSource");
        }

        [Fact]
        public void DataBindTableThrowsWhenDataSourceIsString()
        {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            Assert.ThrowsArgument(() => { chart.DataBindTable(""); }, "dataSource", "A series cannot be data-bound to a string object.");
        }

        [Fact]
        public void GetBytesReturnsNonEmptyArray()
        {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            Assert.True(chart.GetBytes().Length > 0);
        }

        [Fact]
        public void GetBytesThrowsWhenFormatIsEmpty()
        {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            Assert.ThrowsArgumentNullOrEmptyString(() => { chart.GetBytes(format: String.Empty); }, "format");
        }

        [Fact]
        public void GetBytesThrowsWhenFormatIsInvalid()
        {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            Assert.ThrowsArgument(() => { chart.GetBytes(format: "foo"); }, "format", "\"foo\" is invalid image format. Valid values are image format names like: \"JPEG\", \"BMP\", \"GIF\", \"PNG\", etc.");
        }

        [Fact]
        public void GetBytesThrowsWhenFormatIsNull()
        {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            Assert.ThrowsArgumentNullOrEmptyString(() => { chart.GetBytes(format: null); }, "format");
        }

        [Fact]
        public void LegendDefaults()
        {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100).AddLegend();
            AssertBuiltChartAction(chart, c =>
            {
                Legend legend = Assert.Single(c.Legends);
                // NOTE: Chart.Legends.Add will create default name
                Assert.Equal("Legend1", legend.Name);
                Assert.Equal(1, legend.BorderWidth);
            });
        }

        [Fact]
        public void LegendOverrides()
        {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100).AddLegend("Legend1")
                .AddLegend("Legend2", "Legend2Name");
            AssertBuiltChartAction(chart, c =>
            {
                Assert.Equal(2, c.Legends.Count);
                Assert.Equal("Legend1", c.Legends[0].Name);
                Assert.Equal("Legend2", c.Legends[1].Title);
                Assert.Equal("Legend2Name", c.Legends[1].Name);
            });
        }

        [Fact]
        public void SaveAndWriteFromCache()
        {
            var context1 = GetContext();
            var chart = new Chart(context1, GetVirtualPathProvider(), 100, 100);

            string key = chart.SaveToCache();
            Assert.Equal(chart, WebCache.Get(key));

            var context2 = GetContext();
            Assert.Equal(chart, Chart.GetFromCache(context2, key));

            Chart.WriteFromCache(context2, key);

            Assert.Null(context1.Response.ContentType);
            Assert.Equal("image/jpeg", context2.Response.ContentType);
        }

        [Fact]
        public void SaveThrowsWhenFormatIsEmpty()
        {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            Assert.ThrowsArgumentNullOrEmptyString(() => { chart.Save(GetContext(), "chartPath", format: String.Empty); }, "format");
        }

        [Fact]
        public void SaveWorksWhenFormatIsJPG()
        {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);

            string fileName = "chartPath";

            chart.Save(GetContext(), "chartPath", format: "jpg");
            byte[] a = File.ReadAllBytes(fileName);

            chart.Save(GetContext(), "chartPath", format: "jpeg");
            byte[] b = File.ReadAllBytes(fileName);

            Assert.Equal(a, b);
        }

        [Fact]
        public void SaveThrowsWhenFormatIsInvalid()
        {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            Assert.ThrowsArgument(() => { chart.Save(GetContext(), "chartPath", format: "foo"); }, "format", "\"foo\" is invalid image format. Valid values are image format names like: \"JPEG\", \"BMP\", \"GIF\", \"PNG\", etc.");
        }

        [Fact]
        public void SaveThrowsWhenFormatIsNull()
        {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            Assert.ThrowsArgumentNullOrEmptyString(() => { chart.Save(GetContext(), "chartPath", format: null); }, "format");
        }

        [Fact]
        public void SaveThrowsWhenPathIsEmpty()
        {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            Assert.ThrowsArgumentNullOrEmptyString(() => { chart.Save(GetContext(), path: String.Empty, format: "jpeg"); }, "path");
        }

        [Fact]
        public void SaveThrowsWhenPathIsNull()
        {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            Assert.ThrowsArgumentNullOrEmptyString(() => { chart.Save(GetContext(), path: null, format: "jpeg"); }, "path");
        }

        [Fact]
        public void SaveWritesToFile()
        {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            chart.Save(GetContext(), "SaveWritesToFile.jpg", format: "image/jpeg");
            Assert.Equal("SaveWritesToFile.jpg", Path.GetFileName(chart.FileName));
            Assert.True(File.Exists(chart.FileName));
        }

        [Fact]
        public void SaveXmlThrowsWhenPathIsEmpty()
        {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            Assert.ThrowsArgumentNullOrEmptyString(() => { chart.SaveXml(GetContext(), String.Empty); }, "path");
        }

        [Fact]
        public void SaveXmlThrowsWhenPathIsNull()
        {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            Assert.ThrowsArgumentNullOrEmptyString(() => { chart.SaveXml(GetContext(), null); }, "path");
        }

        [Fact]
        public void SaveXmlWritesToFile()
        {
            var template = WriteTemplate(@"<Chart BorderWidth=""2""></Chart>");
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100, themePath: template);
            chart.SaveXml(GetContext(), "SaveXmlWritesToFile.xml");
            Assert.True(File.Exists("SaveXmlWritesToFile.xml"));
            string result = File.ReadAllText("SaveXmlWritesToFile.xml");
            Assert.Contains("BorderWidth=\"2\"", result);
        }

        [Fact]
        public void TemplateWithCommentsDoesNotThrow()
        {
            var template = WriteTemplate(@"<Chart BorderWidth=""2""><!-- This is a XML comment.  --> </Chart>");
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100, themePath: template);
            Assert.NotNull(chart.ToWebImage());
        }

        [Fact]
        public void TemplateWithIncorrectPropertiesThrows()
        {
            var template = WriteTemplate(@"<Chart borderWidth=""2""><fjkjkgjklfg /></Chart>");
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100, themePath: template);
            Assert.Throws<InvalidOperationException>(() => chart.ToWebImage(),
                                                              "Cannot deserialize property. Unknown property name 'borderWidth' in object \" System.Web.UI.DataVisualization.Charting.Chart");
        }

        [Fact]
        public void WriteWorksWithJPGFormat()
        {
            var response = new Mock<HttpResponseBase>();
            var stream = new MemoryStream();
            response.Setup(c => c.Output).Returns(new StreamWriter(stream));

            var context = new Mock<HttpContextBase>();
            context.Setup(c => c.Response).Returns(response.Object);

            var chart = new Chart(context.Object, GetVirtualPathProvider(), 100, 100);
            chart.Write("jpeg");

            byte[] a = stream.GetBuffer();

            stream.SetLength(0);
            chart.Write("jpg");
            byte[] b = stream.GetBuffer();

            Assert.Equal(a, b);
        }

        [Fact]
        public void WriteThrowsWithInvalidFormat()
        {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            Assert.ThrowsArgument(() => chart.Write("foo"),
                                                    "format", "\"foo\" is invalid image format. Valid values are image format names like: \"JPEG\", \"BMP\", \"GIF\", \"PNG\", etc.");
        }

        [Fact]
        public void SeriesOverrides()
        {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100)
                .AddSeries(chartType: "Bar");
            AssertBuiltChartAction(chart, c =>
            {
                var series = Assert.Single(c.Series);
                Assert.Equal(SeriesChartType.Bar, series.ChartType);
            });
        }

        [Fact]
        public void SeriesThrowsWhenChartTypeIsEmpty()
        {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            Assert.ThrowsArgumentNullOrEmptyString(() => { chart.AddSeries(chartType: ""); }, "chartType");
        }

        [Fact]
        public void SeriesThrowsWhenChartTypeIsNull()
        {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            Assert.ThrowsArgumentNullOrEmptyString(() => { chart.AddSeries(chartType: null); }, "chartType");
        }

        [Fact]
        public void TitleDefaults()
        {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100).AddTitle();
            AssertBuiltChartAction(chart, c =>
            {
                var title = Assert.Single(c.Titles);
                // NOTE: Chart.Titles.Add will create default name
                Assert.Equal("Title1", title.Name);
                Assert.Equal(String.Empty, title.Text);
                Assert.Equal(1, title.BorderWidth);
            });
        }

        [Fact]
        public void TitleOverrides()
        {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100).AddTitle(name: "Title1")
                .AddTitle("Title2Text", name: "Title2");
            AssertBuiltChartAction(chart, c =>
            {
                Assert.Equal(2, c.Titles.Count);
                Assert.Equal("Title1", c.Titles[0].Name);
                Assert.Equal("Title2", c.Titles[1].Name);
                Assert.Equal("Title2Text", c.Titles[1].Text);
            });
        }

        [Fact]
        public void ToWebImage()
        {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            var image = chart.ToWebImage();
            Assert.NotNull(image);
            Assert.Equal("jpeg", image.ImageFormat);
        }

        [Fact]
        public void ToWebImageUsesFormat()
        {
            var chart = new Chart(GetContext(), GetVirtualPathProvider(), 100, 100);
            var image = chart.ToWebImage(format: "png");
            Assert.NotNull(image);
            Assert.Equal("png", image.ImageFormat);
        }

        [Fact]
        public void WriteFromCacheIsNoOpIfNotSavedInCache()
        {
            var context = GetContext();
            Assert.Null(Chart.WriteFromCache(context, Guid.NewGuid().ToString()));
            Assert.Null(context.Response.ContentType);
        }

        [Fact]
        public void WriteUpdatesResponse()
        {
            var context = GetContext();
            var chart = new Chart(context, GetVirtualPathProvider(), 100, 100);
            chart.Write();
            Assert.Equal("", context.Response.Charset);
            Assert.Equal("image/jpeg", context.Response.ContentType);
            Assert.True((_writeData != null) && (_writeData.Length > 0));
        }

        private static void AssertBuiltChartAction(Chart chart, Action<UI.DataVisualization.Charting.Chart> action)
        {
            bool actionCalled = false;
            chart.ExecuteChartAction(c =>
            {
                action(c);
                actionCalled = true;
            });
            Assert.True(actionCalled);
        }

        private HttpContextBase GetContext()
        {
            // Strip drive letter for VirtualPathUtility.Combine
            var testPath = Directory.GetCurrentDirectory().Substring(2) + "/Out";
            Mock<HttpRequestBase> request = new Mock<HttpRequestBase>();
            request.Setup(r => r.AppRelativeCurrentExecutionFilePath).Returns(testPath);
            request.Setup(r => r.MapPath(It.IsAny<string>())).Returns((string path) => path);

            Mock<HttpResponseBase> response = new Mock<HttpResponseBase>();
            response.SetupProperty(r => r.ContentType);
            response.SetupProperty(r => r.Charset);
            response.Setup(r => r.BinaryWrite(It.IsAny<byte[]>())).Callback((byte[] data) => _writeData = data);

            Mock<HttpServerUtilityBase> server = new Mock<HttpServerUtilityBase>();
            server.Setup(s => s.MapPath(It.IsAny<string>())).Returns((string s) => s);

            var items = new Hashtable();

            Mock<HttpContextBase> context = new Mock<HttpContextBase>();
            context.Setup(c => c.Request).Returns(request.Object);
            context.Setup(c => c.Response).Returns(response.Object);
            context.Setup(c => c.Server).Returns(server.Object);
            context.Setup(c => c.Items).Returns(items);
            return context.Object;
        }

        private static string WriteTemplate(string xml)
        {
            var path = Guid.NewGuid() + ".xml";
            File.WriteAllText(path, xml);
            return path;
        }

        private static MockVirtualPathProvider GetVirtualPathProvider()
        {
            return new MockVirtualPathProvider();
        }

        class MockVirtualPathProvider : VirtualPathProvider
        {
            public int FileExistsCalls { get; private set; }

            public int GetFileCalls { get; private set; }

            class MockVirtualFile : VirtualFile
            {
                public MockVirtualFile(string virtualPath)
                    : base(virtualPath)
                {
                }

                public override Stream Open()
                {
                    return File.Open(this.VirtualPath, FileMode.Open);
                }
            }

            public override bool FileExists(string virtualPath)
            {
                ++FileExistsCalls;
                return File.Exists(virtualPath);
            }

            public override VirtualFile GetFile(string virtualPath)
            {
                ++GetFileCalls;
                return new MockVirtualFile(virtualPath);
            }
        }
    }
}
