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
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Octopus.Channel
{
    public class HttpReaderChannel : ChannelBase<string>
    {
        protected string _ipHost = string.Empty;
        protected int _port = 0;
        protected string _userName = string.Empty;
        protected string _password = string.Empty;
        protected string _requestUrl = string.Empty;

        protected IPEndPoint _hostIPEndpoint = null;
        protected Thread _thread = null;

        protected int _exceptionIntervalInMS = 10 * 1000;
        protected int _requestIntervalInMS = 5 * 1000;

        protected bool _stopRequest = false;

        public int ExceptionIntervalInMS
        {
            get { return _exceptionIntervalInMS; }
            set { _exceptionIntervalInMS = value; }
        }

        public int RequestIntervalInMS
        {
            get { return _requestIntervalInMS; }
            set { _requestIntervalInMS = value; }
        }


        public HttpReaderChannel(string name, string ipHost, int port, string requestUrl, string userName, string password, Action<ChannelData<string>> channelDataReceivedAction, Action<IPEndPoint> hostConnected, Action<IPEndPoint, string> hostDisconnected)
            : base(name, channelDataReceivedAction, hostConnected, hostDisconnected)
        {
            _ipHost = ipHost;
            _port = port;
            _userName = userName;
            _password = password;
            _requestUrl = requestUrl;
            _hostIPEndpoint = new IPEndPoint(IPAddress.Parse(ipHost), port);
        }

        public HttpReaderChannel(string name, string ipHost, int port, string requestUrl, Action<ChannelData<string>> channelDataReceivedAction, Action<IPEndPoint> hostConnected, Action<IPEndPoint, string> hostDisconnected)
            : this(name, ipHost, port, requestUrl, string.Empty, string.Empty, channelDataReceivedAction, hostConnected, hostDisconnected) { }

        public HttpReaderChannel(string name, string ipHost, int port, string requestUrl, string userName, string password)
            : this(name, ipHost, port, requestUrl, userName, password, null, null, null) { }

        public override void Bind()
        {
            Task.Factory.StartNew(() => ConnectToHost());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        private void ConnectToHost()
        {
            string url = _requestUrl;
            string address = _ipHost + ":" + _port;

            while (!_stopRequest)
            {
                try
                {
                    string responseLine = null;
                    HttpWebResponse response = null;

                    while (response == null && !_stopRequest)
                    {
                        WebRequest request = WebRequest.Create(url);
                        request.ContentType = "text/plain";
                        request.Timeout = 10 * 1000;    // 10 seconds in milliseconds

                        response = (HttpWebResponse)request.GetResponse();

                        if (response != null)
                        {
                            Task.Factory.StartNew(() => OnConnected(_hostIPEndpoint));

                            using (Stream dataStream = response.GetResponseStream())
                            {
                                using (StreamReader reader = new StreamReader(dataStream, _currentEncoding))
                                {
                                    reader.BaseStream.ReadTimeout = 10 * 1000;
                                    while (!reader.EndOfStream && !_stopRequest)
                                    {
                                        responseLine = reader.ReadLine();
                                        if (responseLine != "" && _channelDataReceivedAction != null)
                                        {
                                            ChannelData<string> channelData = new ChannelData<string>(responseLine, _hostIPEndpoint, DateTime.Now);
                                            _channelDataReceivedAction(channelData);
                                        }
                                    }
                                }
                            }
                            response = null;
                        }

                        Thread.Sleep(_requestIntervalInMS);
                    }
                }
                catch (Exception ex)
                {
                    OnDisconnected(_hostIPEndpoint, ex.Message);

                    _logWriter.Log("HttpReaderChannel ConnectToHost function exception", ex);

                    Thread.Sleep(_exceptionIntervalInMS);
                }
            }

            Task.Factory.StartNew(() => OnDisconnected(_hostIPEndpoint, string.Empty));
        }

        public override void UnBind()
        {
            _stopRequest = true;
        }

        public override void SendData(ChannelData<string> channelData)
        {

        }
    }
}
