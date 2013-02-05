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
    public class ByteArrayStringItem : ByteArrayValueItem
    {
        private int _byteCount = 0;

        private Encoding _encoding = Encoding.ASCII;

        public ByteArrayStringItem(string name, int byteCount, Encoding encoding)
            : base(name)
        {
            _byteCount = byteCount;
            _encoding = encoding;
        }

        public ByteArrayStringItem(string name, short sortIndex, int byteCount, Encoding encoding)
            : base(name, sortIndex)
        {
            _byteCount = byteCount;
            _encoding = encoding;
        }

        public override DataItem GetValue(byte[] input, int index)
        {
            return new DataItem(_name, _encoding.GetString(input, index, _byteCount));
        }

        public override int GetRequiredDataLength()
        {
            return _byteCount;
        }

        public override int GetFormattedDataLength()
        {
            return _byteCount;
        }
    }
}
