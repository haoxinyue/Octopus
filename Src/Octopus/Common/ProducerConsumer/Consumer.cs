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

using Octopus.Common.Collection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Octopus.Common.ProducerConsumer
{
    public class Consumer<T> : IConsumer<T>
    {
        private Action<T> _consumerAction = null;

        private ConcurrentQueue<T> _productQueue = null;

        private bool _stopRequest = false;

        private WaitHandle _waitHandle = null;

        public Consumer(Action<T> consumerAction, ConcurrentQueue<T> productQueue) : this(consumerAction, productQueue, null) { }

        public Consumer(Action<T> consumerAction, ConcurrentQueue<T> productQueue, WaitHandle waitHandle)
        {
            _consumerAction = consumerAction;
            _productQueue = productQueue;
            _waitHandle = waitHandle;
        }

        public void StartConsume()
        {
            while (!_stopRequest)
            {
                if (_waitHandle.WaitOne())
                {
                    try
                    {
                        while (_productQueue.Count > 0)
                        {
                            T t = default(T);
                            if (_productQueue.TryDequeue(out t))
                            {
                                _consumerAction(t);
                            }
                        }
                    }
                    catch
                    {
                        throw;
                    }
                }
            }
        }

        public void StopConsume()
        {
            _stopRequest = true;
        }
    }
}
