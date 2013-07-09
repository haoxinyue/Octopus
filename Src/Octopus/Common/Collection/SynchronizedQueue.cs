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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Octopus.Common.Collection
{
    public class SynchronizedQueue<T>
    {
        private readonly Queue<T> queue = new Queue<T>();
        private readonly int maxSize = 1000000;
        public SynchronizedQueue(int maxSize) { this.maxSize = maxSize; }
        public SynchronizedQueue() { }

        public void Enqueue(T item)
        {
            lock (queue)
            {
                while (queue.Count >= maxSize)
                {
                    Monitor.Wait(queue);
                }
                queue.Enqueue(item);
                if (queue.Count >= 1)
                {
                    Monitor.PulseAll(queue);
                }
            }
        }

        public bool TryDequeue(out T result)
        {
            lock (queue)
            {
                while (queue.Count == 0)
                {
                    Monitor.Wait(queue);
                }
                result = queue.Dequeue();
                if (queue.Count == maxSize - 1)
                {
                    Monitor.PulseAll(queue);
                }
                return true;
            }
        }

        public void Clear()
        {
            lock (queue)
            {
                queue.Clear();
            }
        }

        public int Count
        {
            get 
            {
                lock (queue)
                {
                    return queue.Count;
                }
            }
        }

        public bool IsEmpty
        {
            get 
            {
                lock (queue)
                {
                    return queue.Count == 0;
                }
            }
        }
    }
}
