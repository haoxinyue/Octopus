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
using System.Threading.Tasks;
using log4net;

namespace Octopus.Log
{
    public class Log4NetLogWriter : ILogWriter
    {
        public Log4NetLogWriter()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        public void Log(object obj)
        {
            Log(traceLogger, obj.ToString());
        }

        public void Log(object obj, Exception exception)
        {
            string message = obj.ToString() + " Exception Occured：" + exception.Message;
            if (exception.Source != null)
            {
                message += "\nException Source：" + exception.Source;
            }
            Exception ex = exception.InnerException;
            while (ex != null)
            {
                message += "\nInner Exception：" + ex.Message;
                ex = ex.InnerException;
            }
            if (exception.StackTrace != null)
            {
                message += "\n" + exception.StackTrace;
            }
            Log(exceptionLogger, message);
        }

        public void LogTrace(object message)
        {
            Log(traceLogger, message);
        }

        private void Log(ILog logger, Object message)
        {
            try
            {
                logger.Info(message);
            }
            catch { }
        }

        private static ILog traceLogger = LogManager.GetLogger("TraceLogger");
        private static ILog exceptionLogger = LogManager.GetLogger("ExceptionLogger");
    }
}
