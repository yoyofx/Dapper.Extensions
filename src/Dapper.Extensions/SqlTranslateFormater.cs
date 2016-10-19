using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Reflection;

namespace Dapper.Extensions
{
    /// <summary>
    /// Maker:张磊
    /// 解析整个Linq成SQL语句的入口
    /// </summary>
    public class SqlTranslateFormater : ExpressionVisitor
    {
        /// <summary>
        /// 
        /// </summary>
        private ConcurrentDictionary<Type, string> Ass = new ConcurrentDictionary<Type, string>();
        /// <summary>
        /// sql语句变量
        /// </summary>
        StringBuilder sb = null;
        StringBuilder ordby = null;
        /// <summary>
        /// 入口
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public string Translate(Expression expr)
        {
            sb = new StringBuilder();
            ordby = new StringBuilder();

            this.Visit(expr);
            return sb.ToString();
        }

        bool isleft = true;

        /// <summary>
        /// 访问二元操作符, 如 a >= b, 表达式的left 为a , right为b , 表达式的NodeType为 操作符
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        protected override Expression VisitBinary(BinaryExpression b)
        {
            isleft = true;

            this.VisitBinaryWithParent(b, b.Left);

            switch (b.NodeType)
            {
                case ExpressionType.AndAlso:
                    sb.Append(" AND ");
                    break;
                case ExpressionType.OrElse:
                    sb.Append(" OR ");
                    break;
                case ExpressionType.GreaterThan:
                    sb.Append(" > ");
                    break;
                case ExpressionType.LessThan:
                    sb.Append(" < ");
                    break;
                case ExpressionType.Equal:
                    sb.Append(" = ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    sb.Append(" <= ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    sb.Append(" >= ");
                    break;
                case ExpressionType.NotEqual:
                    sb.Append(" <> ");
                    break;
            }

            isleft = false;

            this.VisitBinaryWithParent(b, b.Right);


            return b;
        }

        //访问父节点，用于优先级计算
        private void VisitBinaryWithParent(Expression parent, Expression node)
        {
            var left = isleft;
            if (IsDiffNoteType(parent, node))
                sb.Append("(");

            var bnode = node as BinaryExpression;

            if (bnode != null && bnode.Left is BinaryExpression)
                this.VisitBinaryWithParent(node, bnode.Left);
            else
            {
                isleft = true;
                if (bnode != null)
                    this.Visit(bnode.Left);
                else
                    this.Visit(node);
                isleft = left;
            }

            switch (node.NodeType)
            {
                case ExpressionType.AndAlso:
                    sb.Append(" AND ");
                    break;
                case ExpressionType.OrElse:
                    sb.Append(" OR ");
                    break;
                case ExpressionType.GreaterThan:
                    sb.Append(" > ");
                    break;
                case ExpressionType.LessThan:
                    sb.Append(" < ");
                    break;
                case ExpressionType.Equal:
                    sb.Append(" = ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    sb.Append(" <= ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    sb.Append(" >= ");
                    break;
                case ExpressionType.NotEqual:
                    sb.Append(" <> ");
                    break;
                default:
                    return;
            }

            if (bnode != null && bnode.Right is BinaryExpression)
                this.VisitBinaryWithParent(node, bnode.Right);
            else
            {
                isleft = false;
                if (bnode != null)
                    this.Visit(bnode.Right);
                else
                    this.Visit(node);
                isleft = left;
            }

            if (IsDiffNoteType(parent, node))
                sb.Append(")");
        }

        //得到表达式中c.ID中的c
        private void GetParentMemberName(Expression node)
        {
            if (node.NodeType == ExpressionType.MemberAccess)
            {
                var memberEx = node as MemberExpression;
                GetParentMemberName(memberEx.Expression);
                sb.Append(memberEx.Member.Name);
                sb.Append(".");
            }
        }

        //看上一个节点和下一个是不是一样的操作符
        private bool IsDiffNoteType(Expression parent, Expression children)
        {
            if (IsOpertaorType(parent) && IsOpertaorType(children))
            {
                if (parent.NodeType == children.NodeType)
                    return false;
                else
                    return true;
            }
            else
            {

                return false;
            }

        }

        bool IsOpertaorType(Expression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                    return true;
                default:
                    return false;
            }
        }




        /// <summary>
        /// 访问成员
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression != null && (node.Expression.NodeType == ExpressionType.Parameter || node.Expression.NodeType == ExpressionType.MemberAccess) && isleft)
            {
                bool isAsName = false;
                var pse = node.Expression as ParameterExpression;
                if (pse != null && pse.Type == node.Member.DeclaringType)
                {
                    if (!Ass.Keys.Contains(pse.Type))
                        Ass.TryAdd(pse.Type, pse.Name);
                    isAsName = true;
                }
                string ts;
                if (node.Member.DeclaringType != null && (!isAsName && !Ass.TryGetValue(node.Member.DeclaringType, out ts)))
                    GetParentMemberName(node.Expression);

                var fieldName = node.Member.Name;

                string tn = string.Empty;
                //成员表达式的别名 如 c.Name 
                //if (Ass.TryGetValue(node.Member.DeclaringType,out tn))
                //{
                //    sb.Append(tn); 
                //    sb.Append(".");
                //}

                sb.Append(fieldName);
                return node;
            }
            else
            {
                LambdaExpression lambda = Expression.Lambda(node);
                var fn = lambda.Compile();
                this.Visit(Expression.Constant(fn.DynamicInvoke(null), node.Type));

            }

            return node;
        }
        /// <summary>
        /// 访问常量
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        protected override Expression VisitConstant(ConstantExpression c)
        {
            IQueryable q = c.Value as IQueryable;
            if (q != null)
            {

                var tableName = q.ElementType.Name;

                sb.Append(tableName);

                sb.Append(" ");
                if (Ass.Keys.Count > 0)
                    sb.Append(Ass[q.ElementType]);
                else
                    sb.Append("T");
            }
            else if (c.Value == null)
            {
                sb.Append("NULL");
            }
            else
            {
                switch (Type.GetTypeCode(c.Value.GetType()))
                {
                    case TypeCode.Boolean:
                        sb.Append(((bool)c.Value) ? 1 : 0);
                        break;
                    case TypeCode.DateTime:
                        string dtfs = "'{0}'";
                        sb.AppendFormat(dtfs, c.Value);
                        break;
                    case TypeCode.String:
                        sb.Append("'");
                        sb.Append(c.Value);
                        sb.Append("'");
                        break;
                    case TypeCode.Object:
#if COREFX
                        if (c.Type.GetTypeInfo().GetInterface("IEnumerable", false) != null)
#else
                        if (c.Type.GetInterface("IEnumerable", false) != null)   
#endif
                        {
                            List<object> list = new List<object>();
                            foreach (var item in (IEnumerable)c.Value)
                            {
                                list.Add(item);
                            }

                            if (list.Count == 0)
                                throw new InvalidOperationException($"{c.Value}语句中的数组数据不能为空值。");
                            object elementOne = list[0];
                            Type elementType = elementOne.GetType();

                            if (Type.GetTypeCode(elementType) == TypeCode.Boolean)
                                throw new NotSupportedException($"{c.Value}不支持包含多个bool类型，请使用是否相等作为条件");

                            string format = "{0}";

                            // 字符类型、时间类型、GUID类型单独处理
                            if (elementType == typeof(string) || elementType == typeof(DateTime) || elementType == typeof(Guid))
                                format = "'{0}'";

                            StringBuilder value = new StringBuilder();
                            value.Append("(");
                            for (int i = 0; i < list.Count; i++)
                            {
                                value.AppendFormat(format, list[i]);
                                if (i != list.Count - 1)
                                    value.Append(",");
                            }
                            value.Append(")");
                            sb.Append(value);
                        }
                        else if (c.Type == typeof(Guid))
                        {
                            sb.Append("'");
                            sb.Append(c.Value);
                            sb.Append("'");
                        }
                        else
                            throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", c.Value));
                        break;
                    default:
                        sb.Append(c.Value);
                        break;
                }
            }
            return c;
        }
        /// <summary>
        /// 访问转换
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType == ExpressionType.Convert)
                this.Visit(node.Operand);
            return node;
        }


        /// <summary>
        /// 访问方法
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var methodInfo = node.Method;

            if (methodInfo.DeclaringType == typeof(SQLMethod))
            {
                bool tleft = isleft;
                isleft = true;
                if (node.Arguments.Count > 0)
                    this.Visit(node.Arguments[0]);
                isleft = tleft;
                switch (methodInfo.Name)
                {
                    case "IsNull":
                        sb.Append(" is NULL");
                        break;
                    case "IsNotNull":
                        sb.Append(" is NOT NULL");
                        break;
                    default:
                        string methodName = methodInfo.Name.ToLower();
                        string funcName = SqlMethodNameCallBack.MethodNameCallback(methodName);

                        if (!string.IsNullOrEmpty(funcName))
                            sb.Append(funcName);
                        else
                            throw new NotSupportedException(
                                string.Format("{0}数据库中不支持函数{1}", "sqlserver", methodName));
                        break;
                }



            }


            if (methodInfo.DeclaringType == typeof(string))
            {
                switch (methodInfo.Name)
                {
                    case "Contains":
                        VisitStringFuncByOneParamter(node,
                                        v => string.Format("%{0}%", v));
                        break;
                    case "StartsWith":
                        VisitStringFuncByOneParamter(node,
                                        v => string.Format("{0}%", v));
                        break;
                    case "EndsWith":
                        VisitStringFuncByOneParamter(node,
                                        v => string.Format("%{0}", v));
                        break;
                }
            }
            if (methodInfo.DeclaringType == typeof(Enumerable))
                if (methodInfo.DeclaringType == typeof(Enumerable))
                {
                    this.Visit(node.Arguments[1]);
                    sb.Append(" In ");
                    this.Visit(node.Arguments[0]);
                }
#if COREFX
            if (methodInfo.DeclaringType.GetTypeInfo().GetInterface("IEnumerable", false) != null)
#else
            if (methodInfo.DeclaringType?.GetInterface("IEnumerable", false) != null)
#endif
            {
                this.Visit(node.Arguments[0]);
                sb.Append(" In ");
                this.Visit(node.Object);
            }

            if (methodInfo.DeclaringType == typeof(System.Convert))
            {
                this.Visit(node.Arguments[0]);
            }


            return node;
        }

        private static Expression StripQuotes(Expression e)
        {

            while (e.NodeType == ExpressionType.Quote)
            {

                e = ((UnaryExpression)e).Operand;

            }

            return e;

        }


        /// <summary>
        /// 访问String成员函数中有一个参数和返回值的方法
        /// 此方法 只用于生成Like SQL表达式 。 执行的方法名：Contains , StartWith ,EndWith 
        /// </summary>
        /// <param name="node"></param>
        private void VisitStringFuncByOneParamter(MethodCallExpression node, Func<object, string> valueFunc)
        {
            this.Visit(node.Object);
            sb.Append(" Like ");
            var right = node.Arguments[0];
            this.ChangeMethodParamExpression(ref right, valueFunc);
            this.Visit(right);
        }

        /// <summary>
        /// 修改MethodExpression表达式中的参数表达式，使其值改变为valueFunc函数的结果
        /// </summary>
        /// <param name="right">参数表达式</param>
        /// <param name="valueFunc">函数 : string->string </param>
        private void ChangeMethodParamExpression(ref Expression right, Func<object, string> valueFunc)
        {
            if (right.NodeType == ExpressionType.Constant)
            {
                var value = (right as ConstantExpression).Value;

                right = Expression.Constant(valueFunc(value), typeof(string));
            }
            else if (right.NodeType == ExpressionType.MemberAccess)
            {
                LambdaExpression lambda = Expression.Lambda(right);
                var fn = lambda.Compile();
                var value = fn.DynamicInvoke(null);
                right = Expression.Constant(valueFunc(value), typeof(string));
            }
        }

        /// <summary>
        /// 访问对象构造函数
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitNew(NewExpression node)
        {
            if (node.Type == typeof(DateTime))
            {
                LambdaExpression lambda = Expression.Lambda(node);
                var fn = lambda.Compile();
                var value = fn.DynamicInvoke(null);
                this.Visit(Expression.Constant(value, typeof(DateTime)));
            }
            else
            {
                string names = String.Join(",", node.Members.Select(m => m.Name).ToArray());
                sb.Append(names);
            }
            return node;
        }

    }
}
