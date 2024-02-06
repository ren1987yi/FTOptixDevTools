using System.Reflection.Metadata;
using FigmaLink;
// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

var file =@"../../../../../src/Test/test/Test1.plugin.json";
var doc = FigmaLink.Model.DocumentNode.LoadFile(file);
foreach(var page in doc.Pages){

}



var a = FigmaLink.Document.LoadFile(file);
// Console.WriteLine(a.id);
// var aa = a.children;
// Console.WriteLine(a.children);

if(a.type == "document".ToUpper()){

    Console.WriteLine($"Document name:{a.name}");
    for(var i = 0;i<a.children.Count;i++){
        var page = a.children[i];
        if(page.type == "page".ToUpper()){
            Console.WriteLine($"page name:{page.name}");

            
        }
    }
}
