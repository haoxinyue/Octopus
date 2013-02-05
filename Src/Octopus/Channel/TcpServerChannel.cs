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
using Octopus.Common.Communication.Tcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Octopus.Channel
{
    public class TcpServerChannel : ChannelBase<byte[]>
    {
        private ITcpListener _TcpListener = null;
        private int _listenPort = 0;

        public TcpServerChannel(string name, int listenPort, Action<ChannelData<byte[]>> channelDataReceivedAction)
            : this(name, listenPort, channelDataReceivedAction, null, null) { }

        public TcpServerChannel(string name, int listenPort) : this(name, listenPort, null, null, null) { }

        public TcpServerChannel(string name, int listenPort, Action<ChannelData<byte[]>> channelDataReceivedAction, Action<IPEndPoint> tcpClientConnected, Action<IPEndPoint, string> tcpClientDisconnected)
            : base(name, channelDataReceivedAction, tcpClientConnected, tcpClientDisconnected)
        {
            _listenPort = listenPort;
        }

        public override void Bind()
        {
            _TcpListener = new TcpServer(_listenPort, RawDataReceived, ClientConnected, ClientDisconnected);
            _TcpListener.Start();
        }

        public override void UnBind()
        {
            _TcpListener.Stop();
        }

        public override void SendData(ChannelData<byte[]> channelData)
        {
            _TcpListener.Send(channelData.RemoteEndPoint, channelData.RawData);
        }

        public void RawDataReceived(TcpConnectionInfo connection, byte[] data)
        {
            if (_channelDataReceivedAction != null)
            {
                ChannelData<byte[]> channelData = new ChannelData<byte[]>(data, connection.RemoteEndPoint, DateTime.Now);
                _channelDataReceivedAction(channelData);
            }
        }

        public void ClientConnected(TcpConnectionInfo connection)
        {
            OnConnected(connection.RemoteEndPoint);

            _logWriter.Log(string.Format("Remote tcp client connected. IP:{0}, Port:{1}", connection.RemoteEndPoint.Address.ToString(), connection.RemoteEndPoint.Port));
        }

        public void ClientDisconnected(TcpConnectionInfo connection, Exception ex)
        {
            if (ex != null)
            {
                OnDisconnected(connection.RemoteEndPoint, ex.Message);

                _logWriter.Log(string.Format("Remote tcp client disconnected. IP:{0}, Port:{1}, Reason:{2}", connection.RemoteEndPoint.Address.ToString(), connection.RemoteEndPoint.Port, ex.Message));
                _logWriter.Log("Tcp client disconnected", ex);
            }
            else
            {
                OnDisconnected(connection.RemoteEndPoint, string.Empty);

                _logWriter.Log(string.Format("Remote tcp client disconnected. IP:{0}, Port:{1}, Reason:{2}", connection.RemoteEndPoint.Address.ToString(), connection.RemoteEndPoint.Port, string.Empty));
            }
        }
    }
}
