using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Octopus.Adapter;
using Octopus.Channel;
using Octopus.Config;
using Octopus.Interpreter;
using Octopus.Interpreter.Formatters;
using Octopus.Interpreter.Items;

namespace Octopus.Sample.TcpServer
{
    /// <summary>
    /// Test Case 10 is a case that interprete the bit in a byte
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
    /// index：3     |     Temperature (2 bytes)        |
    ///             ------------------------------------
    /// index：4     |     Longitude (8 bytes)          |
    ///             ------------------------------------
    /// index：5     |     Flag1 (0 Bit)
    ///                    Flag2 (1-3 Bit)
    ///                    Flag3 (4-7 Bit)              |
    ///             ------------------------------------
    /// index：6     |     Tailer 0xAA 0x55  (2 bytes)  |
    ///             ------------------------------------
    /// ******************************************************************************
    /// </summary>
    public class TestCase10
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

            SingleFormatterByteArrayInterpreter interpreter = new SingleFormatterByteArrayInterpreter("Interpreter");
            interpreter.SetHeaders(new byte[] { 0x55, 0xAA });
            interpreter.SetTailers(new byte[] { 0xAA, 0x55 });

            ByteArrayFormatter formatter = new ByteArrayFormatter("Formatter1");

            formatter.AddItem(new ByteArrayStringItem("NodeName", 1, 6, Encoding.ASCII));
            formatter.AddItem(new ByteArrayInt32Item("NodeId", 0));
            formatter.AddItem(new ByteArrayInt16Item("Temperature", 2));
            formatter.AddItem(new ByteArrayDoubleItem("Longitude", 3));

            ByteArrayByteItem babi = new ByteArrayByteItem("Flags", 4);
            babi.AddBitItem("Flag1", 1);
            babi.AddBitItem("Flag2", 3);
            babi.AddBitItem("Flag3", 4);
            formatter.AddItem(babi);

            interpreter.AddFormatter(formatter);

            ByteArrayAdapter baa = new ByteArrayAdapter("ByteArrayAdapter", tcpServerChannel, interpreter, Program.ShowEnvelop);

            baa.Setup();
        }

        public void StartServerWithConfig()
        {
            OctopusConfig oc = new OctopusConfig();
            oc.Load("TcpServer\\TestCase10\\TestCase10.xml");

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

            while (Console.ReadLine() != null)
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
                dataList.Add(226);

                dataList.AddRange(new byte[] { 0xAA, 0x55 });

                byte[] data = dataList.ToArray();

                ns.Write(data, 0, data.Length);
            }
        }
    }
}
