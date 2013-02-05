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
using Octopus.Interpreter.Items;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;

namespace Octopus.Interpreter.Formatters
{
    public interface IFormatter<T> : IObjectWithName
    {
        int GetFormatterRequiredDataLength();

        int GetFormattedDataLength();

        Message Format(T input, IPEndPoint endPoint);

        ReadOnlyCollection<Item> Items { get; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        event Action<bool, IPEndPoint> OnFormatComplete;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        event Action<IPEndPoint> OnFormatStart;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        event Action<IPEndPoint> OnFormatFailed;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        event Action<IPEndPoint> OnFormatSuccessfully;
        //remove this formatter from the interpreter, which own this.
        void Remove();

        IInterpreter<T> Interpreter { get; set; }
    }
}
