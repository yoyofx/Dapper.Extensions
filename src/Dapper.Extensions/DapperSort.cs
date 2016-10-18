using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extensions
{
    /// <summary>
    /// 排序对象
    /// </summary>
    public class DapperSort : List<Sort>
    {
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            this.Sort(new SortCompair());
            for (int i = 0; i < this.Count; i++)
            {
                sb.AppendFormat("{0} {1}", this[i].Field, this[i].ESortType);
                if (i < this.Count - 1)
                {
                    sb.Append(", ");
                }
            }
            return sb.ToString();
        }
    }

    public class SortCompair : IComparer<Sort>
    {
        public int Compare(Sort x, Sort y)
        {
            return x.Index - y.Index;
        }
    }

    public class Sort
    {
        public Sort(int index, string field, ESortType sortType)
        {
            Index = index;
            Field = field;
            ESortType = sortType;
        }

        public int Index { get; set; }
        public string Field { get; set; }
        public ESortType ESortType { get; set; }
    }

    public enum ESortType
    {
        Asc = 1,
        Desc = 2
    }
}
