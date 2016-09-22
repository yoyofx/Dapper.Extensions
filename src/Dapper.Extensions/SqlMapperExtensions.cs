using System;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dapper.Contrib.Extensions;

namespace Dapper.Extensions
{
    using Dapper;
    using Dapper.Contrib;
    using System.Linq.Expressions;
    /// <summary>
    /// Dapper扩展类，扩展支持单表的where语句生成 
    /// </summary>
    public static partial class SqlMapperExtensions
    {
        private static string GetTableName(Type entityType)
        {
            string tableName = string.Empty;
            var attrs = entityType.GetTypeInfo().GetCustomAttributes(typeof(TableAttribute), true).ToArray();
            if (attrs != null && attrs.Length > 0)
                tableName = ((TableAttribute)attrs[0]).Name;
            else
                throw new NotSupportedException("实体类上没有TableAttribute特性！");

            return tableName;

        }


        internal static string CreateQuerySQL<T>(Expression<Func<T, bool>> expression) where T : class
        {
            var translate = new SqlTranslateFormater();
            string sqlWhere = translate.Translate(expression);

            string tableName = GetTableName(typeof(T));
            
            StringBuilder sqlBuilder = new StringBuilder($"select * from {tableName} where ");
            sqlBuilder.Append(sqlWhere);
            return sqlBuilder.ToString();
        }


        internal static string CreateQuerySQLWithActive<T>(Expression<Func<T, bool>> expression) where T : class
        {
            var translate = new SqlTranslateFormater();
            string sqlWhere = translate.Translate(expression);

            string tableName = GetTableName(typeof(T));

            StringBuilder sqlBuilder = new StringBuilder($"select * from {tableName} where IsActive=1 AND ");
            sqlBuilder.Append(sqlWhere);
            return sqlBuilder.ToString();
        }

        /// <summary>
        /// 查找一条数据,并且IsActive=1
        /// </summary>
        /// <typeparam name="T">表名</typeparam>
        /// <param name="connection">数据库连接</param>
        /// <param name="expression">where表达式</param>
        /// <returns>一条实体记录</returns>
        public static T GetEntityForIsActive<T>(this IDbConnection connection, Expression<Func<T, bool>> expression) where T : class
        {
            return connection.QueryFirstOrDefault<T>(CreateQuerySQLWithActive<T>(expression));
        }


        /// <summary>
        /// 同步查找一条数据
        /// </summary>
        /// <typeparam name="T">表名</typeparam>
        /// <param name="connection">数据库连接</param>
        /// <param name="expression">where表达式</param>
        /// <returns>一条实体记录</returns>
        public static T Get<T>(this IDbConnection connection, Expression<Func<T, bool>> expression) where T : class
        {
            return connection.QueryFirstOrDefault<T>(CreateQuerySQL<T>(expression));
        }

        /// <summary>
        /// 异步查找一条数据
        /// </summary>
        /// <typeparam name="T">表名</typeparam>
        /// <param name="connection">数据库连接</param>
        /// <param name="expression">where表达式</param>
        /// <returns>一条实体记录</returns>
        public static Task<T> GetAsync<T>(this IDbConnection connection, Expression<Func<T, bool>> expression)
            where T : class
        {
            return connection.QueryFirstAsync<T>(CreateQuerySQL<T>(expression));
        }


        /// <summary>
        /// 查找数据集合，并且IsActive=1
        /// </summary>
        /// <typeparam name="T">表名</typeparam>
        /// <param name="connection">数据库连接</param>
        /// <param name="expression">where表达式</param>
        /// <returns>实体记录集合</returns>
        public static IEnumerable<T> GetAllForIsActive<T>(this IDbConnection connection, Expression<Func<T, bool>> expression) where T : class
        {
            return connection.Query<T>(CreateQuerySQLWithActive<T>(expression));
        }


        /// <summary>
        /// 同步查找数据数据集合
        /// </summary>
        /// <typeparam name="T">表名</typeparam>
        /// <param name="connection">数据库连接</param>
        /// <param name="expression">where表达式</param>
        /// <returns>实体记录集合</returns>
        public static IEnumerable<T> GetAll<T>(this IDbConnection connection, Expression<Func<T, bool>> expression) where T : class
        {
            return connection.Query<T>(CreateQuerySQL<T>(expression));
        }

        /// <summary>
        /// 异步查找数据数据集合
        /// </summary>
        /// <typeparam name="T">表名</typeparam>
        /// <param name="connection">数据库连接</param>
        /// <param name="expression">where表达式</param>
        /// <returns>实体记录集合</returns>
        public static Task<IEnumerable<T>> GetAllAsync<T>(this IDbConnection connection, Expression<Func<T, bool>> expression)
          where T : class
        {
            return connection.QueryAsync<T>(CreateQuerySQL<T>(expression));
        }


        /// <summary>
        /// 立即执行更新语句,API采用 Update({new entity expression}).Where(expression).Go()的链式使用方式
        /// </summary>
        /// <typeparam name="TModel">模型名称</typeparam>
        /// <param name="connection"></param>
        /// <param name="objExp">更新对象表达式，表示要更新的列信息</param>
        /// <returns>将返回一个操作接口，此接口会有一个Where方法和Go方法，Where表示要添加条件，Go则表示立即执行语句。</returns>
        /// <example> session.Update(u => new AdminUser1 {ID = "5", NameA = "maxzhang"}).Where(p => p.Age > 5).Go</example>
        public static IOperatorWhere<TModel> Update<TModel>(this IDbConnection connection, Expression<Func<TModel, TModel>> objExp) where TModel : class
        {
            string tableName = GetTableName(typeof(TModel));

            FieldsFormater format = new FieldsFormater();
            format.Visit(objExp);
            string paramterNameAndValues = string.Join(",", format.Parameters.Select(kv => kv.Key + "=" + kv.Value.Name));

            string template = string.Format("Update {0} SET {1} Where ", tableName, paramterNameAndValues);
            var ps = format.Parameters.Values.Where(p => p.IsMethodType == false).ToList();

            return new OperatorWhereObject<TModel>(connection, template, ps);
        }

        /// <summary>
        /// 逻辑删除表对应的一条数据记录
        /// </summary>
        /// <typeparam name="TModel">表实体</typeparam>
        /// <param name="connection">数据库连接</param>
        /// <param name="id">记录ID</param>
        /// <param name="isActive">是否逻辑删除</param>
        /// <returns>是否逻辑删除成功</returns>
        public static bool SetActive<TModel>(this IDbConnection connection,dynamic id,bool isActive = false) where TModel : class
        {
            var modelType = typeof(TModel);
            var keyPropertyInfo = modelType.GetProperties().FirstOrDefault(p => p.GetCustomAttributes(typeof(KeyAttribute), true).Count() > 0);
            var IsActivePropertyInfo = modelType.GetProperties().FirstOrDefault(p => p.GetCustomAttributes(typeof(ActiveAttribute), true).Count() > 0);
            if(IsActivePropertyInfo == null)
                throw new NotSupportedException("实体类没有定义Active特性！");

            string tableName = GetTableName(typeof(TModel));
            string template = string.Format("Update {0} SET IsActive=@IsActive Where {1}=@Id",tableName,keyPropertyInfo.Name);
            var dynParms = new DynamicParameters();
            dynParms.Add("@IsActive",isActive);
            dynParms.Add("@Id",id);

            bool ret = connection.Execute(template, dynParms) > 0;
            return ret;
        }

        /// <summary>
        ///  逻辑删除表实体对应的一条数据记录
        /// </summary>
        /// <typeparam name="TModel">表实体</typeparam>
        /// <param name="connection">数据库连接</param>
        /// <param name="model">表实体对象</param>
        /// <returns>是否逻辑删除成功</returns>
        public static bool SetActive<TModel>(this IDbConnection connection, TModel model) where TModel : class
        {
            var modelType = typeof(TModel);
            var keyPropertyInfo = modelType.GetProperties().FirstOrDefault(p => p.GetCustomAttributes(typeof(KeyAttribute), true).Count() > 0);
            var IsActivePropertyInfo = modelType.GetProperties().FirstOrDefault(p => p.GetCustomAttributes(typeof(ActiveAttribute), true).Count() > 0);

            if(keyPropertyInfo == null)
                throw new NotSupportedException("实体类没有定义主键(Key)特性！");

            if (IsActivePropertyInfo == null)
                throw new NotSupportedException("实体类没有定义逻辑删除(Active)特性！");

            //取得表实体对象的值 id and isactive field
            object id = keyPropertyInfo.GetValue(model,null);
            object isActive = IsActivePropertyInfo.GetValue(model,null);

            string tableName = GetTableName(typeof(TModel));
            //execute sql
            string template = string.Format("Update {0} SET IsActive=@IsActive Where {1}=@Id", tableName, keyPropertyInfo.Name);
            var dynParms = new DynamicParameters();
            dynParms.Add("@IsActive", isActive);
            dynParms.Add("@Id", id);

            bool ret = connection.Execute(template, dynParms) > 0;
            return ret;
        }


        /// <summary>
        /// 查找一条数据,并且IsActive=1
        /// </summary>
        /// <typeparam name="TModel">表名</typeparam>
        /// <param name="connection">数据库连接</param>
        /// <param name="id">表记录ID</param>
        /// <returns>一条实体记录</returns>
        public static TModel GetIsActive<TModel>(this IDbConnection connection, dynamic id)
        {
            var modelType = typeof(TModel);
            var tableName = GetTableName(modelType);
            var keyPropertyInfo = modelType.GetProperties().FirstOrDefault(p => p.GetCustomAttributes(typeof(KeyAttribute), true).Count() > 0);
            if (keyPropertyInfo == null)
                throw new NotSupportedException("实体类没有定义主键(Key)特性！");

            string template = $"Select * from {tableName} Where {keyPropertyInfo.Name}=@Id AND IsActive=@IsActive";
            var dynParms = new DynamicParameters();
            dynParms.Add("@IsActive", 1);
            dynParms.Add("@Id", id);
            return connection.QueryFirstOrDefault<TModel>(template, dynParms);

        }

        /// <summary>
        /// 查找数据集合，并且IsActive=1
        /// </summary>
        /// <typeparam name="TModel">表名</typeparam>
        /// <param name="connection">数据库连接</param>
        /// <returns>实体记录集合</returns>
        public static IEnumerable<TModel> GetAllIsActive<TModel>(this IDbConnection connection)
        {
            var modelType = typeof(TModel);
            var tableName = GetTableName(modelType);
            string template = $"Select * from {tableName} Where IsActive=@IsActive";
            var dynParms = new DynamicParameters();
            dynParms.Add("@IsActive", 1);
            return connection.Query<TModel>(template, dynParms);
        }


        public static PageView<TModel> GetPager<TModel>(this IDbConnection connection , PageCondition condition) where TModel:class 
        {
            if(condition == null) throw new ArgumentNullException("condition");

            var dps = new DynamicParameters();
            dps.Add("@TableName",condition.TableName);
            dps.Add("@FieldNames",condition.FieldNames);

            if(!string.IsNullOrEmpty(condition.Where))
                dps.Add("@WhereString",condition.Where);

            dps.Add("@OrderField", condition.OrderField);
            dps.Add("@OrderType", Convert.ToInt32(condition.OrderType));

            dps.Add("@PageIndex", condition.Index);
            dps.Add("@PageSize", condition.Size);

            PageView<TModel> pageModel = null;


            using (var muti = connection.QueryMultiple("Proc_Common_PagerHelper", dps))
            {
               int count = muti.ReadSingle<int>();
               var data = muti.Read<TModel>();
               pageModel = new PageView<TModel>(data,count);
            }

            return pageModel;
        }

        /// <summary>
        /// 立即执行更新语句,API采用 Update(new {ID = "5" NameA = "imyundong"}).Where(expression).Go()的链式使用方式
        /// </summary>
        /// <typeparam name="TModel">模型名称</typeparam>
        /// <param name="connection"></param>
        /// <param name="param">匿名对象或实体对象</param>
        /// <example> session.Update(new {ID = "5", NameA = "imyundong"}).Where(p => p.Age > 5).Go</example>
        /// <example> session.Update(new {entity.ID, entity.NameA}).Where(p => p.Age > 5).Go</example>
        /// <example> session.Update(entity).Where(p => p.Age > 5).Go</example>
        /// <returns></returns>
        public static IOperatorWhere<TModel> Update<TModel>(this IDbConnection connection, object param)
            where TModel : class
        {
            string tableName = GetTableName(typeof(TModel));
            List<Parameter> ps = new List<Parameter>();


            IDictionary<string, object> filesDic = GetObjectValues(param);
            List<string> setSql = new List<string>();
            foreach (KeyValuePair<string, object> f in filesDic)
            {
                setSql.Add(string.Format("{0}=@{0}", f.Key));
                ps.Add(new Parameter()
                {
                    IsMethodType = false,
                    Name = "@" + f.Key,
                    Value = f.Value
                });
            }
            
            string paramterNameAndValues = string.Join(",", setSql);
            string template = string.Format("Update {0} SET {1} Where ", tableName, paramterNameAndValues);

            return new OperatorWhereObject<TModel>(connection, template, ps);
        }

        /// <summary>
        /// 反射获取匿名类型或实体的属性键和值
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IDictionary<string, object> GetObjectValues(object obj)
        {
            IDictionary<string, object> result = new Dictionary<string, object>();
            if (obj == null)
            {
                return result;
            }

            foreach (var propertyInfo in obj.GetType().GetProperties())
            {
                string name = propertyInfo.Name;
                object value = propertyInfo.GetValue(obj, null);
                // 忽略标记Computed特性的属性
                var computedAttr = propertyInfo
                        .GetCustomAttributes(false).SingleOrDefault(attr => attr.GetType().Name == "ComputedAttribute")
                    as dynamic;
                if (computedAttr == null)
                {
                    result[name] = value;
                }
            }


            return result;
        }
    }
}