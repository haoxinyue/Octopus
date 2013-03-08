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
    /// Test Case 5 shows how to use ByteArrayLoopItem. 
    /// 
    /// ******************************************************************************
    /// Bytes Format:
    /// 
    ///             ------------------------------------
    /// index：0     |     Header 0x55 0xAA (2 bytes)   |
    ///             ------------------------------------
    /// index : 1   |     Type Id (4 bytes)            |
    ///             ------------------------------------
    /// index：2     |     Node Id (4 bytes)            |
    ///             ------------------------------------
    /// index：3     |     Node Name    (6 bytes)       |
    ///             ------------------------------------
    /// index：4     |     Temperature (2 bytes)        |
    ///             ------------------------------------
    /// index：5     |     Longitude   (8 bytes)        |
    ///             ------------------------------------
    /// index：6     |     Reserved  (1 byte)           |
    ///             ------------------------------------
    /// index：7     |     Node Id (4 bytes)            |
    ///             ------------------------------------
    /// index：8     |     Node Name    (6 bytes)       |
    ///             ------------------------------------
    /// index：9     |     Temperature (2 bytes)        |
    ///             ------------------------------------
    /// index：10     |     Longitude   (8 bytes)        |
    ///             ------------------------------------
    /// index：11    |     Reserved  (1 byte)           |
    ///             ------------------------------------
    ///             
    /// ......
    /// ......

    /// index : 22   |     Tailer 0xAA 0x55  (2 bytes)  |
    ///             ------------------------------------
    /// ******************************************************************************
    /// </summary>
    public class TestCase5
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

            ByteArrayCompositeValueItem byteArrayCompositeValueItem = new ByteArrayCompositeValueItem("ByteArrayCompositeValueItem1");
            byteArrayCompositeValueItem.AddItem(new ByteArrayStringItem("NodeName", 1, 6, Encoding.ASCII));
            byteArrayCompositeValueItem.AddItem(new ByteArrayInt32Item("NodeId", 0));
            byteArrayCompositeValueItem.AddItem(new ByteArrayInt16Item("Temperature", 2));
            byteArrayCompositeValueItem.AddItem(new ByteArrayDoubleItem("Longitude", 3));
            byteArrayCompositeValueItem.AddItem(new ByteArrayByteItem("Reserved", 4));

            ByteArrayLoopItem byteArrayLoopItem = new ByteArrayLoopItem("byteArrayLoopItem1", 1, byteArrayCompositeValueItem);

            formatter.AddItem(new ByteArrayInt32Item("Type", 0));
            formatter.AddItem(byteArrayLoopItem);

            interpreter.AddFormatter(formatter);

            ByteArrayAdapter baa = new ByteArrayAdapter("ByteArrayAdapter", channelTcpServer, interpreter, Program.ShowEnvelop);
            baa.Setup();
        }

        public void StartServerWithConfig()
        {
            OctopusConfig oc = new OctopusConfig();
            oc.Load("TcpServer\\TestCase5\\TestCase5.xml");

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

            //TypeId
            dataList.AddRange(BitConverter.GetBytes(9999));
            for (int i = 0; i < 3; i++)
            {
                //Node Id
                dataList.AddRange(BitConverter.GetBytes(1001 + i));
                //Node Name
                dataList.AddRange(Encoding.ASCII.GetBytes("NK" + (1001 + i)));
                //Temperature
                dataList.AddRange(BitConverter.GetBytes((short)37));
                //Longitude
                dataList.AddRange(BitConverter.GetBytes((double)121.29));
                dataList.Add(0);
            }
            dataList.AddRange(new byte[] { 0xAA, 0x55 });

            byte[] data = dataList.ToArray();

            Console.WriteLine("Press <Enter> to send");

            bool isPartOne = true;
            while (Console.ReadLine() != null)
            {
                if (isPartOne)
                {   
                    //send first part of the data
                    ns.Write(data, 0, 4);
                    isPartOne = false;
                }
                else
                {
                    //send the rest
                    ns.Write(data, 4, data.Length - 4);
                    isPartOne = true;
                }
            }
        }
    }
}
