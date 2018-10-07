using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Savory.Reference.Generator
{
    static class ParamReader
    {
        public static Param Read(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                return null;
            }

            Param param = new Param();

            foreach (var arg in args)
            {
                if (string.IsNullOrEmpty(arg) || !arg.Contains(":"))
                {
                    continue;
                }

                var type = arg.Substring(0, arg.IndexOf(":"));
                var value = arg.Substring(arg.IndexOf(":") + 1);

                switch (type.ToLower())
                {
                    case "/from":
                        param.InputFolder = value;
                        break;
                    case "/name":
                        param.InputName = value;
                        break;
                    case "/target":
                        param.TargetFile = value;
                        break;
                    default:
                        break;
                }
            }

            return param;
        }
    }
}
