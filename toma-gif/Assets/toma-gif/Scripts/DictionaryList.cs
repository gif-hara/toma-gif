using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace HK
{
    [Serializable]
    public sealed class DictionaryData<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> dictionary = new();

        private readonly Func<TValue, TKey> idSelector;

        public DictionaryData(Func<TValue, TKey> idSelector)
        {
            this.idSelector = idSelector;
        }

        public void Set(IEnumerable<TValue> list)
        {
            dictionary.Clear();
            foreach (var item in list)
            {
                dictionary.Add(idSelector(item), item);
            }
        }

        public void Add(TValue value)
        {
            dictionary.Add(idSelector(value), value);
        }

        public void Remove(TKey key)
        {
            dictionary.Remove(key);
        }

        public void Remove(TValue value)
        {
            var key = idSelector(value);
            dictionary.Remove(key);
        }

        public void Clear()
        {
            dictionary.Clear();
        }

        public TValue Get(TKey key)
        {
            Assert.IsTrue(dictionary.ContainsKey(key), $"TKey={typeof(TKey)} TValue={typeof(TValue)} key={key}");
            return dictionary[key];
        }

        public bool ContainsKey(TKey key)
        {
            return dictionary.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return dictionary.TryGetValue(key, out value);
        }

        public bool IsNullOrEmpty()
        {
            return dictionary == null || dictionary.Count == 0;
        }
    }

    [Serializable]
    public abstract class DictionaryList<TKey, TValue>
    {
        [SerializeField]
        private List<TValue> list = new();
        public IReadOnlyList<TValue> List => list;

        private readonly DictionaryData<TKey, TValue> dictionaryData;

        public DictionaryList(Func<TValue, TKey> idSelector)
        {
            dictionaryData = new DictionaryData<TKey, TValue>(idSelector);
        }

        public void Set(IEnumerable<TValue> list)
        {
            this.list = list.ToList();
            dictionaryData.Set(list);
        }

        public void Add(TValue value)
        {
            list.Add(value);
            dictionaryData.Add(value);
        }

        public void Remove(TValue value)
        {
            list.Remove(value);
            dictionaryData.Remove(value);
        }

        public void Clear()
        {
            list.Clear();
            dictionaryData.Clear();
        }

        public TValue Get(TKey key)
        {
            InitializeIfNull();
            return dictionaryData.Get(key);
        }

        public bool ContainsKey(TKey key)
        {
            InitializeIfNull();
            return dictionaryData.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            InitializeIfNull();
            return dictionaryData.TryGetValue(key, out value);
        }

        private void InitializeIfNull()
        {
            // UnityEditorの場合は毎回初期化する
#if UNITY_EDITOR
            dictionaryData.Set(list);
#else
            if (dictionaryData.IsNullOrEmpty())
            {
                dictionaryData.Set(list);
            }
#endif
        }
    }

    [Serializable]
    public abstract class DictionaryList<TKey1, TKey2, TValue>
    {
        [SerializeField]
        private List<TValue> list;
        public IReadOnlyList<TValue> List => list;

        private readonly DictionaryData<TKey1, TValue> dictionaryData1;

        private readonly DictionaryData<TKey2, TValue> dictionaryData2;

        public DictionaryList(Func<TValue, TKey1> idSelector, Func<TValue, TKey2> idSelector2)
        {
            dictionaryData1 = new DictionaryData<TKey1, TValue>(idSelector);
            dictionaryData2 = new DictionaryData<TKey2, TValue>(idSelector2);
        }

        public void Set(IEnumerable<TValue> list)
        {
            this.list = list.ToList();
            dictionaryData1.Set(list);
            dictionaryData2.Set(list);
        }

        public void Add(TValue value)
        {
            list.Add(value);
            dictionaryData1.Add(value);
            dictionaryData2.Add(value);
        }

        public void Remove(TValue value)
        {
            list.Remove(value);
            dictionaryData1.Remove(value);
            dictionaryData2.Remove(value);
        }

        public void Clear()
        {
            list.Clear();
            dictionaryData1.Clear();
            dictionaryData2.Clear();
        }

        public TValue Get(TKey1 key)
        {
            InitializeIfNull();
            return dictionaryData1.Get(key);
        }

        public bool ContainsKey(TKey1 key)
        {
            InitializeIfNull();
            return dictionaryData1.ContainsKey(key);
        }

        public bool TryGetValue(TKey1 key, out TValue value)
        {
            InitializeIfNull();
            return dictionaryData1.TryGetValue(key, out value);
        }

        public TValue Get(TKey2 key)
        {
            InitializeIfNull();
            return dictionaryData2.Get(key);
        }

        public bool ContainsKey(TKey2 key)
        {
            InitializeIfNull();
            return dictionaryData2.ContainsKey(key);
        }

        public bool TryGetValue(TKey2 key, out TValue value)
        {
            InitializeIfNull();
            return dictionaryData2.TryGetValue(key, out value);
        }

        private void InitializeIfNull()
        {
            // UnityEditorの場合は毎回初期化する
#if UNITY_EDITOR
            dictionaryData1.Set(list);
            dictionaryData2.Set(list);
#else
            if (dictionaryData1.IsNullOrEmpty())
            {
                dictionaryData1.Set(list);
            }
            if (dictionaryData2.IsNullOrEmpty())
            {
                dictionaryData2.Set(list);
            }
#endif
        }
    }
}