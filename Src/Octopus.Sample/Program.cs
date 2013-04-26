using System;
using System.Collections.Generic;
using System.Net.Sockets;
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

            #region RFID

            #region RFID.TestCase1

            //RFID.TestCase1 case1 = new RFID.TestCase1();
            //case1.RunTest();

            #endregion

            #endregion
        }

        static void Test2()
        {
            TcpClient tc = new TcpClient();

            tc.Connect("localhost", 9988);

            NetworkStream ns = tc.GetStream();



            List<byte> dataList = new List<byte>();

            dataList.Add(0xAA);
            dataList.Add(0x55);

            dataList.AddRange(BitConverter.GetBytes((short)123));
            dataList.AddRange(BitConverter.GetBytes(456));
            dataList.AddRange(BitConverter.GetBytes((Int64)456));
            dataList.AddRange(BitConverter.GetBytes((double)67854.987));

            byte[] data = dataList.ToArray();


            while (Console.ReadLine() != null)
            {
                if (count == 0)
                {
                    ns.Write(data, 0, 2);
                }
                else if (count == 1)
                {
                    ns.Write(data, 2, 4);
                }
                else if (count == 2)
                {
                    ns.Write(data, 6, 12);
                }
                else if (count == 3)
                {
                    ns.Write(data, 18, 6);
                }
                else if (count == 4)
                {
                    ns.Write(data, 0, data.Length);
                }

                count++;

                if (count > 4)
                {
                    count = 0;
                }
            }
        }

        static void Test3()
        {
            TcpClient tc = new TcpClient();

            tc.Connect("localhost", 9988);

            NetworkStream ns = tc.GetStream();

            List<byte> dataList = new List<byte>();

            dataList.Add(0xAA);
            dataList.Add(0x55);

            dataList.AddRange(BitConverter.GetBytes((short)8888));

            dataList.AddRange(BitConverter.GetBytes((short)1));
            dataList.AddRange(BitConverter.GetBytes(1456));
            dataList.AddRange(BitConverter.GetBytes((Int64)1456));
            dataList.AddRange(BitConverter.GetBytes((double)167854.987));

            dataList.AddRange(BitConverter.GetBytes((short)2));
            dataList.AddRange(BitConverter.GetBytes(2456));
            dataList.AddRange(BitConverter.GetBytes((Int64)2456));
            dataList.AddRange(BitConverter.GetBytes((double)267854.987));

            dataList.AddRange(BitConverter.GetBytes((short)3));
            dataList.AddRange(BitConverter.GetBytes(3456));
            dataList.AddRange(BitConverter.GetBytes((Int64)3456));
            dataList.AddRange(BitConverter.GetBytes((double)367854.987));

            dataList.Add(0x55);
            dataList.Add(0xAA);

            byte[] data = dataList.ToArray();


            while (Console.ReadLine() != null)
            {
                ns.Write(data, 0, data.Length);
            }
        }

        static void Test4()
        {
            List<byte> dataList = new List<byte>();

            dataList.Add(0xAA);
            dataList.Add(0x55);

            dataList.AddRange(BitConverter.GetBytes((short)8888));
            dataList.Add(1);

            dataList.AddRange(BitConverter.GetBytes((short)1));
            dataList.AddRange(BitConverter.GetBytes(1456));
            dataList.AddRange(BitConverter.GetBytes((Int64)1456));
            dataList.AddRange(BitConverter.GetBytes((double)167854.987));

            dataList.AddRange(BitConverter.GetBytes((short)2));
            dataList.AddRange(BitConverter.GetBytes(2456));
            dataList.AddRange(BitConverter.GetBytes((Int64)2456));
            dataList.AddRange(BitConverter.GetBytes((double)267854.987));

            dataList.AddRange(BitConverter.GetBytes((short)3));
            dataList.AddRange(BitConverter.GetBytes(3456));
            dataList.AddRange(BitConverter.GetBytes((Int64)3456));
            dataList.AddRange(BitConverter.GetBytes((double)367854.987));

            dataList.Add(0x55);
            dataList.Add(0xAA);

            dataList.Add(0xAA);
            dataList.Add(0x55);

            dataList.AddRange(BitConverter.GetBytes((short)8888));
            dataList.Add(2);

            dataList.AddRange(BitConverter.GetBytes((short)21));
            dataList.AddRange(BitConverter.GetBytes(21456));

            dataList.AddRange(BitConverter.GetBytes((short)22));
            dataList.AddRange(BitConverter.GetBytes(22456));

            dataList.AddRange(BitConverter.GetBytes((short)23));
            dataList.AddRange(BitConverter.GetBytes(23456));

            dataList.Add(0x55);
            dataList.Add(0xAA);

            byte[] data = dataList.ToArray();


            TcpClient tc = new TcpClient();

            tc.Connect("localhost", 9988);

            NetworkStream ns = tc.GetStream();

            while (Console.ReadLine() != null)
            {
                ns.Write(data, 0, data.Length);
            }
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
