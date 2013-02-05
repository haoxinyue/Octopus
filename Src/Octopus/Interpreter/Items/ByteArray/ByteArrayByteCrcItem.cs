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
    public class ByteArrayByteCrcItem : ByteArrayByteItem
    {
        private int _crcFromIndex = 0;
        private int _crcToIndex = 0;

        public ByteArrayByteCrcItem(string name, int crcFromIndex, int crcToIndex)
            : base(name)
        {
            _crcToIndex = crcToIndex;
            _crcFromIndex = crcFromIndex;
        }

        public ByteArrayByteCrcItem(string name, short sortIndex, int crcFromIndex, int crcToIndex)
            : base(name, sortIndex)
        {
            _crcToIndex = crcToIndex;
            _crcFromIndex = crcFromIndex;
        }

        public override DataItem GetValue(byte[] input, int index)
        {
            ushort crc = 0;
            for (int i = _crcFromIndex; i <= _crcToIndex; i++)
            {
                crc ^= input[i];
            }

            bool isMatch = crc == input[index];

            return new DataItem(_name, isMatch);
        }
    }
}
