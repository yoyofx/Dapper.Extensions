using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper.Contrib.Extensions;
using Xunit;

namespace Dapper.Extensions.Tests
{
    public class DapperExtensionsTest
    {
        private IDbConnection GetOpenConnection()
        {
            var connection = new SqlConnection(@"");
            //connection.Open();
            return connection;
        }

        [Fact]
        public void GetCustomerById()
        {
            using (var connect = GetOpenConnection())
            {
                var customer = connect.Get<CustomersEntity>(1);
                Assert.Equal(customer.CustomerID , 1);
                Assert.Equal(customer.CustomerName, "A1");
                Assert.Equal(customer.CustomerNumber, "9999");
                Assert.Equal(customer.CustomerCity, "x1");
            }
        }

        [Fact]
        public void GetCustomerByQuery()
        {
            using (var connect = GetOpenConnection())
            {
                var customer = connect.Get<CustomersEntity>(c=>c.CustomerID == 1);
                Assert.Equal(customer.CustomerID, 1);
                Assert.Equal(customer.CustomerName, "A1");
                Assert.Equal(customer.CustomerNumber, "9999");
                Assert.Equal(customer.CustomerCity, "x1");
            }
        }


        [Fact]
        public void GetCustomerListByQuery()
        {
            using (var connect = GetOpenConnection())
            {
                var customers = connect.GetAll<CustomersEntity>(c => c.CustomerID <= 50);
                Assert.Equal(customers.Count() , 50);
            }
        }


        [Fact]
        public void InsertCustomer()
        {
            using (var connect = GetOpenConnection())
            {
                int count = connect.QuerySingle<int>("select count(1) from Customers2");


                var customer = new CustomersEntity() { CustomerName = "A1", CustomerCity = "x1" , CustomerNumber = "9999"};

                long id = connect.Insert(customer);

                Assert.Equal(id - 2, count);
            }
        }


        /// <summary>
        /// 注意：Dapper自带的Update语句；不能只更新某些字段
        /// </summary>
        [Fact]
        public void UpdateCustomer()
        {
            using (var connect = GetOpenConnection())
            {
              
                var customer = new CustomersEntity() { CustomerID = 1 , CustomerName = "A1", CustomerCity = "x1", CustomerNumber = "9999" };
                //更新实体，不能只更新某些字段
                bool s = connect.Update(customer);

                Assert.Equal(s, true);
            }
        }


        /// <summary>
        /// 注意：咱们框架里扩展的复杂Update语句，保证只更新表达式中的字段值
        /// </summary>
        [Fact]
        public void SimpleUpdateExpressionMethodTest1()
        {
            using (var connect = GetOpenConnection())
            {
                //更新一个表达式实体，保证只更新表达式中的字段值
                //IOperatorWhere<Customers2>.Where(exp);
                var updater = connect.Update<CustomersEntity>(c => new CustomersEntity
                {
                    CustomerName = "hello",
                    CustomerCity = "x1"
                })
                .Where(c => c.CustomerID == 2);

                string sql = updater.SQL;

                Assert.Equal(sql,
                    "Update Customers2 SET CustomerName=@CustomerName,CustomerCity=@CustomerCity Where CustomerID = 2");

                //IOperatorWhere<Customers2>.Go();
                //API采用 Update({new entity expression}).Where(expression).Go()的链式使用方式。
                bool ret = updater.Go();

                Assert.Equal(ret, true);
            }
        }

        [Fact]
        public void SimpleUpdateExpressionMethodTest2()
        {
            using (var connect = GetOpenConnection())
            {
                // 匿名对象1
                var entity1 = new { CustomerName = "hello", CustomerCity = "x1" };

                var updater1 = connect.Update<CustomersEntity>(entity1)
                .Where(c => c.CustomerID == 2);

                string sql1 = updater1.SQL;
                Assert.Equal(sql1,
                    "Update Customers2 SET CustomerName=@CustomerName,CustomerCity=@CustomerCity Where CustomerID = 2");

                // 匿名对象2
                CustomersEntity entity2 = new CustomersEntity() { CustomerName = "hello", CustomerCity = "x1", CustomerNumber = "x123" };

                var updater2 = connect.Update<CustomersEntity>(new { entity2.CustomerName, entity2.CustomerCity })
                .Where(c => c.CustomerID == 2);

                string sql2 = updater2.SQL;
                Assert.Equal(sql2,
                    "Update Customers2 SET CustomerName=@CustomerName,CustomerCity=@CustomerCity Where CustomerID = 2");

                // 实体对象/全字段更新且忽略标记了Computed特性的属性
                var updater3 = connect.Update<CustomersEntity>(entity2)
                .Where(c => c.CustomerID == 2);
                string sql3 = updater3.SQL;
            }
        }



        /// <summary>
        /// 注意：Dapper扩展Update语句；用于更新IsActive字段
        /// </summary>
        [Fact]
        public void SetActiveTest()
        {
            using (var connect = GetOpenConnection())
            {

                bool s = connect.SetActive<CustomersEntity>(1, true);

                Assert.Equal(s, true);
            }
        }


        /// <summary>
        /// 注意：Dapper扩展Update语句；用于更新IsActive字段
        /// </summary>
        [Fact]
        public void SetActiveTest2()
        {
            using (var connect = GetOpenConnection())
            {
                var customer = new CustomersEntity { CustomerID = 2 , IsActive = false };
                bool s = connect.SetActive(customer);

                Assert.Equal(s, true);
            }
        }


    }
}
