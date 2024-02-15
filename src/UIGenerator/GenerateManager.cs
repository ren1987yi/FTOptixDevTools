using System.Collections.Generic;
using System.Runtime.InteropServices;
using FTOptix.UI;

namespace UIGenerator
{



    public class GenerateManager
    {

        private List<IAsset> _assets = new List<IAsset>();
        public List<IAsset> Assets
        {
            get { return _assets; }
            private set { _assets = value; }
        }


        private List<ComponentManager> _components = new List<ComponentManager>();
        public List<ComponentManager> Components
        {
            get { return _components; }
            private set { _components = value; }
        }


        private List<WindowManager> _windows = new List<WindowManager>();
        public List<WindowManager> Windows
        {
            get { return _windows; }
            set { _windows = value; }
        }


    }



    public interface IAsset
    {
        public string Name { get; }

        public object Item { get; }
    }


    /// <summary>
    /// 资产，基本是ui素材
    /// </summary>
    public class Asset<T> : IAsset
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            private set { _name = value; }
        }

        private T _item;
        public object Item
        {
            get
            {

                return _item;
            }

        }


        public T ActualItem{
            get{
                return _item;
            }
        }

        public Asset(string name, T item)
        {
            Name = name;
            _item = item;
        }


    }

    public interface IManager
    {
        public DesignMeta Source { get; }
        public UIMeta Target { get; }
    }
    public class WindowManager : IManager
    {
        //原设计稿的 Id
        private DesignMeta _source = new DesignMeta();
        public DesignMeta Source
        {
            get { return _source; }
            protected set { _source = value; }
        }

        //生成的模块 id
        private UIMeta _target = new UIMeta();
        public UIMeta Target
        {
            get { return _target; }
            protected set { _target = value; }
        }
    }

    public class ComponentManager : IManager
    {

        //原设计稿的 Id
        private DesignMeta _source = new DesignMeta();
        public DesignMeta Source
        {
            get { return _source; }
            protected set { _source = value; }
        }

        //生成的模块 id
        private UIMeta _target = new UIMeta();
        public UIMeta Target
        {
            get { return _target; }
            protected set { _target = value; }
        }

        private List<PropertyMapper> _propertyMappers = new List<PropertyMapper>();
        public List<PropertyMapper> PropertyMappers
        {
            get { return _propertyMappers; }
            protected set { _propertyMappers = value; }
        }




    }


    public class DesignMeta
    {
        public object Id { get; set; }
        public object Tag { get; set; }
    }


    public class UIMeta
    {
        public object Id { get; set; }
        public object Tag { get; set; }
    }


    public class PropertyMapper
    {
        public object SourceId { get; set; }
        public object TargetId { get; set; }
        public object Target { get; set; }
    }


}