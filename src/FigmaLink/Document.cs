using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using FigmaLink.Model;
namespace FigmaLink{
    public static class Document{
        public static dynamic LoadFile(string filepath){
            if(File.Exists(filepath)){
                return LoadJson(File.ReadAllText(filepath));
            }else{
                return null;
            }
        }


        public static dynamic LoadJson(string json){
            dynamic obj = JObject.Parse(json);
            return obj;
        }

    }
}