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

using Octopus.Interpreter.Formatters;
using System;
namespace Octopus.Interpreter.FormatterFilters
{
    public class ByteArrayTypedFormatterFilter: FormatterFilterBase<byte[]>
    {
        private int _formatterTypeIndex = 0;

        public ByteArrayTypedFormatterFilter(string name, int formatterTypeIndex)
            : base(name)
        {
            _formatterTypeIndex = formatterTypeIndex;
        }

        protected override bool IsMatch(IFormatter<byte[]> formatter, byte[] input)
        {
            try
            {
                return input[_formatterTypeIndex] == ((ByteArrayFormatter)formatter).GetFormatterId();
            }
            catch (Exception ex)
            {
                _logWriter.Log("ByteArrayTypedFormatterFilter IsMatch function exception.", ex);
                return false;
            }
        }
    }
}
