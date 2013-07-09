using System;
using Octopus.Config;
using Octopus.Log4Net;

namespace Octopus.Sample
{


    class Program
    {
        static int count = 0;

        static void Main(string[] args)
        {
            OctopusConfig oc = new OctopusConfig();
            oc.UseLog4Net();

            #region TcpServer

            #region TcpServer.TestCase1

            //TcpServer.TestCase1 case1 = new TcpServer.TestCase1();
            //case1.RunTest();

            #endregion

            #region TcpServer.TestCase2

            //TcpServer.TestCase2 case2 = new TcpServer.TestCase2();
            //case2.RunTest();

            #endregion

            #region TcpServer.TestCase3

            //TcpServer.TestCase3 case3 = new TcpServer.TestCase3();
            //case3.RunTest();

            #endregion

            #region TcpServer.TestCase5

            //TcpServer.TestCase5 case5 = new TcpServer.TestCase5();
            //case5.RunTest();

            #endregion

            #region TcpServer.TestCase6

            //TcpServer.TestCase6 case6 = new TcpServer.TestCase6();
            //case6.RunTest();

            #endregion

            #region TcpServer.TestCase7

            //TcpServer.TestCase7 case7 = new TcpServer.TestCase7();
            //case7.RunTest();

            #endregion

            #region TcpServer.TestCase8

            //TcpServer.TestCase8 case8 = new TcpServer.TestCase8();
            //case8.RunTest();

            #endregion

            #region TcpServer.TestCase9

            //TcpServer.TestCase9 case9 = new TcpServer.TestCase9();
            //case9.RunTest();

            #endregion

            #region TcpServer.TestCase10

            //TcpServer.TestCase10 case10 = new TcpServer.TestCase10();
            //case10.RunTest();

            #endregion

            #endregion
        }

        public static void ShowEnvelop(Envelop envelop)
        {
            if (envelop != null)
            {
                foreach (Message message in envelop.Messages)
                {
                    Console.WriteLine("---------------------------------------------------------------------");
                    Console.WriteLine("Receive a message");
                    Console.WriteLine("Name:" + message.MessageType);
                    Console.WriteLine("Address:" + envelop.Address);
                    Console.WriteLine("Port:" + envelop.Port);
                    foreach (var subItem in message.DataItems)
                    {
                        PrintItemValue(subItem.Value);
                    }

                    Console.WriteLine("---------------------------------------------------------------------");
                }
            }
        }


        public static void PrintItemValue(DataItem item)
        {
            if (item.Value != null)
            {
                Console.WriteLine("Item Key:" + item.Name + " Item Value:" + item.Value.ToString());
            }

            if (item.HasItems)
            {
                foreach (var subItem in item.DataItems)
                {
                    PrintItemValue(subItem.Value);
                }
            }
        }

    }
}
