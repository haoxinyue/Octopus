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
using Octopus.Command;
using Octopus.Log;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Octopus.Activator
{
    public abstract class ActivatorBase : IActivator
    {
        protected ILogWriter _logWriter = Logging.GetLogWriter();

        protected List<ICommand> _commandList = new List<ICommand>();

        protected List<IPEndPoint> _targetList = new List<IPEndPoint>();

        protected string _name = string.Empty;

        public string Name { get { return _name; } }

        protected object _syncObject = new object();

        public ActivatorBase(string name)
        {
            _name = name;
        }

        public virtual void SendToTarget(IPEndPoint endPoint)
        {
            lock (_syncObject)
            {
                if (!_targetList.Exists((m) => m.Address.ToString() == endPoint.Address.ToString() && m.Port == endPoint.Port))
                {
                    _targetList.Add(endPoint);
                }
            }
        }

        public virtual void RemoveTarget(IPEndPoint endPoint)
        {
            lock (_syncObject)
            {
                for (int i = _targetList.Count; i >= 0; i--)
                {
                    if (_targetList[i].Address.ToString() == endPoint.Address.ToString() && _targetList[i].Port == endPoint.Port)
                    {
                        _targetList.RemoveAt(i);
                    }
                }
            }
        }

        public virtual void ExecuteCommand(IPEndPoint endPoint)
        {
            foreach (ICommand command in _commandList)
            {
                try
                {
                    command.Execute(endPoint);
                }
                catch (Exception ex)
                {
                    _logWriter.Log(this.GetType().Name + " ExecuteCommand function exception.", ex);
                }
            }
        }

        public void AddCommand(ICommand command)
        {
            _commandList.Add(command);
        }
    }
}
