
using FTOptix.Core;
using UAManagedCore;
using FTOptix.HMIProject;
using FigmaLink;
using FigmaLink.Model;
using FigmaLink.Extensions;
using UIGenerator;
using System;
using System.Collections.Generic;
namespace UIGenerator
{



    public class Generator
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target">生成UI的目标节点，一般是Folder</param>
        /// <param name="sourceType"></param>
        /// <param name="jsonfilepath">figma里面 导出的JSON文件</param>
        /// <param name="jsonfilepath">figma里面 导出的本地fig文件</param>
        /// <param name="setting"></param>
        public static void ConvertForFigmaFile(object target, string jsonfilepath, string figfilepath = null, GenerateSetting setting = null)
        {
            IUANode targetNode = target as IUANode;
            if (setting == null)
            {
                setting = new GenerateSetting();
            }

            IUANode[] results = null;



            //读取 figma json文件
            var doc = DOCUMENT.LoadFile(jsonfilepath);

            FigFile fig = null;
            if (!string.IsNullOrWhiteSpace(figfilepath))
            {

                fig = FigFile.LoadFromFile(figfilepath);
            }

            var nodes = FigmaConvert(doc, fig, setting);
            results = nodes;



            if (results != null)
            {

                if (targetNode == null)
                {
                    throw new Exception("TargetNode is null");

                }




                if (setting.ClearBuildFolder)
                {
                    foreach (var item in targetNode.Children)
                    {
                        item.Delete();
                    }
                }
                foreach (var node in results)
                {
                    targetNode.Add(node);
                }
            }
        }


        internal static IUANode[] FigmaConvert(DOCUMENT doc, FigFile fig, GenerateSetting setting)
        {
            var manager = new GenerateManager();

            manager.Assets.Add(new Asset<FigFile>(Figma.Converter.AssetFigName,fig));

            var convert = new Figma.Converter(manager);

            var folders = new List<IUANode>();
            if (setting.EnableComponent)
            {
                var folder = InformationModel.MakeObject<Folder>(setting.ComponentFolderName);
                folders.Add(folder);

                var figma_components = doc.GetComponents();
                foreach (var c in figma_components)
                {
                    var _c = convert.BuildComponent( c);
                    if (_c != null)
                    {

                        folder.Add(_c);
                    }
                }
            }

            if (setting.EnableWindow && setting.EnableComponent)
            {
                var folder = InformationModel.MakeObject<Folder>(setting.WindowFolderName);
                folders.Add(folder);


                var figma_frames = doc.GetFrames();
                foreach (var c in figma_frames)
                {
                    var _c = convert.BuildWindow(c);
                    if (_c != null)
                    {

                        folder.Add(_c);
                    }
                }
            }

            return folders.ToArray();

        }


    }

    public interface IGenerator
    {

    }


}