// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.TestCommon;
using ROOT_PROJECT_NAMESPACE.Areas.HelpPage;
using WebApiHelpPageWebHost.UnitTest.Controllers;

namespace WebApiHelpPageWebHost.UnitTest
{
    public class XmlDocumentationProviderTest
    {
        [Fact]
        public void Constructor_ThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new XmlDocumentationProvider(null));
        }

        public static IEnumerable<object[]> GetDocumentationForAction_PropertyData
        {
            get
            {
                ValuesController controller = new ValuesController();

                Func<IEnumerable<string>> getAction = controller.Get;
                HttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(new HttpControllerDescriptor(), getAction.Method);
                yield return new object[] { actionDescriptor, "Gets all the values." };

                Func<int, string> getIdAction = controller.Get;
                actionDescriptor = new ReflectedHttpActionDescriptor(new HttpControllerDescriptor(), getIdAction.Method);
                yield return new object[] { actionDescriptor, "Gets the value by id." };

                Func<string, string> getNameAction = controller.Get;
                actionDescriptor = new ReflectedHttpActionDescriptor(new HttpControllerDescriptor(), getNameAction.Method);
                yield return new object[] { actionDescriptor, "Gets the value by name." };

                Func<string, HttpResponseMessage> postAction = controller.Post;
                actionDescriptor = new ReflectedHttpActionDescriptor(new HttpControllerDescriptor(), postAction.Method);
                yield return new object[] { actionDescriptor, "Create a new value." };

                Action<int, string> putAction = controller.Put;
                actionDescriptor = new ReflectedHttpActionDescriptor(new HttpControllerDescriptor(), putAction.Method);
                yield return new object[] { actionDescriptor, "Updates the value." };

                Action<List<Tuple<int, string>>> putCollectionAction = controller.Put;
                actionDescriptor = new ReflectedHttpActionDescriptor(new HttpControllerDescriptor(), putCollectionAction.Method);
                yield return new object[] { actionDescriptor, "Updates the value pair collection." };

                Action<int?> deleteAction = controller.Delete;
                actionDescriptor = new ReflectedHttpActionDescriptor(new HttpControllerDescriptor(), deleteAction.Method);
                yield return new object[] { actionDescriptor, "Deletes the value." };

                Action<Tuple<int, string>> patchAction = controller.Patch;
                actionDescriptor = new ReflectedHttpActionDescriptor(new HttpControllerDescriptor(), patchAction.Method);
                yield return new object[] { actionDescriptor, "Patches the value pair." };

                Func<HttpRequestMessage, string> optionsAction = controller.Options;
                actionDescriptor = new ReflectedHttpActionDescriptor(new HttpControllerDescriptor(), optionsAction.Method);
                yield return new object[] { actionDescriptor, "Returns the options." };

                Func<int, string[]> headAction = controller.HeadNoDocumentation;
                actionDescriptor = new ReflectedHttpActionDescriptor(new HttpControllerDescriptor(), headAction.Method);
                yield return new object[] { actionDescriptor, null };
            }
        }

        [Theory]
        [PropertyData("GetDocumentationForAction_PropertyData")]
        public void GetDocumentationForAction(HttpActionDescriptor actionDescriptor, string expectedString)
        {
            XmlDocumentationProvider provider = new XmlDocumentationProvider("WebApiHelpPage.Test.XML");
            string documentationString = provider.GetDocumentation(actionDescriptor);
            Assert.Equal(expectedString, documentationString);
        }

        public static IEnumerable<object[]> GetDocumentationForParameters_PropertyData
        {
            get
            {
                HttpActionDescriptor[] actionDescriptors = GetDocumentationForAction_PropertyData.Select(a => (HttpActionDescriptor)a[0]).ToArray();
                Dictionary<string, string>[] expectedResults = 
                {
                    new Dictionary<string,string>(),
                    new Dictionary<string, string>{{"id","The id."}},
                    new Dictionary<string, string>{{"name","The name."}},
                    new Dictionary<string, string>{{"value","The value."}},
                    new Dictionary<string, string>{{"id","The id."}, {"value","The value."}},
                    new Dictionary<string, string>{{"valuePairCollection","The value pair collection."}},
                    new Dictionary<string, string>{{"id","The id."}},
                    new Dictionary<string, string>{{"valuePair","The pair."}},
                    new Dictionary<string, string>{{"request","The request."}},
                    new Dictionary<string, string>{{"id", null}},
                };

                Assert.Equal(expectedResults.Length, actionDescriptors.Length);

                for (int i = 0; i < actionDescriptors.Length; i++)
                {
                    var actionDescriptor = actionDescriptors[i];
                    foreach (var parameterDescriptor in actionDescriptor.GetParameters())
                    {
                        yield return new object[] { parameterDescriptor, expectedResults[i][parameterDescriptor.ParameterName] };
                    }
                }
            }
        }

        [Theory]
        [PropertyData("GetDocumentationForParameters_PropertyData")]
        public void GetDocumentationForParameters(HttpParameterDescriptor parameterDescriptor, string expectedString)
        {
            XmlDocumentationProvider provider = new XmlDocumentationProvider("WebApiHelpPage.Test.XML");
            string documentationString = provider.GetDocumentation(parameterDescriptor);
            Assert.Equal(expectedString, documentationString);
        }

        public static IEnumerable<object[]> GetDocumentationForController_PropertyData
        {
            get
            {
                HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(new HttpConfiguration(), "Values", typeof(ValuesController));
                yield return new object[] { controllerDescriptor, "Resource for Values." };

                controllerDescriptor = new HttpControllerDescriptor(new HttpConfiguration(), "NestedValues", typeof(ValuesController.NestedValuesController));
                yield return new object[] { controllerDescriptor, "Resource for nested values." };
            }
        }

        [Theory]
        [PropertyData("GetDocumentationForController_PropertyData")]
        public void GetDocumentationForController(HttpControllerDescriptor controllerDescriptor, string expectedString)
        {
            XmlDocumentationProvider provider = new XmlDocumentationProvider("WebApiHelpPage.Test.XML");
            string documentationString = provider.GetDocumentation(controllerDescriptor);
            Assert.Equal(expectedString, documentationString);
        }

        public static IEnumerable<object[]> GetDocumentationForActionResponse_PropertyData
        {
            get
            {
                ValuesController controller = new ValuesController();

                Func<IEnumerable<string>> getAction = controller.Get;
                HttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(new HttpControllerDescriptor(), getAction.Method);
                yield return new object[] { actionDescriptor, "A list of values." };

                Func<int, string> getIdAction = controller.Get;
                actionDescriptor = new ReflectedHttpActionDescriptor(new HttpControllerDescriptor(), getIdAction.Method);
                yield return new object[] { actionDescriptor, "A value string." };

                Func<string, string> getNameAction = controller.Get;
                actionDescriptor = new ReflectedHttpActionDescriptor(new HttpControllerDescriptor(), getNameAction.Method);
                yield return new object[] { actionDescriptor, "A value identified by name." };

                Func<string, HttpResponseMessage> postAction = controller.Post;
                actionDescriptor = new ReflectedHttpActionDescriptor(new HttpControllerDescriptor(), postAction.Method);
                yield return new object[] { actionDescriptor, "A response." };

                Action<int, string> putAction = controller.Put;
                actionDescriptor = new ReflectedHttpActionDescriptor(new HttpControllerDescriptor(), putAction.Method);
                yield return new object[] { actionDescriptor, null };

                Action<List<Tuple<int, string>>> putCollectionAction = controller.Put;
                actionDescriptor = new ReflectedHttpActionDescriptor(new HttpControllerDescriptor(), putCollectionAction.Method);
                yield return new object[] { actionDescriptor, null };

                Action<int?> deleteAction = controller.Delete;
                actionDescriptor = new ReflectedHttpActionDescriptor(new HttpControllerDescriptor(), deleteAction.Method);
                yield return new object[] { actionDescriptor, null };

                Action<Tuple<int, string>> patchAction = controller.Patch;
                actionDescriptor = new ReflectedHttpActionDescriptor(new HttpControllerDescriptor(), patchAction.Method);
                yield return new object[] { actionDescriptor, null };

                Func<HttpRequestMessage, string> optionsAction = controller.Options;
                actionDescriptor = new ReflectedHttpActionDescriptor(new HttpControllerDescriptor(), optionsAction.Method);
                yield return new object[] { actionDescriptor, "All the options." };

                Func<int, string[]> headAction = controller.HeadNoDocumentation;
                actionDescriptor = new ReflectedHttpActionDescriptor(new HttpControllerDescriptor(), headAction.Method);
                yield return new object[] { actionDescriptor, null };
            }
        }

        [Theory]
        [PropertyData("GetDocumentationForActionResponse_PropertyData")]
        public void GetDocumentationForActionResponse(HttpActionDescriptor actionDescriptor, string expectedString)
        {
            XmlDocumentationProvider provider = new XmlDocumentationProvider("WebApiHelpPage.Test.XML");
            string documentationString = provider.GetResponseDocumentation(actionDescriptor);
            Assert.Equal(expectedString, documentationString);
        }
    }
}
