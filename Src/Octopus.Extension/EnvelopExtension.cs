using Newtonsoft.Json;
using Octopus.Common;
using Octopus.Interpreter;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Octopus.Extension
{
    public static class EnvelopExtension
    {
        public static string ToJson(this Envelop envelop)
        {
            return JsonConvert.SerializeObject(envelop);
        }
    }
}
