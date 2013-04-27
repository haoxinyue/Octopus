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

using Octopus.Common.Collection;

namespace Octopus
{
    public class DataItem
    {
        private OrderedDictionary<string, DataItem> _dataItems = new OrderedDictionary<string, DataItem>();
        private string _name = string.Empty;
        private object _value = null;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public object Value
        {
            get { return _value; }
        }

        public OrderedDictionary<string, DataItem> DataItems
        {
            get { return _dataItems; }
            set { _dataItems = value; }
        }

        public DataItem(string name, object value)
        {
            _name = name;
            _value = value;
        }

        public void AddDataItem(DataItem item)
        {
            _dataItems[item.Name] = item;
        }

        public bool HasItems
        {
            get { return _dataItems != null && _dataItems.Count > 0; }
        }
    }
}
