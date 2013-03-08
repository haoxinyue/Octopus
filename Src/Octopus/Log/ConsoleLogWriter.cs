using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Octopus.Log
{
    public class ConsoleLogWriter : ILogWriter
	{
        public void Log(object obj)
        {
            Console.WriteLine(obj.ToString());
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
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
