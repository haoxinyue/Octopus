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
    public class StringFormatter : FormatterBase<string>
    {
        public StringFormatter(string name) : base(name) { }

        public override int GetFormatterRequiredDataLength()
        {
            int length = 0;

            foreach (Item item in _items)
            {
                length += item.GetRequiredDataLength();
            }

            return length;
        }

        protected override Message FormatProcess(string input, ref int formattedDataLength)
        {
            Message m = null;
            formattedDataLength = 0;
            try
            {
                SortedDictionary<string, DataItem> dict = new SortedDictionary<string, DataItem>(DataItemSorter.Instance);
                foreach (Item item in _items)
                {
                    int subFormattedDataLength = 0;

                    if (item is SimpleStringValueItem)
                    {
                        DataItem dataItem = ((SimpleStringValueItem)item).GetValue(input, 0, ref subFormattedDataLength);

                        if (dataItem.Value != null)
                        {
                            dict.Add(dataItem.Name, dataItem);
                        }
                    }

                    subFormattedDataLength += subFormattedDataLength;
                }

                m = new Message(_name, dict);
            }
            catch (Exception ex)
            {
                _logWriter.Log("SimpleStringFormatter FormatProcess function exception", ex);
            }

            return m;
        }
    }
}
