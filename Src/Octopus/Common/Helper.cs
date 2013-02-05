#region License
// Copyright (c) Nick Hao http://www.cnblogs.com/haoxinyue
// 
// Licensed under the Apache License, Version 2.0 (the 'License'); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an 'AS IS' BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Octopus.Common
{
    public static class Helper
    {
        public static byte[] BinaryObjectToByte(Object obj)
        {
            using (MemoryStream mem = new MemoryStream())
            {
                BinaryFormatter binary = new BinaryFormatter();
                binary.Serialize(mem, obj);
                return mem.ToArray();
            }
        }

        public static Object BinaryByteToObject(byte[] buff)
        {
            using (MemoryStream mem = new MemoryStream(buff))
            {
                mem.Position = 0;
                BinaryFormatter binary = new BinaryFormatter();
                return binary.Deserialize(mem);
            }
        }

        public static T XmlLoad<T>(string fileName)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (TextReader reader = new StreamReader(fileName))
            {
                T obj = (T)serializer.Deserialize(reader);
                return obj;
            }
        }

        public static void XmlSave<T>(T obj, string fileName)
        {
            using (TextWriter writer = new StreamWriter(fileName))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(writer, obj);
            }
        }

        public static string ObjectToXMLString<T>(T obj)
        {
            using (StringWriter sw = new StringWriter())
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(sw, obj);
                return sw.ToString();
            }
        }

        public static T XmlStringToObject<T>(string xmlString)
        {
            using (StringReader sr = new StringReader(xmlString))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(sr);
            }
        }
    }
}
