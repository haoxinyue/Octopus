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
using Octopus.Interpreter.Items;
using System;
using System.Collections.Generic;

namespace Octopus.Interpreter.Formatters
{
    public class ByteArrayFormatter : FormatterBase<byte[]>
    {
        protected int _formattedDataLength = 0;

        public byte _formatterId = 0;

        protected int _index = 0;

        public ByteArrayFormatter(string name) : base(name) { }

        public ByteArrayFormatter(string name, byte formatterId) : base(name) 
        {
            _formatterId = formatterId;
        }

        public override int GetFormatterRequiredDataLength()
        {
            int length = 0;

            foreach (Item item in _items)
            {                
                length += item.GetRequiredDataLength();
            }

            return length;
        }

        public override int GetFormattedDataLength()
        {
            return _formattedDataLength;
        }

        public int GetFormatterId()
        {
            return _formatterId;
        }

        protected override Message FormatProcess(byte[] input)
        {
            Message m = null;

            _index = 0;
            _formattedDataLength = 0;

            try
            {
                SortedDictionary<string, DataItem> dict = new SortedDictionary<string, DataItem>(DataItemSorter.Instance);

                foreach (Item item in _items)
                {
                    if (item is ValueItem<byte[], DataItem>)
                    {
                        DataItem dataItem = ((ValueItem<byte[], DataItem>)item).GetValue(input, _index);
                        _index += item.GetFormattedDataLength();

                        if (dataItem.Value != null || dataItem.HasItems)
                        {
                            dict.Add(item.Name, dataItem);
                        }
                    }

                    _formattedDataLength += item.GetFormattedDataLength();
                }

                m = new Message(_name, dict);
            }
            catch (Exception ex)
            {
                _logWriter.Log("FlatByteArrayFormatter Format function exception", ex);
            }

            return m;
        }
    }
}
