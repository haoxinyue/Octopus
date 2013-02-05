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

using Octopus.Exceptions;
using Octopus.Interpreter.FormatterFilters;
using Octopus.Interpreter.Formatters;
using System;
using System.Collections.Generic;

namespace Octopus.Interpreter
{
    public class MultipleFormatterByteArrayInterpreter : ByteArrayInterpreter
    {
        private List<IFormatterFilter<byte[]>> _filters = new List<IFormatterFilter<byte[]>>();

        public MultipleFormatterByteArrayInterpreter(string name, Action<Envelop> notifyMessageCreated)
            : base(name, notifyMessageCreated) { }

        public MultipleFormatterByteArrayInterpreter(string name) : base(name) { }

        public override IFormatter<byte[]> GetMatchedFormatter(byte[] input)
        {
            if (_filters.Count == 0)
            {
                return _formatters[0];
            }

            foreach (IFormatter<byte[]> f in _formatters)
            {
                foreach (IFormatterFilter<byte[]> filter in _filters)
                {
                    bool result = filter.Process(f, input);
                    if (result)
                    {
                        return f;
                    }
                }
            }

            _logWriter.Log("Cannot found any formatter in this MultipleFormatterByteArrayInterpreter.");
            throw new FormatterNotFoundException("Cannot found any formatter in MultipleFormatterByteArrayInterpreter.");

        }

        public void AddFormatterFilter(IFormatterFilter<byte[]> filter)
        {
            _filters.Add(filter);
        }
    }
}
