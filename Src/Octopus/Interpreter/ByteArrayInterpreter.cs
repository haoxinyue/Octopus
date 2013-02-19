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

using Octopus.Channel;
using Octopus.Interpreter.Formatters;
using Octopus.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace Octopus.Interpreter
{
    public abstract class ByteArrayInterpreter : InterpreterBase<byte[]>
    {
        protected byte[] _headers;

        protected byte[] _tailers;

        protected ConcurrentDictionary<string, List<byte>> _bufferList = new ConcurrentDictionary<string,List<byte>>();

        protected int _minFormatterDataLength = short.MaxValue;

        protected object syncRoot = new object();

        public ByteArrayInterpreter(string name, Action<Envelop> notifyMessageCreated)
            : base(name, notifyMessageCreated)
        {

        }

        public ByteArrayInterpreter(string name) : this(name, null) { }

        public void SetHeaders(byte[] headers)
        {
            _headers = headers;
        }

        public void SetTailers(byte[] tailers)
        {
            _tailers = tailers;
        }

        public override void AddFormatter(IFormatter<byte[]> formatter)
        {
            base.AddFormatter(formatter);

            int length = formatter.GetFormatterRequiredDataLength();
            if (length < _minFormatterDataLength)
            {
                _minFormatterDataLength = length;
            }
        }

        public override List<Message> InterpreteProcess(ChannelData<byte[]> channelData)
        {
            List<Message> messageList = new List<Message>();
            string dicKey = channelData.RemoteEndPoint.Address.ToString();
            if(!_bufferList.ContainsKey(channelData.RemoteEndPoint.Address.ToString()))
            {
                _bufferList[dicKey] = new List<byte>(4096);
            }

            List<byte> buffer = _bufferList[dicKey];

            try
            {
                buffer.AddRange(channelData.RawData);

                int tailerIndex = 0;

                while (buffer.Count > 0 && buffer.Count >= _minFormatterDataLength + GetHeaderLength() + GetTailerLength())
                {
                    if (HasHeader())
                    {
                        if (!IsStartWithHeader(dicKey))
                        {
                            ClearToHeader(dicKey);
                            continue;
                        }
                    }

                    if (HasTailer())
                    {
                        tailerIndex = GetTailerIndex(dicKey);
                        //if no tailer in the buffer
                        if (tailerIndex <= 0)
                        {
                            break;
                        }
                    }

                    byte[] byteArray = buffer.ToArray();

                    byte[] byteArrayForFormat = GetBytesForFormat(byteArray, tailerIndex);

                    IFormatter<byte[]> formatter = GetMatchedFormatter(byteArrayForFormat);

                    if (formatter != null)
                    {
                        int byteCountRequired = formatter.GetFormatterRequiredDataLength();

                        if (byteArrayForFormat.Length < byteCountRequired)
                        {
                            if (HasTailer())
                            {
                                buffer.RemoveRange(0, tailerIndex + _tailers.Length);
                            }

                            break;
                        }

                        int formattedDataLength = 0;
                        Message message = formatter.Format(byteArrayForFormat, channelData.RemoteEndPoint, ref formattedDataLength);

                        if (message != null)
                        {
                            if (HasTailer())
                            {
                                buffer.RemoveRange(0, tailerIndex + _tailers.Length);
                            }
                            else
                            {
                                buffer.RemoveRange(0, formattedDataLength + GetHeaderLength());
                            }

                            messageList.Add(message);
                        }
                        else
                        {
                            if (HasTailer())
                            {
                                buffer.RemoveRange(0, tailerIndex + _tailers.Length);
                            }
                            else
                            {
                                buffer.RemoveRange(0, formattedDataLength + GetHeaderLength());
                            }
                        }
                    }
                    else
                    {
                        buffer.Clear();
                        _logWriter.Log("ByteArrayInterpreter InterpreteProcess function unknown formatter type:" + GetByteArrayLogString(channelData.RawData));
                    }
                }
            }
            catch (Exception ex)
            {
                _logWriter.Log("ByteArrayInterpreter InterpreteProcess function exception", ex);
            }

            return messageList;
        }

        private byte[] GetBytesForFormat(byte[] byteArray, int tailerIndex)
        {
            byte[] byteArrayForFormat = null;
            int byteCount = 0;

            if (HasTailer() && HasHeader())
            {
                byteCount = tailerIndex - _headers.Length;
                byteArrayForFormat = new byte[byteCount];
                Buffer.BlockCopy(byteArray, _headers.Length, byteArrayForFormat, 0, byteCount);
            }
            else if (HasTailer() && !HasHeader())
            {
                byteCount = tailerIndex;
                Buffer.BlockCopy(byteArray, 0, byteArrayForFormat, 0, byteCount);
            }
            else if (!HasTailer() && HasHeader())
            {
                byte[] byteArrayWithoutHeader = GetByteArrayWithoutHeaders(byteArray);
                int nextHeaderIndex = byteArrayForFormat.IndexOf(_headers);
                if (nextHeaderIndex > 0) // has another header in the buffer, should get the data between two headers
                {
                    byteArrayForFormat = new byte[nextHeaderIndex];
                    Buffer.BlockCopy(byteArrayWithoutHeader, 0, byteArrayForFormat, 0, nextHeaderIndex);
                }
                else
                {
                    byteArrayForFormat = byteArrayWithoutHeader;
                }
            }
            else if (!HasTailer() && !HasHeader())
            {
                byteArrayForFormat = byteArray;
            }

            return byteArrayForFormat;
        }

        protected bool IsStartWithHeader(string dicKey)
        {
            if (_headers != null && _headers.Length > 0)
            {
                return _bufferList[dicKey].ToArray().IndexOf(_headers) == 0;
            }
            else
            {
                return true;
            }
        }

        protected bool IsEndWithTailer(string dicKey)
        {
            if (_tailers != null && _tailers.Length > 0)
            {
                return _bufferList[dicKey].ToArray().IndexOf(_tailers) == _bufferList[dicKey].Count - _tailers.Length;
            }

            return false;
        }

        protected int GetTailerIndex(string dicKey)
        {
            return _bufferList[dicKey].ToArray().IndexOf(_tailers);
        }

        protected int GetHeaderIndex(string dicKey)
        {
            return _bufferList[dicKey].ToArray().IndexOf(_headers);
        }

        /// <summary>
        /// must begin with headers, if not remove the data before headers
        /// </summary>
        protected virtual void ClearToHeader(string dicKey)
        {
            int headerIndex = GetHeaderIndex(dicKey);
            if (headerIndex < 0)
            {
                _bufferList[dicKey].Clear();
            }
            else
            {
                _bufferList[dicKey].RemoveRange(0, headerIndex);
            }
        }

        private string GetByteArrayLogString(byte[] input)
        {
            StringBuilder sb = new StringBuilder();
            if (null != input)
            {
                int length = input.Length;
                for (int i = 0; i <= length - 1; i++)
                {
                    if (i == length - 1)
                        sb.Append(input[i].ToString());
                    else
                        sb.Append(input[i].ToString() + ",");
                }
            }

            return sb.ToString();
        }

        private byte[] GetByteArrayWithoutHeaders(byte[] rawData)
        {
            if (_headers != null && _headers.Length > 0)
            {
                return rawData.Skip(_headers.Length).ToArray();
            }
            else
            {
                return rawData;
            }
        }

        private short GetHeaderLength()
        {
            return (short)(_headers == null ? 0 : _headers.Length);
        }

        private short GetTailerLength()
        {
            return (short)(_tailers == null ? 0 : _tailers.Length);
        }

        protected bool HasTailer()
        {
            return (_tailers != null && _tailers.Length > 0);
        }

        protected bool HasHeader()
        {
            return (_headers != null && _headers.Length > 0);
        }
    }
}
