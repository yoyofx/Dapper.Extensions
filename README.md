# Dapper.Extensions
YOYOFx中使用的Dapper扩展
###1. Get Entity：
```javascript
using (var connect = GetOpenConnection())
{
    var customer = connect.Get<CustomersEntity>(1);
    var customer1 = connect.Get<CustomersEntity>(c=>c.CustomerID == 1);
}
```

###2. Insert Entity：
```javascript
using (var connect = GetOpenConnection())
{
    int count = connect.QuerySingle<int>("select count(1) from Customers2");
    var customer = new CustomersEntity() { CustomerName = "A1", CustomerCity = "x1" , CustomerNumber = "9999"};
    long id = connect.Insert(customer);
}
```

###3. Update Entity：
```javascript
using (var connect = GetOpenConnection())
{
     var customer = new CustomersEntity() { CustomerID = 1 , CustomerName = "A1", CustomerCity = "x1", CustomerNumber = "9999" };
      //1.更新实体，不能只更新某些字段
      bool s = connect.Update(customer);
      //2.更新一个表达式实体，保证只更新表达式中的字段值
      //IOperatorWhere<Customers2>.Where(exp).Go();
      var updater = connect.Update<CustomersEntity>(c => new CustomersEntity
      {
          CustomerName = "hello",
          CustomerCity = "x1"
      });
      bool ret = updater.Go();
}
```
