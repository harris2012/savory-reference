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

                Run(param);

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                return -1;
            }
        }

        private static void Run(Param param)
        {
            Console.WriteLine(nameof(Environment.CurrentDirectory) + " = " + Environment.CurrentDirectory);

            var dllPath = $"{param.InputFolder}\\{param.InputName}.dll";
            var xmlPath = $"{param.InputFolder}\\{param.InputName}.xml";

            var items = AssemblyLoader.LoadAssembly(dllPath, xmlPath, false);

            //namespace
            {
                var namespaces = items.Select(v => v.Namespace).Distinct().ToList();

                var namespaceItems = namespaces.Select(v => new Node { Name = v, FullName = v, NodeType = NodeType.Namespace }).ToList();

                items.AddRange(namespaceItems);
            }

            Console.WriteLine("items count = " + items.Count);

            var content = JsonConvert.SerializeObject(items, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            var targetFilePath = Path.Combine(param.TargetFolder, param.TargetFile);
            FileInfo file = new FileInfo(targetFilePath);
            if (!file.Directory.Exists)
            {
                file.Directory.Create();
            }

            File.WriteAllText(targetFilePath, content);

            Console.WriteLine("reference runs successfully.");
        }
    }
}