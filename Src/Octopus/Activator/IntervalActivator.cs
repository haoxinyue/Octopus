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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading;

namespace Octopus.Activator
{
    public class IntervalActivator: ActivatorBase
    {
        private Thread _thread = null;
        private int _commandExecuteIntervalInMS = 10000;
        private bool _stopRequest = false;

        public IntervalActivator(string name, int commandExecuteIntervalInMS = 10000)
            : base(name)
        {
            _commandExecuteIntervalInMS = commandExecuteIntervalInMS;
        }

        public void StopProcess()
        {
            _stopRequest = true;
        }

        public void StartProcess()
        {
            _thread = new Thread(() => DoWork());
            _thread.Start();
        }

        private void DoWork()
        {
            while (!_stopRequest)
            {
                foreach (ICommand command in _commandList)
                {
                    if (_targetList.Count > 0)
                    {
                        lock (_syncObject)
                        {
                            foreach (IPEndPoint endPoint in _targetList)
                            {
                                ExecuteCommand(endPoint);
                            }
                        }
                    }
                    else//send to all
                    {
                        ExecuteCommand(null);
                    }
                }

                Thread.Sleep(_commandExecuteIntervalInMS);
            }
        }
    }
}
