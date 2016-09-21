using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Dapper;
using Dapper.Contrib;

namespace Dapper.Extensions
{
    /// <summary>
    /// 条件执行操作类
    ///API采用 .Where(expression).Go()的链式使用方式。
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public class OperatorWhereObject<TModel> : IOperatorWhere<TModel> where TModel:class
    {
        internal OperatorWhereObject(IDbConnection p,string sql,List<Parameter> ps )
        {
            Provider = p;
            Parameters = ps;
            _sql = sql;
        }

        private string _sql;
        private List<Parameter> Parameters { set; get; } 
        private IDbConnection Provider {set; get; }
        /// <summary>
        /// 执行条件执行的表达式语句
        /// </summary>
        public bool Go()
        {
            if (Provider == null)
                return false;

            var dynParms = new DynamicParameters();
            Parameters.ForEach(p=> dynParms.Add(p.Name,p.Value));
            bool ret = Provider.Execute(this._sql, dynParms) > 0;

            if(Provider.State == ConnectionState.Open) Provider.Close();

            this._sql = null;
            return ret;
        }

        /// <summary>
        /// 在没有执行Go之前生成的SQL语句
        /// </summary>
        public string SQL {
            get { return _sql; }
        }

        /// <summary>
        /// 添加条件表达式
        /// </summary>
        /// <param name="whereExp">表达式</param>
        /// <returns>条件执行对象</returns>
        public IOperatorWhere<TModel> Where(Expression<Func<TModel, bool>> whereExp)
        {
            var translate = new SqlTranslateFormater();
            string whereSql = translate.Translate(whereExp);
            _sql +=  whereSql;
            return this;
        }
    }
}
