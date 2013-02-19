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
using Octopus.Exceptions;
using Octopus.Interpreter.Formatters;
using System.Collections.Generic;

namespace Octopus.Interpreter
{
    public class StringInterpreter : InterpreterBase<string>
    {
        public StringInterpreter(string name) : base(name) { }

        public override List<Message> InterpreteProcess(ChannelData<string> input)
        {
            List<Message> messages = null;

            IFormatter<string> formatter = GetMatchedFormatter(input.RawData);

            int formattedDataLength = 0;
            Message m = formatter.Format(input.RawData, input.RemoteEndPoint, ref formattedDataLength);

            if (m != null)
            {
                messages = new List<Message>();
                messages.Add(m);
            }

            return messages;
        }

        public override IFormatter<string> GetMatchedFormatter(string input)
        {
            if (_formatters.Count > 0)
            {
                return _formatters[0];
            }
            else
            {
                _logWriter.Log("Cannot found any formatter in this StringInterpreter.");
                throw new FormatterNotFoundException("Cannot found any formatter in this StringInterpreter.");
            }
        }
    }
}
