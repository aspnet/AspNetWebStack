// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Microsoft.TestCommon;

namespace Microsoft.Web.Mvc.ModelBinding.Test
{
    public class ModelValidationNodeTest
    {
        [Fact]
        public void ConstructorSetsCollectionInstance()
        {
            // Arrange
            ModelMetadata metadata = GetModelMetadata();
            string modelStateKey = "someKey";
            ModelValidationNode[] childNodes = new[]
            {
                new ModelValidationNode(metadata, "someKey0"),
                new ModelValidationNode(metadata, "someKey1")
            };

            // Act
            ModelValidationNode node = new ModelValidationNode(metadata, modelStateKey, childNodes);

            // Assert
            Assert.Equal(childNodes, node.ChildNodes.ToArray());
        }

        [Fact]
        public void ConstructorThrowsIfModelMetadataIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { new ModelValidationNode(null, "someKey"); }, "modelMetadata");
        }

        [Fact]
        public void ConstructorThrowsIfModelStateKeyIsNull()
        {
            // Arrange
            ModelMetadata metadata = GetModelMetadata();

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { new ModelValidationNode(metadata, null); }, "modelStateKey");
        }

        [Fact]
        public void PropertiesAreSet()
        {
            // Arrange
            ModelMetadata metadata = GetModelMetadata();
            string modelStateKey = "someKey";

            // Act
            ModelValidationNode node = new ModelValidationNode(metadata, modelStateKey);

            // Assert
            Assert.Equal(metadata, node.ModelMetadata);
            Assert.Equal(modelStateKey, node.ModelStateKey);
            Assert.NotNull(node.ChildNodes);
            Assert.Empty(node.ChildNodes);
        }

        [Fact]
        public void CombineWith()
        {
            // Arrange
            List<string> log = new List<string>();

            ModelValidationNode[] allChildNodes = new[]
            {
                new ModelValidationNode(GetModelMetadata(), "key1"),
                new ModelValidationNode(GetModelMetadata(), "key2"),
                new ModelValidationNode(GetModelMetadata(), "key3"),
            };

            ModelValidationNode parentNode1 = new ModelValidationNode(GetModelMetadata(), "parent1");
            parentNode1.ChildNodes.Add(allChildNodes[0]);
            parentNode1.Validating += delegate { log.Add("Validating parent1."); };
            parentNode1.Validated += delegate { log.Add("Validated parent1."); };

            ModelValidationNode parentNode2 = new ModelValidationNode(GetModelMetadata(), "parent2");
            parentNode2.ChildNodes.Add(allChildNodes[1]);
            parentNode2.ChildNodes.Add(allChildNodes[2]);
            parentNode2.Validating += delegate { log.Add("Validating parent2."); };
            parentNode2.Validated += delegate { log.Add("Validated parent2."); };

            // Act
            parentNode1.CombineWith(parentNode2);
            parentNode1.Validate(new ControllerContext { Controller = new EmptyController() });

            // Assert
            Assert.Equal(new[] { "Validating parent1.", "Validating parent2.", "Validated parent1.", "Validated parent2." }, log.ToArray());
            Assert.Equal(allChildNodes, parentNode1.ChildNodes.ToArray());
        }

        [Fact]
        public void CombineWith_OtherNodeIsSuppressed_DoesNothing()
        {
            // Arrange
            List<string> log = new List<string>();

            ModelValidationNode[] allChildNodes = new[]
            {
                new ModelValidationNode(GetModelMetadata(), "key1"),
                new ModelValidationNode(GetModelMetadata(), "key2"),
                new ModelValidationNode(GetModelMetadata(), "key3"),
            };

            ModelValidationNode[] expectedChildNodes = new[]
            {
                allChildNodes[0]
            };

            ModelValidationNode parentNode1 = new ModelValidationNode(GetModelMetadata(), "parent1");
            parentNode1.ChildNodes.Add(allChildNodes[0]);
            parentNode1.Validating += delegate { log.Add("Validating parent1."); };
            parentNode1.Validated += delegate { log.Add("Validated parent1."); };

            ModelValidationNode parentNode2 = new ModelValidationNode(GetModelMetadata(), "parent2");
            parentNode2.ChildNodes.Add(allChildNodes[1]);
            parentNode2.ChildNodes.Add(allChildNodes[2]);
            parentNode2.Validating += delegate { log.Add("Validating parent2."); };
            parentNode2.Validated += delegate { log.Add("Validated parent2."); };
            parentNode2.SuppressValidation = true;

            // Act
            parentNode1.CombineWith(parentNode2);
            parentNode1.Validate(new ControllerContext { Controller = new EmptyController() });

            // Assert
            Assert.Equal(new[] { "Validating parent1.", "Validated parent1." }, log.ToArray());
            Assert.Equal(expectedChildNodes, parentNode1.ChildNodes.ToArray());
        }

        [Fact]
        public void Validate_Ordering()
        {
            // Proper order of invocation:
            // 1. OnValidating()
            // 2. Child validators
            // 3. This validator
            // 4. OnValidated()

            // Arrange
            List<string> log = new List<string>();
            LoggingDataErrorInfoModel model = new LoggingDataErrorInfoModel(log);
            ModelMetadata modelMetadata = GetModelMetadata(model);

            ControllerContext controllerContext = new ControllerContext
            {
                Controller = new EmptyController()
            };
            ModelValidationNode node = new ModelValidationNode(modelMetadata, "theKey");

            ModelMetadata childMetadata = new EmptyModelMetadataProvider().GetMetadataForProperty(() => model, model.GetType(), "ValidStringProperty");
            node.ChildNodes.Add(new ModelValidationNode(childMetadata, "theKey.ValidStringProperty"));

            node.Validating += delegate { log.Add("In OnValidating()"); };
            node.Validated += delegate { log.Add("In OnValidated()"); };

            // Act
            node.Validate(controllerContext);

            // Assert
            Assert.Equal(new[] { "In OnValidating()", "In IDataErrorInfo.get_Item('ValidStringProperty')", "In IDataErrorInfo.get_Error()", "In OnValidated()" }, log.ToArray());
        }

        [Fact]
        public void Validate_PassesNullContainerInstanceIfCannotBeConvertedToProperType()
        {
            // Arrange
            List<string> log1 = new List<string>();
            LoggingDataErrorInfoModel model1 = new LoggingDataErrorInfoModel(log1);
            ModelMetadata modelMetadata1 = GetModelMetadata(model1);

            List<string> log2 = new List<string>();
            LoggingDataErrorInfoModel model2 = new LoggingDataErrorInfoModel(log2);
            ModelMetadata modelMetadata2 = GetModelMetadata(model2);

            ControllerContext controllerContext = new ControllerContext
            {
                Controller = new EmptyController()
            };
            ModelValidationNode node = new ModelValidationNode(modelMetadata1, "theKey");
            node.ChildNodes.Add(new ModelValidationNode(modelMetadata2, "theKey.SomeProperty"));

            // Act
            node.Validate(controllerContext);

            // Assert
            Assert.Equal(new[] { "In IDataErrorInfo.get_Error()" }, log1.ToArray());
            Assert.Equal(new[] { "In IDataErrorInfo.get_Error()" }, log2.ToArray());
        }

        [Fact]
        public void Validate_SkipsRemainingValidationIfModelStateIsInvalid()
        {
            // Because a property validator fails, the model validator shouldn't run

            // Arrange
            List<string> log = new List<string>();
            LoggingDataErrorInfoModel model = new LoggingDataErrorInfoModel(log);
            ModelMetadata modelMetadata = GetModelMetadata(model);

            ControllerContext controllerContext = new ControllerContext
            {
                Controller = new EmptyController()
            };
            ModelValidationNode node = new ModelValidationNode(modelMetadata, "theKey");

            ModelMetadata childMetadata = new EmptyModelMetadataProvider().GetMetadataForProperty(() => model, model.GetType(), "InvalidStringProperty");
            node.ChildNodes.Add(new ModelValidationNode(childMetadata, "theKey.InvalidStringProperty"));

            node.Validating += delegate { log.Add("In OnValidating()"); };
            node.Validated += delegate { log.Add("In OnValidated()"); };

            // Act
            node.Validate(controllerContext);

            // Assert
            Assert.Equal(new[] { "In OnValidating()", "In IDataErrorInfo.get_Item('InvalidStringProperty')", "In OnValidated()" }, log.ToArray());
            Assert.Equal("Sample error message", controllerContext.Controller.ViewData.ModelState["theKey.InvalidStringProperty"].Errors[0].ErrorMessage);
        }

        [Fact]
        public void Validate_SkipsValidationIfHandlerCancels()
        {
            // Arrange
            List<string> log = new List<string>();
            LoggingDataErrorInfoModel model = new LoggingDataErrorInfoModel(log);
            ModelMetadata modelMetadata = GetModelMetadata(model);

            ControllerContext controllerContext = new ControllerContext
            {
                Controller = new EmptyController()
            };
            ModelValidationNode node = new ModelValidationNode(modelMetadata, "theKey");

            node.Validating += (sender, e) =>
            {
                log.Add("In OnValidating()");
                e.Cancel = true;
            };
            node.Validated += delegate { log.Add("In OnValidated()"); };

            // Act
            node.Validate(controllerContext);

            // Assert
            Assert.Equal(new[] { "In OnValidating()" }, log.ToArray());
        }

        [Fact]
        public void Validate_SkipsValidationIfSuppressed()
        {
            // Arrange
            List<string> log = new List<string>();
            LoggingDataErrorInfoModel model = new LoggingDataErrorInfoModel(log);
            ModelMetadata modelMetadata = GetModelMetadata(model);

            ControllerContext controllerContext = new ControllerContext
            {
                Controller = new EmptyController()
            };
            ModelValidationNode node = new ModelValidationNode(modelMetadata, "theKey")
            {
                SuppressValidation = true
            };

            node.Validating += (sender, e) => { log.Add("In OnValidating()"); };
            node.Validated += delegate { log.Add("In OnValidated()"); };

            // Act
            node.Validate(controllerContext);

            // Assert
            Assert.Empty(log);
        }

        [Fact]
        public void Validate_ThrowsIfControllerContextIsNull()
        {
            // Arrange
            ModelValidationNode node = new ModelValidationNode(GetModelMetadata(), "someKey");

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { node.Validate(null); }, "controllerContext");
        }

        [Fact]
        [ReplaceCulture]
        public void Validate_ValidateAllProperties_AddsValidationErrors()
        {
            // Arrange
            ValidateAllPropertiesModel model = new ValidateAllPropertiesModel
            {
                RequiredString = null /* error */,
                RangedInt = 0 /* error */,
                ValidString = "dog"
            };

            ModelMetadata modelMetadata = GetModelMetadata(model);
            ControllerContext controllerContext = new ControllerContext
            {
                Controller = new EmptyController()
            };
            ModelValidationNode node = new ModelValidationNode(modelMetadata, "theKey")
            {
                ValidateAllProperties = true
            };

            controllerContext.Controller.ViewData.ModelState.AddModelError("theKey.RequiredString.Dummy", "existing Error Text");

            // Act
            node.Validate(controllerContext);

            // Assert
            Assert.Null(controllerContext.Controller.ViewData.ModelState["theKey.RequiredString"]);
            Assert.Equal("existing Error Text", controllerContext.Controller.ViewData.ModelState["theKey.RequiredString.Dummy"].Errors[0].ErrorMessage);
            Assert.Equal("The field RangedInt must be between 10 and 30.", controllerContext.Controller.ViewData.ModelState["theKey.RangedInt"].Errors[0].ErrorMessage);
            Assert.Null(controllerContext.Controller.ViewData.ModelState["theKey.ValidString"]);
            Assert.Null(controllerContext.Controller.ViewData.ModelState["theKey"]);
        }

        private static ModelMetadata GetModelMetadata()
        {
            EmptyModelMetadataProvider provider = new EmptyModelMetadataProvider();
            return provider.GetMetadataForType(null, typeof(object));
        }

        private static ModelMetadata GetModelMetadata(object o)
        {
            DataAnnotationsModelMetadataProvider provider = new DataAnnotationsModelMetadataProvider();
            return provider.GetMetadataForType(() => o, o.GetType());
        }

        private sealed class EmptyController : Controller
        {
        }

        private sealed class LoggingDataErrorInfoModel : IDataErrorInfo
        {
            private readonly IList<string> _log;

            public LoggingDataErrorInfoModel(IList<string> log)
            {
                _log = log;
            }

            string IDataErrorInfo.Error
            {
                get
                {
                    _log.Add("In IDataErrorInfo.get_Error()");
                    return null;
                }
            }

            string IDataErrorInfo.this[string columnName]
            {
                get
                {
                    _log.Add("In IDataErrorInfo.get_Item('" + columnName + "')");
                    return (columnName == "ValidStringProperty") ? null : "Sample error message";
                }
            }

            public string ValidStringProperty { get; set; }
            public string InvalidStringProperty { get; set; }
        }

        private class ValidateAllPropertiesModel
        {
            [Required]
            public string RequiredString { get; set; }

            [Range(10, 30)]
            public int RangedInt { get; set; }

            [RegularExpression("dog")]
            public string ValidString { get; set; }
        }
    }
}
