namespace UIGenerator{
    public class GenerateSetting{
        

        private bool _clearBuildFolder = true;
        public bool ClearBuildFolder
        {
            get { return _clearBuildFolder; }
            set { _clearBuildFolder = value; }
        }
        
        private bool _enableComponent = true;
        public bool EnableComponent
        {
            get { return _enableComponent; }
            set { _enableComponent = value; }
        }

        private bool _enableWindow = true;
        public bool EnableWindow
        {
            get { return _enableWindow; }
            set { _enableWindow = value; }
        }
        
        private string _componentFolderName = "Components";
        public string ComponentFolderName
        {
            get { return _componentFolderName; }
            set { _componentFolderName = value; }
        }
            
            
     private string _windowFolderName = "Windows";
     public string WindowFolderName
     {
        get { return _windowFolderName; }
        set { _windowFolderName = value; }
     }
     

    }


    
}