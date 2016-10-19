using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using Dapper;
using Dapper.Contrib;
using Dapper.Contrib.Extensions;
using Dapper.Extensions;
using Dapper.Extensions.Tests;
using Xunit;
using System.Linq;

namespace Dapper.Extensions.Tests
{

    public class WhereExpressionTest
    {
        [Fact]
        public void SimpleWhereTest1()
        {
            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => ct.CustomerID < 50 && ct.CustomerCity == "B-City";

            var translate = new SqlTranslateFormater();
            string sql = translate.Translate(queryExp1);

            Assert.Equal(sql, "CustomerID < 50 AND CustomerCity = 'B-City'");

        }

        [Fact]
        public void SimpleWhereTest2()
        {
            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => ct.CustomerID == 1 &&
                                                            (ct.CustomerCity == "B-City" || ct.CustomerNumber == "0000");

            var translate = new SqlTranslateFormater();
            string sql = translate.Translate(queryExp1);

            Assert.Equal(sql, "CustomerID = 1 AND (CustomerCity = 'B-City' OR CustomerNumber = '0000')");

        }

        [Fact]
        public void SimpleWhereMethodTest1()
        {
            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => ct.CustomerID <= 50 && (SQLMethod.IsNull(ct.CustomerCity));

            var translate = new SqlTranslateFormater();
            string sql = translate.Translate(queryExp1);

            Assert.Equal(sql, "CustomerID <= 50 AND CustomerCity is NULL");

        }


        [Fact]
        public void SimpleWhereMethodTest2()
        {
            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => ct.CustomerID <= 50 && (SQLMethod.IsNull(ct.CustomerCity));

            var translate = new SqlTranslateFormater();
            string sql = translate.Translate(queryExp1);

            Assert.Equal(sql, "CustomerID <= 50 AND CustomerCity is NULL");

        }


        [Fact]
        public void WhereMethodForParametersTest()
        {
            TestValueTypeParam(50);
        }

        private void TestValueTypeParam(int id)
        {
            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => ct.CustomerID <= id && (SQLMethod.IsNull(ct.CustomerCity));

            var translate = new SqlTranslateFormater();
            string sql = translate.Translate(queryExp1);

            Assert.Equal(sql, "CustomerID <= 50 AND CustomerCity is NULL");
        }

        [Fact]
        public void WhereMethodForParametersTest2()
        {
            TestEntityParam(new TestValue { Id = 50 });
        }

        private void TestEntityParam(TestValue v)
        {
            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => ct.CustomerID <= v.Id && (SQLMethod.IsNull(ct.CustomerCity));

            var translate = new SqlTranslateFormater();
            string sql = translate.Translate(queryExp1);

            Assert.Equal(sql, "CustomerID <= 50 AND CustomerCity is NULL");
        }

        [Fact]
        public void SimpleWhereMethodTest3()
        {
            //IEnumerable<int> ids = new List<int>() { 40, 50 };//通过测试
            //int[] ids = new[] {40, 50};//通过测试
            List<int> ids = new List<int>() { 40, 50 };//通过测试

            Expression<Func<CustomersEntity, bool>> queryExp1 = ct => ids.Contains(ct.CustomerID) && (SQLMethod.IsNull(ct.CustomerCity));

            var translate = new SqlTranslateFormater();
            string sql = translate.Translate(queryExp1);

            Assert.Equal(sql, "CustomerID In (40,50) AND CustomerCity is NULL");

        }


    }

    public class TestValue
    {
        public int Id { set; get; }
    }


}
