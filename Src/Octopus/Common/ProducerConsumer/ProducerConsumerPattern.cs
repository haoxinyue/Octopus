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
using System.Collections;
using Octopus.Common.Collection;
using System.Threading;
using System.Collections.Concurrent;

namespace Octopus.Common.ProducerConsumer
{
    public class ProducerConsumerPattern<T> : IProducerConsumerPattern
    {
        private List<IProducer<T>> _producers = new List<IProducer<T>>();

        private List<IConsumer<T>> _consumers = new List<IConsumer<T>>();

        private BlockingCollection<T> _queue = null;

        private bool _isDisposed = false;

        public Action PreStartAction { get; set; }

        public ProducerConsumerPattern(BlockingCollection<T> queue, Func<T> producerFunc, int producerCount, Action<T> consumerAction, int consumerCount)
        {
            _queue = queue;

            for (int i = 0; i < producerCount; i++)
            {
                IProducer<T> producer = new Producer<T>(producerFunc, queue);
                _producers.Add(producer);
            }

            for (int i = 0; i < consumerCount; i++)
            {
                Consumer<T> consumer = new Consumer<T>(consumerAction, queue);
                _consumers.Add(consumer);
            }
        }

        public ProducerConsumerPattern(BlockingCollection<T> queue, Func<T> producerFunc, Action<T> consumerAction) : this(queue, producerFunc, 1, consumerAction, 1) { }

        public ProducerConsumerPattern(BlockingCollection<T> queue, Func<T> producerFunc, Action<T> consumerAction, int consumerCount) : this(queue, producerFunc, 1, consumerAction, consumerCount) { }

        public void Start()
        {
            if (PreStartAction != null)
            {
                PreStartAction();
            }

            if (_producers.Count > 0 && _consumers.Count > 0)
            {
                foreach (IConsumer<T> consumer in _consumers)
                {
                    Thread tConsumer = new Thread(() => consumer.StartConsume());
                    tConsumer.IsBackground = true;
                    tConsumer.Start();
                }

                foreach (IProducer<T> producer in _producers)
                {
                    Thread tProducer = new Thread(() => producer.StartProduce());
                    tProducer.IsBackground = true;
                    tProducer.Start();
                }
            }
        }

        public int GetItemCountInQueue()
        {
            if (_queue != null)
            {
                return _queue.Count;
            }
            else
            {
                return 0;
            }
        }

        private void Close()
        {
            foreach (IConsumer<T> consumer in _consumers)
            {
                consumer.StopConsume();
            }

            foreach (IProducer<T> producer in _producers)
            {
                producer.StopProduce();
            }

            _consumers.Clear();
            _producers.Clear();
            _consumers = null;
            _producers = null;
            _queue = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool diposing)
        {
            if (!_isDisposed)
            {
                if (diposing)
                {
                    Close();
                }
            }

            _isDisposed = true;
        }
    }
}
