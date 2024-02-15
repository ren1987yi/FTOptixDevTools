using System.Reflection.Metadata;
using FigmaLink;
using Newtonsoft.Json;
using FigmaLink.Model;
using FigmaLink.Extensions;
using UIGenerator;
using System;
using System.Collections.Generic;
using System.IO;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

var file = @"../../../../../src/Test/test/Test1.plugin2.json";
var fig =  @"../../../../../src/Test/test/Test1.fig";


if (1 == 1)
{





    KnownTypesBinder knownTypesBinder = new KnownTypesBinder
    {
        KnownTypes = new List<Type> { typeof(BaseNode), typeof(DOCUMENT), typeof(PAGE) }
    };



    var dd = new DOCUMENT();
    dd.id = "aa";

    string json = JsonConvert.SerializeObject(dd, Formatting.Indented, new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.Objects,
        SerializationBinder = knownTypesBinder
    });



    if (File.Exists(file))
    {
        var txt = File.ReadAllText(file);
        txt = txt.Replace("\"type\"", "\"$type\"");
        // var obj = JsonConvert.DeserializeObject<FigmaLink.Model.BaseNode>(txt,new JsonSerializerSettings{
        //     ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
        //     TypeNameHandling = TypeNameHandling.All
        // });



        var obj = JsonConvert.DeserializeObject<DOCUMENT>(txt, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
            SerializationBinder = knownTypesBinder
        });

        obj.Init();

        var aa = obj.GetComponents();

        var vv = aa[0].GetPropertyDefinitions;
        vv = aa[1].GetPropertyDefinitions;

        //  obj = JsonConvert.DeserializeObject(json,new JsonSerializerSettings{
        //       TypeNameHandling = TypeNameHandling.Objects,
        //     SerializationBinder = knownTypesBinder
        //     });


        var xx = new List<BaseNode>();


    }
}
else
{







    UIGenerator.Generator.ConvertForFigmaFile(null,  file,fig);
}
