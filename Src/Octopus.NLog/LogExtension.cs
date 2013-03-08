using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Octopus.Config;
using Octopus.Log;

namespace Octopus.NLog
{
    public static class LogExtension
    {
        public static void UseNLog(this OctopusConfig oc)
        {
            Logging.UseLogger(new NLogWriterFactory());
        }
    }
}
