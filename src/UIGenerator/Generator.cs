
using FTOptix.Core;
using UAManagedCore;
using FTOptix.HMIProject;
using FigmaLink;
using FigmaLink.Model;
using FigmaLink.Extensions;
using UIGenerator;
using System;
using System.Collections.Generic;
namespace UIGenerator{



public class Generator
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="targetNode"></param>
    /// <param name="sourceType"></param>
    /// <param name="filepath"></param>
    /// <param name="setting"></param>
    public static void ConvertForFile(object target, UiSourceType sourceType, string filepath, GenerateSetting setting = null)
    {
        IUANode targetNode = target as IUANode;
        if (setting == null)
        {
            setting = new GenerateSetting();
        }

        IUANode[] results = null;

        if (sourceType == UiSourceType.Figma)
        {
            var doc = DOCUMENT.LoadFile(filepath);
            var nodes = FigmaConvert(doc, setting);
            results = nodes;
        }


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


    internal static IUANode[] FigmaConvert(DOCUMENT doc, GenerateSetting setting)
    {
        var manager = new GenerateManager();
        var folders = new List<IUANode>();
        if (setting.EnableComponent)
        {
            var folder = InformationModel.MakeObject<Folder>(setting.ComponentFolderName);
            folders.Add(folder);

            var figma_components = doc.GetComponents();
            foreach (var c in figma_components)
            {
                var _c = Figma.Convert.BuildComponent(manager, c);
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
                var _c = Figma.Convert.BuildWindow(manager, c);
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