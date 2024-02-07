using System.Collections.Generic;

namespace UIGenerator
{



    public class GenerateManager
    {
        private List<ComponentManager> _components = new List<ComponentManager>();
        public List<ComponentManager> Components
        {
            get { return _components; }
            private set { _components = value; }
        }
        

    }

    public class ComponentManager{

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


    public class DesignMeta{
        public object Id { get; set; }
        public object Tag { get; set; }
    }


    public class UIMeta{
        public object Id { get; set; }
        public object Tag { get; set; }
    }


    public class PropertyMapper{
        public object SourceId { get; set; }
        public object TargetId { get; set; }
    }


}