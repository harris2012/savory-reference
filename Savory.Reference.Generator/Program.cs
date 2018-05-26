using Newtonsoft.Json;
using SavoryReference.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Savory.Reference.Generator
{
    class Program
    {
        /// <summary>
        /// 运行文件夹：项目所在文件夹
        /// 参数 /csproj:xxx.csproj
        /// </summary>
        /// <param name="args"></param>
        static int Main(string[] args)
        {
            try
            {
                var param = ParamReader.Read(args);
                if (param == null)
                {
                    return -1;
                }

                var targetFolder = ConfigurationManager.AppSettings["TargetFolder"];
                if (targetFolder == null)
                {
                    Console.WriteLine("TargetFolder is not configed in app.config. press any key to exit.");
                    return -1;
                }
                Console.WriteLine("targetFolder = " + targetFolder);

                Run(param, targetFolder);

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                return -1;
            }
        }

        private static void Run(Param param, string targetFolder)
        {
            Console.WriteLine(nameof(Environment.CurrentDirectory) + " = " + Environment.CurrentDirectory);

            Console.WriteLine(nameof(param.DLLFilePath) + " = " + param.DLLFilePath);
            Console.WriteLine(nameof(param.XmlFilePath) + " = " + param.XmlFilePath);

            var items = AssemblyLoader.LoadAssembly(param.DLLFilePath, param.XmlFilePath, false);

            //namespace
            {
                var namespaces = items.Select(v => v.Namespace).Distinct().ToList();

                var namespaceItems = namespaces.Select(v => new Node { Name = v, FullName = v, NodeType = NodeType.Namespace }).ToList();

                items.AddRange(namespaceItems);
            }

            Console.WriteLine("items count = " + items.Count);

            var content = JsonConvert.SerializeObject(items, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            var targetFilePath = Path.Combine(targetFolder, param.TargetFilePath);
            FileInfo file = new FileInfo(targetFilePath);
            if (!file.Directory.Exists)
            {
                file.Directory.Create();
            }

            File.WriteAllText(targetFilePath, content);

            Console.WriteLine("reference runs successfully.");
        }

        private static string ToFolderPath(string targetFolder, string spaceName)
        {
            return Path.Combine(targetFolder, spaceName.Replace(".", "/"));
        }

        private static string ToValidFileName(string name)
        {
            return name.Replace("<", "_").Replace(">", "_");
        }
    }
}