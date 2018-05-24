using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace SavoryReference.Common
{
    public sealed class Node
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 全名称
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public NodeType NodeType { get; set; }

        /// <summary>
        /// 【XML注释】摘要
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// 命名空间
        /// </summary>
        public string Namespace { get; set; }


        #region Class
        /// <summary>
        /// 父类
        /// </summary>
        //public ClassNode BaseClass { get; set; }

        public bool IsAbstract { get; set; }

        public string BaseTypeName { get; set; }

        public string BaseTypeFullName { get; set; }

        /// <summary>
        /// 属性
        /// </summary>
        public List<Node> PropertyList { get; set; }

        /// <summary>
        /// 字段
        /// </summary>
        public List<Node> FieldList { get; set; }

        /// <summary>
        /// 方法
        /// </summary>
        public List<Node> MethodList { get; set; }
        #endregion

        #region Field
        public string TypeName { get; set; }

        public string TypeFullName { get; set; }

        public string ConstValue { get; set; }
        #endregion

        #region Method
        /// <summary>
        /// 【C#】方法返回类型名称
        /// </summary>
        public string MethodReturnTypeName { get; set; }

        /// <summary>
        /// 【C#】方法返回类型完整名称
        /// </summary>
        public string MethodReturnTypeFullName { get; set; }

        /// <summary>
        /// 方法的参数
        /// </summary>
        public List<MethodParameter> MethodParameters { get; set; }

        /// <summary>
        /// 是否是静态方法
        /// </summary>
        public bool MethodIsStatic { get; set; }

        /// <summary>
        /// 属性名称
        /// example: Name
        /// </summary>
        public string PropertyName { get; set; }
        #endregion

        #region Property
        public string PropertyTypeName { get; set; }

        public string PropertyTypeFullName { get; set; }

        /// <summary>
        /// 属性是继承的
        /// </summary>
        public bool IsInherit { get; set; }
        #endregion
    }
}
