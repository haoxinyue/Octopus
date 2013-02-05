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
using Octopus.Interpreter;
using System;

namespace Octopus.Adapter
{
    public class StringAdapter : AdapterBase<string>
    {
        public StringAdapter(string name, IChannel<string> channel, IInterpreter<string> interpreter) : base(name, channel, interpreter) { }

        public StringAdapter(string name) : this(name, null, null) { }

        public StringAdapter(string name, IChannel<string> channel, IInterpreter<string> interpreter, Action<Envelop> messageHandler)
            : base(name, channel, interpreter, messageHandler) { }
    }
}
