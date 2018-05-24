using SavoryReference.Common;
using SavoryReference.Common.Xml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Savory.Reference.Generator
{
    internal static class AssemblyLoader
    {
        public static List<Node> LoadAssembly(string exePath, string xmlPath, bool reflectionOnlyLoad)
        {
            //Load XML
            var xmlDoc = LoadXml(xmlPath);
            if (xmlDoc != null)
            {
                if (xmlDoc.Members != null && xmlDoc.Members.Length > 0)
                {
                    foreach (var member in xmlDoc.Members)
                    {
                        if (!string.IsNullOrEmpty(member.Summary))
                        {
                            member.Summary = member.Summary.Trim();
                        }
                    }
                }
            }

            //LoadTypes
            var types = LoadTypes(exePath, reflectionOnlyLoad);

            //Process
            if (xmlDoc != null)
            {
                return Transform(types, xmlDoc.Members, reflectionOnlyLoad);
            }
            else
            {
                return Transform(types, null, reflectionOnlyLoad);
            }
        }

        private static Type[] LoadTypes(string path, bool reflectionOnlyLoad)
        {
            Type[] types = null;

            if (reflectionOnlyLoad)
            {
                AppDomain appDomain = AppDomain.CurrentDomain;

                appDomain.ReflectionOnlyAssemblyResolve += (sender, args) =>
                {
                    AssemblyName name = new AssemblyName(args.Name);

                    string asmToCheck = Path.GetDirectoryName(path) + "\\" + name.Name + ".dll";

                    if (File.Exists(asmToCheck))
                    {
                        return Assembly.ReflectionOnlyLoadFrom(asmToCheck);
                    }

                    return Assembly.ReflectionOnlyLoad(args.Name);
                };

                Assembly asm = Assembly.ReflectionOnlyLoadFrom(path);

                types = asm.GetTypes();
            }
            else
            {
                types = Assembly.LoadFrom(path).GetTypes();
            }

            return types;
        }

        static XmlDoc LoadXml(string xmlPath)
        {
            XmlDoc doc = null;

            if (!string.IsNullOrEmpty(xmlPath))
            {
                var element = XElement.Load(xmlPath);

                doc = LoadXml(element);
            }

            return doc;
        }

        static XmlDoc LoadXml(XElement document)
        {
            XmlDoc returnValue = new XmlDoc();

            if (document != null)
            {
                var assemblyElement = document.Element("assembly");
                if (assemblyElement != null)
                {
                    returnValue.Assembly = new XmlAssembly();
                    var nameElement = assemblyElement.Element("name");
                    if (nameElement != null)
                    {
                        returnValue.Assembly.Name = nameElement.Value;
                    }
                }

                var members = document.Element("members");
                if (members != null)
                {
                    var xmlMembers = new List<XmlMember>();

                    var items = members.Elements();
                    foreach (var member in items)
                    {
                        XmlMember xmlMember = new XmlMember();

                        //Name
                        var nameAttribute = member.Attribute("name");
                        if (nameAttribute != null)
                        {
                            xmlMember.Name = nameAttribute.Value;
                        }

                        //Summary
                        var summaryElement = member.Element("summary");
                        if (summaryElement != null)
                        {
                            if (!string.IsNullOrEmpty(summaryElement.Value))
                            {
                                xmlMember.Summary = summaryElement.Value.Trim();
                            }
                        }

                        var paramElements = member.Elements("param");
                        if (paramElements != null)
                        {
                            var xmlMemberParamList = new List<XmlMemberParam>();
                            foreach (var paramElement in paramElements)
                            {
                                var paramNameAttribute = paramElement.Attribute("name");
                                var paramValueElement = paramElement.Value;
                                if (paramNameAttribute != null && !string.IsNullOrEmpty(paramNameAttribute.Value) && !string.IsNullOrEmpty(paramValueElement))
                                {
                                    var xmlMemberParam = new XmlMemberParam();

                                    xmlMemberParam.Name = paramNameAttribute.Value;
                                    xmlMemberParam.Value = paramValueElement;

                                    xmlMemberParamList.Add(xmlMemberParam);
                                }
                            }
                            if (xmlMemberParamList.Count > 0)
                            {
                                xmlMember.Param = xmlMemberParamList.ToArray();
                            }
                        }

                        var returns = member.Element("returns");
                        if (returns != null)
                        {
                            if (!string.IsNullOrEmpty(returns.Value))
                            {
                                xmlMember.Returns = returns.Value;
                            }
                        }

                        xmlMembers.Add(xmlMember);
                    }

                    returnValue.Members = xmlMembers.ToArray();
                }
            }

            return returnValue;
        }

        /// <summary>
        /// 根据namespace聚合
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        private static List<Node> Transform(Type[] types, XmlMember[] xmlMembers, bool reflectionOnlyLoad)
        {
            var returnValue = new List<Node>();

            if (types != null && types.Length > 0)
            {
                foreach (var type in types)
                {
                    if (type.IsClass)
                    {
                        var classNode = LoadClass(type, xmlMembers, reflectionOnlyLoad);
                        if (classNode.Name == null || classNode.Namespace == null)
                        {
                            continue;
                        }

                        returnValue.Add(classNode);
                        continue;
                    }

                    if (type.IsEnum)
                    {
                        var enumNode = LoadEnum(type);

                        returnValue.Add(enumNode);
                        continue;
                    }
                }
            }

            return returnValue;
        }

        private static Node LoadClass(Type clazz, XmlMember[] xmlMembers, bool reflectionOnlyLoad)
        {
            var classNode = new Node { NodeType = NodeType.Class };

            classNode.Name = clazz.Name;
            classNode.FullName = clazz.FullName;
            classNode.Namespace = clazz.Namespace;
            classNode.IsAbstract = clazz.IsAbstract;

            var classMember = xmlMembers != null ? xmlMembers.FirstOrDefault(v => string.Format("T:{0}", clazz.FullName).Equals(v.Name)) : null;
            if (classMember != null)
            {
                classNode.Summary = classMember.Summary;
            }

            //BaseType
            if (clazz.BaseType != typeof(object))
            {
                classNode.BaseTypeName = clazz.BaseType.Name;
                classNode.BaseTypeFullName = clazz.BaseType.FullName;
            }

            //Properties
            var propertyNodes = LoadProperties(clazz, xmlMembers);
            if (propertyNodes != null && propertyNodes.Count > 0)
            {
                classNode.PropertyList = new List<Node>();
                foreach (var propertyNode in propertyNodes)
                {
                    classNode.PropertyList.Add(propertyNode);
                }
            }

            //Fields
            var fields = clazz.GetMembers().Where(v => v.MemberType == MemberTypes.Field && v.DeclaringType == clazz).Select(v => (FieldInfo)v).ToArray();
            var fieldNodes = LoadFields(fields, xmlMembers, reflectionOnlyLoad);
            if (fieldNodes != null && fieldNodes.Count > 0)
            {
                classNode.FieldList = new List<Node>();
                foreach (var fieldNode in fieldNodes)
                {
                    classNode.FieldList.Add(fieldNode);
                }
            }

            //Methods
            var methods = clazz.GetMembers()
                .Where(v => v.MemberType == MemberTypes.Method)
                .Where(v => v.DeclaringType == clazz)
                .Select(v => (MethodInfo)v)
                .Where(v => v.IsPublic && !v.IsSpecialName)
                .ToArray();
            var methodNodes = LoadMethods(clazz.FullName, methods, xmlMembers);
            if (methodNodes != null && methodNodes.Count > 0)
            {
                classNode.MethodList = new List<Node>();
                foreach (var methodNode in methodNodes)
                {
                    classNode.MethodList.Add(methodNode);
                }
            }

            return classNode;
        }

        private static List<Node> LoadProperties(Type clazz, XmlMember[] xmlMembers)
        {
            var properties = clazz.GetProperties();

            List<Node> returnValue = new List<Node>();

            if (properties != null && properties.Length > 0)
            {
                foreach (var property in properties)
                {
                    var propertyNode = new Node();

                    propertyNode.Name = property.Name;
                    propertyNode.TypeName = property.PropertyType.Name;
                    propertyNode.TypeFullName = property.PropertyType.FullName;
                    if (property.DeclaringType != clazz)
                    {
                        propertyNode.IsInherit = true;
                    }

                    var propertyMember = xmlMembers != null ? xmlMembers.FirstOrDefault(v => string.Format("P:{0}.{1}", property.DeclaringType.FullName, property.Name).Equals(v.Name)) : null;
                    if (propertyMember != null)
                    {
                        propertyNode.Summary = propertyMember.Summary;
                    }

                    returnValue.Add(propertyNode);
                }
            }

            return returnValue;
        }

        private static List<Node> LoadFields(FieldInfo[] fields, XmlMember[] xmlMembers, bool reflectionOnlyLoad)
        {
            List<Node> returnValue = new List<Node>();

            if (fields != null && fields.Length > 0)
            {
                foreach (var field in fields)
                {
                    var fieldNode = new Node();

                    fieldNode.Name = field.Name;
                    fieldNode.TypeName = field.FieldType.Name;
                    fieldNode.TypeFullName = field.FieldType.FullName;

                    if (!reflectionOnlyLoad && field.IsStatic)
                    {
                        var fieldValue = field.GetValue(null);
                        switch (field.FieldType.FullName)
                        {
                            case "System.Int32":
                                fieldNode.ConstValue = fieldValue.ToString();
                                break;
                            case "System.String":
                                fieldNode.ConstValue = string.Format("\"{0}\"", fieldValue);
                                break;
                            default:
                                break;
                        }
                    }

                    var fieldMember = xmlMembers != null ? xmlMembers.FirstOrDefault(v => string.Format("F:{0}.{1}", field.DeclaringType.FullName, field.Name).Equals(v.Name)) : null;
                    if (fieldMember != null)
                    {
                        fieldNode.Summary = fieldMember.Summary;
                    }

                    returnValue.Add(fieldNode);
                }
            }

            return returnValue;
        }

        private static List<Node> LoadMethods(string classFullName, MethodInfo[] methods, XmlMember[] xmlMembers)
        {
            List<Node> returnValue = new List<Node>();

            if (methods != null && methods.Length > 0)
            {
                foreach (var method in methods)
                {
                    var methodModel = new Node();

                    methodModel.Name = method.Name;
                    methodModel.FullName = string.Concat(classFullName, ".", method.Name);
                    methodModel.MethodReturnTypeName = method.ReturnType.Name;
                    methodModel.MethodReturnTypeFullName = method.ReturnType.FullName;
                    methodModel.MethodIsStatic = method.IsStatic;
                    var parameters = method.GetParameters();
                    List<string> parameterTypes = new List<string>();
                    if (parameters.Length > 0)
                    {
                        methodModel.MethodParameters = new List<MethodParameter>();
                        foreach (var parameter in parameters)
                        {
                            var methodParameter = new MethodParameter();

                            methodParameter.Name = parameter.Name;
                            methodParameter.TypeName = parameter.ParameterType.Name;
                            methodParameter.TypeFullName = parameter.ParameterType.FullName;

                            parameterTypes.Add(parameter.ParameterType.Name);
                            methodModel.FullName += "_" + parameter.ParameterType.Name;

                            methodModel.MethodParameters.Add(methodParameter);
                        }
                    }
                    methodModel.Name = string.Concat(method.Name, "(", string.Join(", ", parameterTypes), ")");

                    var xmlMemberName = string.Format("M:{0}.{1}", method.DeclaringType.FullName, method.Name);
                    if (methodModel.MethodParameters != null && methodModel.MethodParameters.Count > 0)
                    {
                        xmlMemberName = string.Concat(xmlMemberName, "(", string.Join(",", methodModel.MethodParameters.Select(v => v.TypeFullName)), ")");
                    }
                    var methodMember = xmlMembers != null ? xmlMembers.FirstOrDefault(v => xmlMemberName.Equals(v.Name)) : null;
                    if (methodMember != null)
                    {
                        methodModel.Summary = methodMember.Summary;
                    }

                    returnValue.Add(methodModel);
                }
            }

            return returnValue;
        }

        private static Node LoadEnum(Type clazz)
        {
            Node node = new Node { NodeType = NodeType.Enum };

            node.Name = clazz.Name;
            node.FullName = clazz.FullName;
            node.Namespace = clazz.Namespace;

            return node;
        }

        private static List<Node> LoadStructs(Type[] structs)
        {
            List<Node> returnValue = new List<Node>();

            return returnValue;
        }
    }
}
