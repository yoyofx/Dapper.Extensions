using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extensions
{
    /// <summary>
    /// SQL语句中的函数映射
    /// </summary>
    public class SqlMethodNameCallBack
    {
        /// <summary>
        /// 返回在SQLMethod类中定义函数在SQLServer中调用名称
        /// </summary>
        /// <param name="methodName">SQLMethod类的静态函数名</param>
        /// <returns>SQLServer中的函数名称</returns>
        public static string MethodNameCallback(string methodName)
        {
            string retName = string.Empty;
            switch (methodName)
            {
                case "getdate":
                    retName = "GETDATE()";
                    break;

                default:
                    break;
            }

            return retName; 
        } 

    }
}
