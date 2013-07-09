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
using Octopus.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Octopus.Interpreter.FormatterFilters
{
    public abstract class FormatterFilterBase<T> : IFormatterFilter<T>
    {
        private string _name = string.Empty;

        private List<IFormatterFilter<T>> _filters = new List<IFormatterFilter<T>>();

        protected ILogWriter _logWriter = Logging.GetLogWriter();

        public FormatterFilterBase(string name)
        {
            _name = name;
        }

        public void AddNextFormatterFilter(IFormatterFilter<T> filter)
        {
            _filters.Add(filter);
        }

        public List<IFormatterFilter<T>> GetNextFormatterFilters()
        {
            return _filters;
        }

        public virtual bool Process(IFormatter<T> formatter, T input)
        {
            bool result = false;
            try
            {
                if (!IsMatch(formatter, input))
                {
                    if (!IsBreak)
                    {
                        if (_filters.Count > 0)
                        {
                            foreach (IFormatterFilter<T> filter in _filters)
                            {
                                bool subResult = filter.Process(formatter, input);
                                if (subResult)
                                {
                                    result = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    result = true; 
                }
            }
            catch (Exception ex)
            {
                _logWriter.Log("", ex);
            }

            return result;
        }

        protected abstract bool IsMatch(IFormatter<T> formatter, T input);

        public string Name
        {
            get { return _name; }
        }

        public bool IsBreak { get; set; }
    }
}
