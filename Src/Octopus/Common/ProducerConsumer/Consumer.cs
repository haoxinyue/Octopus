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

        private BlockingCollection<T> _productQueue = null;

        private bool _stopRequest = false;

        public Consumer(Action<T> consumerAction, BlockingCollection<T> productQueue)
        {
            _consumerAction = consumerAction;
            _productQueue = productQueue;
        }

        public void StartConsume()
        {
            while (!_stopRequest)
            {
                T t = default(T);
                if (_productQueue.TryTake(out t))
                {
                    _consumerAction(t);
                }
            }
        }

        public void StopConsume()
        {
            _stopRequest = true;
        }
    }
}
