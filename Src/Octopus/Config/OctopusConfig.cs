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

using Octopus.Activator;
using Octopus.Adapter;
using Octopus.Channel;
using Octopus.Command;
using Octopus.Exceptions;
using Octopus.Interpreter;
using Octopus.Interpreter.FormatterFilters;
using Octopus.Interpreter.Formatters;
using Octopus.Interpreter.Items;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace Octopus.Config
{
    public class OctopusConfig
    {
        private static Dictionary<string, IAdapter> _adapters = new Dictionary<string, IAdapter>();

        private static string _currentAdapterName = string.Empty;

        private static Dictionary<string, Dictionary<string, object>> _adapterObjects = new Dictionary<string, Dictionary<string, object>>();

        public Dictionary<string, IAdapter> Adapters
        {
            get { return _adapters; }
        }

        public static OctopusConfig Load(string filePath)
        {
            XDocument xDoc = null;

            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                xDoc = XDocument.Load(fs);
            }

            return Load(xDoc);
        }

        private static OctopusConfig Load(XDocument xDoc)
        {
            OctopusConfig oc = new OctopusConfig();

            foreach (var adapterElement in xDoc.Elements("Adapter"))
            {
                IAdapter adapter = null;

                string adapterType = GetAttribute(adapterElement, "Type");
                string adapterName = GetAttribute(adapterElement, "Name");

                _currentAdapterName = adapterName;

                Dictionary<string, object> adapterObjects = new Dictionary<string, object>();
                _adapterObjects.Add(adapterName, adapterObjects);

                adapter = GetAdapter(adapterElement);

                oc.Adapters.Add(adapter.Name, adapter);

                foreach (var activatorElement in adapterElement.Elements("Activator"))
                {
                    IActivator activator = GetActivator(activatorElement, adapter.GetChannel());
                    adapter.AddActivator(activator);
                }

                if (adapterElement.Element("EventConnecter") != null)
                {
                    foreach (var eventConnecterElement in adapterElement.Elements("EventConnecter"))
                    {
                        GetEventConnecter(eventConnecterElement);
                    }
                }
            }

            return oc;
        }

        private static IAdapter GetAdapter(XElement element)
        {
            string adapterType = GetAttribute(element, "Type");
            string adapterName = GetAttribute(element, "Name");

            IAdapter adapter = null;

            var channelElement = element.Element("Channel");
            if (channelElement == null)
            {
                throw new ElementNotFoundException("Channel elemnt not found.");
            }

            var interpreterElement = element.Element("Interpreter");
            if (interpreterElement == null)
            {
                throw new ElementNotFoundException("Interpreter elemnt not found.");
            }

            if (adapterType == "ByteArrayAdapter")
            {
                IChannel<byte[]> channel = (IChannel<byte[]>)GetChannel(channelElement);
                IInterpreter<byte[]> interpreter = (IInterpreter<byte[]>)GetInterpreter(interpreterElement);

                adapter = new ByteArrayAdapter(adapterName, channel, interpreter);
            }
            else if (adapterType == "StringAdapter")
            {
                IChannel<string> channel = (IChannel<string>)GetChannel(channelElement);
                IInterpreter<string> interpreter = (IInterpreter<string>)GetInterpreter(interpreterElement);

                adapter = new StringAdapter(adapterName, channel, interpreter);
            }
            else
            {
                throw new UnknownElementException("Unknown adapter type:" + adapterType);
            }

            _adapterObjects[_currentAdapterName].Add(adapter.Name, adapter);

            return adapter;
        }

        private static object GetChannel(XElement element)
        {
            string channelType = GetAttribute(element, "Type");
            string name = GetAttribute(element, "Name");

            IObjectWithName channel = null;

            if (channelType == "TcpServerChannel")
            {

                int port = int.Parse(GetAttribute(element, "LocalPort"));

                TcpServerChannel tcpServerChannel = new TcpServerChannel(name, port);

                channel = tcpServerChannel;
            }
            else if (channelType == "TcpClientChannel")
            {
                TcpClientChannel tcpClientChannel = null;

                string ip = GetAttribute(element, "RemoteIP");
                int port = int.Parse(GetAttribute(element, "RemotePort"));


                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);

                if (element.Attribute("RetryInterval") != null && !string.IsNullOrEmpty(element.Attribute("RetryInterval").Value))
                {
                    int retryInterval = int.Parse(GetAttribute(element, "RetryInterval"));
                    tcpClientChannel = new TcpClientChannel(name, ipEndPoint, retryInterval);
                }
                else
                {
                    tcpClientChannel = new TcpClientChannel(name, ipEndPoint);
                }

                channel = tcpClientChannel;
            }
            else if (channelType == "UdpChannel")
            {
                UdpChannel udpChannel = null;

                int remotePort = int.Parse(GetAttribute(element, "RemotePort"));
                int localPort = int.Parse(GetAttribute(element, "LocalPort"));

                if (element.Attribute("LocalIP") != null && !string.IsNullOrEmpty(element.Attribute("LocalIP").Value))
                {
                    string localIP = GetAttribute(element, "LocalIP");
                    udpChannel = new UdpChannel(name, remotePort, localPort, IPAddress.Parse(localIP));
                }
                else
                {
                    udpChannel = new UdpChannel(name, remotePort, localPort);
                }

                channel = udpChannel;
            }
            else if (channelType == "HttpReaderChannel")
            {
                HttpReaderChannel httpReaderChannel = null;

                int remotePort = int.Parse(GetAttribute(element, "RemotePort"));
                string remoteIP = GetAttribute(element, "RemoteIP");
                string requestUrl = GetAttribute(element, "RequestUrl");
                string userName = string.Empty;
                string password = string.Empty;

                if (element.Attribute("UserName") != null && !string.IsNullOrEmpty(element.Attribute("UserName").Value))
                {
                    userName = GetAttribute(element, "UserName");
                }

                if (element.Attribute("Password") != null && !string.IsNullOrEmpty(element.Attribute("Password").Value))
                {
                    password = GetAttribute(element, "Password");
                }

                httpReaderChannel = new HttpReaderChannel(name, remoteIP, remotePort, requestUrl, userName, password);

                channel = httpReaderChannel;
            }
            else
            {
                throw new UnknownElementException("Unknown channel type:" + channelType);
            }

            _adapterObjects[_currentAdapterName].Add(channel.Name, channel);

            return channel;
        }

        private static string GetAttribute(XElement element, string attributeName)
        {
            XAttribute xAttri = element.Attribute(attributeName);

            if (xAttri != null)
            {
                string attriValue = xAttri.Value;

                if (!string.IsNullOrEmpty(attriValue))
                {
                    return attriValue;
                }
                else
                {
                    throw new AttributeNotFoundException("Attribute Name:" + attributeName + " cannot be empty");
                }
            }
            else
            {
                throw new AttributeNotFoundException("Attribute Name:" + attributeName + " not found");
            }
        }

        private static object GetInterpreter(XElement element)
        {
            string interpreterType = GetAttribute(element, "Type");
            string interpreterName = GetAttribute(element, "Name");

            var headerElement = element.Element("Header");
            var trailerElement = element.Element("Tailer");

            IObjectWithName returnInterpreter = null;

            if (interpreterType == "MultipleFormatterByteArrayInterpreter")
            {
                MultipleFormatterByteArrayInterpreter interpreter = new MultipleFormatterByteArrayInterpreter(interpreterName);

                if (headerElement != null && !string.IsNullOrEmpty(headerElement.Value.Trim()))
                {
                    string headerString = headerElement.Value;
                    interpreter.SetHeaders(HexStringToByteArray(headerString));
                }

                if (trailerElement != null && !string.IsNullOrEmpty(trailerElement.Value.Trim()))
                {
                    string tailerString = trailerElement.Value;
                    interpreter.SetTailers(HexStringToByteArray(tailerString));
                }

                foreach (var formatterElement in element.Elements("Formatter"))
                {
                    interpreter.AddFormatter((IFormatter<byte[]>)GetFormatter(formatterElement));
                }

                if (element.Element("FormatterFilter") != null)
                {
                    foreach (var filterElement in element.Elements("FormatterFilter"))
                    {
                        IFormatterFilter<byte[]> filter = (IFormatterFilter<byte[]>)GetFormatterFilter(filterElement);
                        interpreter.AddFormatterFilter(filter);
                    }
                }

                returnInterpreter = interpreter;
            }
            else if (interpreterType == "SingleFormatterByteArrayInterpreter")
            {
                SingleFormatterByteArrayInterpreter interpreter = new SingleFormatterByteArrayInterpreter(interpreterName);

                if (headerElement != null && !string.IsNullOrEmpty(headerElement.Value.Trim()))
                {
                    string headerString = headerElement.Value;
                    interpreter.SetHeaders(HexStringToByteArray(headerString));
                }

                if (trailerElement != null && !string.IsNullOrEmpty(trailerElement.Value.Trim()))
                {
                    string tailerString = trailerElement.Value;
                    interpreter.SetTailers(HexStringToByteArray(tailerString));
                }

                var formatterElement = element.Element("Formatter");

                interpreter.AddFormatter((IFormatter<byte[]>)GetFormatter(formatterElement));

                returnInterpreter = interpreter;
            }
            else if (interpreterType == "StringInterpreter")
            {
                StringInterpreter interpreter = new StringInterpreter(interpreterName);

                var formatterElement = element.Element("Formatter");

                interpreter.AddFormatter((IFormatter<string>)GetFormatter(formatterElement));

                returnInterpreter = interpreter;
            }
            else
            {
                throw new UnknownElementException("Unknown interpreter type:" + interpreterType);
            }

            _adapterObjects[_currentAdapterName].Add(returnInterpreter.Name, returnInterpreter);

            return returnInterpreter;
        }

        private static object GetFormatter(XElement element)
        {
            string formatterType = GetAttribute(element, "Type");
            string formatterName = GetAttribute(element, "Name");

            IObjectWithName returnFormatter = null;

            if (formatterType == "ByteArrayFormatter")
            {
                ByteArrayFormatter formatter = null;

                if (element.Attribute("TypeId") != null && !string.IsNullOrEmpty(element.Attribute("TypeId").Value))
                {
                    formatter = new ByteArrayFormatter(formatterName, byte.Parse(element.Attribute("TypeId").Value));
                }
                else
                {
                    formatter = new ByteArrayFormatter(formatterName);
                }

                foreach (var itemElement in element.Elements("Item"))
                {
                    formatter.AddItem(GetItem(itemElement));
                }

                returnFormatter = formatter;
            }
            else if (formatterType == "StringFormatter")
            {
                StringFormatter formatter = new StringFormatter(formatterName);

                foreach (var itemElement in element.Elements("Item"))
                {
                    formatter.AddItem(GetItem(itemElement));
                }

                returnFormatter = formatter;
            }
            else
            {
                throw new UnknownElementException("Unkown formatter type:" + formatterType);
            }

            _adapterObjects[_currentAdapterName].Add(returnFormatter.Name, returnFormatter);

            return returnFormatter;
        }

        private static object GetFormatterFilter(XElement element)
        {
            string filterType = GetAttribute(element, "Type");
            string filterName = GetAttribute(element, "Name");

            if (filterType == "ByteArrayTypedFormatterFilter")
            {
                int formatterTypeIndex = int.Parse(GetAttribute(element, "FormatterTypeIndex"));

                ByteArrayTypedFormatterFilter filter = new ByteArrayTypedFormatterFilter(filterName, formatterTypeIndex);

                if (element.Element("FormatterFilter") != null)
                {
                    foreach (var subFilterElement in element.Elements("FormatterFilter"))
                    {
                        IFormatterFilter<byte[]> subFilter = (IFormatterFilter<byte[]>)GetFormatterFilter(subFilterElement);
                    }
                }

                return filter;
            }
            else if (filterType == "ByteArrayLengthGreatThanFormatterFilter")
            {
                int length = int.Parse(GetAttribute(element, "Length"));

                ByteArrayLengthGreatThanFormatterFilter filter = new ByteArrayLengthGreatThanFormatterFilter(filterName, length);

                if (element.Element("FormatterFilter") != null)
                {
                    foreach (var subFilterElement in element.Elements("FormatterFilter"))
                    {
                        IFormatterFilter<byte[]> subFilter = (IFormatterFilter<byte[]>)GetFormatterFilter(subFilterElement);
                    }
                }

                return filter;
            }
            else if (filterType == "ByteArrayLengthEqualFormatterFilter")
            {
                int length = int.Parse(GetAttribute(element, "Length"));

                ByteArrayLengthEqualFormatterFilter filter = new ByteArrayLengthEqualFormatterFilter(filterName, length);

                if (element.Element("FormatterFilter") != null)
                {
                    foreach (var subFilterElement in element.Elements("FormatterFilter"))
                    {
                        IFormatterFilter<byte[]> subFilter = (IFormatterFilter<byte[]>)GetFormatterFilter(subFilterElement);
                    }
                }

                return filter;
            }
            else if (filterType == "Custom")
            {
                string customTypeName = GetAttribute(element, "CustomTypeName");
                Type type = Type.GetType(customTypeName);

                object[] args = null;

                if (element.Elements("Parameter") != null)
                {
                    args = new object[element.Elements("Parameter").Count()];
                    int i = 0;
                    foreach (var paraElement in element.Elements("Parameter"))
                    {
                        string pType = GetAttribute(paraElement, "Type");
                        string pValue = GetAttribute(paraElement, "Value");
                        object p = Convert.ChangeType(pValue, Type.GetType(pType));
                        args[i] = p;
                        i++;
                    }
                }

                object custom  = System.Activator.CreateInstance(type, args);

                return custom;
            }
            else
            {
                throw new UnknownElementException("Unknown FormatterFilter type:" + filterType);
            }
        }

        private static Item GetItem(XElement element)
        {
            Item item = null;
            string itemType = GetAttribute(element, "Type");
            string name = GetAttribute(element, "Name");
            short index = short.Parse(GetAttribute(element, "SortIndex"));

            switch (itemType)
            {
                case "ByteArrayByteCrcItem":
                    int crcFromIndex = int.Parse(GetAttribute(element, "CrcFromIndex"));
                    int crcToIndex = int.Parse(GetAttribute(element, "CrcToIndex"));
                    item = new ByteArrayByteCrcItem(name, index, crcFromIndex, crcToIndex);
                    break;
                case "ByteArrayByteItem":
                    item = new ByteArrayByteItem(name, index);
                    break;
                case "ByteArrayCompositeValueItem":
                    item = new ByteArrayCompositeValueItem(name, index);
                    foreach (var subItemElement in element.Elements("Item"))
                    {
                        Item subItem = GetItem(subItemElement);
                        ((ByteArrayCompositeValueItem)item).AddItem((ValueItem<byte[], DataItem>)subItem);
                    }
                    break;
                case "ByteArrayDoubleItem":
                    item = new ByteArrayDoubleItem(name, index);
                    break;
                case "ByteArrayInt16Item":
                    item = new ByteArrayInt16Item(name, index);
                    break;
                case "ByteArrayInt32Item":
                    item = new ByteArrayInt32Item(name, index);
                    break;
                case "ByteArrayInt64Item":
                    item = new ByteArrayInt64Item(name, index);
                    break;
                case "ByteArrayLoopItem":
                    ByteArrayCompositeValueItem itemByteArrayCompositeValue = null;
                    foreach (var subItemElement in element.Elements("Item"))
                    {
                        if (GetAttribute(subItemElement, "Type") == "ByteArrayCompositeValueItem")
                        {
                            itemByteArrayCompositeValue = (ByteArrayCompositeValueItem)GetItem(subItemElement);
                            break;
                        }
                    }

                    if (itemByteArrayCompositeValue == null)
                    {
                        throw new ElementNotFoundException("Element ByteArrayCompositeValueItem Item not found");
                    }
                    item = new ByteArrayLoopItem(name, index, itemByteArrayCompositeValue);
                    break;
                case "ByteArrayStringItem":
                    int byteCount = int.Parse(GetAttribute(element, "ByteCount"));
                    string encoding = GetAttribute(element, "Encoding");
                    item = new ByteArrayStringItem(name, index, byteCount, Encoding.GetEncoding(encoding));
                    break;
                case "SimpleStringValueItem":
                    item = new SimpleStringValueItem(name, index);
                    break;
                case "CustomItem":
                    {
                        string customTypeName = GetAttribute(element, "CustomTypeName");
                        Type type = Type.GetType(customTypeName);

                        object[] args = null;

                        if (element.Elements("Parameter") != null)
                        {
                            args = new object[element.Elements("Parameter").Count()+2];
                            int i = 0;
                            args[0] = name;
                            args[1] = index;
                            foreach (var paraElement in element.Elements("Parameter"))
                            {
                                string pType = GetAttribute(paraElement, "Type");
                                string pValue = GetAttribute(paraElement, "Value");
                                object p = Convert.ChangeType(pValue, Type.GetType(pType));
                                args[i+2] = p;
                                i++;
                            }
                        }

                        item = (Item)System.Activator.CreateInstance(type, args);
                        break;
                    }
                default:
                    throw new UnknownElementException("Unknown item type:" + itemType);
            }

            _adapterObjects[_currentAdapterName].Add(item.Name, item);

            return item;
        }

        private static IActivator GetActivator(XElement element, object channel)
        {
            IActivator activator = null;

            string activatorType = GetAttribute(element, "Type");
            string activatorName = GetAttribute(element, "Name");

            if (activatorType == "OneTimeActivator")
            {
                activator = new OneTimeActivator(activatorName);
            }
            else if (activatorType == "IntervalActivator")
            {
                if (element.Attribute("ExecuteInterval") != null && !string.IsNullOrEmpty(element.Attribute("ExecuteInterval").Value))
                {
                    activator = new IntervalActivator(activatorName, int.Parse(element.Attribute("ExecuteInterval").Value));
                }
                else
                {
                    activator = new IntervalActivator(activatorName);
                }
            }
            else
            {
                throw new UnknownElementException("Unknown activator type:" + activatorType);
            }

            if (element.Element("Command") == null)
            {
                throw new ElementNotFoundException("Element Command cannot be found");
            }

            foreach (var commandElement in element.Elements("Command"))
            {
                ICommand command = GetCommand(commandElement, channel);
                activator.AddCommand(command);
            }

            _adapterObjects[_currentAdapterName].Add(activator.Name, activator);

            return activator;
        }

        private static ICommand GetCommand(XElement element, object channel)
        {
            ICommand command = null;
            string commandType = GetAttribute(element, "Type");
            string commandName = GetAttribute(element, "Name");

            if (commandType == "ByteArrayCommand")
            {
                if (!string.IsNullOrEmpty(element.Value.Trim()))
                {
                    command = new ByteArrayCommand(commandName, (IChannel<byte[]>)channel, HexStringToByteArray(element.Value.Trim().Replace(" ", "")));
                }
                else
                {
                    throw new ElementValueNotFoundException("Command value not found.");
                }
            }
            else if (commandType == "StringCommand")
            {
                if (!string.IsNullOrEmpty(element.Value.Trim()))
                {
                    command = new StringCommand(commandName, (IChannel<string>)channel, element.Value.Trim());
                }
                else
                {
                    throw new ElementValueNotFoundException("Command value not found.");
                }
            }
            else
            {
                throw new UnknownElementException("Unknown command type:" + commandType);
            }

            _adapterObjects[_currentAdapterName].Add(command.Name, command);

            return command;
        }

        private static byte[] HexStringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        private static void GetEventConnecter(XElement element)
        {
            if (element.Elements("Linker") != null)
            {
                foreach (var linkerElement in element.Elements("Linker"))
                {
                    string raiserName = GetAttribute(linkerElement, "RaiserName");
                    string eventName = GetAttribute(linkerElement, "EventName");
                    string handlerName = GetAttribute(linkerElement, "HandlerName");
                    string handlerMethodName = GetAttribute(linkerElement, "HandlerMethodName");

                    EventInfo ei = _adapterObjects[_currentAdapterName][raiserName].GetType().GetEvent(eventName);

                    Delegate d = Delegate.CreateDelegate(ei.EventHandlerType, _adapterObjects[_currentAdapterName][handlerName], handlerMethodName);

                    ei.AddEventHandler(_adapterObjects[_currentAdapterName][raiserName], d);
                }
            }
        }
    }
}
