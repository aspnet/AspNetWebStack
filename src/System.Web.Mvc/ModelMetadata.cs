// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Mvc.ExpressionUtil;
using System.Web.Mvc.Properties;

namespace System.Web.Mvc
{
    public class ModelMetadata
    {
        public const int DefaultOrder = 10000;

        private readonly Type _containerType;
        private readonly Type _modelType;
        private readonly string _propertyName;

        /// <summary>
        /// Explicit backing store for the things we want initialized by default, so don't have to call
        /// the protected virtual setters of an auto-generated property
        /// </summary>
        private Dictionary<string, object> _additionalValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        private bool _convertEmptyStringToNull = true;
        private bool _htmlEncode = true;
        private bool _isRequired;
        private object _model;
        private Func<object> _modelAccessor;
        private int _order = DefaultOrder;
        private IEnumerable<ModelMetadata> _properties;
        private ModelMetadata[] _propertiesInternal;
        private Type _realModelType;
        private bool _requestValidationEnabled = true;
        private bool _showForDisplay = true;
        private bool _showForEdit = true;
        private string _simpleDisplayText;

        public ModelMetadata(ModelMetadataProvider provider, Type containerType, Func<object> modelAccessor, Type modelType, string propertyName)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            if (modelType == null)
            {
                throw new ArgumentNullException("modelType");
            }

            Provider = provider;

            _containerType = containerType;
            _isRequired = !TypeHelpers.TypeAllowsNullValue(modelType);
            _modelAccessor = modelAccessor;
            _modelType = modelType;
            _propertyName = propertyName;
        }

        public virtual Dictionary<string, object> AdditionalValues
        {
            get { return _additionalValues; }
        }

        /// <summary>
        /// A reference to the model's container object. Will be non-null if the model represents a property.
        /// </summary>
        public object Container { get; set; }

        public Type ContainerType
        {
            get { return _containerType; }
        }

        public virtual bool ConvertEmptyStringToNull
        {
            get { return _convertEmptyStringToNull; }
            set { _convertEmptyStringToNull = value; }
        }

        public virtual string DataTypeName { get; set; }

        public virtual string Description { get; set; }

        public virtual string DisplayFormatString { get; set; }

        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "The method is a delegating helper to choose among multiple property values")]
        public virtual string DisplayName { get; set; }

        public virtual string EditFormatString { get; set; }

        internal virtual bool HasNonDefaultEditFormat { get; set; }

        public virtual bool HideSurroundingHtml { get; set; }

        public virtual bool HtmlEncode
        {
            get { return _htmlEncode; }
            set { _htmlEncode = value; }
        }

        public virtual bool IsComplexType
        {
            get { return !(TypeDescriptor.GetConverter(ModelType).CanConvertFrom(typeof(string))); }
        }

        public bool IsNullableValueType
        {
            get { return TypeHelpers.IsNullableValueType(ModelType); }
        }

        public virtual bool IsReadOnly { get; set; }

        public virtual bool IsRequired
        {
            get { return _isRequired; }
            set { _isRequired = value; }
        }

        public object Model
        {
            get
            {
                if (_modelAccessor != null)
                {
                    _model = _modelAccessor();
                    _modelAccessor = null;
                }
                return _model;
            }
            set
            {
                _model = value;
                _modelAccessor = null;
                _properties = null;
                _realModelType = null;
            }
        }

        public Type ModelType
        {
            get { return _modelType; }
        }

        public virtual string NullDisplayText { get; set; }

        public virtual int Order
        {
            get { return _order; }
            set { _order = value; }
        }

        public virtual IEnumerable<ModelMetadata> Properties
        {
            get
            {
                if (_properties == null)
                {
                    IEnumerable<ModelMetadata> originalProperties = Provider.GetMetadataForProperties(Model, RealModelType);
                    // This will be returned as a copied out array in the common case, so reuse the returned array for performance.
                    _propertiesInternal = SortProperties(originalProperties.AsArray());
                    _properties = new ReadOnlyCollection<ModelMetadata>(_propertiesInternal);
                }
                return _properties;
            }
        }

        internal ModelMetadata[] PropertiesAsArray 
        {
            get
            {
                IEnumerable<ModelMetadata> virtualProperties = Properties;
                if (Object.ReferenceEquals(virtualProperties, _properties))
                {
                    Contract.Assert(_propertiesInternal != null);
                    return _propertiesInternal;
                }
                return virtualProperties.AsArray();
            }
        }

        public string PropertyName
        {
            get { return _propertyName; }
        }

        protected ModelMetadataProvider Provider { get; set; }

        internal Type RealModelType
        {
            get
            {
                if (_realModelType == null)
                {
                    _realModelType = ModelType;

                    // Don't call GetType() if the model is Nullable<T>, because it will
                    // turn Nullable<T> into T for non-null values
                    if (Model != null && !TypeHelpers.IsNullableValueType(ModelType))
                    {
                        _realModelType = Model.GetType();
                    }
                }

                return _realModelType;
            }
        }

        public virtual bool RequestValidationEnabled
        {
            get { return _requestValidationEnabled; }
            set { _requestValidationEnabled = value; }
        }

        public virtual string ShortDisplayName { get; set; }

        public virtual bool ShowForDisplay
        {
            get { return _showForDisplay; }
            set { _showForDisplay = value; }
        }

        public virtual bool ShowForEdit
        {
            get { return _showForEdit; }
            set { _showForEdit = value; }
        }

        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "This property delegates to the method when the user has not yet set a simple display text value.")]
        public virtual string SimpleDisplayText
        {
            get
            {
                if (_simpleDisplayText == null)
                {
                    _simpleDisplayText = GetSimpleDisplayText();
                }
                return _simpleDisplayText;
            }
            set { _simpleDisplayText = value; }
        }

        public virtual string TemplateHint { get; set; }

        public virtual string Watermark { get; set; }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types")]
        public static ModelMetadata FromLambdaExpression<TParameter, TValue>(Expression<Func<TParameter, TValue>> expression,
                                                                             ViewDataDictionary<TParameter> viewData)
        {
            return FromLambdaExpression(expression, viewData, metadataProvider: null);
        }

        internal static ModelMetadata FromLambdaExpression<TParameter, TValue>(Expression<Func<TParameter, TValue>> expression,
                                                                               ViewDataDictionary<TParameter> viewData,
                                                                               ModelMetadataProvider metadataProvider)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }
            if (viewData == null)
            {
                throw new ArgumentNullException("viewData");
            }

            string propertyName = null;
            Type containerType = null;
            bool legalExpression = false;

            // Need to verify the expression is valid; it needs to at least end in something
            // that we can convert to a meaningful string for model binding purposes

            switch (expression.Body.NodeType)
            {
                case ExpressionType.ArrayIndex:
                    // ArrayIndex always means a single-dimensional indexer; multi-dimensional indexer is a method call to Get()
                    legalExpression = true;
                    break;

                case ExpressionType.Call:
                    // Only legal method call is a single argument indexer/DefaultMember call
                    legalExpression = ExpressionHelper.IsSingleArgumentIndexer(expression.Body);
                    break;

                case ExpressionType.MemberAccess:
                    // Property/field access is always legal
                    MemberExpression memberExpression = (MemberExpression)expression.Body;
                    propertyName = memberExpression.Member is PropertyInfo ? memberExpression.Member.Name : null;
                    containerType = memberExpression.Expression.Type;
                    legalExpression = true;
                    break;

                case ExpressionType.Parameter:
                    // Parameter expression means "model => model", so we delegate to FromModel
                    return FromModel(viewData, metadataProvider);
            }

            if (!legalExpression)
            {
                throw new InvalidOperationException(MvcResources.TemplateHelpers_TemplateLimitations);
            }

            TParameter container = viewData.Model;
            Func<object> modelAccessor = () =>
            {
                try
                {
                    return CachedExpressionCompiler.Process(expression)(container);
                }
                catch (NullReferenceException)
                {
                    return null;
                }
            };

            return GetMetadataFromProvider(modelAccessor, typeof(TValue), propertyName, container, containerType, metadataProvider);
        }

        private static ModelMetadata FromModel(ViewDataDictionary viewData, ModelMetadataProvider metadataProvider)
        {
            return viewData.ModelMetadata ?? GetMetadataFromProvider(null, typeof(string), null, null, null, metadataProvider);
        }

        public static ModelMetadata FromStringExpression(string expression, ViewDataDictionary viewData)
        {
            return FromStringExpression(expression, viewData, metadataProvider: null);
        }

        internal static ModelMetadata FromStringExpression(string expression, ViewDataDictionary viewData, ModelMetadataProvider metadataProvider)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }
            if (viewData == null)
            {
                throw new ArgumentNullException("viewData");
            }
            if (expression.Length == 0)
            {
                // Empty string really means "model metadata for the current model"
                return FromModel(viewData, metadataProvider);
            }

            ViewDataInfo vdi = viewData.GetViewDataInfo(expression);
            object container = null;
            Type containerType = null;
            Type modelType = null;
            Func<object> modelAccessor = null;
            string propertyName = null;

            if (vdi != null)
            {
                if (vdi.Container != null)
                {
                    container = vdi.Container;
                    containerType = vdi.Container.GetType();
                }

                modelAccessor = () => vdi.Value;

                if (vdi.PropertyDescriptor != null)
                {
                    propertyName = vdi.PropertyDescriptor.Name;
                    modelType = vdi.PropertyDescriptor.PropertyType;
                }
                else if (vdi.Value != null)
                {
                    // We only need to delay accessing properties (for LINQ to SQL)
                    modelType = vdi.Value.GetType();
                }
            }
            else if (viewData.ModelMetadata != null)
            {
                //  Try getting a property from ModelMetadata if we couldn't find an answer in ViewData
                ModelMetadata propertyMetadata = viewData.ModelMetadata.Properties.Where(p => p.PropertyName == expression).FirstOrDefault();
                if (propertyMetadata != null)
                {
                    return propertyMetadata;
                }
            }

            return GetMetadataFromProvider(modelAccessor, modelType ?? typeof(string), propertyName, container, containerType, metadataProvider);
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "The method is a delegating helper to choose among multiple property values")]
        public string GetDisplayName()
        {
            return DisplayName ?? PropertyName ?? ModelType.Name;
        }

        private static ModelMetadata GetMetadataFromProvider(Func<object> modelAccessor, Type modelType, string propertyName, object container, Type containerType, ModelMetadataProvider metadataProvider)
        {
            metadataProvider = metadataProvider ?? ModelMetadataProviders.Current;
            if (containerType != null && !String.IsNullOrEmpty(propertyName))
            {
                ModelMetadata metadata = metadataProvider.GetMetadataForProperty(modelAccessor, containerType, propertyName);
                if (metadata != null)
                {
                    metadata.Container = container;
                }
                return metadata;
            }
            return metadataProvider.GetMetadataForType(modelAccessor, modelType);
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method is used to resolve the simple display text when it was not explicitly set through other means.")]
        protected virtual string GetSimpleDisplayText()
        {
            if (Model == null)
            {
                return NullDisplayText;
            }

            string toStringResult = Convert.ToString(Model, CultureInfo.CurrentCulture);
            if (toStringResult == null)
            {
                return String.Empty;
            }

            if (!toStringResult.Equals(Model.GetType().FullName, StringComparison.Ordinal))
            {
                return toStringResult;
            }

            ModelMetadata firstProperty = Properties.FirstOrDefault();
            if (firstProperty == null)
            {
                return String.Empty;
            }

            if (firstProperty.Model == null)
            {
                return firstProperty.NullDisplayText;
            }

            return Convert.ToString(firstProperty.Model, CultureInfo.CurrentCulture);
        }

        public virtual IEnumerable<ModelValidator> GetValidators(ControllerContext context)
        {
            return ModelValidatorProviders.Providers.GetValidators(this, context);
        }

        private static ModelMetadata[] SortProperties(ModelMetadata[] properties)
        {
            // Performance-senstive
            // Common case is that properties do not need sorting
            int? previousOrder = null;
            bool needSort = false;
            for (int i = 0; i < properties.Length; i++)
            {
                ModelMetadata metadata = properties[i];
                if (previousOrder != null && previousOrder > metadata.Order)
                {
                    needSort = true;
                    break;
                }
                previousOrder = metadata.Order;
            }
            if (!needSort)
            {
                return properties;
            }
            // For compatibility the sort must be stable so use OrderBy rather than Array.Sort
            return properties.OrderBy(m => m.Order).ToArray();
        }
    }
}
