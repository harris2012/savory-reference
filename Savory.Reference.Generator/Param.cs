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
        /// Savory.Config.Net45\bin\Release
        /// </summary>
        public string InputFolder { get; set; }

        /// <summary>
        /// Savory.Config
        /// </summary>
        public string InputName { get; set; }

        /// <summary>
        /// savory-lib\Savory.Config.txt
        /// </summary>
        public string TargetFile { get; set; }

        /// <summary>
        /// G:\tmp003
        /// </summary>
        public string TargetFolder { get { return @"D:\HarrisData\HarrisReference"; } }
    }
}
