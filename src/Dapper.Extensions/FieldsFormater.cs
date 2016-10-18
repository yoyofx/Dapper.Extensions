using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Dapper.Extensions
{
    /// <summary>
    /// 访问 o => new obj{ ID = "1" }，表达式的解析器
    /// </summary>
    public class FieldsFormater : ExpressionVisitor
    {
        /// <summary>
        /// sql server prefix of the paramter name SQLSERVER的参数修正符
        /// </summary>
        public string ParamPrefix => "@";

        public FieldsFormater()
        {

        }
        /// <summary>
        /// 解析后得到的参数如 [ { key: "ID"  Value: "1" } , ... ]
        /// </summary>
        Dictionary<string , Parameter> parameters = new Dictionary<string, Parameter>(); 

        /// <summary>
        /// 访问常量表达式，将字段和值添加到参数列表中。
        /// </summary>
        /// <param name="node">表达式节点</param>
        /// <param name="fieldName">字段名</param>
        /// <returns></returns>
        protected  Expression VisitConstant(ConstantExpression node,string fieldName)
        {
            if (parameters.Keys.Any(k => k == fieldName))
                parameters[fieldName].Value = node.Value;
            else
            {
                parameters.Add(fieldName, new Parameter(ParamPrefix + fieldName, node.Value));
            }
            return base.VisitConstant(node);
        }

        //将右值表达式如 ID = Convert(i) 直接通过编译计算出来
        protected  Expression MyVisitMember(MemberExpression node, string fieldName)
        {
            LambdaExpression lambda = Expression.Lambda(node);
            var fn = lambda.Compile();
            VisitConstant(Expression.Constant(fn.DynamicInvoke(null), node.Type), fieldName);
            return base.VisitMember(node);
        }

        /// <summary>
        /// 访问成员，如 ID = 1 时。
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            string propertyName = node.Member.Name;
            string fieldName = propertyName;

            if (node.Expression.NodeType == ExpressionType.Call)
            {
                VisitMethodCall((MethodCallExpression)node.Expression,fieldName);
            }
            else if (node.Expression.NodeType == ExpressionType.Convert)
            {
                var ue = node.Expression as UnaryExpression;
                var call = ue.Operand as MethodCallExpression;

                if (call != null)
                    this.VisitMethodCall(call, fieldName);
                else
                    this.MyVisitMember(ue.Operand as MemberExpression,fieldName);
            }
            else
            {
                parameters.Add(fieldName, new Parameter(ParamPrefix + fieldName, null));
                var constant = node.Expression as ConstantExpression;
                if (constant != null)
                    VisitConstant(constant, fieldName);
                else
                {
                    LambdaExpression lambda = Expression.Lambda(node.Expression);
                    var fn = lambda.Compile();
                    VisitConstant(Expression.Constant(fn.DynamicInvoke(null), node.Expression.Type), fieldName);
                }
            }
            return node;
        }

        /// <summary>
        /// 访问匿名类型成员
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitNew(NewExpression node)
        {
            if (node.Members != null)
            {
                foreach (var item in node.Members)
                {
                    Parameters.Add(item.Name, null);
                }
            }
            return base.VisitNew(node);
        }

        /// <summary>
        /// 访问表达式中的函数调用
        /// </summary>
        /// <param name="node"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        protected  Expression VisitMethodCall(MethodCallExpression node,string fieldName)
        {
            string sqlMethodName = node.Method.Name.ToLower();
            string callName = SqlMethodNameCallBack.MethodNameCallback(sqlMethodName);
            if (!string.IsNullOrEmpty(callName))
            {
                parameters.Add(fieldName, new Parameter(callName, callName) { IsMethodType = true });
            }
            else
                throw new NotSupportedException(string.Format("{0}数据库中不支持函数{1}","SqlServer",sqlMethodName));

            return base.VisitMethodCall(node);
        }

        
        internal Dictionary<string , Parameter> Parameters
        {
            get { return parameters; }
        }


    }
}
