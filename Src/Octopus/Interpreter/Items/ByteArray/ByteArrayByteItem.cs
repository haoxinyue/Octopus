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
using Octopus.Exceptions;

namespace Octopus.Interpreter.Items
{
    public class ByteArrayByteItem : ByteArrayValueItem
    {
        private List<BitItem> _bitItems = new List<BitItem>();

        public ByteArrayByteItem(string name) : base(name) { }

        public ByteArrayByteItem(string name, short sortIndex) : base(name, sortIndex) { }

        public override DataItem GetValue(byte[] input, int index, ref int formattedDataLength)
        {
            DataItem di = new DataItem(_name, input[index]);
            formattedDataLength = sizeof(byte);

            if (_bitItems != null && _bitItems.Count > 0)
            {
                int indexInByte = 0;
                foreach (BitItem bi in _bitItems)
                {
                    DataItem subDataItem = bi.GetValue(input[index], ref indexInByte);
                    di.DataItems.Add(subDataItem.Name, subDataItem);
                }
            }

            return di;
        }

        public override int GetRequiredDataLength()
        {
            return sizeof(byte);
        }

        public void AddBitItem(string name, int length)
        {
            if (length > 0)
            {
                if (_bitItems != null && _bitItems.Count > 0 && _bitItems.Exists(m => m.Name == name))
                {
                    throw new DuplicateItemException("Item exists, Same item name.");
                }

                if (CheckLength(length))
                {
                    BitItem bi = new BitItem(name, length);
                    _bitItems.Add(bi);
                }
                else
                {
                    throw new OutOfLengthLimitationException("The total length is big than size of byte.");
                }
            }
        }

        private bool CheckLength(int length)
        {
            bool valid = true;

            if (length > 8)
            {
                valid = false;
            }
            else
            {
                if (_bitItems != null && _bitItems.Count > 0)
                {
                    int totalLength = 0;
                    foreach (BitItem bi in _bitItems)
                    {
                        totalLength += bi.Length;
                    }

                    if (totalLength > 8)
                    {
                        valid = false;
                    }
                }
            }

            return valid;
        }

        private class BitItem
        {
            private int _length = 0;
            private string _name = string.Empty;

            public BitItem(string name, int length)
            {
                this._length = length;
                this._name = name;
            }

            public string Name { get { return _name; } set { _name = value; } }

            public int Length { get { return _length; } set { _length = value; } }

            public DataItem GetValue(byte input, ref int index)
            {
                int value = 0;

                for (int i = 0; i < _length; i++)
                {
                    int bit = (input & ((uint)1 << (8 - (index + i) - 1))) > 0 ? 1 : 0;
                    value += bit * (int)Math.Pow(2, _length - 1 - i);
                }

                index += _length;

                DataItem di = new DataItem(_name, value);
                return di;
            }
        }
    }
}
