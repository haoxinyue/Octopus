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

#region License
// Copyright (c) nick hao
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Collections.Concurrent;

namespace Octopus.Common.Communication.Tcp
{
    public class TcpServer : ITcpListener, IDisposable
    {
        private Socket _serverSocket;
        private int port;
        private Action<TcpConnectionInfo, byte[]> dataReceived;
        private Action<TcpConnectionInfo> clientConnected;
        private Action<TcpConnectionInfo, Exception> clientDisconnected;
        protected bool _isDisposed = false;

        public TcpServer(int port, Action<TcpConnectionInfo, byte[]> dataReceived, Action<TcpConnectionInfo> clientConnected, Action<TcpConnectionInfo, Exception> clientDisconnected)
        {
            this.port = port;
            this.dataReceived = dataReceived;
            this.clientConnected = clientConnected;
            this.clientDisconnected = clientDisconnected;
        }

        private ConcurrentDictionary<string, TcpConnectionInfo> _connections = new ConcurrentDictionary<string, TcpConnectionInfo>();

        private void SetupServerSocket()
        {
            IPEndPoint myEndpoint = new IPEndPoint(IPAddress.Any, port);

            _serverSocket = new Socket(myEndpoint.Address.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);
            _serverSocket.Bind(myEndpoint);
            _serverSocket.Listen((int)SocketOptionName.MaxConnections);
        }

        public void Start()
        {
            SetupServerSocket();

            IsWorking = true;

            _serverSocket.BeginAccept(
                new AsyncCallback(AcceptCallback), _serverSocket);
        }

        private void AcceptCallback(IAsyncResult result)
        {
            TcpConnectionInfo connection = new TcpConnectionInfo();
            try
            {
                Socket s = (Socket)result.AsyncState;
                connection.Socket = s.EndAccept(result);
                connection.Buffer = new byte[255];
                connection.RemoteAddress = connection.Socket.RemoteEndPoint.ToString();
                connection.RemoteEndPoint = (IPEndPoint)connection.Socket.RemoteEndPoint;

                _connections[((IPEndPoint)connection.Socket.RemoteEndPoint).Address.ToString()] = connection;

                if (clientConnected != null)
                {
                    clientConnected(connection);
                }

                connection.Socket.BeginReceive(connection.Buffer, 0, connection.Buffer.Length, SocketFlags.None,
                    new AsyncCallback(ReceiveCallback), connection);

                _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), result.AsyncState);
            }
            catch (SocketException e)
            {
                CloseConnection(connection, e);
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                CloseConnection(connection, ex);
            }
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            TcpConnectionInfo connection = (TcpConnectionInfo)result.AsyncState;
            try
            {
                int bytesRead = connection.Socket.EndReceive(result);
                if (0 != bytesRead)
                {
                    if (dataReceived != null)
                    {
                        byte[] data = new byte[bytesRead];

                        Buffer.BlockCopy(connection.Buffer, 0, data, 0, bytesRead);

                        dataReceived(connection, data);
                    }

                    connection.Socket.BeginReceive(connection.Buffer, 0,
                        connection.Buffer.Length, SocketFlags.None,
                        new AsyncCallback(ReceiveCallback), connection);
                }
                else
                {
                    CloseConnection(connection, null);
                }
            }
            catch (SocketException e)
            {
                CloseConnection(connection, e);
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                CloseConnection(connection, ex);
            }
        }

        private void CloseConnection(TcpConnectionInfo ci, Exception ex)
        {

            if (ci != null && ci.Socket != null)
            {
                try
                {
                    TcpConnectionInfo removed = null;
                    _connections.TryRemove(ci.RemoteEndPoint.Address.ToString(), out removed);

                    ci.Socket.Close();

                }
                catch { }

                if (clientDisconnected != null)
                {
                    clientDisconnected(ci, ex);
                }
            }
        }

        public int GetClientCount()
        {
            return _connections.Count;
        }

        public void Stop()
        {
            try
            {
                if (_serverSocket != null)
                {
                    try
                    {
                        _serverSocket.Close();
                    }
                    catch { }
                }

                foreach (var item in _connections)
                {
                    CloseConnection(item.Value, null);
                }
                _connections.Clear();

                IsWorking = false;
            }
            catch { }
        }

        public bool IsWorking { get; set; }


        public void Send(IPEndPoint endPoint, byte[] data)
        {
            if (endPoint != null)
            {
                if (_connections.ContainsKey(endPoint.Address.ToString()))
                {
                    TcpConnectionInfo connection = _connections[endPoint.Address.ToString()];
                    try
                    {
                        connection.Socket.Send(data);
                    }
                    catch (SocketException e)
                    {
                        CloseConnection(connection, e);
                    }
                    catch (ObjectDisposedException) { }
                    catch (Exception)
                    {

                    }
                }
            }
            else
            {
                foreach (var connection in _connections)
                {
                    try
                    {
                        connection.Value.Socket.Send(data);
                    }
                    catch (SocketException e)
                    {
                        CloseConnection(connection.Value, e);
                    }
                    catch (ObjectDisposedException) { }
                    catch (Exception)
                    {

                    }
                }
            }
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
                    Stop();
                }
            }

            _isDisposed = true;
        }
    }
}
