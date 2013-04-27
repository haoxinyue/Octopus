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


namespace Octopus.Interpreter.Items
{
    public class ByteArrayLoopItem : ValueItem<byte[], DataItem>
    {
        private ByteArrayCompositeValueItem _byteArrayCompositeValueItem = null;

        public ByteArrayLoopItem(string name, ByteArrayCompositeValueItem byteArrayCompositeValueItem)
            : base(name)
        {
            _byteArrayCompositeValueItem = byteArrayCompositeValueItem;
        }

        public ByteArrayLoopItem(string name, short sortIndex, ByteArrayCompositeValueItem byteArrayCompositeValueItem)
            : base(name, sortIndex)
        {
            _byteArrayCompositeValueItem = byteArrayCompositeValueItem;
        }

        public override int GetRequiredDataLength()
        {
            return _byteArrayCompositeValueItem.GetRequiredDataLength();
        }

        public override DataItem GetValue(byte[] input, int index, ref int formattedDataLength)
        {
            formattedDataLength = 0;

            DataItem dataItem = new DataItem(_name, null);

            int loopCount = (input.Length - index) / _byteArrayCompositeValueItem.GetRequiredDataLength();
            int currentIndex = index;
            int nameSuffix = 1;

            while (loopCount-- > 0)
            {
                int subFormattedDataLength = 0;
                DataItem dataItemChild = _byteArrayCompositeValueItem.GetValue(input, currentIndex, ref subFormattedDataLength);
                dataItemChild.Name = dataItemChild.Name + "_" + nameSuffix.ToString();
                dataItem.AddDataItem(dataItemChild);

                currentIndex += subFormattedDataLength;
                formattedDataLength += subFormattedDataLength;

                nameSuffix++;
            }

            return dataItem;
        }
    }
}
