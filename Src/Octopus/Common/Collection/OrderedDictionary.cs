
using System;
using System.Collections.Generic;
namespace Octopus.Common.Collection
{    
    public class OrderedDictionary<K, V> : List<KeyValuePair<K, V>>
    {
        public List<K> Keys
        {
            get
            {
                List<K> keys = new List<K>();

                foreach (KeyValuePair<K, V> pair in this)
                    keys.Add(pair.Key);

                return keys;
            }
        }

        public List<V> Values
        {
            get
            {
                List<V> values = new List<V>();

                foreach (KeyValuePair<K, V> pair in this)
                    values.Add(pair.Value);

                return values;
            }
        }

        public void Add(K key, V val)
        {
            if (ContainsKey(key))
                throw new Exception("Duplicate Key");

            base.Add(new KeyValuePair<K, V>(key, val));
        }

        public bool ContainsKey(K key)
        {
            return Keys.Contains(key);
        }

        public bool ContainsValue(V val)
        {
            return Values.Contains(val);
        }

        public void Insert(int index, K key, V val)
        {
            if (ContainsKey(key))
                throw new Exception("Duplicate Key");

            base.Insert(index, new KeyValuePair<K, V>(key, val));
        }

        public void Remove(K key)
        {
            int i = Keys.IndexOf(key);

            if (i < 0)
                throw new Exception("Key not found");

            base.RemoveAt(i);
        }

        public V this[K key]
        {
            get
            {
                int index = Keys.IndexOf(key);

                if (index < 0)
                    return default(V);

                return base[index].Value;
            }
            set
            {
                int index = Keys.IndexOf(key);

                if (index < 0)
                    Add(key, value);
                else
                    this[index] = new KeyValuePair<K, V>(key, value);
            }
        }
    }
}
