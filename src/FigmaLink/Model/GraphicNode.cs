
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FigmaLink.Model
{


  /// <summary>
  /// 图形节点
  /// </summary>
  public class GraphicBaseNode : BaseNode
  {


    //属性引用
    public object componentPropertyReferences { get; set; }

    /// <summary>
    /// 可见性
    /// </summary>
    /// <value></value>
    public bool visible { get; set; }
    /// <summary>
    /// x坐标
    /// </summary>
    /// <value></value>
    public float x { get; set; }
    /// <summary>
    /// y坐标
    /// </summary>
    /// <value></value>
    public float y { get; set; }
    /// <summary>
    /// 旋转角度
    /// </summary>
    /// <value></value>
    public float rotation { get; set; }
    /// <summary>
    /// 宽度
    /// </summary>
    /// <value></value>
    public float width { get; set; }
    /// <summary>
    /// 高度
    /// </summary>
    /// <value></value>
    public float height { get; set; }
    /// <summary>
    /// 透明度
    /// </summary>
    /// <value></value>
    public float opacity { get; set; }
    /// <summary>
    /// 描述
    /// </summary>
    /// <value></value>
    public string description { get; set; }
    /// <summary>
    /// 线条宽度
    /// </summary>
    /// <value></value>
    public float strokeWeight { get; set; }
    /// <summary>
    /// 角度半径
    /// </summary>
    /// <value></value>
    public float cornerRadius { get; set; }


    /// <summary>
    /// 背景色
    /// </summary>
    /// <value></value>
    public List<object> backgrounds { get; set; }
    /// <summary>
    /// 填充色
    /// </summary>
    /// <value></value>
    public List<object> fills { get; set; }
    /// <summary>
    /// 线条样式
    /// </summary>
    /// <value></value>
    public List<object> strokes { get; set; }
    /// <summary>
    /// 动作
    /// </summary>
    /// <value></value>
    public List<Reaction> reactions { get; set; }

  }

  public class GROUP : GraphicBaseNode
  {

  }

  public class FRAME : GraphicBaseNode
  {

  }
  public class VECTOR : GraphicBaseNode
  {
    public List<VectorPath> vectorPaths { get; set; }
    public object vectorNetwork { get; set; }

  }


  public class VectorPath
  {
    [JsonProperty("windingRule")]
    public string WindingRule { get; set; }
    [JsonProperty("data")]
    public string Data { get; set; }
  }

  public class COMPONENT : GraphicBaseNode
  {
    public JObject componentPropertyDefinitions { get; set; }

    public IEnumerable<ComponentPropertyDefinition> GetPropertyDefinitions
    {
      get
      {

        if (this.componentPropertyDefinitions != null)
        {
          var defs = new List<ComponentPropertyDefinition>();
          foreach (var token in this.componentPropertyDefinitions.Children())
          {

            var name = token.Path;
            var datatype = (string)this.componentPropertyDefinitions[name]["$type"];
            var defvalue = (string)this.componentPropertyDefinitions[name]["defaultValue"];

            defs.Add(new ComponentPropertyDefinition() { name = name, datatype = datatype, defaultValue = defvalue });
          }
          return defs;
        }
        else
        {

          return null;
        }

      }
    }
  }


  public class RECTANGLE : GraphicBaseNode
  {

  }
  public class INSTANCE : GraphicBaseNode
  {
    public ComponentReference mainComponent { get; set; }

    public JObject componentProperties { get; set; }

    public IEnumerable<ComponentProperty> GetPropertys
    {
      get
      {

        if (this.componentProperties != null)
        {
          var defs = new List<ComponentProperty>();
          foreach (var token in this.componentProperties.Children())
          {

            var name = token.Path;


            var datatype = (string)this.componentProperties[name]["$type"];
            var value = (string)this.componentProperties[name]["value"];

            defs.Add(new ComponentProperty() { name = name, datatype = datatype, value = value });
          }
          return defs;
        }
        else
        {

          return null;
        }

      }
    }
  }


  public class TEXT : GraphicBaseNode
  {
    public Font fontName { get; set; }
    public float fontSize { get; set; }
    public float fontWeight { get; set; }
    public string characters { get; set; }
    public TextAlignHorizontalType textAlignHorizontal { get; set; }
    public TextAlignVerticalType textAlignVertical { get; set; }

    public string textAutoResize { get; set; }

  }

  public class ELLIPSE : GraphicBaseNode
  {

  }


  public class ComponentReference
  {
    public string id { get; set; }
  }


  public class Reaction
  {
    public ActionBase action { get; set; }
    public List<ActionBase> actions { get; set; }
    public ActionTriggerBase trigger { get; set; }
  }
  public class ActionBase
  {

  }



  public class NODE : ActionBase
  {
    public string destinationId { get; set; }
    public string navigation { get; set; }
    public object transition { get; set; }
    public bool reresetVideoPositionset { get; set; }
  }
  public class BACK : ActionBase
  {

  }

  public class ActionTriggerBase
  {
    //public string type { get; set; }
  }


  public class ON_CLICK : ActionTriggerBase
  {

  }



  public class SOLID
  {
    public bool visible { get; set; }
    public float opacity { get; set; }
    public string blendMode { get; set; }
    public Color color { get; set; }

    public object boundVariables { get; set; }
  }

  public class IMAGE
  {
    public bool visible { get; set; }
    public float opacity { get; set; }
    public string blendMode { get; set; }

    public string scaleMode { get; set; }

    public float[][] imageTransform { get; set; }

    public float rotation { get; set; }
    public float scalingFactor { get; set; }

    public string imageHash { get; set; }


  }

  public class Color
  {
    public double r { get; set; }
    public double g { get; set; }
    public double b { get; set; }
  }

  public class ComponentPropertyDefinition
  {
    public string name { get; set; }
    public string datatype { get; set; }
    public string defaultValue { get; set; }
  }

  public class ComponentProperty
  {
    public string name { get; set; }
    public string datatype { get; set; }
    public string value { get; set; }
  }

  public class Font
  {
    public string family { get; set; }
    public string style { get; set; }
  }

  public enum TextAlignHorizontalType
  {
    NONE,
    LEFT,
    RIGHT,
    CENTER
  }

  public enum TextAlignVerticalType
  {
    NONE,
    TOP,
    BOTTOM,
    CENTER,
  }

}