// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Tracing;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.ModelBinding
{
    /// <summary>
    /// End to end functional tests for tracing.
    /// Verifies ValuesController from standard WebApi project template
    /// behaves the same whether tracing is on or off.  Also verifies
    /// silent and verbose trace writers do not affect the responses.
    /// 
    /// </summary>
    public class TracingTest
    {
        private readonly string _baseAddress = "http://localhost";

        public TracingTest()
        {
        }

        // The test trace writers used when testing that the controller
        // is unaffected by presence or behavior of trace writers
        public static TheoryDataSet<ITestTraceWriter> TestTraceWriters
        {
            get
            {
                return new TheoryDataSet<ITestTraceWriter>
                {
                    // Null means tracing is disabled 
                    null,

                    // This trace writer enables tracing, and the tracer
                    // never calls back any tracing client to ask for more info.
                    new NeverTracesTraceWriter(),

                    // This trace writer enables tracing, and the tracer
                    // always calls back all tracing clients to ask for more info.
                    new MemoryTraceWriter(),
                };
            }
        }

        // These are all the Begin/End traces we expect in a successful ValuesController.Get(id).
        // Tests assert all these are traced (both a Begin and End) and that no others are traced.
        // We verify only the stable parts of the trace record, not the optional messages etc.
        public static List<ExpectedTraceRecord> ExpectedTraceRecords = new List<ExpectedTraceRecord>() {
            new ExpectedTraceRecord("System.Web.Http.Request",      string.Empty,                     string.Empty),
            new ExpectedTraceRecord("System.Web.Http.MessageHandlers",  "DelegatingHandlerProxy",     "SendAsync"),
            new ExpectedTraceRecord("System.Web.Http.Controllers",  "DefaultHttpControllerSelector",  "SelectController"),
            new ExpectedTraceRecord("System.Web.Http.Controllers",  "HttpControllerDescriptor",       "CreateController"),
            new ExpectedTraceRecord("System.Web.Http.Controllers",  "DefaultHttpControllerActivator", "Create"),
            new ExpectedTraceRecord("System.Web.Http.Controllers",  "DefaultHttpControllerTypeResolver", "GetControllerTypes"),
            new ExpectedTraceRecord("System.Web.Http.Controllers",  "ValuesController",               "ExecuteAsync"),
            new ExpectedTraceRecord("System.Web.Http.Action",       "ApiControllerActionSelector",    "SelectAction"),
            new ExpectedTraceRecord("System.Web.Http.Action",       "ApiControllerActionInvoker",     "InvokeActionAsync"),
            new ExpectedTraceRecord("System.Web.Http.Action",       "ReflectedHttpActionDescriptor",  "ExecuteAsync"),            
            new ExpectedTraceRecord("System.Web.Http.ModelBinding", "HttpActionBinding",              "ExecuteBindingAsync"),
            new ExpectedTraceRecord("System.Web.Http.ModelBinding", "ModelBinderParameterBinding",    "ExecuteBindingAsync"),
            new ExpectedTraceRecord("System.Net.Http.Formatting",   "DefaultContentNegotiator",       "Negotiate"),
            new ExpectedTraceRecord("System.Net.Http.Formatting",   "JsonMediaTypeFormatter",         "GetPerRequestFormatterInstance"),
            new ExpectedTraceRecord("System.Net.Http.Formatting",   "JsonMediaTypeFormatter",         "WriteToStreamAsync"),
        };

        // These are all the Begin/End traces we expect in a successful ValuesController.Get(id).
        public static List<ExpectedTraceRecord> ExpectedTraceRecordOrder = new List<ExpectedTraceRecord>() {
            new ExpectedTraceRecord(TraceKind.Begin,     "System.Web.Http.Request",      string.Empty,                     string.Empty),
            new ExpectedTraceRecord(TraceKind.Begin,     "System.Web.Http.MessageHandlers",  "DelegatingHandlerProxy",  "SendAsync"),
            new ExpectedTraceRecord(TraceKind.Begin,     "System.Web.Http.Controllers",  "DefaultHttpControllerSelector",  "SelectController"),
            new ExpectedTraceRecord(TraceKind.Begin,     "System.Web.Http.Controllers",  "DefaultHttpControllerTypeResolver", "GetControllerTypes"),
            new ExpectedTraceRecord(TraceKind.End,       "System.Web.Http.Controllers",  "DefaultHttpControllerTypeResolver", "GetControllerTypes"),
            new ExpectedTraceRecord(TraceKind.End,       "System.Web.Http.Controllers",  "DefaultHttpControllerSelector",  "SelectController"),
            new ExpectedTraceRecord(TraceKind.Begin,     "System.Web.Http.Controllers",  "HttpControllerDescriptor",       "CreateController"),
            new ExpectedTraceRecord(TraceKind.Begin,     "System.Web.Http.Controllers",  "DefaultHttpControllerActivator", "Create"),
            new ExpectedTraceRecord(TraceKind.End,       "System.Web.Http.Controllers",  "DefaultHttpControllerActivator", "Create"),
            new ExpectedTraceRecord(TraceKind.End,       "System.Web.Http.Controllers",  "HttpControllerDescriptor",       "CreateController"),
            new ExpectedTraceRecord(TraceKind.Begin,     "System.Web.Http.Controllers",  "ValuesController",               "ExecuteAsync"),
            new ExpectedTraceRecord(TraceKind.Begin,    "System.Web.Http.Action",       "ApiControllerActionSelector",    "SelectAction"),
            new ExpectedTraceRecord(TraceKind.End,      "System.Web.Http.Action",       "ApiControllerActionSelector",    "SelectAction"),
            new ExpectedTraceRecord(TraceKind.Begin,    "System.Web.Http.ModelBinding", "HttpActionBinding",              "ExecuteBindingAsync"),
            new ExpectedTraceRecord(TraceKind.Begin,    "System.Web.Http.ModelBinding", "ModelBinderParameterBinding",    "ExecuteBindingAsync"),
            new ExpectedTraceRecord(TraceKind.End,      "System.Web.Http.ModelBinding", "ModelBinderParameterBinding",    "ExecuteBindingAsync"),
            new ExpectedTraceRecord(TraceKind.End,      "System.Web.Http.ModelBinding", "HttpActionBinding",              "ExecuteBindingAsync"),
            new ExpectedTraceRecord(TraceKind.Begin,    "System.Web.Http.Action",       "ApiControllerActionInvoker",     "InvokeActionAsync"),
            new ExpectedTraceRecord(TraceKind.Begin,    "System.Web.Http.Action",       "ReflectedHttpActionDescriptor",  "ExecuteAsync"),     
            new ExpectedTraceRecord(TraceKind.End,      "System.Web.Http.Action",       "ReflectedHttpActionDescriptor",  "ExecuteAsync"),
            new ExpectedTraceRecord(TraceKind.Begin,    "System.Net.Http.Formatting",   "DefaultContentNegotiator",       "Negotiate"),
            new ExpectedTraceRecord(TraceKind.Begin,    "System.Net.Http.Formatting",   "JsonMediaTypeFormatter",         "GetPerRequestFormatterInstance"),
            new ExpectedTraceRecord(TraceKind.End,      "System.Net.Http.Formatting",   "JsonMediaTypeFormatter",         "GetPerRequestFormatterInstance"),
            new ExpectedTraceRecord(TraceKind.End,      "System.Net.Http.Formatting",   "DefaultContentNegotiator",       "Negotiate"),
            new ExpectedTraceRecord(TraceKind.End,      "System.Web.Http.Action",       "ApiControllerActionInvoker",     "InvokeActionAsync"),
            new ExpectedTraceRecord(TraceKind.End,      "System.Web.Http.Controllers",  "ValuesController",               "ExecuteAsync"),
            new ExpectedTraceRecord(TraceKind.End,      "System.Web.Http.MessageHandlers",  "DelegatingHandlerProxy",  "SendAsync"),
            new ExpectedTraceRecord(TraceKind.End,      "System.Web.Http.Request",      string.Empty,                     string.Empty),
            new ExpectedTraceRecord(TraceKind.Begin,    "System.Net.Http.Formatting",   "JsonMediaTypeFormatter",         "WriteToStreamAsync"),
            new ExpectedTraceRecord(TraceKind.End,      "System.Net.Http.Formatting",   "JsonMediaTypeFormatter",         "WriteToStreamAsync"),
        };

        [Theory]
        [PropertyData("TestTraceWriters")]
        public void ValuesController_Behavior_Unchanged_By_Tracing(ITestTraceWriter traceWriter)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{id}", new { id = RouteParameter.Optional });

            // The null trace writer case is tested as well to verify the
            // ValuesController works as expected without tracing.
            if (traceWriter != null)
            {
                config.Services.Replace(typeof(ITraceWriter), traceWriter);
                traceWriter.Start();
            }

            ValuesController valuesController = new ValuesController();

            using (HttpServer server = new HttpServer(config))
            {
                using (HttpClient client = new HttpClient(server))
                {
                    if (traceWriter != null)
                    {
                        traceWriter.Start();
                    }

                    // Calls ValuesController.Get()
                    string uri = _baseAddress + "/api/Values";
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
                    HttpResponseMessage response = client.SendAsync(request).Result;
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    string[] expectedGetResponse = valuesController.Get().ToArray();
                    string[] actualGetResponse = response.Content.ReadAsAsync<string[]>().Result;
                    Assert.Equal(expectedGetResponse, actualGetResponse);
                    if (traceWriter != null)
                    {
                        traceWriter.Finish();
                        Assert.True(traceWriter.DidReceiveTraceRequests);
                        traceWriter.Start();
                    }

                    // Calls ValuesController.Get(id) using query string
                    uri = _baseAddress + "/api/Values?id=5";
                    request = new HttpRequestMessage(HttpMethod.Get, uri);
                    response = client.SendAsync(request).Result;
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    string expectedGetQueryStringResponse = valuesController.Get(5);
                    string actualGetQueryStringResponse = response.Content.ReadAsAsync<string>().Result;
                    Assert.Equal(expectedGetQueryStringResponse, actualGetQueryStringResponse);
                    if (traceWriter != null)
                    {
                        traceWriter.Finish();
                        Assert.True(traceWriter.DidReceiveTraceRequests);
                        traceWriter.Start();
                    }

                    // Calls ValuesController.Get(id) using route
                    uri = _baseAddress + "/api/Values/5";
                    request = new HttpRequestMessage(HttpMethod.Get, uri);
                    response = client.SendAsync(request).Result;
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    string expectedGetRouteResponse = valuesController.Get(5);
                    string actualGetRouteResponse = response.Content.ReadAsAsync<string>().Result;
                    Assert.Equal(expectedGetQueryStringResponse, actualGetRouteResponse);
                    if (traceWriter != null)
                    {
                        traceWriter.Finish();
                        Assert.True(traceWriter.DidReceiveTraceRequests);
                        traceWriter.Start();
                    }

                    // Calls ValuesController.Get(id) using query string that causes model binding error
                    uri = _baseAddress + "/api/Values?id=x";
                    request = new HttpRequestMessage(HttpMethod.Get, uri);
                    response = client.SendAsync(request).Result;
                    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                    if (traceWriter != null)
                    {
                        traceWriter.Finish();
                        Assert.True(traceWriter.DidReceiveTraceRequests);
                        traceWriter.Start();
                    }

                    // Calls ValuesController.Post(value) with no parameters
                    uri = _baseAddress + "/api/Values";
                    request = new HttpRequestMessage(HttpMethod.Post, uri);
                    response = client.SendAsync(request).Result;
                    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
                    if (traceWriter != null)
                    {
                        traceWriter.Finish();
                        Assert.True(traceWriter.DidReceiveTraceRequests);
                        traceWriter.Start();
                    }

                    // Calls ValuesController.Post(value) using query string
                    uri = _baseAddress + "/api/Values?value=hello";
                    request = new HttpRequestMessage(HttpMethod.Post, uri);
                    response = client.SendAsync(request).Result;
                    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
                    if (traceWriter != null)
                    {
                        traceWriter.Finish();
                        Assert.True(traceWriter.DidReceiveTraceRequests);
                        traceWriter.Start();
                    }

                    // Calls ValuesController.Put(id, value) using query strings
                    uri = _baseAddress + "/api/Values?id=5&value=hello";
                    request = new HttpRequestMessage(HttpMethod.Put, uri);
                    response = client.SendAsync(request).Result;
                    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
                    if (traceWriter != null)
                    {
                        traceWriter.Finish();
                        Assert.True(traceWriter.DidReceiveTraceRequests);
                        traceWriter.Start();
                    }

                    // Calls ValuesController.Put(id, value) using route + query string
                    uri = _baseAddress + "/api/Values/5?value=hello";
                    request = new HttpRequestMessage(HttpMethod.Put, uri);
                    response = client.SendAsync(request).Result;
                    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
                    if (traceWriter != null)
                    {
                        traceWriter.Finish();
                        Assert.True(traceWriter.DidReceiveTraceRequests);
                        traceWriter.Start();
                    }

                    // Calls ValuesController.Delete(id) using query string
                    uri = _baseAddress + "/api/Values?id=5";
                    request = new HttpRequestMessage(HttpMethod.Delete, uri);
                    response = client.SendAsync(request).Result;
                    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
                    if (traceWriter != null)
                    {
                        traceWriter.Finish();
                        Assert.True(traceWriter.DidReceiveTraceRequests);
                        traceWriter.Start();
                    }

                    // Calls ValuesController.Delete(id) using route
                    uri = _baseAddress + "/api/Values/5";
                    request = new HttpRequestMessage(HttpMethod.Delete, uri);
                    response = client.SendAsync(request).Result;
                    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
                    if (traceWriter != null)
                    {
                        traceWriter.Finish();
                        Assert.True(traceWriter.DidReceiveTraceRequests);
                    }
                }
            }
        }

        [Fact]
        public void ValuesController_Get_Id_Writes_Expected_Traces()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{id}", new { id = RouteParameter.Optional });
            Mock<DelegatingHandler> customMessageHandler = new Mock<DelegatingHandler>() { CallBase = true };
            config.MessageHandlers.Add(customMessageHandler.Object);
            MemoryTraceWriter traceWriter = new MemoryTraceWriter();
            config.Services.Replace(typeof(ITraceWriter), traceWriter);

            using (HttpServer server = new HttpServer(config))
            {
                using (HttpClient client = new HttpClient(server))
                {
                    traceWriter.Start();

                    // Calls ValueController.Get(id) using query string
                    string uri = _baseAddress + "/api/Values?id=5";
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
                    HttpResponseMessage response = client.SendAsync(request).Result;
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    traceWriter.Finish();

                    IList<string> missingTraces = MissingTraces(ExpectedTraceRecords, traceWriter.Records);
                    Assert.True(missingTraces.Count == 0,
                                string.Format("These expected traces were missing:{0}    {1}",
                                                Environment.NewLine, string.Join(Environment.NewLine + "    ", missingTraces)));

                    IList<string> unexpectedTraces = UnexpectedTraces(ExpectedTraceRecords, traceWriter.Records);
                    Assert.True(unexpectedTraces.Count == 0,
                                string.Format("These traces were not expected:{0}    {1}",
                                                Environment.NewLine, string.Join(Environment.NewLine + "    ", unexpectedTraces)));
                }
            }
        }

        [Fact]
        public void ValuesController_Get_Id_Writes_Expected_Traces_InTheCorrectOrder()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{id}", new { id = RouteParameter.Optional });
            Mock<DelegatingHandler> customMessageHandler = new Mock<DelegatingHandler>() { CallBase = true };
            config.MessageHandlers.Add(customMessageHandler.Object);
            MemoryTraceWriter traceWriter = new MemoryTraceWriter();
            config.Services.Replace(typeof(ITraceWriter), traceWriter);

            using (HttpServer server = new HttpServer(config))
            {
                using (HttpClient client = new HttpClient(server))
                {
                    traceWriter.Start();

                    // Calls ValueController.Get(id) using query string
                    string uri = _baseAddress + "/api/Values?id=5";
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
                    HttpResponseMessage response = client.SendAsync(request).Result;
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    traceWriter.Finish();

                    Assert.True(ConfirmTracingOrder(ExpectedTraceRecordOrder, traceWriter.Records));
                }
            }
        }
        // Returns a list of strings describing all of the expected trace records that were not
        // actually traced.
        // If you experience test failures from this list, it means someone stopped tracing or 
        // changed the content of what was traced.  
        // Update the ExpectedTraceRecords property to reflect what is expected.
        private static IList<string> MissingTraces(IList<ExpectedTraceRecord> expectedRecords, IList<TraceRecord> actualRecords)
        {
            List<string> missing = new List<string>();

            foreach (ExpectedTraceRecord expectedRecord in expectedRecords)
            {
                TraceRecord beginTrace = actualRecords.SingleOrDefault(r =>
                    String.Equals(r.Category, expectedRecord.Category, StringComparison.OrdinalIgnoreCase) &&
                    String.Equals(r.Operator, expectedRecord.OperatorName, StringComparison.OrdinalIgnoreCase) &&
                    String.Equals(r.Operation, expectedRecord.OperationName, StringComparison.OrdinalIgnoreCase) &&
                    r.Kind == TraceKind.Begin
                    );

                if (beginTrace == null)
                {
                    missing.Add(string.Format("Begin category={0}, operator={1}, operation={2}",
                                    expectedRecord.Category, expectedRecord.OperatorName, expectedRecord.OperationName));
                }

                TraceRecord endTrace = actualRecords.SingleOrDefault(r =>
                    String.Equals(r.Category, expectedRecord.Category, StringComparison.OrdinalIgnoreCase) &&
                    String.Equals(r.Operator, expectedRecord.OperatorName, StringComparison.OrdinalIgnoreCase) &&
                    String.Equals(r.Operation, expectedRecord.OperationName, StringComparison.OrdinalIgnoreCase) &&
                    r.Kind == TraceKind.End
                    );

                if (endTrace == null)
                {
                    missing.Add(string.Format("End category={0}, operator={1}, operation={2}",
                                    expectedRecord.Category, expectedRecord.OperatorName, expectedRecord.OperationName));
                }
            }

            return missing;
        }

        // Returns a list of strings of trace records we did not expect.
        // If you experience failures from this list, it means someone added new traces
        // or changed the contents of the traces.  
        // Update the ExpectedTraceRecords property to reflect what is expected.
        private static IList<string> UnexpectedTraces(IList<ExpectedTraceRecord> expectedRecords, IList<TraceRecord> actualRecords)
        {
            List<string> unexpected = new List<string>();

            foreach (TraceRecord actualRecord in actualRecords)
            {
                ExpectedTraceRecord expectedTrace = expectedRecords.FirstOrDefault(r =>
                    String.Equals(r.Category, actualRecord.Category, StringComparison.OrdinalIgnoreCase) &&
                    String.Equals(r.OperatorName, actualRecord.Operator, StringComparison.OrdinalIgnoreCase) &&
                    String.Equals(r.OperationName, actualRecord.Operation, StringComparison.OrdinalIgnoreCase));

                if (expectedTrace == null)
                {
                    unexpected.Add(string.Format("kind={0} category={1}, operator={2}, operation={3}",
                                    actualRecord.Kind, actualRecord.Category, actualRecord.Operator, actualRecord.Operation));
                }
            }

            return unexpected;
        }

        // Returns true if the tracing records are in the correct order, else returns false.
        private static bool ConfirmTracingOrder(IList<ExpectedTraceRecord> expectedRecords, IList<TraceRecord> actualRecords)
        {
            int traceBeginPos = 0;
            foreach (ExpectedTraceRecord expectedRecord in expectedRecords)
            {
                TraceRecord beginTrace = actualRecords.SingleOrDefault(r =>
                    String.Equals(r.Category, expectedRecord.Category, StringComparison.OrdinalIgnoreCase) &&
                    String.Equals(r.Operator, expectedRecord.OperatorName, StringComparison.OrdinalIgnoreCase) &&
                    String.Equals(r.Operation, expectedRecord.OperationName, StringComparison.OrdinalIgnoreCase) &&
                    object.Equals(r.Kind, expectedRecord.TraceKind) 
                    );

                if (!object.ReferenceEquals(beginTrace, actualRecords.ElementAt(traceBeginPos)))
                {
                    return false;
                }
                traceBeginPos++;
            }
            return true;
        }
    }

    public class ExpectedTraceRecord
    {
        public ExpectedTraceRecord(string category, string operatorName, string operationName)
        {
            Category = category;
            OperatorName = operatorName;
            OperationName = operationName;
        }

        public ExpectedTraceRecord(TraceKind kind, string category, string operatorName, string operationName)
        {
            TraceKind = kind;
            Category = category;
            OperatorName = operatorName;
            OperationName = operationName;
        }

        public string Category { get; private set; }
        public TraceKind TraceKind { get; private set; }
        public string OperatorName { get; private set; }
        public string OperationName { get; private set; }
    }
}