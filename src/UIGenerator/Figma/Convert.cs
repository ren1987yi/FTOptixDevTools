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
using FigmaLink;
using FTOptix.CoreBase;
using System.IO;
namespace UIGenerator.Figma
{


    /// <summary>
    /// Figma的转换器
    /// </summary>
    public class Converter
    {


        private GenerateManager Manager;
        private string AssetFolder;
        Asset<FigFile> asset;


        public const string AssetFigName = "Fig";
        const string IMPORT_ASSET_FOLDER = "FIGMA";
        const string COMPONENT_NAME_FIX = "Widget";
        const string WINDOW_NAME_FIX = "Window";
        public Converter(GenerateManager manager)
        {
            Manager = manager;

            var cc = manager.Assets.Where(c => c.Name == AssetFigName).FirstOrDefault();
            if (cc != null)
            {
                asset = cc as Asset<FigFile>;

            }


            var uri = ResourceUri.FromProjectRelativePath("");
            var _path = uri.Uri;
            var _folder = AssetFolder = Path.Combine(_path, IMPORT_ASSET_FOLDER);

            if (!Directory.Exists(_folder))
            {
                Directory.CreateDirectory(_folder);
            }




        }


        /// <summary>
        /// 创建模板
        /// </summary>
        /// <param name="cmpt">figma 模块</param>
        /// <returns></returns>
        public IUANode BuildComponent(COMPONENT cmpt)
        {
            var name = CreateBrowseName(cmpt);
            var rootNode = CreateContainerType(name);
            var mgt = new ComponentManager();
            mgt.Source.Id = cmpt.id;
            mgt.Source.Tag = cmpt;
            mgt.Target.Id = rootNode.NodeId;
            mgt.Target.Tag = rootNode;
            Manager.Components.Add(mgt);
            CreateVariables(mgt, cmpt, rootNode);
            TransferProperty(mgt, cmpt, rootNode as IUANode, true);

            CreateAllChildren(mgt, cmpt, rootNode, true);




            return rootNode;
        }




        /// <summary>
        /// 创建画面
        /// </summary>
        /// <param name="frame">figma 画面</param>
        /// <returns></returns>
        public IUANode BuildWindow(FRAME frame)
        {
            var name = CreateBrowseName(frame);
            var rootNode = CreateContainerType(name);
            var mgt = new WindowManager();
            mgt.Source.Id = frame.id;
            mgt.Source.Tag = frame;
            mgt.Target.Id = rootNode.NodeId;
            mgt.Target.Tag = rootNode;
            Manager.Windows.Add(mgt);

            TransferProperty(mgt, frame, rootNode as IUANode, true);

            CreateAllChildren(mgt, frame, rootNode, false);



            return rootNode;
        }

        /// <summary>
        /// 生成optix的名称，根据figma里的名称
        /// </summary>
        /// <param name="node">figma node</param>
        /// <param name="isRoot">是否是根节点</param>
        /// <returns></returns>
        private string CreateBrowseName(IBaseNode node, bool isRoot = true)
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


        /// <summary>
        /// 创建模板 optix 的接口变量
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="cmpt"></param>
        /// <param name="parent"></param>
        private void CreateVariables(ComponentManager manager, COMPONENT cmpt, IUANode parent)
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


        /// <summary>
        /// 创建所有子节点
        /// </summary>
        /// <param name="mgt">管理器</param>
        /// <param name="root">figma父节点</param>
        /// <param name="parent">optix父节点</param>
        /// <param name="isComponent">是否是模板</param>
        private void CreateAllChildren(IManager mgt, IBaseNode root, IUANode parent, bool isComponent)
        {
            foreach (var c in root.Children)
            {
                //ctrl 控件
                //isInstance 是否是实例
                //cmgt 对应的模板管理器
                var (ctrl, isInstance, cmgt) = CreateControl(c, Manager);

                if (ctrl != null)
                {

                    parent.Add(ctrl);
                    TransferProperty(mgt, c as GraphicBaseNode, ctrl, false);
                    if (isComponent == true)
                    {

                        //link property
                        LinkProperty(mgt as ComponentManager, c as GraphicBaseNode, ctrl);
                    }

                    //是模块实例化，并且找到了模板
                    if (isInstance && cmgt != null)
                    {
                        //set instance property value
                        SetInstancePropertyValue(cmgt, c as INSTANCE, ctrl);

                    }

                    if (!isInstance)
                    {

                        CreateAllChildren(mgt, c, ctrl, isComponent);
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
        private (Item, bool, ComponentManager) CreateControl(IBaseNode node, GenerateManager gmgt = null)
        {
            var name = CreateBrowseName(node, false);
            if (node.Type == typeof(FigmaLink.Model.RECTANGLE))
            {
                //image 填充的对应 Optix.Image
                //否则 Optix.Rectangle

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
                //TODO 矢量控件的转换，里面的内容怎么转过去
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
                    //找到模板管理器
                    var cmgt = gmgt.Components.Where(c => c.Source.Id.ToString() == componentId).FirstOrDefault();

                    //提取到optix里，模板 type 的nodeId
                    NodeId uiId = null;
                    if (cmgt != null)
                    {
                        uiId = cmgt.Target.Id as NodeId;
                    }


                    if (uiId != null)
                    {
                        //创建实例
                        return (InformationModel.MakeObject(name, uiId) as Item, true, cmgt);
                    }
                    else
                    {

                        throw new Exception($"create instance : miss the type :{componentId}");

                    }
                }
                return (null, true, null);
            }
            else
            {
                throw new Exception($"create control : miss the type :{node.Type.FullName}");
            }

        }

        /// <summary>
        /// 转换属性
        /// </summary>
        /// <param name="manager">管理器 window/component</param>
        /// <param name="source">figma node</param>
        /// <param name="target">optix node</param>
        /// <param name="isRoot">是否是根节点</param>
        private void TransferProperty(IManager manager, GraphicBaseNode source, IUANode target, bool isRoot = false)
        {
            if (source == null)
            {
                return;
            }
            //外观
            TransferProperty_Appearance(source, target);
            //尺寸
            TransferProperty_Size(source, target, isRoot);
            //填充
            TransferProperty_Fill(source, target);
            //边框
            TransferProperty_Border(source, target);

            if (target.GetType() == typeof(Label))
            {
                //文字
                TransferProperty_Text(source, target);
            }
            // else if (target.GetType() == typeof(Image))
            // {

            //     // TransferProperty_Text(source, target);
            // }




        }


        /// <summary>
        /// 转换 size
        /// x,y,width,height
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="isRoot"></param>
        private void TransferProperty_Size(GraphicBaseNode source, IUANode target, bool isRoot)
        {

            Utils.SetWidth(target, (float)source.width);
            Utils.SetHeight(target, (float)source.height);

            //todo 处理在 figma group里面的东西,他里面的坐标都是相对根节点的

            if (!isRoot)
            {

                Utils.SetLeftMargin(target, (float)source.x);
                Utils.SetTopMargin(target, (float)source.y);
            }

        }


        /// <summary>
        /// 转换外观 可见性，透明度，圆角。。。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        private void TransferProperty_Appearance(GraphicBaseNode source, IUANode target)
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

            if (target.GetType() == typeof(Rectangle))
            {

                Utils.SetCornerRadius(target, source.cornerRadius);
            }

            //set the description
            target.Description = new LocalizedText(NodeId.InvalidNamespaceIndex, source.description);

        }

        /// <summary>
        /// 转换填充
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        private void TransferProperty_Fill(GraphicBaseNode source, IUANode target)
        {



            if (source.fills == null)
            {
                return;
            }
            if (source.fills.Count > 0)
            {



                if (target.GetType() == typeof(Label))
                {
                    //填充文字颜色
                    var c = ConvertColor(source.fills[0]);
                    Utils.SetTextColor(target, c);
                }
                else if (target.GetType() == typeof(Image))
                {
                    
                    var img = target as Image;
                    var _fillimage = source.fills[0] as IMAGE;

                    // img.Rotation = _fillimage.rotation;
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

                    //图片的链接
                    var source_img_path = asset.ActualItem.GetImage(_fillimage.imageHash);
                    if (source_img_path != null)
                    {

                        var dest_img_path = Path.Combine(AssetFolder, _fillimage.imageHash + ".png");

                        File.Copy(source_img_path, dest_img_path, true);

                        img.Path = ResourceUri.FromProjectRelativePath(Path.Combine(dest_img_path));
                    }

                }
                else
                {
                    var c = ConvertColor(source.fills[0]);
                    Utils.SetFillColor(target, c);
                }
            }
            else
            {
                Utils.SetFillColor(target, Colors.Transparent);
            }


        }


        /// <summary>
        /// 转换边框
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        private void TransferProperty_Border(GraphicBaseNode source, IUANode target)
        {
            if (source.strokes == null)
            {
                return;
            }
            if (source.strokes.Count > 0)
            {
                var c = ConvertColor(source.strokes[0]);

                if (target.GetType() == typeof(Label))
                {

                    // Utils.SetTextColor(target, c);
                }
                else if (target.GetType() == typeof(Image))
                {
                    //TODO 图片填充怎么办，xian pass
                }
                else
                {

                    Utils.SetBorderColor(target, c);


                }
            }
            else
            {
                Utils.SetBorderColor(target, Colors.Transparent);
            }


            if (target.GetType() == typeof(Rectangle)
            || target.GetType() == typeof(Ellipse))
            {

                Utils.SetBorderThickness(target, source.strokeWeight);
            }



        }


        /// <summary>
        /// 转换文字
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        private void TransferProperty_Text(GraphicBaseNode source, IUANode target)
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
        private void LinkProperty(ComponentManager manager, GraphicBaseNode source, IUANode target)
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
        private void SetInstancePropertyValue(ComponentManager manager, INSTANCE source, IUANode target)
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

                    object property_value = null;
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
                                if (property_value == null)
                                {
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


        /// <summary>
        /// 转换颜色
        /// </summary>
        /// <param name="oldColor"></param>
        /// <returns></returns>
        private FTOptix.Core.Color ConvertColor(object oldColor)
        {
            if (oldColor.GetType() == typeof(SOLID))
            {

                var color = (oldColor as SOLID);
                if (color.visible)
                {

                    var newColor = new FTOptix.Core.Color(
                         (byte)(color.opacity * 255)
                        , (byte)(color.color.r * 255)
                        , (byte)(color.color.g * 255)
                        , (byte)(color.color.b * 255));

                    return newColor;
                }
                else
                {
                    var newColor = new FTOptix.Core.Color(
                     (byte)(color.opacity * 255 * 0)
                    , (byte)(color.color.r * 255)
                    , (byte)(color.color.g * 255)
                    , (byte)(color.color.b * 255));

                    return newColor;
                }
            }
            else
            {
                return Colors.Transparent;
            }
        }


        internal IUAObjectType CreateContainerType(string name)
        {
            NodeId eventTypeId = new NodeId(NamespaceMapProvider.GetNamespaceIndex("urn:FTOptix:UI"), 60u);
            var rootNode = InformationModel.MakeObjectType(name, eventTypeId);
            return rootNode;
        }
    }

}