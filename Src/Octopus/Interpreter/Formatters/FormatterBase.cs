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

using Octopus.Channel;
using Octopus.Exceptions;
using Octopus.Interpreter.Items;
using Octopus.Log;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;

namespace Octopus.Interpreter.Formatters
{
    public abstract class FormatterBase<T> : IFormatter<T>
    {
        protected List<Item> _items = new List<Item>();

        private IComparer<Item> _itemSorter = new ItemSorter();

        protected IInterpreter<T> _interpreter = null;

        protected ILogWriter _logWriter = Logging.GetLogWriter();

        protected string _name = string.Empty;

        public string Name
        {
            get { return _name; }
        }

        public FormatterBase(string name)
        {
            _name = name;
        }

        public virtual void AddItem(Item item)
        {
            if (!Exists(item))
            {
                _items.Add(item);
                _items.Sort(_itemSorter);
            }
            else
            {
                throw new DuplicateItemException("Item exists. Same item name");
            }
        }

        public abstract int GetFormatterRequiredDataLength();

        public abstract int GetFormattedDataLength();

        protected bool Exists(Item item)
        {
            return _items.Exists((m) => m.Name == item.Name);
        }

        public virtual Message Format(T input, IPEndPoint endPoint)
        {
            if (OnFormatStart != null)
            {
                OnFormatStart(endPoint);
            }

            bool successful = false;

            Message message = FormatProcess(input);

            successful = (message != null);

            if (OnFormatComplete != null)
            {
                OnFormatComplete(successful, endPoint);
            }

            if (successful)
            {
                if (OnFormatSuccessfully != null)
                {
                    OnFormatSuccessfully(endPoint);
                }
            }
            else
            {
                if (OnFormatFailed != null)
                {
                    OnFormatFailed(endPoint);
                }
            }

            return message;
        }

        protected abstract Message FormatProcess(T input);

        public ReadOnlyCollection<Item> Items
        {
            get
            {
                return new ReadOnlyCollection<Item>(_items);
            }
        }

        public event Action<bool, IPEndPoint> OnFormatComplete;

        public event Action<IPEndPoint> OnFormatFailed;

        public event Action<IPEndPoint> OnFormatSuccessfully;

        public event Action<IPEndPoint> OnFormatStart;


        public void Remove()
        {
            if (_interpreter != null)
            {
                _interpreter.DeleteFormatter(this);
            }
        }

        public IInterpreter<T> Interpreter
        {
            get { return _interpreter; }
            set { _interpreter = value; }
        }
    }
}
