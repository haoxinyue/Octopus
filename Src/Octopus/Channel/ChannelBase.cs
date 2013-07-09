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

using Octopus.Common;
using Octopus.Log;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Octopus.Channel
{
    public abstract class ChannelBase<T> : IChannel<T>
    {
        protected Action<ChannelData<T>> _channelDataReceivedAction = null;

        protected ILogWriter _logWriter = Logging.GetLogWriter();

        protected Encoding _currentEncoding = Encoding.ASCII;

        public event Action<IPEndPoint> Connected;

        public event Action<IPEndPoint, string> Disconnected;

        private bool _isDisposed = false;

        private string _name = string.Empty;

        public string Name { get { return _name; } }

        public abstract void Bind();

        public abstract void UnBind();

        public abstract void SendData(ChannelData<T> channelData);

        public ChannelBase(string name) : this(name, null) { }

        public ChannelBase(string name, Action<ChannelData<T>> channelDataReceivedAction) : this(name ,channelDataReceivedAction, null, null) { }

        public ChannelBase(string name, Action<ChannelData<T>> channelDataReceivedAction, Action<IPEndPoint> connected, Action<IPEndPoint, string> disconnected)
        {
            _channelDataReceivedAction += channelDataReceivedAction;
            Connected += connected;
            Disconnected += disconnected;
            _name = name;
        }

        public void SubscribeRawDataReceived(Action<ChannelData<T>> channelDataReceivedAction)
        {
            _channelDataReceivedAction += channelDataReceivedAction;
        }

        protected Encoding CurrentEncoding
        {
            get { return _currentEncoding; }
            set { _currentEncoding = value; }
        }

        public void UnSubscribeRawDataReceived(Action<ChannelData<T>> channelDataReceivedAction)
        {
            _channelDataReceivedAction -= channelDataReceivedAction;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool diposing)
        {
            if (!_isDisposed)
            {
                if (diposing)
                {
                    UnBind();
                }
            }

            _isDisposed = true;
        }

        protected virtual void OnConnected(IPEndPoint endPoint)
        {
            if (Connected != null)
            {
                Connected(endPoint);
            }
        }

        protected virtual void OnDisconnected(IPEndPoint endPoint, string reason)
        {
            if (Disconnected != null)
            {
                Disconnected(endPoint, reason);
            }
        }
    }
}
