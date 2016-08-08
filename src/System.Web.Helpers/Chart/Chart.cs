// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web.Helpers.Resources;
using System.Web.Hosting;
using System.Web.UI.DataVisualization.Charting;
using System.Web.UI.WebControls;
using System.Web.WebPages;
using System.Xml;
using Microsoft.Internal.Web.Utils;

namespace System.Web.Helpers
{
    // Post-Beta Work:
    // -DataBind and Points.DataBind - need to find scenarios
    // -Elements: Annotations, MapAreas
    // -Interactivity / AJAX support?
    public class Chart
    {
        private readonly int _height;
        private readonly int _width;
        private readonly string _themePath;
        private readonly string _theme;

        private readonly List<LegendData> _legends = new List<LegendData>();
        private readonly List<SeriesData> _series = new List<SeriesData>();
        private readonly List<TitleData> _titles = new List<TitleData>();

        private HttpContextBase _httpContext;
        private Func<VirtualPathProvider> _virtualPathProviderFunc;

        private string _path;

        private DataSourceData _dataSource;
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Target = "_xAxis, _yAxis",
           Justification = "These names make most sense.")]
        private ChartAxisData _xAxis, _yAxis;

#if CODE_COVERAGE
        [ExcludeFromCodeCoverage]
#endif

        /// <param name="width">Chart width in pixels.</param>
        /// <param name="height">Chart height in pixels.</param>
        /// <param name="theme">String containing chart theme definition. Chart's theme defines properties like colors, positions, etc.
        /// This parameter is primarily meant for one of the predefined Chart themes, however any valid chart theme is acceptable.</param>
        /// <param name="themePath">Path to a file containing definition of chart theme, default is none.</param>
        /// <remarks>Both the theme and themePath parameters can be specified. In this case, the Chart class applies the theme xml first 
        /// followed by the content of file at themePath.
        /// </remarks>
        /// <example>
        /// Chart(100, 100, theme: ChartTheme.Blue)
        /// Chart(100, 100, theme: ChartTheme.Vanilla, themePath: "my-theme.xml")
        /// Chart(100, 100, theme: ".... definition inline ...." ) 
        /// Chart(100, 100, themePath: "my-theme.xml")
        /// Any valid theme definition can be used as content of the file specified in themePath
        /// </example>
        public Chart(
            int width,
            int height,
            string theme = null,
            string themePath = null)
            : this(GetDefaultContext(), () => HostingEnvironment.VirtualPathProvider, width, height, theme, themePath)
        {
        }

        // Overload used only for testing
        internal Chart(HttpContextBase httpContext, VirtualPathProvider virtualPathProvider, int width, int height,
                       string theme = null, string themePath = null)
            : this(httpContext, () => virtualPathProvider, width, height, theme, themePath)
        {
        }

        internal Chart(HttpContextBase httpContext, Func<VirtualPathProvider> virtualPathProviderFunc,
            int width, int height, string theme = null, string themePath = null)
        {
            Contract.Assert(httpContext != null);
            Contract.Assert(virtualPathProviderFunc != null);

            // HostingEnvironment.VirtualPathProvider never null in running host but may change at any time.
            Contract.Assert(virtualPathProviderFunc() != null);

            if (width < 0)
            {
                throw new ArgumentOutOfRangeException("width", String.Format(
                    CultureInfo.CurrentCulture,
                    CommonResources.Argument_Must_Be_GreaterThanOrEqualTo,
                    0));
            }
            if (height < 0)
            {
                throw new ArgumentOutOfRangeException("height", String.Format(
                    CultureInfo.CurrentCulture,
                    CommonResources.Argument_Must_Be_GreaterThanOrEqualTo,
                    0));
            }

            _httpContext = httpContext;
            _virtualPathProviderFunc = virtualPathProviderFunc;
            _width = width;
            _height = height;
            _theme = theme;

            // path must be app-relative in case chart is rendered from handler in different directory
            if (!String.IsNullOrEmpty(themePath))
            {
                _themePath = VirtualPathUtil.ResolvePath(TemplateStack.GetCurrentTemplate(httpContext), httpContext, themePath);
                if (!virtualPathProviderFunc().FileExists(_themePath))
                {
                    throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, HelpersResources.Chart_ThemeFileNotFound, _themePath), "themePath");
                }
            }
        }

        public string FileName
        {
            get { return _path; }
        }

        public int Height
        {
            get { return _height; }
        }

        public int Width
        {
            get { return _width; }
        }

        /// <param name="title">Legend title.</param>
        /// <param name="name">Legend name.</param>
        public Chart AddLegend(
            string title = null,
            string name = null)
        {
            _legends.Add(new LegendData
            {
                Name = name,
                Title = title
            });
            return this;
        }

        /// <param name="name">Series name.</param>
        /// <param name="chartType">Chart type (see: SeriesChartType).</param>
        /// <param name="chartArea">Chart area where the series is displayed.</param>
        /// <param name="axisLabel">Axis label for the series.</param>
        /// <param name="legend">Legend for the series.</param>
        /// <param name="markerStep">Axis marker step.</param>
        /// <param name="xValue">X data source, if data-binding the series.</param>
        /// <param name="xField">Column for the X data points, if data-binding the series.</param>
        /// <param name="yValues">Y data source(s), if data-binding the series.</param>
        /// <param name="yFields">Column(s) for the Y data points, if data-binding the series.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "x",
            Justification = "Name based on X-axis. Suppressed in source because this is a one-time occurrence")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "y",
            Justification = "Name based on Y-axis. Suppressed in source because this is a one-time occurrence")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Target = "xValue, xField, yValues, yFields",
            Justification = "These names cannot be changed because this is a public method.")]
        public Chart AddSeries(
            string name = null,
            string chartType = "Column",
            string chartArea = null,
            string axisLabel = null,
            string legend = null,
            int markerStep = 1,
            IEnumerable xValue = null,
            string xField = null,
            IEnumerable yValues = null,
            string yFields = null)
        {
            if (String.IsNullOrEmpty(chartType))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "chartType");
            }

            DataSourceData dataSource = null;
            if (yValues != null)
            {
                dataSource = new DataSourceData
                {
                    XDataSource = xValue,
                    XField = xField,
                    DataSource = yValues,
                    YFields = yFields
                };
            }

            _series.Add(new SeriesData
            {
                Name = name,
                ChartType = ConvertStringArgument<SeriesChartType>("chartType", chartType),
                ChartArea = chartArea,
                AxisLabel = axisLabel,
                Legend = legend,
                MarkerStep = markerStep,
                DataSource = dataSource
            });
            return this;
        }

        /// <param name="text">Title text.</param>
        /// <param name="name">Title name.</param>
        public Chart AddTitle(
            string text = null,
            string name = null)
        {
            _titles.Add(new TitleData
            {
                Name = name,
                Text = text
            });
            return this;
        }

        /// <param name="title">Title for X-axis</param>
        /// <param name="min">The minimum value on X-axis. Default 0</param>
        /// <param name="max">The maximum value on X-axis. Default NaN</param>
        public Chart SetXAxis(
            string title = "",
            double min = 0,
            double max = Double.NaN)
        {
            _xAxis = new ChartAxisData { Title = title, Minimum = min, Maximum = max };
            return this;
        }

        /// <param name="title">Title for Y-axis</param>
        /// <param name="min">The minimum value on Y-axis. Default 0</param>
        /// <param name="max">The maximum value on Y-axis. Default NaN</param>
        public Chart SetYAxis(
            string title = "",
            double min = 0,
            double max = Double.NaN)
        {
            _yAxis = new ChartAxisData { Title = title, Minimum = min, Maximum = max };
            return this;
        }

        /// <summary>
        /// Data-binds the chart by grouping values in a series.  The series will be created by the chart.
        /// </summary>
        /// <param name="dataSource">Chart data source.</param>
        /// <param name="groupByField">Column which series should be grouped by.</param>
        /// <param name="xField">Column for the X data points.</param>
        /// <param name="yFields">Column(s) for the Y data points, separated by comma.</param>
        /// <param name="otherFields"></param>
        /// <param name="pointSortOrder">Sort order (see: PointSortOrder).</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "x",
            Justification = "Name based on X-axis. Suppressed in source because this is a one-time occurrence")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "y",
            Justification = "Name based on Y-axis. Suppressed in source because this is a one-time occurrence")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Target = "xField, yFields",
            Justification = "These names cannot be changed because this is a public method.")]
        public Chart DataBindCrossTable(IEnumerable dataSource, string groupByField, string xField, string yFields,
                                        string otherFields = null, string pointSortOrder = "Ascending")
        {
            if (dataSource == null)
            {
                throw new ArgumentNullException("dataSource");
            }
            if (dataSource is string)
            {
                throw new ArgumentException(HelpersResources.Chart_ExceptionDataBindSeriesToString, "dataSource");
            }
            if (String.IsNullOrEmpty(groupByField))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "groupByField");
            }
            if (String.IsNullOrEmpty(yFields))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "yFields");
            }

            _dataSource = new DataSourceData
            {
                DataSource = dataSource,
                GroupByField = groupByField,
                XField = xField,
                YFields = yFields,
                OtherFields = otherFields,
                PointSortOrder = ConvertStringArgument<PointSortOrder>("pointSortOrder", pointSortOrder)
            };
            return this;
        }

        /// <summary>
        /// Data-binds the chart using a data source, with multiple y values supported.  The series will be created by the chart.
        /// </summary>
        /// <param name="dataSource">Chart data source.</param>
        /// <param name="xField">Column for the X data points.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "x",
            Justification = "Name based on X-axis. Suppressed in source because this is a one-time occurrence")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Target = "xField",
            Justification = "These names cannot be changed because this is a public method.")]
        public Chart DataBindTable(IEnumerable dataSource, string xField = null)
        {
            if (dataSource == null)
            {
                throw new ArgumentNullException("dataSource");
            }
            if (dataSource is string)
            {
                throw new ArgumentException(HelpersResources.Chart_ExceptionDataBindSeriesToString, "dataSource");
            }

            _dataSource = new DataSourceData
            {
                DataBindTable = true,
                DataSource = dataSource,
                XField = xField
            };
            return this;
        }

        /// <summary>
        /// Get the bytes for the chart image.
        /// </summary>
        /// <param name="format">Image format (see: ChartImageFormat).</param>
        public byte[] GetBytes(string format = "jpeg")
        {
            var imageFormat = ConvertStringToChartImageFormat(format);
            using (MemoryStream stream = new MemoryStream())
            {
                ExecuteChartAction(c =>
                {
                    c.SaveImage(stream, imageFormat);
                });
                return stream.ToArray();
            }
        }

#if CODE_COVERAGE
        [ExcludeFromCodeCoverage]
#endif

        /// <summary>
        /// Loads a chart from the cache. This can be used to render from an image handler.
        /// </summary>
        /// <param name="key">Cache key.</param>
        public static Chart GetFromCache(string key)
        {
            return GetFromCache(GetDefaultContext(), key);
        }

        /// <summary>
        /// Saves the chart image to a file.
        /// </summary>
        /// <param name="path">File path.</param>
        /// <param name="format">Chart image format (see: ChartImageFormat).</param>
        public Chart Save(string path, string format = "jpeg")
        {
            return Save(GetDefaultContext(), path, format);
        }

        internal Chart Save(HttpContextBase httpContext, string path, string format)
        {
            if (String.IsNullOrEmpty(path))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "path");
            }
            var imageFormat = ConvertStringToChartImageFormat(format);

            _path = VirtualPathUtil.MapPath(httpContext, path);
            ExecuteChartAction(c =>
            {
                c.RenderType = RenderType.ImageTag;
                c.SaveImage(FileName, imageFormat);
            });
            return this;
        }

        /// <summary>
        /// Saves the chart in cache.  This can be used to render from an image handler.
        /// </summary>
        /// <param name="key">Cache key.  Uses new GUID by default.</param>
        /// <param name="minutesToCache">Number of minutes to save in cache.</param>
        /// <param name="slidingExpiration">Whether a sliding expiration policy is used.</param>
        /// <returns>Cache key.</returns>
        public string SaveToCache(string key = null, int minutesToCache = 20, bool slidingExpiration = true)
        {
            if (String.IsNullOrEmpty(key))
            {
                key = GetUniqueKey();
            }

            WebCache.Set(key, this, minutesToCache, slidingExpiration);
            return key;
        }

        /// <summary>
        /// Saves the chart to the specified template file.
        /// </summary>
        /// <param name="path">XML template file path.</param>
        public Chart SaveXml(string path)
        {
            return SaveXml(GetDefaultContext(), path);
        }

        /// <summary>
        /// Saves the chart to the specified template file.
        /// </summary>
        /// <param name="httpContext">The <see cref="HttpContextBase"/>.</param>
        /// <param name="path">XML template file path.</param>
        internal Chart SaveXml(HttpContextBase httpContext, string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "path");
            }

            ExecuteChartAction(c =>
            {
                c.SaveXml(VirtualPathUtil.MapPath(httpContext, path));
            });
            return this;
        }

        public WebImage ToWebImage(string format = "jpeg")
        {
            return new WebImage(GetBytes(format));
        }

        /// <summary>
        /// Writes the chart image to the response stream.  This can be used to render from an image handler.
        /// </summary>
        /// <param name="format">Image format (see: ChartImageFormat).</param>
        public Chart Write(string format = "jpeg")
        {
            var response = _httpContext.Response;
            response.Charset = String.Empty;
            response.ContentType = "image/" + NormalizeFormat(format);
            response.BinaryWrite(GetBytes(format));
            return this;
        }

#if CODE_COVERAGE
        [ExcludeFromCodeCoverage]
#endif

        /// <summary>
        /// Writes a chart stored in cache to the response stream.  This can be used to render from an image handler.
        /// </summary>
        /// <param name="key">Cache key.</param>
        /// <param name="format">Image format (see: ChartImageFormat).</param>
        public static Chart WriteFromCache(string key, string format = "jpeg")
        {
            return WriteFromCache(GetDefaultContext(), key, format);
        }

        // create and execute an action against the WebForm control in a limited scope since the control is disposable.
        internal void ExecuteChartAction(Action<UI.DataVisualization.Charting.Chart> action)
        {
            using (UI.DataVisualization.Charting.Chart chart = new UI.DataVisualization.Charting.Chart())
            {
                chart.Width = new Unit(_width);
                chart.Height = new Unit(_height);

                ApplyChartArea(chart);
                ApplyLegends(chart);
                ApplySeries(chart);
                ApplyTitles(chart);

                DataBindChart(chart);

                // load the template last so that it can be applied to all the chart elements
                LoadThemes(chart);

                action(chart);
            }
        }

        private void LoadThemes(UI.DataVisualization.Charting.Chart chart)
        {
            if (!String.IsNullOrEmpty(_theme))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    byte[] themeContent = Encoding.UTF8.GetBytes(_theme);
                    memoryStream.Write(themeContent, 0, themeContent.Length);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    LoadChartThemeFromFile(chart, memoryStream);
                }
            }

            if (!String.IsNullOrEmpty(_themePath))
            {
                using (Stream stream = _virtualPathProviderFunc().GetFile(_themePath).Open())
                {
                    LoadChartThemeFromFile(chart, stream);
                }
            }
        }

        private static void LoadChartThemeFromFile(UI.DataVisualization.Charting.Chart chart, Stream templateStream)
        {
            // workarounds for Chart templating bugs mentioned in:
            // http://social.msdn.microsoft.com/Forums/en-US/MSWinWebChart/thread/b50d5b7e-30e2-4948-af7a-370d9be1268a
            chart.Serializer.Content = SerializationContents.All;
            chart.Serializer.SerializableContent = String.Empty; // deserialize all content
            chart.Serializer.IsTemplateMode = true;
            chart.Serializer.IsResetWhenLoading = false;
            // loading serializer with stream to avoid bug with template file getting locked in VS

            // The default xml reader used by the serializer does not ignore comments
            // Using the IsUnknownAttributeIgnored fixes this, but then it would give no feedback to the user 
            // if member names do not match the spelling and casing of Chart properties. 
            XmlReader reader = XmlReader.Create(templateStream, new XmlReaderSettings { IgnoreComments = true });
            chart.Serializer.Load(reader);
        }

        internal static Chart GetFromCache(HttpContextBase context, string key)
        {
            Contract.Assert(context != null);

            if (String.IsNullOrEmpty(key))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "key");
            }

            var chart = WebCache.Get(key) as Chart;
            if (chart != null)
            {
                chart._httpContext = context;
            }
            return chart;
        }

        internal static Chart WriteFromCache(HttpContextBase context, string key, string format = "jpeg")
        {
            var chart = GetFromCache(context, key);
            if (chart != null)
            {
                chart.Write(format);
            }
            return chart;
        }

        // Notes on ApplyXXX methods:
        // Chart elements should be configured before they are added to the chart, otherwise there
        // will be some rendering problems.
        // We must catch all exceptions when configuring chart elements and dispose of them manually
        // if they have not been added to the chart yet, otherwise FxCop will complain.

        private void ApplyChartArea(UI.DataVisualization.Charting.Chart chart)
        {
            ChartArea chartArea = new ChartArea("Default");
            try
            {
                ApplyAxis(chartArea.AxisX, _xAxis);
                ApplyAxis(chartArea.AxisY, _yAxis);
                chart.ChartAreas.Add(chartArea);
            }
            catch
            {
                // This is to appease FxCop
                chartArea.Dispose();
                throw;
            }
        }

        private static void ApplyAxis(Axis axis, ChartAxisData axisData)
        {
            if (axisData == null)
            {
                return;
            }

            if (!String.IsNullOrEmpty(axisData.Title))
            {
                axis.Title = axisData.Title;
            }
            axis.Minimum = axisData.Minimum;
            axis.Maximum = axisData.Maximum;
        }

        private void ApplyLegends(UI.DataVisualization.Charting.Chart chart)
        {
            foreach (var legendData in _legends)
            {
                var legend = new Legend();
                try
                {
                    legend.Name = legendData.Name ?? String.Empty;
                    legend.Title = legendData.Title ?? String.Empty;
                }
                catch (Exception)
                {
                    // see notes above
                    legend.Dispose();
                    throw;
                }
                chart.Legends.Add(legend);
            }
        }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Target = "yValues, yValuesArray",
            Justification = "These names make the most sense.")]
        private void ApplySeries(UI.DataVisualization.Charting.Chart chart)
        {
            foreach (var seriesData in _series)
            {
                var series = new Series();
                try
                {
                    series.AxisLabel = seriesData.AxisLabel ?? String.Empty;
                    series.ChartArea = seriesData.ChartArea ?? String.Empty;
                    series.ChartType = seriesData.ChartType;
                    series.Legend = seriesData.Legend ?? String.Empty;
                    series.MarkerStep = seriesData.MarkerStep;
                    series.Name = seriesData.Name ?? String.Empty;

                    // data-bind the series (todo - support o.Points.DataBind())
                    if (seriesData.DataSource != null)
                    {
                        if (String.IsNullOrEmpty(seriesData.DataSource.YFields))
                        {
                            var yValues = seriesData.DataSource.DataSource;
                            var yValuesArray = yValues as IEnumerable[];
                            if ((yValuesArray != null) && !(yValues is string[]))
                            {
                                series.Points.DataBindXY(seriesData.DataSource.XDataSource, yValuesArray);
                            }
                            else
                            {
                                series.Points.DataBindXY(seriesData.DataSource.XDataSource, yValues);
                            }
                        }
                        else
                        {
                            series.Points.DataBindXY(seriesData.DataSource.XDataSource, seriesData.DataSource.XField,
                                                     seriesData.DataSource.DataSource, seriesData.DataSource.YFields);
                        }
                    }
                }
                catch (Exception)
                {
                    // see notes above
                    series.Dispose();
                    throw;
                }
                chart.Series.Add(series);
            }
        }

        private void ApplyTitles(UI.DataVisualization.Charting.Chart chart)
        {
            foreach (var titleData in _titles)
            {
                var title = new Title();
                try
                {
                    title.Name = titleData.Name;
                    title.Text = titleData.Text;
                }
                catch (Exception)
                {
                    // see notes above
                    title.Dispose();
                    throw;
                }
                chart.Titles.Add(title);
            }
        }

        private static T ConvertStringArgument<T>(string paramName, string value)
        {
            object result;
            if (!ConversionUtil.TryFromString(typeof(T), value, out result))
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture,
                                                          HelpersResources.Chart_ArgumentConversionFailed, typeof(T).FullName), paramName);
            }
            return (T)result;
        }

        /// <summary>
        /// Method to convert a string to a ChartImageFormat.
        /// The chart image needs to be normalized to allow for alternate names such as 'jpg', 'xpng' etc 
        /// to be mapped to their appropriate ChartImageFormat.
        /// </summary>
        private static ChartImageFormat ConvertStringToChartImageFormat(string format)
        {
            object result;
            format = NormalizeFormat(format);
            if (!ConversionUtil.TryFromString(typeof(ChartImageFormat), format, out result))
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture,
                                                          HelpersResources.Image_IncorrectImageFormat, format), "format");
            }
            return (ChartImageFormat)result;
        }

        private void DataBindChart(UI.DataVisualization.Charting.Chart chart)
        {
            // NOTE: WebForms chart will throw null refs if optional values are set to null
            if (_dataSource != null)
            {
                if (!String.IsNullOrEmpty(_dataSource.GroupByField))
                {
                    chart.DataBindCrossTable(
                        _dataSource.DataSource,
                        _dataSource.GroupByField,
                        _dataSource.XField ?? String.Empty,
                        _dataSource.YFields,
                        _dataSource.OtherFields ?? String.Empty,
                        _dataSource.PointSortOrder);
                }
                else if (_dataSource.DataBindTable)
                {
                    chart.DataBindTable(
                        _dataSource.DataSource,
                        _dataSource.XField ?? String.Empty);
                }
                else
                {
                    Debug.Assert(false, "Chart.DataBind was removed - should not reach here");
                    //chart.DataSource = _dataSource.DataSource;
                    //chart.DataBind();
                }
            }
        }

#if CODE_COVERAGE
        [ExcludeFromCodeCoverage]
#endif

        private static HttpContextBase GetDefaultContext()
        {
            return new HttpContextWrapper(HttpContext.Current);
        }

        // review: should GUIDs be used in a handler's querystring?
        private static string GetUniqueKey()
        {
            return Guid.NewGuid().ToString();
        }

        private static string NormalizeFormat(string format)
        {
            if (String.IsNullOrEmpty(format))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "format");
            }
            if (format.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                format = format.Substring(6);
            }
            return ConversionUtil.NormalizeImageFormat(format);
        }

        // data-binding can be done through Chart or individual Series
        private class DataSourceData
        {
            public bool DataBindTable { get; set; }
            public IEnumerable DataSource { get; set; }
            public string GroupByField { get; set; }
            public string OtherFields { get; set; }
            public string XField { get; set; }
            public string YFields { get; set; }
            public PointSortOrder PointSortOrder { get; set; }

            // optional XValue for Series.Points.DataBindXY only:
            public IEnumerable XDataSource { get; set; }
        }

        private class LegendData
        {
            public string Name { get; set; }
            public string Title { get; set; }
        }

        private class SeriesData
        {
            public string AxisLabel { get; set; }
            public string ChartArea { get; set; }
            public SeriesChartType ChartType { get; set; }
            public string Legend { get; set; }
            public int MarkerStep { get; set; }
            public string Name { get; set; }
            public DataSourceData DataSource { get; set; }
        }

        private class TitleData
        {
            public string Name { get; set; }
            public string Text { get; set; }
        }

        private class ChartAxisData
        {
            public double Minimum { get; set; }
            public double Maximum { get; set; }
            public string Title { get; set; }
        }
    }
}
