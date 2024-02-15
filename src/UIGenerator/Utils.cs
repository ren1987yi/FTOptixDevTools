using System.Collections.Generic;
using FTOptix.Core;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.CoreBase;
using FTOptix.HMIProject;
using FTOptix.UI;
using UAManagedCore;

namespace UIGenerator
{


    public static class Utils
    {
        static void SetProperty(IUANode node, object value, string propertyName)
        {
            var pinfo = node.GetType().GetProperty(propertyName);
            if (pinfo != null)
            {
                pinfo.SetValue(node, value);
            }
            else
            {
                var v = node.GetVariable($"{propertyName}Variable");
                if (v != null)
                {
                    v.Value = new UAValue(value);
                }
            }
        }


        public static void SetTextHorizontalAlignment(IUANode node, TextHorizontalAlignment value)
        {
            SetProperty(node, value, "TextHorizontalAlignment");
            // var v = node.Get("FillColorVariable") as IUAVariable;
            // if (v != null)
            // {

            //     v.Value = new UAValue(value);
            // }

        }


        public static void SetTextVerticalAlignment(IUANode node, TextVerticalAlignment value)
        {
            SetProperty(node, value, "TextVerticalAlignment");
            // var v = node.Get("FillColorVariable") as IUAVariable;
            // if (v != null)
            // {

            //     v.Value = new UAValue(value);
            // }

        }
        public static void SetText(IUANode node, string value)
        {
            SetProperty(node, value, "Text");
            // var v = node.Get("FillColorVariable") as IUAVariable;
            // if (v != null)
            // {

            //     v.Value = new UAValue(value);
            // }

        }


        public static void SetFontFamily(IUANode node, string value)
        {
            SetProperty(node, value, "FontFamily");
            // var v = node.Get("FillColorVariable") as IUAVariable;
            // if (v != null)
            // {

            //     v.Value = new UAValue(value);
            // }

        }
        public static void SetFontSize(IUANode node, float value)
        {
            SetProperty(node, value, "FontSize");
            // var v = node.Get("FillColorVariable") as IUAVariable;
            // if (v != null)
            // {

            //     v.Value = new UAValue(value);
            // }

        }

        public static void SetDynamicLink(IUANode node, string variableName, NodeId sourceId)
        {
            IUAVariable v = node.GetVariable(variableName);
            if (v != null)
            {
                var source = InformationModel.GetVariable(sourceId);
                if (source != null)
                {
                    v.SetDynamicLink(source);
                    var dl = v.Get(nameof(DynamicLink));
                    if (dl != null)
                    {
                        var dlink = dl as DynamicLink;
                        var path = GetRelativeNodePath(dlink, source);
                        if (string.IsNullOrEmpty(path))
                        {

                        }
                        else
                        {
                            dlink.Value = path;
                        }
                    }
                }
            }

        }


        public static void SetVariableValue(IUAVariable v, object value)
        {
            v.Value = new UAValue(value);

        }

        /// <summary>
        /// 向上找
        /// </summary>
        /// <param name="source">起点节点</param>
        /// <param name="target">终点的节点</param>
        /// <returns></returns>
        public static string GetRelativeNodePath(IUANode source, IUANode target)
        {
            var _paths = new List<string>();

            var curNode = source;
            if (curNode.Owner == null)
            {
                return null;
            }
            else
            {
                curNode = curNode.Owner;

            }
            var found = false;
            do
            {

                foreach (var item in curNode.Children)
                {
                    if (item.NodeId == target.NodeId)
                    {
                        found = true;
                        _paths.Add(target.BrowseName);
                    }
                }
                if (!found)
                {
                    if (curNode.Owner == null)
                    {
                        break;
                    }
                    else
                    {
                        curNode = curNode.Owner;
                        _paths.Add("..");
                    }
                }

            } while (!found);




            // _paths.Reverse();

            return string.Join("/", _paths);


        }


        public static void SetFontWeight(IUANode node, int value)
        {
            SetProperty(node, value, "FontWeight");
            // var v = node.Get("FillColorVariable") as IUAVariable;
            // if (v != null)
            // {

            //     v.Value = new UAValue(value);
            // }

        }

        public static void SetFillColor(IUANode node, Color value)
        {
            SetProperty(node, value, "FillColor");
            // var v = node.Get("FillColorVariable") as IUAVariable;
            // if (v != null)
            // {

            //     v.Value = new UAValue(value);
            // }

        }

        public static void SetTextColor(IUANode node, Color value)
        {
            SetProperty(node, value, "TextColor");
            // var v = node.Get("FillColorVariable") as IUAVariable;
            // if (v != null)
            // {

            //     v.Value = new UAValue(value);
            // }

        }


        public static void SetBorderColor(IUANode node, Color value)
        {
            SetProperty(node, value, "BorderColor");


            // var v = node.Get("BorderColorVariable") as IUAVariable;
            // if (v != null)
            // {

            //     v.Value = new UAValue(color);
            // }

        }

        public static void SetBorderThickness(IUANode node, float value)
        {
            SetProperty(node, value, "BorderThickness");



        }

        public static void SetCornerRadius(IUANode node, float value)
        {
            SetProperty(node, value, "CornerRadius");

            // Rectangle a;
            //         a.CornerRadius

        }
        public static void SetHeight(IUANode node, float value)
        {
            SetProperty(node, value, "Height");
            // var v = node.Get("HeightVariable") as IUAVariable;
            // if (v != null)
            // {

            //     v.Value = new UAValue(value);
            // }

        }
        public static void SetWidth(IUANode node, float value)
        {
            SetProperty(node, value, "Width");
            // var v = node.Get("WidthVariable") as IUAVariable;
            // if (v != null)
            // {

            //     v.Value = new UAValue(value);
            // }


        }




        public static void SetVisible(IUANode node, bool value)
        {
            SetProperty(node, value, "Visible");
            // var v = node.Get("VisibleVariable") as IUAVariable;
            // if (v != null)
            // {

            //     v.Value = new UAValue(value);
            // }

        }


        public static void SetOpacity(IUANode node, float value)
        {
            SetProperty(node, value, "Opacity");
            // var v = node.Get("OpacityVariable") as IUAVariable;
            // if (v != null)
            // {

            //     v.Value = new UAValue(value * 100);
            // }
            // else
            // {

            // }

        }

        public static void SetLeftMargin(IUANode node, float value)
        {
            SetProperty(node, value, "LeftMargin");
            // var v = node.Get("LeftMarginVariable") as IUAVariable;
            // if (v != null)
            // {

            //     v.Value = new UAValue(value);
            // }

        }

        public static void SetTopMargin(IUANode node, float value)
        {
            SetProperty(node, value, "TopMargin");
            // var v = node.Get("TopMarginVariable") as IUAVariable;
            // if (v != null)
            // {

            //     v.Value = new UAValue(value);
            // }

        }




    }

}