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
            set { _children = value; }
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
                    catch(Exception ex)
                    {
                        Console.WriteLine("1");
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


        public string id { get; set; }

        public string name { get; set; }
        public bool isAsset { get; set; }

        public JArray jsonChildren { get; set; }


        public void Init();

    }


    // public class FigmaJsonConverter : JsonConverter
    // {
    //     private readonly Type[] _types;

    //     public FigmaJsonConverter(params Type[] types)
    //     {
    //         _types = types;
    //     }

    //     public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    //     {
    //         JToken t = JToken.FromObject(value);

    //         if (t.Type != JTokenType.Object)
    //         {
    //             t.WriteTo(writer);
    //         }
    //         else
    //         {
    //             JObject o = (JObject)t;
    //             IList<string> propertyNames = o.Properties().Select(p => p.Name).ToList();

    //             o.AddFirst(new JProperty("Keys", new JArray(propertyNames)));

    //             o.WriteTo(writer);
    //         }
    //     }

    //     public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    //     {
    //         // throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");

    //         var aa = reader.Value;
    //         return new BaseNode();
    //         return null;
    //     }

    //     public override bool CanRead
    //     {
    //         get { return true; }
    //     }

    //     public override bool CanConvert(Type objectType)
    //     {
    //         return _types.Any(t => t == objectType);
    //     }
    // }



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