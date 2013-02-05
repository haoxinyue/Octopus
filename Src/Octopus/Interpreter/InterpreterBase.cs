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
using Octopus.Channel;
using Octopus.Interpreter.Formatters;
using Octopus.Log;
using Octopus.Common;
using Octopus.Exceptions;
using System.Collections.ObjectModel;

namespace Octopus.Interpreter
{
    public abstract class InterpreterBase<Tinput> : IInterpreter<Tinput>
    {
        protected string _name = string.Empty;

        public string Name { get { return _name; } }

        protected ILogWriter _logWriter = Logging.GetLogWriter();

        protected List<IFormatter<Tinput>> _formatters = new List<IFormatter<Tinput>>();

        protected Action<Envelop> _notifyMessageCreated = null;

        public InterpreterBase(string name, Action<Envelop> notifyMessageCreated)
        {
            _notifyMessageCreated = notifyMessageCreated;
            _name = name;
        }

        public InterpreterBase(string name) : this(name, null) { }

        public virtual void Interprete(ChannelData<Tinput> input)
        {
            List<Message> output = InterpreteProcess(input);

            if (output != null && output.Count > 0)
            {
                Envelop e = new Envelop() { OriginalMessage = input.RawData, Messages = output, Address = input.RemoteEndPoint.Address.ToString(),  Port =input.RemoteEndPoint.Port, Timestamp = input.Timestamp };

                if (_notifyMessageCreated != null)
                {
                    try
                    {
                        _notifyMessageCreated(e);
                    }
                    catch (Exception ex)
                    {
                        _logWriter.Log("Invoke notifyMessageCreated action exception", ex);
                    }
                }
            }
            else
            {
                _logWriter.Log("No Message created");
            }
        }

        public abstract List<Message> InterpreteProcess(ChannelData<Tinput> input);

        public virtual void AddFormatter(IFormatter<Tinput> formatter)
        {
            Guard.NotNull(formatter, "formatter cannot be null");

            foreach (IFormatter<Tinput> f in _formatters)
            {
                if (f.Name == formatter.Name)
                {
                    throw new DuplicateFormatterException("Formatter with the same name exists");
                }
            }

            _formatters.Add(formatter);
            formatter.Interpreter = this;
        }

        public void DeleteFormatter(IFormatter<Tinput> formatter)
        {
            Guard.NotNull(formatter, "formatter cannot be null");

            _formatters.Remove(formatter);
        }

        public abstract IFormatter<Tinput> GetMatchedFormatter(Tinput input);

        public void AddMessageHandler(Action<Envelop> messageHandler)
        {
            Guard.NotNull(messageHandler, "messageHandler cannot be null");

            _notifyMessageCreated += messageHandler;
        }

        public ReadOnlyCollection<IFormatter<Tinput>> GetFormatters()
        {
            return new ReadOnlyCollection<IFormatter<Tinput>>(_formatters);
        }
    }
}
