using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper.Contrib.Extensions;

namespace Dapper.Extensions.Tests
{
    [Table("Customers2")]
    public partial class CustomersEntity
    {

        [Key]

        ///<summary>
        ///
        ///</summary>               
        public Int32 CustomerID { set; get; }

        ///<summary>
        ///
        ///</summary>               
        public String CustomerNumber { set; get; }

        ///<summary>
        ///
        ///</summary>               
        public String CustomerName { set; get; }

        ///<summary>
        ///
        ///</summary>               
        public String CustomerCity { set; get; }

        [Active]

        ///<summary>
        ///
        ///</summary>               
        public Nullable<Boolean> IsActive { set; get; }

    }
}
