// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.WebPages.Resources;

namespace System.Web.WebPages
{
    // Wrapper for list that lets us return empty string for non existant pieces of the Url
    internal class UrlDataList : IList<string>
    {
        private List<string> _urlData;

        public UrlDataList(string pathInfo)
        {
            if (String.IsNullOrEmpty(pathInfo))
            {
                _urlData = new List<string>();
            }
            else
            {
                _urlData = pathInfo.Split(new char[] { '/' }).ToList();
            }
        }

        public int Count
        {
            get { return _urlData.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public string this[int index]
        {
            get
            {
                // REVIEW: what about index < 0
                if (index >= _urlData.Count)
                {
                    return String.Empty;
                }
                return _urlData[index];
            }
            set { throw new NotSupportedException(WebPageResources.UrlData_ReadOnly); }
        }

        public int IndexOf(string item)
        {
            return _urlData.IndexOf(item);
        }

        public void Insert(int index, string item)
        {
            throw new NotSupportedException(WebPageResources.UrlData_ReadOnly);
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException(WebPageResources.UrlData_ReadOnly);
        }

        public void Add(string item)
        {
            throw new NotSupportedException(WebPageResources.UrlData_ReadOnly);
        }

        public void Clear()
        {
            throw new NotSupportedException(WebPageResources.UrlData_ReadOnly);
        }

        public bool Contains(string item)
        {
            return _urlData.Contains(item);
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            _urlData.CopyTo(array, arrayIndex);
        }

        public bool Remove(string item)
        {
            throw new NotSupportedException(WebPageResources.UrlData_ReadOnly);
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _urlData.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _urlData.GetEnumerator();
        }
    }
}
