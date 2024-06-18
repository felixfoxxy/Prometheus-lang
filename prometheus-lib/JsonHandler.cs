using Newtonsoft.Json;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace prometheus
{
    public static class JsonHandler
    {
        public static string ConvertToString(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static T ConvertToObj<T>(string obj)
        {
            return JsonConvert.DeserializeObject<T>(obj);
        }
    }
}
