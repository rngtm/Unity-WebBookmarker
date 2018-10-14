using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WebBookmarker
{
    [CreateAssetMenu]
    internal class BookmarkData : ScriptableObject
    {
        [SerializeField] private List<UrlData> m_dataList = new List<UrlData>();

        public int Count => m_dataList.Count;

        public void InsertAt(int index, UrlData data)
        {
            m_dataList.Insert(index, data);
        }

        public IList GetList()
        {
            return m_dataList;
        }

        internal IEnumerable<UrlData> GetURLAll()
        {
            return m_dataList;
        }

        internal void DeleteAt(int id)
        {
            m_dataList.RemoveAt(id);
        }

        internal void DeleteEmptyItems()
        {
            m_dataList.RemoveAll(item => IsEmpty(item.Title) && IsEmpty(item.URL));
        }

        private bool IsEmpty(string s)
        {
            if (string.IsNullOrEmpty(s)) { return true; }
            return string.IsNullOrEmpty(s.Replace(" ", ""));
        }
    }
}