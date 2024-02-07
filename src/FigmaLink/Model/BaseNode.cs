using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Linq;
using Newtonsoft.Json.Serialization;
namespace FigmaLink.Model
{
    public class BaseNode : IBaseNode
    {
        public Type Type
        {
            get
            {
                return this.GetType();
            }
        }

        public string id { get; set; }

        public string name { get; set; }
        public bool isAsset { get; set; }

        [JsonProperty("children")]
        public JArray jsonChildren { get; set; }


        private List<IBaseNode> _children = new List<IBaseNode>();
        [JsonIgnore]
        public List<IBaseNode> Children
        {
            get { return _children; }
            private set { _children = value; }
        }




        public virtual void Init()
        {
            KnownTypesBinder knownTypesBinder = new KnownTypesBinder
            {
                KnownTypes = new List<Type> {
                    typeof(BaseNode)
                    , typeof(DOCUMENT)
                    , typeof(PAGE)
                    , typeof(FRAME)
                    , typeof(GROUP)
                    , typeof(VECTOR)
                    , typeof(IMAGE)
                    , typeof(COMPONENT)
                    , typeof(RECTANGLE)
                    , typeof(INSTANCE)
                    , typeof(SOLID)
                    , typeof(NODE)
                    , typeof(ON_CLICK)
                    , typeof(BACK)
                    }
            };

            if (jsonChildren == null)
            {
                return;
            }
            foreach (var item in jsonChildren)
            {
                var vv = (string)item.SelectToken("$type");
                if (vv == "ON_CLICK")
                {

                }
                Type type = Type.GetType("FigmaLink.Model." + vv);
                if (type != null)
                {
                    var _json = item.ToString();
                    try
                    {
                        var page = JsonConvert.DeserializeObject(item.ToString(), type, new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.Objects,
                            SerializationBinder = knownTypesBinder
                        });
                        (page as IBaseNode).Init();
                        this.Children.Add(page as IBaseNode);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
                else
                {
                    throw new NullReferenceException("miss type");
                }
            }
        }

    }

    public interface IBaseNode
    {

        public Type Type { get; }
        public string id { get; set; }

        public string name { get; set; }
        public bool isAsset { get; set; }

        public JArray jsonChildren { get; set; }
        public List<IBaseNode> Children{get;}

        public void Init();

    }


    public class KnownTypesBinder : ISerializationBinder
    {
        public IList<Type> KnownTypes { get; set; }

        public Type BindToType(string assemblyName, string typeName)
        {
            return KnownTypes.SingleOrDefault(t => t.Name == typeName);
        }

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = serializedType.Name;
        }
    }

}