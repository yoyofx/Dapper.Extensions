using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extensions
{
    /// <summary>
    /// 通用分页输入条件
    /// </summary>
    public class PageCondition
    {
        /// <summary>
        /// 通用分页输入条件构造器
        /// </summary>
        /// <param name="tableName">表名或SQL语句如1.customer 2.(select * from customer) as t1</param>
        /// <param name="pageIndex">当前页数</param>
        /// <param name="pageSize">每页输出的记录数</param>
        /// <param name="orderField">排序不含'ORDER BY'字符，当@SortType=3时生效，必须指定ASC或DESC，建议在最后加上主键  </param>
        /// <param name="orderType">排序规则（1:单列正序ASC；2:单列倒序DESC；3:多列排序；）</param>
        /// <param name="fieldNames">要显示的列名，如果是全部字段则为*  </param>
        /// <param name="where">查询条件 不含'WHERE'字符，如t1.Id > 6  5 AND t1.userid > 10000 ,字段应该与表名对应(TableName)如 t1.Id > 6  </param>
        public PageCondition(string tableName, int pageIndex, int pageSize, string orderField , PageOrderType orderType = PageOrderType.Asc, string fieldNames = "*", string where = null)
        {
            this.TableName = tableName;
            this.Index = pageIndex;
            this.Size = pageSize;
            this.FieldNames = fieldNames;
            this.Where = where;
            this.OrderField = orderField;
            this.OrderType = orderType;
        }

        /// <summary>
        /// 表名或SQL语句如1.customer 2.(select * from customer) as t1
        /// </summary>
        public string TableName { set; get; }

        /// <summary>
        /// 要显示的列名，如果是全部字段则为*  
        /// </summary>
        public string FieldNames { set; get; }

        /// <summary>
        /// 查询条件 不含'WHERE'字符，如t1.Id > 6  5 AND t1.userid > 10000 ,字段应该与表名对应(TableName)如 t1.Id > 6  
        /// </summary>
        public string Where { set; get; }
        /// <summary>
        /// 排序不含'ORDER BY'字符，当@SortType=3时生效，必须指定ASC或DESC，建议在最后加上主键   
        /// </summary>
        public string OrderField { set; get; }
        /// <summary>
        /// 排序规则（1:单列正序ASC；2:单列倒序DESC；3:多列排序；）    
        /// </summary>
        public PageOrderType OrderType { set; get; }
        /// <summary>
        /// 当前页数 
        /// </summary>
        public int Index { set; get; }
        /// <summary>
        /// 每页输出的记录数
        /// </summary>
        public int Size { set; get; }

    }

    /// <summary>
    /// 1:单列正序ASC；
    /// 2:单列倒序DESC；
    /// 3:多列排序；
    /// </summary>
    public enum PageOrderType
    { 
        Asc = 1,
        Desc = 2,
        Muti =3
    }


}
