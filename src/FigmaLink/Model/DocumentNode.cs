
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
namespace FigmaLink.Model
{
    public class DocumentNode : BaseNode
    {


        public IEnumerable<PageNode> Pages{
            get{
                
                return children.Children().Where(c=>(string)c.SelectToken("type") == "PAGE").Select(d=>new PageNode((JObject)d));
            }
            
        }

        public string documentColorProfile
        {
            get
            {
                return (string)_jobject.SelectToken("documentColorProfile");
            }

        }
        public object detachedInfo
        {
            get
            {
                return (object)_jobject.SelectToken("detachedInfo");
            }

        }


        public DocumentNode(JObject jobject) : base(jobject)
        {

        }


        public static DocumentNode LoadFile(string filepath)
        {
            Console.WriteLine(Path.GetFullPath(filepath));
            if (File.Exists(filepath))
            {
                var txt = File.ReadAllText(filepath);
                return LoadString(txt);
            }
            return null;
        }

        public static DocumentNode LoadString(string jsonContent)
        {
            // var jsonObject = JsonConvert.DeserializeObject(jsonContent);
            var jsonObject = JObject.Parse(jsonContent);

            var id = (string)jsonObject.SelectToken("id");
            var type = (string)jsonObject.SelectToken("type");
            var name = (string)jsonObject.SelectToken("name");

            var doc = new DocumentNode(jsonObject);




            return doc;
        }
    }
}