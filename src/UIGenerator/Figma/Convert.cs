using FTOptix.Core;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.HMIProject;
using FigmaLink.Model;
using FTOptix.UI;
using Newtonsoft.Json.Linq;
using System.Linq;
using FTOptix.CODESYS;
using System;
namespace UIGenerator.Figma
{




    public class Convert
    {
        const string COMPONENT_NAME_FIX = "Widget";
        const string WINDOW_NAME_FIX = "Widget";
        public static IUANode BuildComponent(GenerateManager manager, COMPONENT cmpt)
        {
            var name = CreateBrowseName(cmpt);
            NodeId eventTypeId = new NodeId(NamespaceMapProvider.GetNamespaceIndex("urn:FTOptix:UI"), 60u);
            var rootNode = InformationModel.MakeObjectType(name, eventTypeId);
            var mgt = new ComponentManager();
            mgt.Source.Id = cmpt.id;
            mgt.Source.Tag = cmpt;
            mgt.Target.Id = rootNode.NodeId;
            mgt.Target.Tag = rootNode;
            manager.Components.Add(mgt);
            CreateVariables(mgt, cmpt, rootNode);
            TransferProperty(mgt, cmpt, rootNode as IUANode, true);

            CreateAllChildren(mgt, cmpt, rootNode, true);









            return rootNode;
        }



        public static IUANode BuildWindow(GenerateManager manager, FRAME frame)
        {
            var name = CreateBrowseName(frame);
            NodeId eventTypeId = new NodeId(NamespaceMapProvider.GetNamespaceIndex("urn:FTOptix:UI"), 60u);
            var rootNode = InformationModel.MakeObjectType(name, eventTypeId);
            var mgt = new WindowManager();
            mgt.Source.Id = frame.id;
            mgt.Source.Tag = frame;
            mgt.Target.Id = rootNode.NodeId;
            mgt.Target.Tag = rootNode;
            manager.Windows.Add(mgt);

            TransferProperty(mgt, frame, rootNode as IUANode, true);

            CreateAllChildren(mgt, frame, rootNode, false, manager);



            return rootNode;
        }

        private static string CreateBrowseName(IBaseNode node, bool isRoot = true)
        {
            if (isRoot)
            {

                var id = node.id.Replace(':', '_');

                var name = node.name.Replace(" ", "_");

                return $"{COMPONENT_NAME_FIX}_{name}_{id}";
            }
            else
            {
                var name = node.name.Replace(" ", "_");
                return name;
            }
        }

        private static void CreateVariables(ComponentManager manager, COMPONENT cmpt, IUANode parent)
        {

            var ps = cmpt.GetPropertyDefinitions;
            foreach (var p in ps)
            {
                var name = p.name;
                var datatype = p.datatype;
                var names = name.Split('#');

                var _name = names[0];
                NodeId _datatype = OpcUa.DataTypes.LocalizedText;
                switch (datatype.ToUpper())
                {
                    case "BOOLEAN":
                        _datatype = OpcUa.DataTypes.Boolean;
                        break;
                    case "TEXT":
                        _datatype = OpcUa.DataTypes.LocalizedText;
                        break;
                }


                var v = InformationModel.MakeVariable(_name, _datatype);
                parent.Add(v);


                manager.PropertyMappers.Add(new PropertyMapper() { SourceId = name, TargetId = v.NodeId, Target = v });
            }
        }



        private static void CreateAllChildren(IManager mgt, IBaseNode root, IUANode parent, bool isComponent, GenerateManager gmgt = null)
        {
            foreach (var c in root.Children)
            {
                var (ctrl, isInstance, cmgt) = CreateControl(c, gmgt);
                if (ctrl != null)
                {

                    parent.Add(ctrl);
                    TransferProperty(mgt, c as GraphicBaseNode, ctrl, false);
                    if (isComponent == true)
                    {

                        //link property
                        LinkProperty(mgt as ComponentManager, c as GraphicBaseNode, ctrl);
                    }

                    if (isInstance && cmgt != null)
                    {
                        //set instance property value
                        SetInstancePropertyValue(cmgt, c as INSTANCE, ctrl);

                    }

                    if (!isInstance)
                    {

                        CreateAllChildren(mgt, c, ctrl, isComponent, gmgt);
                    }
                }


            }
        }


        /// <summary>
        /// 创建Optix UI 控件
        /// </summary>
        /// <param name="node"></param>
        /// <param name="gmgt"></param>
        /// <returns>
        ///     /// 创建出来的 UI控件
        ///     /// 是否是 instance 实例化的组件
        ///     /// instance 的模板 component manager
        /// </returns>
        private static (Item, bool, ComponentManager) CreateControl(IBaseNode node, GenerateManager gmgt = null)
        {
            var name = CreateBrowseName(node, false);
            if (node.Type == typeof(FigmaLink.Model.RECTANGLE))
            {
                var gnode = (node as GraphicBaseNode);
                if (gnode != null)
                {
                    if (gnode.fills.Count > 0)
                    {
                        if (gnode.fills[0].GetType() == typeof(IMAGE))
                        {
                            return (InformationModel.MakeObject<Image>(name), false, null);
                        }
                    }
                }
                return (InformationModel.MakeObject<Rectangle>(name), false, null);
            }
            else if (node.Type == typeof(FigmaLink.Model.ELLIPSE))
            {
                return (InformationModel.MakeObject<Ellipse>(name), false, null);
            }
            else if (node.Type == typeof(FigmaLink.Model.TEXT))
            {
                return (InformationModel.MakeObject<Label>(name), false, null);
            }
            else if (node.Type == typeof(FigmaLink.Model.GROUP))
            {
                return (InformationModel.MakeObject<Panel>(name), false, null);
            }
            else if (node.Type == typeof(FigmaLink.Model.VECTOR))
            {
                return (InformationModel.MakeObject<Panel>(name), false, null);
            }
            else if (node.Type == typeof(FigmaLink.Model.FRAME))
            {
                return (InformationModel.MakeObject<Rectangle>(name), false, null);
            }
            else if (node.Type == typeof(FigmaLink.Model.INSTANCE))
            {
                var ins = (node as INSTANCE);
                var componentId = ins.mainComponent.id;
                if (gmgt != null)
                {
                    var cmgt = gmgt.Components.Where(c => c.Source.Id.ToString() == componentId).FirstOrDefault();
                    NodeId uiId = null;
                    if (cmgt != null)
                    {
                        uiId = cmgt.Target.Id as NodeId;
                    }
                    // var uiId = gmgt.Components.Where(c=>c.Source.Id.ToString() == componentId).FirstOrDefault()?.Target.Id as NodeId;
                    if (uiId != null)
                    {
                        return (InformationModel.MakeObject(name, uiId) as Item, true, cmgt);
                    }
                    else
                    {

                        throw new Exception($"create instance : miss the type :{componentId}");
                        // return (null, true, null);
                    }
                }
                return (null, true, null);
            }
            else
            {
                throw new Exception($"create control : miss the type :{node.Type.FullName}");
            }

        }


        private static void TransferProperty(IManager manager, GraphicBaseNode source, IUANode target, bool isRoot = false)
        {
            if (source == null)
            {
                return;
            }
            TransferProperty_Size(source, target, isRoot);
            TransferProperty_Appearance(source, target);
            TransferProperty_Fill(source, target);
            TransferProperty_Border(source, target);

            if (target.GetType() == typeof(Label))
            {

                TransferProperty_Text(source, target);
            }
            if (target.GetType() == typeof(Image))
            {

                // TransferProperty_Text(source, target);
            }




        }


        private static void TransferProperty_Size(GraphicBaseNode source, IUANode target, bool isRoot)
        {

            Utils.SetWidth(target, (float)source.width);
            Utils.SetHeight(target, (float)source.height);

            //todo 处理在 figma group里面的东西

            if (!isRoot)
            {

                Utils.SetLeftMargin(target, (float)source.x);
                Utils.SetTopMargin(target, (float)source.y);
            }

        }

        private static void TransferProperty_Appearance(GraphicBaseNode source, IUANode target)
        {

            var _opacity = source.opacity * 100;
            if (_opacity > 100)
            {
                _opacity = 100;
            }
            else if (_opacity < 0)
            {
                _opacity = 0;
            }
            Utils.SetOpacity(target, _opacity);
            Utils.SetVisible(target, source.visible);


        }

        private static void TransferProperty_Fill(GraphicBaseNode source, IUANode target)
        {



            if (source.fills == null)
            {
                return;
            }
            if (source.fills.Count > 0)
            {
                var c = ConvertColor(source.fills[0]);

                if (target.GetType() == typeof(Label))
                {

                    Utils.SetTextColor(target, c);
                }
                else if (target.GetType() == typeof(Image))
                {
                    //TODO 图片填充怎么办
                }
                else
                {

                    Utils.SetFillColor(target, c);
                }
            }
            else
            {
                Utils.SetFillColor(target, Colors.Transparent);
            }


        }


        private static void TransferProperty_Border(GraphicBaseNode source, IUANode target)
        {
            if (source.strokes == null)
            {
                return;
            }
            if (source.strokes.Count > 0)
            {

            }
            else
            {
                Utils.SetBorderColor(target, Colors.Transparent);
            }


        }


        private static void TransferProperty_Text(GraphicBaseNode source, IUANode target)
        {
            if (target.GetType() == typeof(Label)

            )
            {
                var textNode = (source as TEXT);
                if (textNode != null)
                {
                    Utils.SetText(target, textNode.characters);

                    switch (textNode.textAlignHorizontal)
                    {
                        case TextAlignHorizontalType.NONE:
                        case TextAlignHorizontalType.LEFT:
                            Utils.SetTextHorizontalAlignment(target, TextHorizontalAlignment.Left);
                            break;
                        case TextAlignHorizontalType.RIGHT:

                            Utils.SetTextHorizontalAlignment(target, TextHorizontalAlignment.Right);
                            break;
                        case TextAlignHorizontalType.CENTER:

                            Utils.SetTextHorizontalAlignment(target, TextHorizontalAlignment.Center);
                            break;
                    }
                    switch (textNode.textAlignVertical)
                    {
                        case TextAlignVerticalType.NONE:
                        case TextAlignVerticalType.TOP:
                            Utils.SetTextVerticalAlignment(target, TextVerticalAlignment.Top);
                            break;
                        case TextAlignVerticalType.BOTTOM:

                            Utils.SetTextVerticalAlignment(target, TextVerticalAlignment.Bottom);
                            break;
                        case TextAlignVerticalType.CENTER:

                            Utils.SetTextVerticalAlignment(target, TextVerticalAlignment.Center);
                            break;

                    }

                    if (textNode.fontName.family != null)
                    {
                        Utils.SetFontFamily(target, textNode.fontName.family);

                    }
                    Utils.SetFontSize(target, textNode.fontSize);
                    Utils.SetFontWeight(target, (int)textNode.fontWeight);

                }
            }
        }



        /// <summary>
        /// 根据 figma ，链接 OptixUI内与自定义属性的链接  dynamic link
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        private static void LinkProperty(ComponentManager manager, GraphicBaseNode source, IUANode target)
        {
            var vv = source.componentPropertyReferences;
            if (vv == null)
            {
                return;
            }
            if (source.componentPropertyReferences.GetType() == typeof(JObject))
            {
                var refs = vv as JObject;
                foreach (var kv in refs.Children())
                {
                    var source_property = kv.Path;
                    var property_id = (string)kv;

                    var hit = manager.PropertyMappers.Where(c => c.SourceId.ToString() == property_id).FirstOrDefault();
                    if (hit != null)
                    {
                        var varNodeId = hit.TargetId as NodeId;

                        switch (source_property)
                        {
                            case "characters":
                                Utils.SetDynamicLink(target, "Text", varNodeId);
                                break;
                            case "visible":
                                Utils.SetDynamicLink(target, "Visible", varNodeId);
                                break;
                        }
                    }

                }
            }
        }


        /// <summary>
        /// 根据 figma 的设计，把实例化后的 optix ui对象的自定义属性赋值一下
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="source"></param>
        /// <param name="target"></param> <summary>
        /// 
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        private static void SetInstancePropertyValue(ComponentManager manager, INSTANCE source, IUANode target)
        {
            var vv = source.componentProperties;
            if (vv == null)
            {
                return;
            }
            if (source.componentProperties.GetType() == typeof(JObject))
            {
                var refs = vv as JObject;
                foreach (var kv in refs.Children())
                {
                    var source_property_name = kv.Path; //figma 中属性的名称 id
                    var property_datatype = (string)kv.First["$type"];   //figma 中属性的值

                    object property_value=null;
                    switch (property_datatype)
                    {
                        case "BOOLEAN":

                            property_value = (bool)kv.First["value"];   //figma 中属性的值
                            break;
                        case "TEXT":

                            property_value = (string)kv.First["value"];   //figma 中属性的值
                            break;
                        
                    }


                    var target_property = manager.PropertyMappers.Where(p => p.SourceId.ToString() == source_property_name).FirstOrDefault()?.Target as IUAVariable;
                    if (target_property != null)
                    {

                        var v = target.GetVariable(target_property.BrowseName);
                        if (v != null)
                        {
                            try
                            {
                                if(property_value == null){
                                    return;
                                }
                                Utils.SetVariableValue(v, property_value);
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                    }



                }
            }
        }


        private static FTOptix.Core.Color ConvertColor(object oldColor)
        {
            if (oldColor.GetType() == typeof(SOLID))
            {
                var color = (oldColor as SOLID);
                var newColor = new FTOptix.Core.Color(
                     (byte)(color.opacity * 255)
                    , (byte)(color.color.r * 255)
                    , (byte)(color.color.g * 255)
                    , (byte)(color.color.b * 255));

                return newColor;
            }
            else
            {
                return Colors.White;
            }
        }
    }

}