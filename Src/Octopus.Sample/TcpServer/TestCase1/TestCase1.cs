using Octopus.Adapter;
using Octopus.Channel;
using Octopus.Config;
using Octopus.Interpreter;
using Octopus.Interpreter.Formatters;
using Octopus.Interpreter.Items;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Octopus.Sample.TcpServer
{
    /// <summary>
    /// Test Case 1 is a very simple case that use tcp server to receive bytes and interpret
    /// the formatter is pretty simple, and without heads and tailers
    /// 
    /// ******************************************************************************
    /// Bytes Format:
    /// 
    ///             --------------------------------
    /// index：0     |     Node Id (4 bytes)        |
    ///             --------------------------------
    /// index：1     |     Node Name    (6 bytes)   |
    ///             --------------------------------
    /// index：2     |     Temperature (2 bytes)    |
    ///             --------------------------------
    /// index：3     |     Longitude   (8 bytes)    |
    ///             --------------------------------
    /// index：4     |     Reserved  (1 byte)       |
    ///             --------------------------------
    /// 
    /// ******************************************************************************
    /// </summary>
    public class TestCase1
    {
        public void RunTest()
        {
            Task startServerTask = new Task(() => StartServer());
            startServerTask.Start();
            startServerTask.Wait();

            StartClient();
        }

        public void StartServer()
        {
            TcpServerChannel channelTcpServer = new TcpServerChannel("TcpServerChannel", 9988);
            SingleFormatterByteArrayInterpreter interpreter = new SingleFormatterByteArrayInterpreter("Interpreter");

            ByteArrayFormatter formatter = new ByteArrayFormatter("Formatter1");

            formatter.AddItem(new ByteArrayStringItem("NodeName", 1, 6, Encoding.ASCII));
            formatter.AddItem(new ByteArrayInt32Item("NodeId", 0));
            formatter.AddItem(new ByteArrayInt16Item("Temperature", 2));
            formatter.AddItem(new ByteArrayDoubleItem("Longitude", 3));
            formatter.AddItem(new ByteArrayByteItem("Reserved", 4));

            interpreter.AddFormatter(formatter);

            ByteArrayAdapter baa = new ByteArrayAdapter("ByteArrayAdapter", channelTcpServer, interpreter, Program.ShowEnvelop);
            baa.Setup();
        }

        public void StartServerWithConfig()
        {
            OctopusConfig oc = new OctopusConfig();
            oc.Load("TcpServer\\TestCase1\\TestCase1.xml");

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

            //Node Id
            dataList.AddRange(BitConverter.GetBytes(1001));
            //Node Name
            dataList.AddRange(Encoding.ASCII.GetBytes("NK1001"));
            //Temperature
            dataList.AddRange(BitConverter.GetBytes((short)37));
            //Longitude
            dataList.AddRange(BitConverter.GetBytes((double)121.29));
            dataList.Add(0);

            byte[] data = dataList.ToArray();

            Console.WriteLine("Press <Enter> to send");
            while (Console.ReadLine() != null)
            {
                ns.Write(data, 0, data.Length);
            }
        }
    }
}
