#region License
// Copyright (c) Nick Hao http://www.cnblogs.com/haoxinyue
// 
// Licensed under the Apache License, Version 2.0 (the 'License'); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an 'AS IS' BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Octopus.Common.Collection
{
    public class PriorityQueue<P, V>
    {
        private SortedDictionary<P, Queue<V>> list = new SortedDictionary<P, Queue<V>>();

        private object locker = new object();

        public void Enqueue(P priority, V value)
        {
            lock(locker)
            {
                Queue<V> q;
                if (!list.TryGetValue(priority, out q))
                {
                    q = new Queue<V>();
                    list.Add(priority, q);
                }
                q.Enqueue(value);
            }
        }
        public V Dequeue()
        {
            lock(locker)
            {
                var pair = list.First();
                var v = pair.Value.Dequeue();
                if (pair.Value.Count == 0)
                    list.Remove(pair.Key);
                return v;
            }
        }
        public bool IsEmpty
        {
            get { 
                lock(locker)
                {
                return !list.Any(); 
                }
            }
        }

        public bool ContainsKey(P key)
        {
            lock (locker)
            {
                return list.ContainsKey(key);
            }
        }
    }
}
