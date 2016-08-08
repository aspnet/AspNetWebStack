// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebApiHelpPageWebHost.UnitTest.Controllers
{
    /// <summary>
    /// Resource for Values.
    /// </summary>
    public class ValuesController : ApiController
    {
        /// <summary>
        /// Gets all the values.
        /// </summary>
        /// <returns>A list of values.</returns>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        /// <summary>
        /// Gets the value by id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>A value string.</returns>
        public string Get(int id)
        {
            return "value";
        }

        /// <summary>
        /// Gets the value by name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A value identified by name.</returns>
        public string Get(string name)
        {
            return "name";
        }

        /// <summary>
        /// Gets the value by the point.
        /// This is a test for a type with TypeConverter.
        /// </summary>
        /// <param name="point">The type defined with TypeConverter.</param>
        /// <returns>A point string.</returns>
        public string Get([FromUri]Point point)
        {
            return "point";
        }

        /// <summary>
        /// Create a new value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A response.</returns>
        public HttpResponseMessage Post([FromBody]string value)
        {
            return Request.CreateResponse<string>(HttpStatusCode.OK, "hello");
        }

        /// <summary>
        /// Updates the value.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="value">The value.</param>
        public void Put(int id, [FromBody]string value)
        {
        }

        /// <summary>
        /// Updates the value pair collection.
        /// </summary>
        /// <param name="valuePairCollection">The value pair collection.</param>
        public void Put(List<Tuple<int, string>> valuePairCollection)
        {
        }

        /// <summary>
        /// Deletes the value.
        /// </summary>
        /// <param name="id">The id.</param>
        public void Delete(int? id)
        {
        }

        /// <summary>
        /// Patches the value pair.
        /// </summary>
        /// <param name="valuePair">The pair.</param>
        public void Patch(Tuple<int, string> valuePair)
        {
        }

        /// <summary>
        /// Returns the options.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>All the options.</returns>
        public string Options(HttpRequestMessage request)
        {
            return "options";
        }

        public string[] HeadNoDocumentation(int id)
        {
            return new string[] { "value1", "value2" };
        }

        /// <summary>
        /// Resource for nested values.
        /// </summary>
        public class NestedValuesController : ApiController
        {
            public string[] Get()
            {
                return new string[] { "nvalue1", "nvalue2" };
            }
        }

        public class PointConverter : TypeConverter
        {
            // Overrides the CanConvertFrom method of TypeConverter.
            // The ITypeDescriptorContext interface provides the context for the
            // conversion. Typically, this interface is used at design time to 
            // provide information about the design-time container.
            public override bool CanConvertFrom(ITypeDescriptorContext context,
               Type sourceType)
            {
                if (sourceType == typeof(string))
                {
                    return true;
                }
                return base.CanConvertFrom(context, sourceType);
            }
            // Overrides the ConvertFrom method of TypeConverter.
            public override object ConvertFrom(ITypeDescriptorContext context,
               CultureInfo culture, object value)
            {
                if (value is string)
                {
                    string[] v = ((string)value).Split(new char[] { ',' });
                    return new Point(int.Parse(v[0]), int.Parse(v[1]));
                }
                return base.ConvertFrom(context, culture, value);
            }
            // Overrides the ConvertTo method of TypeConverter.
            public override object ConvertTo(ITypeDescriptorContext context,
               CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == typeof(string))
                {
                    return ((Point)value).X + "," + ((Point)value).Y;
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }

        [TypeConverter(typeof(PointConverter))]
        public class Point
        {
            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }

            public int X { get; set; }

            public int Y { get; set; }
        }
    }
}
