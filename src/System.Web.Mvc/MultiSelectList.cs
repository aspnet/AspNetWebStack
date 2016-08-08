// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;

namespace System.Web.Mvc
{
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi", Justification = "FxCop won't accept this in the custom dictionary, so we're suppressing it in source")]
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "This is a shipped API")]
    public class MultiSelectList : IEnumerable<SelectListItem>
    {
        private IList<SelectListGroup> _groups;

        public MultiSelectList(IEnumerable items)
            : this(items, selectedValues: null)
        {
        }

        public MultiSelectList(IEnumerable items, IEnumerable selectedValues)
            : this(items, dataValueField: null, dataTextField: null, selectedValues: selectedValues)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MultiSelectList class by using the items to include in the list, 
        /// the selected values, the disabled values.
        /// </summary>
        /// <param name="items">The items used to build each <see cref="SelectListItem"/> of the list.</param>
        /// <param name="selectedValues">The selected values field. Used to match the Selected property of the 
        /// corresponding <see cref="SelectListItem"/>.</param>
        /// <param name="disabledValues">The disabled values. Used to match the Disabled property of the corresponding
        /// <see cref="SelectListItem"/>.</param>>
        public MultiSelectList(IEnumerable items, IEnumerable selectedValues, IEnumerable disabledValues)
            : this(items,
                   dataValueField: null,
                   dataTextField: null,
                   selectedValues: selectedValues,
                   disabledValues: disabledValues)
        {
        }

        public MultiSelectList(IEnumerable items, string dataValueField, string dataTextField)
            : this(items, dataValueField, dataTextField, selectedValues: null)
        {
        }

        public MultiSelectList(IEnumerable items, string dataValueField, string dataTextField, IEnumerable selectedValues)
            : this(items, dataValueField, dataTextField, dataGroupField: null, selectedValues: selectedValues)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MultiSelectList class by using the items to include in the list, 
        /// the data value field, the data text field, and the data group field.
        /// </summary>
        /// <param name="items">The items used to build each <see cref="SelectListItem"/> of the list.</param>
        /// <param name="dataValueField">The data value field. Used to match the Value property of the corresponding 
        /// <see cref="SelectListItem"/>.</param>
        /// <param name="dataTextField">The data text field. Used to match the Text property of the corresponding 
        /// <see cref="SelectListItem"/>.</param>
        /// <param name="dataGroupField">The data group field. Used to match the Group property of the corresponding 
        /// <see cref="SelectListItem"/>.</param>
        public MultiSelectList(IEnumerable items, string dataValueField, string dataTextField, string dataGroupField)
            : this(items, dataValueField, dataTextField, dataGroupField: dataGroupField, selectedValues: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MultiSelectList class by using the items to include in the list, 
        /// the data value field, the data text field, the selected values, and the disabled values.
        /// </summary>
        /// <param name="items">The items used to build each <see cref="SelectListItem"/> of the list.</param>
        /// <param name="dataValueField">The data value field. Used to match the Value property of the corresponding 
        /// <see cref="SelectListItem"/>.</param>
        /// <param name="dataTextField">The data text field. Used to match the Text property of the corresponding 
        /// <see cref="SelectListItem"/>.</param>
        /// <param name="selectedValues">The selected values field. Used to match the Selected property of the 
        /// corresponding <see cref="SelectListItem"/>.</param>
        /// <param name="disabledValues">The disabled values. Used to match the Disabled property of the corresponding
        /// <see cref="SelectListItem"/>.</param>>
        public MultiSelectList(IEnumerable items,
                               string dataValueField,
                               string dataTextField,
                               IEnumerable selectedValues,
                               IEnumerable disabledValues)
            : this(items,
                   dataValueField,
                   dataTextField,
                   dataGroupField: null,
                   selectedValues: selectedValues,
                   disabledValues: disabledValues)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MultiSelectList class by using the items to include in the list, 
        /// the data value field, the data text field, the data group field, and the selected values.
        /// </summary>
        /// <param name="items">The items used to build each <see cref="SelectListItem"/> of the list.</param>
        /// <param name="dataValueField">The data value field. Used to match the Value property of the corresponding 
        /// <see cref="SelectListItem"/>.</param>
        /// <param name="dataTextField">The data text field. Used to match the Text property of the corresponding 
        /// <see cref="SelectListItem"/>.</param>
        /// <param name="dataGroupField">The data group field. Used to match the Group property of the corresponding 
        /// <see cref="SelectListItem"/>.</param>
        /// <param name="selectedValues">The selected values field. Used to match the Selected property of the 
        /// corresponding <see cref="SelectListItem"/>.</param>
        public MultiSelectList(IEnumerable items,
                               string dataValueField,
                               string dataTextField,
                               string dataGroupField,
                               IEnumerable selectedValues)
            : this(items, dataValueField, dataTextField, dataGroupField, selectedValues, disabledValues: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MultiSelectList class by using the items to include in the list,
        /// the data value field, the data text field, the data group field, the selected values, and the disabled
        /// values.
        /// </summary>
        /// <param name="items">The items used to build each <see cref="SelectListItem"/> of the list.</param>
        /// <param name="dataValueField">The data value field. Used to match the Value property of the corresponding 
        /// <see cref="SelectListItem"/>.</param>
        /// <param name="dataTextField">The data text field. Used to match the Text property of the corresponding 
        /// <see cref="SelectListItem"/>.</param>
        /// <param name="dataGroupField">The data group field. Used to match the Group property of the corresponding 
        /// <see cref="SelectListItem"/>.</param>
        /// <param name="selectedValues">The selected values field. Used to match the Selected property of the 
        /// corresponding <see cref="SelectListItem"/>.</param>
        /// <param name="disabledValues">The disabled values. Used to match the Disabled property of the corresponding
        /// <see cref="SelectListItem"/>.</param>
        public MultiSelectList(IEnumerable items,
                               string dataValueField,
                               string dataTextField,
                               string dataGroupField,
                               IEnumerable selectedValues,
                               IEnumerable disabledValues)
            : this(items,
                   dataValueField,
                   dataTextField,
                   dataGroupField,
                   selectedValues,
                   disabledValues,
                   disabledGroups: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MultiSelectList class by using the items to include in the list, 
        /// the data value field, the data text field, the data group field, the selected values, the disabled values,
        /// and the disabled groups.
        /// </summary>
        /// <param name="items">The items used to build each <see cref="SelectListItem"/> of the list.</param>
        /// <param name="dataValueField">The data value field. Used to match the Value property of the corresponding 
        /// <see cref="SelectListItem"/>.</param>
        /// <param name="dataTextField">The data text field. Used to match the Text property of the corresponding 
        /// <see cref="SelectListItem"/>.</param>
        /// <param name="dataGroupField">The data group field. Used to match the Group property of the corresponding 
        /// <see cref="SelectListItem"/>.</param>
        /// <param name="selectedValues">The selected values field. Used to match the Selected property of the 
        /// corresponding <see cref="SelectListItem"/>.</param>
        /// <param name="disabledValues">The disabled values. Used to match the Disabled property of the corresponding
        /// <see cref="SelectListItem"/>.</param>
        /// <param name="disabledGroups">The disabled groups. Used to match the Disabled property of the corresponding
        /// <see cref="SelectListGroup"/>.</param>
        public MultiSelectList(IEnumerable items,
                               string dataValueField,
                               string dataTextField,
                               string dataGroupField,
                               IEnumerable selectedValues,
                               IEnumerable disabledValues,
                               IEnumerable disabledGroups)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            Items = items;
            DataValueField = dataValueField;
            DataTextField = dataTextField;
            SelectedValues = selectedValues;
            DataGroupField = dataGroupField;
            DisabledValues = disabledValues;
            DisabledGroups = disabledGroups;

            if (DataGroupField != null)
            {
                _groups = new List<SelectListGroup>();
            }
        }

        /// <summary>
        /// Gets the data group field.
        /// </summary>
        public string DataGroupField { get; private set; }

        public string DataTextField { get; private set; }

        public string DataValueField { get; private set; }

        /// <summary>
        /// Gets the disabled groups.
        /// </summary>
        public IEnumerable DisabledGroups { get; private set; }

        /// <summary>
        /// Gets the disabled values.
        /// </summary>
        public IEnumerable DisabledValues { get; private set; }

        public IEnumerable Items { get; private set; }

        public IEnumerable SelectedValues { get; private set; }

        public virtual IEnumerator<SelectListItem> GetEnumerator()
        {
            return GetListItems().GetEnumerator();
        }

        internal IList<SelectListItem> GetListItems()
        {
            return (!String.IsNullOrEmpty(DataValueField))
                       ? GetListItemsWithValueField()
                       : GetListItemsWithoutValueField();
        }

        private IList<SelectListItem> GetListItemsWithValueField()
        {
            HashSet<string> selectedValues = GetStringHashSet(SelectedValues);
            HashSet<string> disabledValues = GetStringHashSet(DisabledValues);
            HashSet<string> disabledGroups = GetStringHashSet(DisabledGroups);

            IEnumerable<SelectListItem> listItems = Items.Cast<object>().Select(item =>
            {
                string value = Eval(item, DataValueField);
                return new SelectListItem
                {
                    Group = GetGroup(item, disabledGroups),
                    Value = value,
                    Text = Eval(item, DataTextField),
                    Selected = selectedValues.Contains(value),
                    Disabled = disabledValues.Contains(value),
                };
            });

            return listItems.ToList();
        }

        private IList<SelectListItem> GetListItemsWithoutValueField()
        {
            HashSet<object> selectedValues = GetObjectHashSet(SelectedValues);
            HashSet<object> disabledValues = GetObjectHashSet(DisabledValues);
            HashSet<string> disabledGroups = GetStringHashSet(DisabledGroups);

            IEnumerable<SelectListItem> listItems = Items.Cast<object>().Select(item =>
            {
                return new SelectListItem
                {
                    Group = GetGroup(item, disabledGroups),
                    Text = Eval(item, DataTextField),
                    Selected = selectedValues.Contains(item),
                    Disabled = disabledValues.Contains(item),
                };
            });

            return listItems.ToList();
        }

        private static string Eval(object container, string expression)
        {
            object value = container;
            if (!String.IsNullOrEmpty(expression))
            {
                value = DataBinder.Eval(container, expression);
            }
            return Convert.ToString(value, CultureInfo.CurrentCulture);
        }

        private SelectListGroup GetGroup(object container, HashSet<string> disabledGroups)
        {
            if (_groups == null)
            {
                return null;
            }

            string groupName = Eval(container, DataGroupField);
            if (String.IsNullOrEmpty(groupName))
            {
                return null;
            }

            // We use StringComparison.CurrentCulture because the group name is used to display as the value of 
            // optgroup HTML tag's label attribute.
            SelectListGroup group = _groups.FirstOrDefault(
                g => String.Equals(g.Name, groupName, StringComparison.CurrentCulture));
            if (group == null)
            {
                group = new SelectListGroup() { Name = groupName, Disabled = disabledGroups.Contains(groupName) };
                _groups.Add(group);
            }

            return group;
        }

        private static HashSet<string> GetStringHashSet(IEnumerable values)
        {
            HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (values != null)
            {
                hashSet.UnionWith(
                    values.Cast<object>().Select(value => Convert.ToString(value, CultureInfo.CurrentCulture)));
            }
            return hashSet;
        }

        private static HashSet<object> GetObjectHashSet(IEnumerable values)
        {
            HashSet<object> hashSet = new HashSet<object>();
            if (values != null)
            {
                hashSet.UnionWith(values.Cast<object>());
            }
            return hashSet;
        }

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
