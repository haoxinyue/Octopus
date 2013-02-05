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
using System.Runtime.Serialization;

namespace Octopus.Exceptions
{
    [Serializable]
    public class UnknownElementException : Exception 
    {
        public UnknownElementException() : base() { }

        public UnknownElementException(string message) : base(message) { }

        public UnknownElementException(string message, Exception innerException) : base(message, innerException) { }

        private UnknownElementException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
