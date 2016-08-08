// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Moq;

namespace System.Web.Mvc.Html.Test
{
    /// <summary>
    /// <para>
    /// A scope within which the <see cref="ModelMetadata"/> for a single <see cref="Type"/> is overridden.  Could be
    /// used for example to ensure the metadata for <see cref="Exception"/> includes an additional property or
    /// has a non-<c>null</c> <see cref="ModelMetadata.DisplayName"/>.
    /// </para>
    /// <para>
    /// Notes: Does _not_ override the metadata for subclasses of the given <see cref="Type"/>.  And callers should
    /// override (likely, mock) the metadata of the containing <see cref="Type"/> when changing the metadata of a
    /// property e.g. modifying <see cref="ModelMetadata.IsRequired"/>.
    /// </para>
    /// </summary>
    public class MetadataOverrideScope : IDisposable
    {
        private static readonly DataAnnotationsModelMetadataProvider AnnotationsProvider =
            new DataAnnotationsModelMetadataProvider();

        private readonly ModelMetadataProvider _oldMetadataProvider;
        private readonly ModelMetadata _metadata;
        private readonly Type _modelType;

        public MetadataOverrideScope(ModelMetadata metadata)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException("metadata");
            }
            if (metadata.ModelType == null)
            {
                throw new ArgumentException("Need ModelType", "metadata");
            }

            _oldMetadataProvider = ModelMetadataProviders.Current;
            _metadata = metadata;
            _modelType = metadata.ModelType;

            // Mock a ModelMetadataProvider which delegates to the old one in most cases.  No need to special-case
            // GetMetadataForProperties() because product code uses it only within ModelMetadata.Properties and our
            // metadata instance will call _oldMetadataProvider there.
            var metadataProvider = new Mock<ModelMetadataProvider>();
            metadataProvider
                .Setup(p => p.GetMetadataForProperties(It.IsAny<object>(), It.IsAny<Type>()))
                .Returns((object container, Type containerType) =>
                    _oldMetadataProvider.GetMetadataForProperties(container, containerType));
            metadataProvider
                .Setup(p => p.GetMetadataForType(It.IsAny<Func<object>>(), It.IsAny<Type>()))
                .Returns((Func<object> modelAccessor, Type modelType) =>
                    _oldMetadataProvider.GetMetadataForType(modelAccessor, modelType));

            // When metadata for _modelType is requested, then return a clone of the provided metadata instance.
            // GetMetadataForProperty() is important because the static discovery methods (e.g.
            // ModelMetadata.FromLambdaExpression) use it.
            metadataProvider
                .Setup(p => p.GetMetadataForType(It.IsAny<Func<object>>(), _modelType))
                .Returns((Func<object> modelAccessor, Type modelType) => GetMetadataForType(modelAccessor, modelType));
            metadataProvider
                .Setup(p =>
                    p.GetMetadataForProperty(It.IsAny<Func<object>>(), It.IsAny<Type>(), It.IsAny<string>()))
                .Returns((Func<object> modelAccessor, Type containerType, string propertyName) =>
                    GetMetadataForProperty(modelAccessor, containerType, propertyName));

            // Calls to GetMetadataForProperties for the modelType are incorrect because _metadata.Provider must
            // reference _oldMetadataProvider and not this mock.
            metadataProvider
                .Setup(p => p.GetMetadataForProperty(It.IsAny<Func<object>>(), _modelType, It.IsAny<string>()))
                .Throws<InvalidOperationException>();

            // Finally make our ModelMetadataProvider visible everywhere.
            ModelMetadataProviders.Current = metadataProvider.Object;
        }

        public void Dispose()
        {
            ModelMetadataProviders.Current = _oldMetadataProvider;
        }

        private ModelMetadata GetMetadataForType(Func<object> modelAccessor, Type modelType)
        {
            return CloneMetadata(modelAccessor);
        }

        private ModelMetadata GetMetadataForProperty(
            Func<object> modelAccessor,
            Type containerType,
            string propertyName)
        {
            var propertyMetadata =
                _oldMetadataProvider.GetMetadataForProperty(modelAccessor, containerType, propertyName);
            if (propertyMetadata == null)
            {
                return null;
            }

            if (propertyMetadata.ModelType == _modelType)
            {
                return CloneMetadata(() => propertyMetadata.Model);
            }

            return propertyMetadata;
        }

        private ModelMetadata CloneMetadata(Func<object> modelAccessor)
        {
            var cachedMetadata = _metadata as CachedDataAnnotationsModelMetadata;
            var annotationsMetadata = _metadata as DataAnnotationsModelMetadata;
            ModelMetadata clonedMetadata;
            if (cachedMetadata != null)
            {
                clonedMetadata = new CachedDataAnnotationsModelMetadata(cachedMetadata, modelAccessor);
            }
            else if (annotationsMetadata != null)
            {
                var provider = (_oldMetadataProvider as DataAnnotationsModelMetadataProvider) ?? AnnotationsProvider;
                clonedMetadata = new DataAnnotationsModelMetadata(
                    provider,
                    annotationsMetadata.ContainerType,
                    modelAccessor,
                    _modelType,
                    annotationsMetadata.PropertyName,
                    displayColumnAttribute: null);      // Copying SimpleDisplayText below compensates for null here.
            }
            else
            {
                clonedMetadata = new ModelMetadata(
                    _oldMetadataProvider,
                    _metadata.ContainerType,
                    modelAccessor,
                    _modelType,
                    _metadata.PropertyName);
            }

            // Undo all the lazy-initialization of ModelMetadata and CachedDataAnnotationsModelMetadata...
            clonedMetadata.Container = _metadata.Container;        // May be incorrect.
            clonedMetadata.ConvertEmptyStringToNull = _metadata.ConvertEmptyStringToNull;
            clonedMetadata.DataTypeName = _metadata.DataTypeName;
            clonedMetadata.Description = _metadata.Description;
            clonedMetadata.DisplayFormatString = _metadata.DisplayFormatString;
            clonedMetadata.DisplayName = _metadata.DisplayName;
            clonedMetadata.EditFormatString = _metadata.EditFormatString;
            clonedMetadata.HasNonDefaultEditFormat = _metadata.HasNonDefaultEditFormat;
            clonedMetadata.HideSurroundingHtml = _metadata.HideSurroundingHtml;
            clonedMetadata.HtmlEncode = _metadata.HtmlEncode;
            clonedMetadata.IsReadOnly = _metadata.IsReadOnly;
            clonedMetadata.IsRequired = _metadata.IsRequired;
            clonedMetadata.NullDisplayText = _metadata.NullDisplayText;
            clonedMetadata.Order = _metadata.Order;
            clonedMetadata.RequestValidationEnabled = _metadata.RequestValidationEnabled;
            clonedMetadata.ShortDisplayName = _metadata.ShortDisplayName;
            clonedMetadata.ShowForDisplay = _metadata.ShowForDisplay;
            clonedMetadata.ShowForEdit = _metadata.ShowForEdit;
            clonedMetadata.SimpleDisplayText = _metadata.SimpleDisplayText;
            clonedMetadata.TemplateHint = _metadata.TemplateHint;
            clonedMetadata.Watermark = _metadata.Watermark;
            foreach (var keyValuePair in _metadata.AdditionalValues)
            {
                clonedMetadata.AdditionalValues.Add(keyValuePair.Key, keyValuePair.Value);
            }

            return clonedMetadata;
        }
    }
}
