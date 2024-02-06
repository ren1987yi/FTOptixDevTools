using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
namespace FigmaLink.Model{
    public class BaseNode{


        public string id{
            get{
                return (string)_jobject.SelectToken("id");
            }
            
        }
        
        public string type{
            get{
                return (string)_jobject.SelectToken("type");
            }
            
        }

        public string name{
            get{
                return (string)_jobject.SelectToken("name");
            }
            
        }

        public bool isAsset{
            get{
                return (bool)_jobject.SelectToken("isAsset");
            }
            
        }

        public JArray children{
            get{
                return _jobject.SelectToken("children") as JArray;
            }
        }


        protected JObject _jobject;

        public BaseNode(JObject jobject){
            _jobject = jobject;
        }
    }
}