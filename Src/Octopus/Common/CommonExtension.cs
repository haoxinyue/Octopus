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

using Octopus.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Octopus.Common
{
    public static class CommonExtension
    {
        public static int IndexOf(this byte[] array, byte[] pattern)
        {
            Guard.NotNull(array, "Array cannot be null");
            Guard.NotNull(pattern, "Pattern cannot be null");

            int index = -1;
            for (int i = 0; i <= array.Length - pattern.Length; i++)
            {
                index = i;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (array[i + j] != pattern[j])
                    {
                        index = -1;
                        break;
                    }
                }
                if (index != -1) break;
            }
            return index;
        }
    }
}
