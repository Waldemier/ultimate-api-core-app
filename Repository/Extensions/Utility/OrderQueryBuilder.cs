using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Repository.Extensions.Utility
{
    public static class OrderQueryBuilder
    {
        public static string CreateOrderQuery<T>(string orderByQueryString)
        {
            var orderParams = orderByQueryString.Trim().Split(',');

            var propertyInfos = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance); // get prop which are a public and non-static

            var orderQueryBuilder = new StringBuilder();

            foreach (var param in orderParams)
            {
                if(string.IsNullOrWhiteSpace(param))
                    continue;

                var propertyFromQueryName = param.Split(' ')[0];
                var objectProperty = propertyInfos.FirstOrDefault(prop =>
                    prop.Name.Equals(propertyFromQueryName, StringComparison.InvariantCultureIgnoreCase)); // InvariantCultureIgnoreCase ignoring register
                
                if(objectProperty == null) 
                    continue;

                var direction = param.EndsWith(" desc") ? "descending" : "ascending";

                orderQueryBuilder.Append($"{objectProperty.Name.ToString()} {direction}, "); // ..., ..., ..., etc
            }

            var orderQuery = orderQueryBuilder.ToString().TrimEnd(',', ' '); // ... ascending, ... descending, ... ascending

            return orderQuery;
        }
    }
}