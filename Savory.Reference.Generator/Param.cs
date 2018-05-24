using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Savory.Reference.Generator
{
    public class Param
    {
        /// <summary>
        /// DLL路径
        /// </summary>
        public string DLLFilePath { get; set; }

        /// <summary>
        /// 注释xml文件的路径
        /// </summary>
        public string XmlFilePath { get; set; }

        /// <summary>
        /// 目标文件位置(相对路径)
        /// </summary>
        public string TargetFilePath { get; set; }
    }
}
