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
using System.Threading.Tasks;

namespace Octopus.Interpreter.Items
{
    public class ByteArrayLoopItem : ValueItem<byte[], DataItem>
    {
        private ByteArrayCompositeValueItem _byteArrayCompositeValueItem = null;

        private int _formattedDataLength = 0;

        public ByteArrayLoopItem(string name, ByteArrayCompositeValueItem byteArrayCompositeValueItem) : base(name)
        {
            _byteArrayCompositeValueItem = byteArrayCompositeValueItem;
        }

        public ByteArrayLoopItem(string name, short sortIndex, ByteArrayCompositeValueItem byteArrayCompositeValueItem) : base(name, sortIndex)
        {
            _byteArrayCompositeValueItem = byteArrayCompositeValueItem;
        }

        public override int GetRequiredDataLength()
        {
            return _byteArrayCompositeValueItem.GetRequiredDataLength();
        }

        public override DataItem GetValue(byte[] input, int index)
        {
            _formattedDataLength = 0;

            DataItem dataItem = new DataItem(_name, null);

            int loopCount = (input.Length - index) / _byteArrayCompositeValueItem.GetRequiredDataLength();
            int currentIndex = index;

            while (loopCount-- > 0)
            {
                DataItem dataItemChild = _byteArrayCompositeValueItem.GetValue(input, currentIndex);

                dataItem.AddDataItem(dataItemChild);

                currentIndex += _byteArrayCompositeValueItem.GetFormattedDataLength();

                _formattedDataLength += _byteArrayCompositeValueItem.GetFormattedDataLength();
            }

            return dataItem;
        }

        public override int GetFormattedDataLength()
        {
            return _formattedDataLength;
        }
    }
}
