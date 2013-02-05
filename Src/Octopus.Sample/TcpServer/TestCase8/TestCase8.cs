using Octopus.Activator;
using Octopus.Adapter;
using Octopus.Channel;
using Octopus.Command;
using Octopus.Config;
using Octopus.Interpreter;
using Octopus.Interpreter.Formatters;
using Octopus.Interpreter.Items;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Octopus.Sample.TcpServer
{
    /// <summary>
    /// Test Case 8 is a case that use activator
    /// some time device will not send data automatically, we should send a command to activator it
    /// 
    /// ******************************************************************************
    /// Bytes Format:
    /// 
    ///             ------------------------------------
    /// index：0     |     Header 0x55 0xAA (2 bytes)   |
    ///             ------------------------------------
    /// index：1     |     Node Id (4 bytes)            |
    ///             ------------------------------------
    /// index：2     |     Node Name    (6 bytes)       |
    ///             ------------------------------------
    /// index：4     |     CustomItem (10 bytes)        |
    ///             ------------------------------------
    /// index：5     |     Reserved  (1 byte)           |
    ///             ------------------------------------
    /// index：6     |     Tailer 0xAA 0x55  (2 bytes)  |
    ///             ------------------------------------
    /// ******************************************************************************
    /// </summary>
    public class TestCase8
    {
        public void RunTest()
        {
            Task startServerTask = new Task(() => StartServerWithConfig());
            startServerTask.Start();
            startServerTask.Wait();

            StartClient();
        }

        public void StartServer()
        {
            TcpServerChannel tcpServerChannel = new TcpServerChannel("TcpServerChannel", 9988);

            IActivator activator1 = new OneTimeActivator("Activator1");

            byte[] command1Bytes = new byte[] { 0x01, 0x16 };
            activator1.AddCommand(new ByteArrayCommand("command1", tcpServerChannel, command1Bytes));

            SingleFormatterByteArrayInterpreter interpreter = new SingleFormatterByteArrayInterpreter("Interpreter");
            interpreter.SetHeaders(new byte[] { 0x55, 0xAA });
            interpreter.SetTailers(new byte[] { 0xAA, 0x55 });

            ByteArrayFormatter formatter = new ByteArrayFormatter("Formatter1");

            formatter.AddItem(new ByteArrayStringItem("NodeName", 1, 6, Encoding.ASCII));
            formatter.AddItem(new ByteArrayInt32Item("NodeId", 0));
            formatter.AddItem(new ByteArrayInt16Item("Temperature", 2));
            formatter.AddItem(new ByteArrayDoubleItem("Longitude", 3));
            formatter.AddItem(new ByteArrayByteItem("Reserved", 4));

            interpreter.AddFormatter(formatter);

            tcpServerChannel.Connected += activator1.SendToTarget;

            ByteArrayAdapter baa = new ByteArrayAdapter("ByteArrayAdapter", tcpServerChannel, interpreter, Program.ShowEnvelop);
            baa.Setup();
        }

        public void StartServerWithConfig()
        {
            OctopusConfig oc = OctopusConfig.Load("TcpServer\\TestCase8\\TestCase8.xml");

            foreach (var item in oc.Adapters)
            {
                item.Value.AddEnvelopHandler(Program.ShowEnvelop);
                item.Value.Setup();
            }
        }

        public void StartClient()
        {
            TcpClient tc = new TcpClient();

            tc.Connect("localhost", 9988);

            NetworkStream ns = tc.GetStream();

            List<byte> dataList = new List<byte>();

           byte[] buffer = new byte[1024];
           int count = ns.Read(buffer, 0, 1024);

           if (buffer[0] == 0x01 && buffer[1] == 0x16)
           {
               dataList.AddRange(new byte[] { 0x55, 0xAA });

               //Node Id
               dataList.AddRange(BitConverter.GetBytes(1001));
               //Node Name
               dataList.AddRange(Encoding.ASCII.GetBytes("NK1001"));
               //Temperature
               dataList.AddRange(BitConverter.GetBytes((short)37));
               //Longitude
               dataList.AddRange(BitConverter.GetBytes((double)121.29));
               dataList.Add(0);

               dataList.AddRange(new byte[] { 0xAA, 0x55 });

               byte[] data = dataList.ToArray();

               while (true)
               {
                   ns.Write(data, 0, data.Length);

                   Thread.Sleep(5000);
               }
           }
        }
    }
}
