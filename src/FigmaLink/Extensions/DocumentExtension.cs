using FigmaLink.Model;
using System;
using System.Collections.Generic;
using System.Linq;
namespace FigmaLink.Extensions{
    public static class DocumentExtension{
        public static COMPONENT[] GetComponents(this DOCUMENT doc){
            var ss = new List<COMPONENT>();
            foreach(var page in doc.Children.OfType<PAGE>()){
                ss.AddRange(page.Children.OfType<COMPONENT>());
            }
            return ss.ToArray();
               
           
        }
    }
}