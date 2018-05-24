using Newtonsoft.Json;
using SavoryReference.Common;
using SavoryReference.Request;
using SavoryReference.Response;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace SavoryReference.Controllers
{
    public class NodeController : ApiController
    {
        [HttpPost]
        public NodeItemsResponse Items(NodeItemsRequest request)
        {
            NodeItemsResponse response = new NodeItemsResponse();

            response.ItemList = GetItems(request.Id);

            return response;
        }

        public List<Item> GetItems(string id)
        {
            var root = new DirectoryInfo(@"D:\HarrisData\HarrisReference");

            List<Node> nodes = new List<Node>();

            var files = root.GetFiles("*.txt", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var content = File.ReadAllText(file.FullName);

                var item = JsonConvert.DeserializeObject<List<Node>>(content);

                nodes.AddRange(item);
            }

            if (id.Equals("#"))
            {
                return ToNamespaceList(nodes);
            }
            else
            {
                return LoadChildren(nodes, id);
            }
        }

        /// <summary>
        /// 从所有类型中获取命名空间
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        private static List<Item> ToNamespaceList(List<Node> nodes)
        {
            return nodes.Where(v => v.NodeType == NodeType.Namespace).Select(v => new Item { Id = v.FullName, Text = v.Name, Children = true }).ToList();
        }

        /// <summary>
        /// 加载子元素
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private static List<Item> LoadChildren(List<Node> nodes, string id)
        {
            var node = nodes.FirstOrDefault(v => v.FullName == id);
            if (node == null)
            {
                return new List<Item>();
            }

            List<Item> returnValue = new List<Item>();
            switch (node.NodeType)
            {
                case NodeType.Class://当前点击的节点为命名空间
                    {
                        //加载属性
                        returnValue.AddRange(LoadProperty(node.FullName, node.PropertyList));

                        //加载方法
                        returnValue.AddRange(LoadMethod(node.FullName, node.MethodList));
                    }
                    break;
                case NodeType.Property://当前点击的节点为命名空间
                    {

                    }
                    break;
                case NodeType.Field://当前点击的节点为命名空间
                    {

                    }
                    break;
                case NodeType.Method://当前点击的节点为命名空间
                    {

                    }
                    break;
                case NodeType.Namespace://当前点击的节点为命名空间
                    {
                        //加载类
                        returnValue.AddRange(LoadClass(nodes, id));

                        //加载枚举
                        returnValue.AddRange(LoadEnum(nodes, id));
                    }
                    break;
                case NodeType.None:
                case NodeType.Enum:
                default:
                    break;
            }

            return returnValue;
        }

        /// <summary>
        /// 加载属性
        /// </summary>
        /// <param name="propertyList"></param>
        /// <returns></returns>
        private static List<Item> LoadProperty(string classFullName, List<Node> propertyList)
        {
            if (propertyList == null || propertyList.Count == 0)
            {
                return new List<Item>();
            }

            List<Item> returnValue = new List<Item>();
            foreach (var property in propertyList)
            {
                var item = new Item();

                item.Text = property.Name;
                item.Id = string.Concat(classFullName, ".", property.Name);
                if (property.IsInherit)
                {
                    item.Type = "property_inherit";
                }
                else
                {
                    item.Type = "property";
                }

                returnValue.Add(item);
            }

            return returnValue;
        }

        private static List<Item> LoadMethod(string classFullName, List<Node> methodList)
        {
            if (methodList == null || methodList.Count == 0)
            {
                return new List<Item>();
            }

            List<Item> returnValue = new List<Item>();
            foreach (var method in methodList)
            {
                var item = new Item();

                item.Text = method.Name;
                item.Id = method.FullName;
                if (method.IsInherit)
                {
                    item.Type = "method_inherit";
                }
                else
                {
                    item.Type = "method";
                }

                returnValue.Add(item);
            }

            return returnValue;
        }

        /// <summary>
        /// 加载命名空间里面的类
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="spaceName"></param>
        /// <param name="returnValue"></param>
        private static List<Item> LoadClass(List<Node> nodes, string spaceName)
        {
            List<Item> returnValue = new List<Item>();

            var classNodes = nodes.Where(v => v.NodeType == NodeType.Class && v.Namespace == spaceName).ToList();
            foreach (var classNode in classNodes)
            {
                var item = new Item { Type = "class" };

                item.Id = classNode.FullName;
                item.Text = classNode.Name;

                if (classNode.PropertyList != null && classNode.PropertyList.Count > 0)
                {
                    item.Children = true;
                }

                if (classNode.FieldList != null && classNode.FieldList.Count > 0)
                {
                    item.Children = true;
                }

                if (classNode.MethodList != null && classNode.MethodList.Count > 0)
                {
                    item.Children = true;
                }

                returnValue.Add(item);
            }

            return returnValue;
        }

        /// <summary>
        /// 加载命名控件里面的枚举
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="spaceName"></param>
        /// <param name="returnValue"></param>
        private static List<Item> LoadEnum(List<Node> nodes, string spaceName)
        {
            List<Item> returnValue = new List<Item>();

            var enumNodes = nodes.Where(v => v.NodeType == NodeType.Enum && v.Namespace == spaceName).ToList();
            foreach (var enumNode in enumNodes)
            {
                var item = new Item { Type = "enum" };

                item.Id = enumNode.FullName;
                item.Text = enumNode.Name;

                returnValue.Add(item);
            }

            return returnValue;
        }
    }
}
