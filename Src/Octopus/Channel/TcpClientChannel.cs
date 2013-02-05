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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Octopus.Channel
{
    public class TcpClientChannel : ChannelBase<byte[]>
    {
        private TcpClient _tcpClient = null;
        private IPEndPoint _remoteIPEndPoint = null;
        private NetworkStream _ns = null;
        private byte[] _buffer = new byte[1024];

        // if exception, reconnect to the tcp server every _retryIntervalInS seconds
        private int _retryIntervalInS = 10;

        public TcpClientChannel(string name, IPEndPoint remoteIPEndPoint) : this(name, remoteIPEndPoint, 0) { }

        public TcpClientChannel(string name, IPEndPoint remoteIPEndPoint, int retryIntervalInS) : this(name, remoteIPEndPoint, retryIntervalInS, null) { }

        public TcpClientChannel(string name, IPEndPoint remoteIPEndPoint, int retryIntervalInS, Action<ChannelData<byte[]>> channelDataReceivedAction)
            : this(name, remoteIPEndPoint, retryIntervalInS, channelDataReceivedAction, null, null) { }

        public TcpClientChannel(string name, IPEndPoint remoteIPEndPoint, int retryIntervalInS, Action<ChannelData<byte[]>> channelDataReceivedAction, Action<IPEndPoint> tcpServerConnected, Action<IPEndPoint, string> tcpServerDisconnected)
            : base(name, channelDataReceivedAction, tcpServerConnected, tcpServerDisconnected)
        {
            _remoteIPEndPoint = remoteIPEndPoint;
            _retryIntervalInS = retryIntervalInS;
        }

        public override void Bind()
        {
            try
            {
                _tcpClient = new TcpClient();
                TryToConnect();
            }
            catch (SocketException e)
            {
                _logWriter.Log("TcpClient Bind function socketexception", e);
                throw;
            }
            catch (Exception ex)
            {
                _logWriter.Log("TcpClient Bind function exception", ex);
                throw;

            }
        }

        private void TryToConnect()
        {
            _logWriter.Log(string.Format("TcpClient try to connect host:{0} port:{1}", _remoteIPEndPoint.Address.ToString(), _remoteIPEndPoint.Port));

            _tcpClient.BeginConnect(_remoteIPEndPoint.Address, _remoteIPEndPoint.Port, new AsyncCallback(ConnectCallback), _tcpClient);
        }

        private void TryToConnectAfterException()
        {
            if (_retryIntervalInS > 0 && !_tcpClient.Connected)
            {
                Thread.Sleep(_retryIntervalInS * 1000);
                TryToConnect();
            }
        }

        public override void UnBind()
        {
            try
            {
                if (_ns != null)
                {
                    _ns.Close();
                }
                if (_tcpClient != null)
                {
                    _tcpClient.Close();
                }
            }
            catch (SocketException) { }
            catch (Exception ex)
            {
                _logWriter.Log("TcpClient UnBind function exception", ex);
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            TcpClient tcpClient = (TcpClient)ar.AsyncState;
            try
            {
                tcpClient.EndConnect(ar);

                OnConnected(_remoteIPEndPoint);

                _logWriter.Log(string.Format("TcpClient connection established, host:{0} port:{1}", _remoteIPEndPoint.Address.ToString(), _remoteIPEndPoint.Port));

                _ns = tcpClient.GetStream();
                _ns.BeginRead(_buffer, 0, _buffer.Length, new AsyncCallback(ReadCallback), _ns);
            }
            catch (SocketException e)
            {
                OnDisconnected(_remoteIPEndPoint, e.Message);

                _logWriter.Log("TcpClient ConnectCallback function socket exception", e);
                TryToConnectAfterException();
            }
            catch (Exception ex)
            {
                _logWriter.Log("TcpClient ConnectCallback function exception", ex);
            }
        }

        private void ReadCallback(IAsyncResult ar)
        {
            try
            {
                NetworkStream ns = (NetworkStream)ar.AsyncState;
                int receivedBytesCount = ns.EndRead(ar);

                if (receivedBytesCount != 0)
                {
                    byte[] receivedBytes = new byte[receivedBytesCount];
                    Buffer.BlockCopy(_buffer, 0, receivedBytes, 0, receivedBytesCount);

                    ns.BeginRead(_buffer, 0, _buffer.Length, new AsyncCallback(ReadCallback), ns);

                    NotifyRawDataReceived(receivedBytes);
                }
            }
            catch (SocketException e)
            {
                OnDisconnected(_remoteIPEndPoint, e.Message);

                _logWriter.Log("TcpClient ReadCallback function socket exception", e);
                TryToConnectAfterException();
            }
            catch (Exception ex)
            {
                _logWriter.Log("TcpClient ReadCallback function exception", ex);
            }
        }

        private void NotifyRawDataReceived(byte[] rawData)
        {
            if (_channelDataReceivedAction != null)
            {
                ChannelData<byte[]> channelData = new ChannelData<byte[]>(rawData, _remoteIPEndPoint, DateTime.Now);
                _channelDataReceivedAction(channelData);
            }
        }

        public override void SendData(ChannelData<byte[]> channelData)
        {
            try
            {
                if (_ns != null && _ns.CanWrite)
                {
                    _ns.Write(channelData.RawData, 0, channelData.RawData.Length);
                }
            }
            catch (Exception ex)
            {
                _logWriter.Log("TcpClient SendData function exception", ex);
            }
        }
    }
}
