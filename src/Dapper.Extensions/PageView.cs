using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extensions
{
    /// <summary>
    /// 数据分页模型
    /// </summary>
    /// <typeparam name="TModel">返回的分页数据</typeparam>
    public class PageView<TModel> where TModel : class 
    {
        public PageView(IEnumerable<TModel> data, int count)
        {
            this.Data = data;
            this.Count = count;
        }

        /// <summary>
        /// 分页数据
        /// </summary>
        public IEnumerable<TModel> Data { set; get; }

        /// <summary>
        /// 要分页数据源的总条数
        /// </summary>
        public int Count { set; get; }

    }
}
