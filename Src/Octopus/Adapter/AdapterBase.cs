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

using Octopus.Activator;
using Octopus.Channel;
using Octopus.Common;
using Octopus.Interpreter;
using Octopus.Log;
using System;
using System.Collections.Generic;
using System.Net;

namespace Octopus.Adapter
{
    public abstract class AdapterBase<T> : IAdapter
    {
        protected IChannel<T> _channel = null;

        protected IInterpreter<T> _interpreter = null;

        protected Action<Envelop> _messageHandler = null;

        protected List<IActivator> _activators = new List<IActivator>();

        protected ILogWriter _logWriter = Logging.GetLogWriter();

        public string Name { get; set; }

        public event Action SetupFinished;

        public IChannel<T> Channel
        {
            get { return _channel; }
            set { _channel = value; }
        }

        public IInterpreter<T> Interpreter
        {
            get { return _interpreter; }
            set { _interpreter = value; }
        }

        public Action<Envelop> MessageHandler
        {
            get { return _messageHandler; }
        }

        public AdapterBase(string name, IChannel<T> channel, IInterpreter<T> interpreter)
        {
            Name = name;
            _channel = channel;
            _interpreter = interpreter;
        }

        public AdapterBase(string name) : this(name, null, null) { }

        public AdapterBase(string name, IChannel<T> channel, IInterpreter<T> interpreter, Action<Envelop> messageHandler)
        {
            Name = name;
            _channel = channel;
            _interpreter = interpreter;
            _messageHandler += messageHandler;
        }

        public virtual void Setup()
        {
            Guard.NotNull(_channel, "Channel cannot be null");
            Guard.NotNull(_interpreter, "Interpreter cannot be null");

            _channel.SubscribeRawDataReceived(_interpreter.Interprete);
            _interpreter.AddMessageHandler(_messageHandler);
            _channel.Bind();

            if (SetupFinished != null)
            {
                SetupFinished();
            }
        }

        public virtual void Close()
        {
            _channel.UnSubscribeRawDataReceived(_interpreter.Interprete);

            _messageHandler = null;

            if (_channel != null)
            {
                _channel.UnBind();
            }
        }

        /// <summary>
        /// sync 
        /// </summary>
        /// <param name="message"></param>
        public void InvokeMessageHandler(Envelop envelop)
        {
            if (_messageHandler != null)
            {
                _messageHandler(envelop);
            }
            else
            {
                _logWriter.Log("Envelop created, but there is no MessageHandler");
            }
        }

        public void AddActivator(IActivator activator)
        {
            Guard.NotNull(activator, "Activator cannot be null");

            _activators.Add(activator);
        }

        public virtual void SendData(object data, IPEndPoint endPoint)
        {
            Guard.NotNull(data, "data cannot be null");
            Guard.NotNull(endPoint, "endPoint cannot be null");

            ChannelData<T> channelData = new ChannelData<T>((T)data, endPoint, DateTime.Now);
            _channel.SendData(channelData);
        }

        public object GetChannel()
        {
            return _channel;
        }

        public object GetInterpreter()
        {
            return _interpreter;
        }

        public void AddEnvelopHandler(Action<Envelop> envelopHandler)
        {
            _messageHandler += envelopHandler;
        }

        public void RemoveEnvelopHandler(Action<Envelop> envelopHandler)
        {
            _messageHandler -= envelopHandler;
        }
    }
}
