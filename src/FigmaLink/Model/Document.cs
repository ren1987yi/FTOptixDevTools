
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace FigmaLink.Model
{



    public class DOCUMENT : BaseNode
    {

        public object detachedInfo { get; set; }
        public string documentColorProfile { get; set; }


        public static DOCUMENT LoadFile(string filepath)
        {
            if (File.Exists(filepath))
            {
                return LoadJson(File.ReadAllText(filepath));
            }
            else
            {
                return null;
            }

        }

        public static DOCUMENT LoadJson(string jsonContent)
        {

            KnownTypesBinder knownTypesBinder = new KnownTypesBinder
            {
                KnownTypes = new List<Type> { typeof(BaseNode), typeof(DOCUMENT), typeof(PAGE) }
            };

            var txt = jsonContent.Replace("\"type\"", "\"$type\"");

            var obj = JsonConvert.DeserializeObject<DOCUMENT>(txt, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                SerializationBinder = knownTypesBinder
            });

            obj.Init();

            return obj;

        }

    }
}