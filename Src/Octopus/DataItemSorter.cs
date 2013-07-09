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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Octopus
{
    public class DataItemSorter : IComparer<string>
    {
        private static readonly Lazy<DataItemSorter> _instance
                = new Lazy<DataItemSorter>(() => new DataItemSorter());

        private DataItemSorter() { }

        public static DataItemSorter Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        public int Compare(string x, string y)
        {
            return 1;
        }
    }
}
