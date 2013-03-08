using Octopus.Adapter;
using Octopus.Channel;
using Octopus.Config;
using Octopus.Interpreter;
using Octopus.Interpreter.FormatterFilters;
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
    /// Test Case 6 shows how to use MultipleFormatterByteArrayInterpreter. 
    /// and you will create IFormatterFilter to help MultipleFormatterByteArrayInterpreter to find which formatter the interpreter will use
    /// if input byte array has 2 or more format type, which determined by a byte in the input byte array. for example:
    /// ******************************************************************************
    /// Bytes Format:
    /// 
    /// 1. if the value of byte in index[1] (Type id) equals 1, the formatter is shown below:
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
    ///             
    /// 
    /// 2. if the value of byte in index[1] (Type id) equals 2, the formatter is shown below:
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
    /// index : 7   |     Tailer 0xAA 0x55  (2 bytes)  |
    ///             ------------------------------------
    /// ******************************************************************************
    /// </summary>
    public class TestCase6
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
            MultipleFormatterByteArrayInterpreter interpreter = new MultipleFormatterByteArrayInterpreter("Interpreter");

            interpreter.SetTailers(new byte[] { 0xAA, 0x55 });
            interpreter.SetHeaders(new byte[] { 0x55, 0xAA });

            /////////////////////////////////////////////////////////////////////////////////////////

            ByteArrayFormatter formatter_F1 = new ByteArrayFormatter("Formatter1", 1);

            ByteArrayCompositeValueItem byteArrayCompositeValueItem_F1 = new ByteArrayCompositeValueItem("F1_ByteArrayCompositeValueItem1");
            byteArrayCompositeValueItem_F1.AddItem(new ByteArrayStringItem("F1_NodeName", 1, 6, Encoding.ASCII));
            byteArrayCompositeValueItem_F1.AddItem(new ByteArrayInt32Item("F1_NodeId", 0));
            byteArrayCompositeValueItem_F1.AddItem(new ByteArrayInt16Item("F1_Temperature", 2));
            byteArrayCompositeValueItem_F1.AddItem(new ByteArrayDoubleItem("F1_Longitude", 3));
            byteArrayCompositeValueItem_F1.AddItem(new ByteArrayByteItem("F1_Reserved", 4));

            ByteArrayLoopItem byteArrayLoopItem_F1 = new ByteArrayLoopItem("F1_byteArrayLoopItem1", 1, byteArrayCompositeValueItem_F1);

            formatter_F1.AddItem(new ByteArrayInt32Item("F1_Type", 0));
            formatter_F1.AddItem(byteArrayLoopItem_F1);

            /////////////////////////////////////////////////////////////////////////////////////////

            ByteArrayFormatter formatter_F2 = new ByteArrayFormatter("Formatter2", 2);

            formatter_F2.AddItem(new ByteArrayInt32Item("F2_Type", 0));
            formatter_F2.AddItem(new ByteArrayStringItem("F2_NodeName", 2, 6, Encoding.ASCII));
            formatter_F2.AddItem(new ByteArrayInt32Item("F2_NodeId", 1));
            formatter_F2.AddItem(new ByteArrayInt16Item("F2_Temperature", 3));
            formatter_F2.AddItem(new ByteArrayDoubleItem("F2_Longitude", 4));
            formatter_F2.AddItem(new ByteArrayByteItem("F2_Reserved", 5));

            ///////////////////////////////////////////////////////////////////////////////////////////////

            ByteArrayTypedFormatterFilter byteArrayTypedFormatterFilter = new ByteArrayTypedFormatterFilter("ByteArrayTypedFormatterFilter", 0);
            //the index we use 0 meaning the Type Id is the index[0] in the input buffer without header.

            interpreter.AddFormatter(formatter_F1);
            interpreter.AddFormatter(formatter_F2);
            interpreter.AddFormatterFilter(byteArrayTypedFormatterFilter);

            ByteArrayAdapter baa = new ByteArrayAdapter("ByteArrayAdapter", channelTcpServer, interpreter, Program.ShowEnvelop);
            baa.Setup();
        }

        public void StartServerWithConfig()
        {
            OctopusConfig oc = new OctopusConfig();
            oc.Load("TcpServer\\TestCase6\\TestCase6.xml");

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
            dataList.AddRange(BitConverter.GetBytes(1));
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


            /////frame 2 ///////

            dataList.AddRange(new byte[] { 0x55, 0xAA });

            //TypeId
            dataList.AddRange(BitConverter.GetBytes(2));

            //Node Id
            dataList.AddRange(BitConverter.GetBytes(1001));
            //Node Name
            dataList.AddRange(Encoding.ASCII.GetBytes("NK" + (1001)));
            //Temperature
            dataList.AddRange(BitConverter.GetBytes((short)22));
            //Longitude
            dataList.AddRange(BitConverter.GetBytes((double)222.99));
            dataList.Add(0);

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
