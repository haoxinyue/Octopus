using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Octopus.Log
{
    public class ConsoleLogWriterFactory : ILogWriterFactory
    {
        public ILogWriter CreateLogWriter()
        {
            return new ConsoleLogWriter();
        }
    }
}
