using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Octopus.Config;
using Octopus.Log;

namespace Octopus.Log4Net
{
    public static class LogExtension
    {
        public static void UseLog4Net(this OctopusConfig oc)
        {
            Logging.UseLogger(new Log4NetLogWriterFactory());
        }
    }
}
