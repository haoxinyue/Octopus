using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Octopus.Log;
using Octopus.Interpreter.Formatters;
using Octopus.Channel;
using System.Collections.ObjectModel;
using Octopus.Common;

namespace Octopus.Interpreter
{
    public abstract class EmptyInterpreter<Tinput> : IInterpreter<Tinput>
    {
        protected string _name = string.Empty;

        public string Name { get { return _name; } }

        protected ILogWriter _logWriter = Logging.GetLogWriter();

        protected List<IFormatter<Tinput>> _formatters = new List<IFormatter<Tinput>>();

        protected Action<Envelop> _notifyMessageCreated = null;

        public EmptyInterpreter(string name, Action<Envelop> notifyMessageCreated)
        {
            _notifyMessageCreated = notifyMessageCreated;
            _name = name;
        }

        public EmptyInterpreter(string name) : this(name, null) { }

        public void Interprete(ChannelData<Tinput> input)
        {
            List<Message> output = InterpreteProcess(input);

            if (output != null && output.Count > 0)
            {
                Envelop e = new Envelop() { OriginalMessage = input.RawData, Messages = output, Address = input.RemoteEndPoint.Address.ToString(), Port = input.RemoteEndPoint.Port, Timestamp = input.Timestamp };

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
                //_logWriter.Log("No Message created");
            }
        }

        public abstract List<Message> InterpreteProcess(ChannelData<Tinput> input);

        public void AddFormatter(IFormatter<Tinput> formatter) { }

        public void DeleteFormatter(IFormatter<Tinput> formatter){}

        public IFormatter<Tinput> GetMatchedFormatter(Tinput input)
        {
            return null;
        }

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
