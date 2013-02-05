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

using Octopus.Common.Collection;
using Octopus.Common.ProducerConsumer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Octopus.Channel
{
    public class UdpChannel : ChannelBase<byte[]>
    {

        #region Fileds

        private byte[] _buffer = new byte[MAX_DATA];

        private EndPoint _remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

        private static object _queueLocker = new object();

        private Socket _socket;

        private int _sendPort;

        private int _receivePort;

        private const int MAX_DATA = 1024 * 20;

        private const int TIME_OUT = 10000;

        //private static readonly int SUSPEND_INTERVAL_FOR_START = 1000;

        private IProducerConsumerPattern _producerConsumerPattern = null;

        private ConcurrentQueue<ChannelData<byte[]>> _concurrentQueue;

        private IPAddress _bindAddress = IPAddress.Any;

        #endregion

        public UdpChannel(string name,int sendPort, int receivePort, Action<ChannelData<byte[]>> channelDataReceivedAction, IPAddress bindAddress = null)
            : base(name, channelDataReceivedAction)
        {
            _sendPort = sendPort;
            _receivePort = receivePort;
            _channelDataReceivedAction = channelDataReceivedAction;
            _bindAddress = bindAddress ?? IPAddress.Any;
        }

        public UdpChannel(string name, int sendPort, int receivePort, IPAddress bindAddress = null)
            : base(name, null)
        {
            _sendPort = sendPort;
            _receivePort = receivePort;
            _channelDataReceivedAction = null;
            _bindAddress = bindAddress ?? IPAddress.Any;
        }


        public override void Bind()
        {
            _logWriter.Log("Try to bind udp socket");

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            uint IOC_IN = 0x80000000;
            uint IOC_VENDOR = 0x18000000;
            uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
            _socket.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);

            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, 64 * 1024);
            _socket.ReceiveTimeout = TIME_OUT;

            IPAddress localAddress = _bindAddress;
            IPEndPoint localEndPoint = new IPEndPoint(localAddress, _receivePort);
            _socket.Bind(localEndPoint);

            _concurrentQueue = new ConcurrentQueue<ChannelData<byte[]>>();
            _producerConsumerPattern = new ProducerConsumerPattern<ChannelData<byte[]>>(_concurrentQueue, ProduceData, ConsumeData);

            _producerConsumerPattern.Start();

            _logWriter.Log("Udp socket start to receive data");
        }

        public override void UnBind()
        {
            try
            {
                _socket.Close();
            }
            catch { }

            if (_producerConsumerPattern != null)
            {
                _producerConsumerPattern.Dispose();
            }
        }

        public override void SendData(ChannelData<byte[]> channelData)
        {
            _socket.SendTo(channelData.RawData, channelData.RawData.Length, SocketFlags.None, channelData.RemoteEndPoint);
        }

        public void Close()
        {
            _socket.Close();
        }

        private ChannelData<byte[]> ProduceData()
        {
            ChannelData<byte[]> channelData = null;

            int receivedBytesCount = 0;
            try
            {
                receivedBytesCount = _socket.ReceiveFrom(_buffer, ref _remoteEndPoint);
            }
            catch { }

            if (receivedBytesCount > 0)
            {
                try
                {
                    byte[] receivedBytes = new byte[receivedBytesCount];
                    Buffer.BlockCopy(_buffer, 0, receivedBytes, 0, receivedBytesCount);
                    IPEndPoint currentEndPoint = new IPEndPoint(((IPEndPoint)_remoteEndPoint).Address, ((IPEndPoint)_remoteEndPoint).Port);
                    channelData = new ChannelData<byte[]>(receivedBytes, currentEndPoint, DateTime.Now);
                }
                catch (Exception ex) 
                {
                    _logWriter.Log("UdpChannel ProduceData exception", ex);
                }
            }

            return channelData;
        }

        private void ConsumeData(ChannelData<byte[]> channelData)
        {
            if (channelData != null && _channelDataReceivedAction != null)
            {
                _channelDataReceivedAction(channelData);
            }
        }

        public void Send(byte[] data, string ip)
        {
            IPEndPoint remote = new IPEndPoint(IPAddress.Parse(ip), _sendPort);
            _socket.SendTo(data, data.Length, SocketFlags.None, remote);
        }
    }
}
