using System;
using System.Linq.Expressions;

namespace Dapper.Extensions
{
    /// <summary>
    /// 条件执行操作类
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public interface IOperatorWhere<TModel> where TModel : class
    {
        /// <summary>
        /// 添加条件语句
        /// </summary>
        /// <param name="whereExp"></param>
        /// <returns></returns>
        IOperatorWhere<TModel> Where(Expression<Func<TModel, bool>> whereExp);
        /// <summary>
        /// 执行表达式
        /// </summary>
        bool Go();

        /// <summary>
        /// 调试用的SQL语句
        /// </summary>
        string SQL { get; }
    }
}
