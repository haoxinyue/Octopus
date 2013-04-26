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
    public class Message
    {
        protected string _messageType = string.Empty;

        private OrderedDictionary<string, DataItem> _dataItems = null;

        public string MessageType
        {
            get { return _messageType; }
        }

        public Message() { }

        public Message(string messageType) { _messageType = messageType; }

        public Message(string messageType, OrderedDictionary<string, DataItem> dataItems)
        {
            _messageType = messageType;
            _dataItems = dataItems;
        }

        public OrderedDictionary<string, DataItem> DataItems
        {
            get { return _dataItems; }
            set { _dataItems = value; }
        }
    }
}
