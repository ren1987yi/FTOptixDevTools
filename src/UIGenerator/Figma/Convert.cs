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

            CreateVariables(mgt, cmpt, rootNode);
            TransferProperty(mgt, cmpt, rootNode as IUANode, true);

            CreateAllChildren(mgt, cmpt, rootNode);






            manager.Components.Add(mgt);
            return rootNode;
        }


        private static string CreateBrowseName(IBaseNode node, bool isRoot = true)
        {
            if (isRoot)
            {

                var id = node.id.Replace(':', '_');
                return $"{COMPONENT_NAME_FIX}_{node.name}_{id}";
            }
            else
            {
                return node.name;
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


                manager.PropertyMappers.Add(new PropertyMapper() { SourceId = name, TargetId = v.NodeId });
            }
        }



        private static void CreateAllChildren(ComponentManager manager, IBaseNode root, IUANode parent)
        {
            foreach (var c in root.Children)
            {
                var ctrl = CreateControl(c);
                if (ctrl != null)
                {

                    parent.Add(ctrl);
                    TransferProperty(manager, c as GraphicBaseNode, ctrl, false);
                    CreateAllChildren(manager, c, ctrl);
                }


            }
        }

        private static Item CreateControl(IBaseNode node)
        {
            var name = CreateBrowseName(node, false);
            if (node.Type == typeof(FigmaLink.Model.RECTANGLE))
            {
                return InformationModel.MakeObject<Rectangle>(name);
            }
            else if (node.Type == typeof(FigmaLink.Model.ELLIPSE))
            {
                return InformationModel.MakeObject<Ellipse>(name);
            }
            else if (node.Type == typeof(FigmaLink.Model.TEXT))
            {
                return InformationModel.MakeObject<Label>(name);
            }
            else if (node.Type == typeof(FigmaLink.Model.GROUP))
            {
                return InformationModel.MakeObject<Panel>(name);
            }
            else if (node.Type == typeof(FigmaLink.Model.FRAME))
            {
                return InformationModel.MakeObject<Rectangle>(name);
            }
            else
            {
                throw new Exception("create control : miss the type");
            }
        }


        private static void TransferProperty(ComponentManager manager, GraphicBaseNode source, IUANode target, bool isRoot = false)
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



            //link property
            LinkProperty(manager, source, target);

        }


        private static void TransferProperty_Size(GraphicBaseNode source, IUANode target, bool isRoot)
        {

            Utils.SetWidth(target, (float)source.width);
            Utils.SetHeight(target, (float)source.height);

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


            //TODO 图片填充怎么办
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