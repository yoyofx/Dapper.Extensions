using System;
using System.Data;

namespace Dapper.Extensions
{
    /// <summary>
    /// new 表达式对象中的SQL语句参数，
    /// </summary>
    internal class Parameter
    {
        public string Name { get; set; }
        public object Value { get; set; }
   
        public bool IsMethodType { set; get; }

        public Parameter() { }

        public Parameter(string name, object value)
        {
            Name = name;
            if(value != null)
                Value = value;
        }

        public override string ToString()
        {
            return String.Format("  {0}={1}", Name, Value);
        }
    }
}