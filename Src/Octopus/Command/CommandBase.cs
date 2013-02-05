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

using Octopus.Channel;
using Octopus.Log;
using System;
using System.Net;
namespace Octopus.Command
{
    public class CommandBase<T> : ICommand<T>
    {
        protected T _data = default(T);

        protected string _name = string.Empty;
        
        protected IChannel<T> _channel = null;

        protected ILogWriter _logWriter = Logging.GetLogWriter();

        public CommandBase(string name, IChannel<T> channel, T t)
        {
            _name = name;
            _data = t;
            _channel = channel;
        }

        public T GetData()
        {
            return _data;
        }

        public string Name
        {
            get { return _name; }
        }

        public void Execute(IPEndPoint endPoint)
        {
            try
            {
                ChannelData<T> channelData = new ChannelData<T>(GetData(), endPoint, DateTime.Now);
                _channel.SendData(channelData);
            }
            catch (Exception ex)
            {
                _logWriter.Log(this.GetType().Name + " Execute function exception.", ex);
            }
        }
    }
}
