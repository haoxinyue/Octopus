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
    /// Test Case 7 is a simple case that use custom item
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
    public class TestCase7
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
            TcpServerChannel channelTcpServer = new TcpServerChannel("TcpServerChannel", 9988);
            SingleFormatterByteArrayInterpreter interpreter = new SingleFormatterByteArrayInterpreter("Interpreter");

            interpreter.SetTailers(new byte[] { 0xAA, 0x55 });
            interpreter.SetHeaders(new byte[] { 0x55, 0xAA });

            ByteArrayFormatter formatter = new ByteArrayFormatter("Formatter1");

            formatter.AddItem(new ByteArrayStringItem("NodeName", 1, 6, Encoding.ASCII));
            formatter.AddItem(new ByteArrayInt32Item("NodeId", 0));
            formatter.AddItem(new CustomItem("CustomItem", 2, "test2"));
            formatter.AddItem(new ByteArrayByteItem("Reserved", 3));

            interpreter.AddFormatter(formatter);

            ByteArrayAdapter baa = new ByteArrayAdapter("ByteArrayAdapter", channelTcpServer, interpreter, Program.ShowEnvelop);
            baa.Setup();
        }

        public void StartServerWithConfig()
        {
            OctopusConfig oc = new OctopusConfig();
            oc.Load("TcpServer\\TestCase7\\TestCase7.xml");

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

            Console.WriteLine("Press <Enter> to send");
            while (Console.ReadLine() != null)
            {
                ns.Write(data, 0, data.Length);
            }
        }

        public class CustomItem : CustomValueItem<byte[]>
        {
            private string _para = string.Empty;

            public CustomItem(string name, short sortIndex, string para)
                : base(name, sortIndex) 
            {
                _para = para;
            }

            public override DataItem GetValue(byte[] input, int index, ref int formattedDataLength)
            {
                DataItem di = new DataItem("Custom", null);

                short temperature = BitConverter.ToInt16(input, index);
                double longitude = BitConverter.ToDouble(input, index + 2);
                DataItem t = new DataItem("Temperature", temperature);
                DataItem l = new DataItem("Longitude", longitude);

                di.AddDataItem(t);
                di.AddDataItem(l);

                formattedDataLength = 10;
                return di;
            }

            public override int GetRequiredDataLength()
            {
                return 10;
            }
        }
    }
}
