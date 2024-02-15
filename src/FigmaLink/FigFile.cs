using System;
using System.Data;
using System.IO;
using System.IO.Compression;

namespace FigmaLink
{

    /// <summary>
    /// Figma 本地文件
    /// </summary>
    public class FigFile
    {

        private string _figFilePath;
        /// <summary>
        /// fig 文件地址
        /// </summary>
        /// <value></value>
        public string FigFilePath
        {
            get { return _figFilePath; }
            protected set { _figFilePath = value; }
        }


        private string _extFolder;
        /// <summary>
        /// 解压文件夹
        /// </summary>
        /// <value></value>
        public string ExtFolder
        {
            get { return _extFolder; }
            protected set { _extFolder = value; }
        }


        public FigFile(string figfile, string extfolder)
        {
            if (string.IsNullOrWhiteSpace(figfile))
            {
                throw new NullReferenceException("fig file is empty");
            }


            if (!File.Exists(figfile))
            {
                throw new FileNotFoundException("fig file is not exist");
            }


            if (string.IsNullOrWhiteSpace(extfolder))
            {
                throw new NullReferenceException("extra folder is empty");

            }

            if (!Directory.Exists(extfolder))
            {
                throw new DirectoryNotFoundException("extra folder is not exist");
            }

            _figFilePath = figfile;
            _extFolder = extfolder;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns>
        ///     if image exist :return image file path
        ///     else:return null
        /// </returns>
        public string GetImage(string name)
        {
            var filepath = Path.Combine(_extFolder, "images", name);
            if (File.Exists(filepath))
            {
                return Path.GetFullPath(filepath);
            }
            else
            {
                return null;
            }
        }



        /// <summary>
        /// 文件加载
        /// 解压文件夹空的话，就在文件本地生成个同名文件夹
        /// </summary>
        /// <param name="filepath">文件地址</param>
        /// <param name="destFolder">解压的目标文件夹</param>
        /// <param name="overwriteFiles">覆盖？</param>
        /// <returns>FigFile 对象</returns>
        public static FigFile LoadFromFile(string filepath, string destFolder = null, bool overwriteFiles = true)
        {
            if (!File.Exists(filepath))
            {
                throw new FileNotFoundException("fig file not found");
            }


            if (Path.GetExtension(filepath).ToUpper() != ".FIG".ToUpper())
            {
                throw new FileLoadException("fig file format is wrong");

            }

            //fig 文件的文件夹
            var _fileFolder = Path.GetDirectoryName(filepath);
            var _fileName = Path.GetFileNameWithoutExtension(filepath);
            //实际解压目标文件夹
            var _actDestFolder = string.Empty;

            if (destFolder == null)
            {
                _actDestFolder = Path.Combine(_fileFolder, _fileName);
            }
            else
            {
                _actDestFolder = destFolder;
            }



            ZipFile.ExtractToDirectory(filepath, _actDestFolder, System.Text.Encoding.UTF8, overwriteFiles);

            return new FigFile(filepath, _actDestFolder);


        }



    }
}
