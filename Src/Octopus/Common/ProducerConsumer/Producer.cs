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
using System.Threading;
using Octopus.Common.Collection;
using System.Collections.Concurrent;

namespace Octopus.Common.ProducerConsumer
{
    public class Producer<T> : IProducer<T>
    {
        private Func<T> _produceFunc = null;

        private ConcurrentQueue<T> _productQueue = null;

        private bool stopRequest;

        private AutoResetEvent _autoResetEvent = null;

        public Producer(Func<T> produceFunc, ConcurrentQueue<T> productQueue, AutoResetEvent autoResetEvent)
        {
            _produceFunc = produceFunc;
            _productQueue = productQueue;
            _autoResetEvent = autoResetEvent;
        }

        public void StartProduce()
        {
            while (!stopRequest)
            {
                try
                {
                    T t = _produceFunc();

                    _productQueue.Enqueue(t);

                    _autoResetEvent.Set();
                }
                catch
                {
                    throw;
                }
            }
        }

        public void StopProduce()
        {
            stopRequest = true;
        }
    }
}
