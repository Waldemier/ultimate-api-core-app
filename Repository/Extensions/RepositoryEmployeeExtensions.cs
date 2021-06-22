using System;
using System.Linq;
using Entities.Models;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Text;
using Repository.Extensions.Utility;

namespace Repository.Extensions
{
    /// <summary>
    /// Expanded IQueryable type object
    /// </summary>
    public static class RepositoryEmployeeExtensions
    {
        public static IQueryable<Employee> FilterEmployees(this IQueryable<Employee> employees, uint minAge, uint maxAge) =>
            employees.Where(e => (e.Age >= minAge && e.Age <= maxAge));

        public static IQueryable<Employee> Search(this IQueryable<Employee> employees, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return employees;

            var lowerCaseTerm = searchTerm.Trim().ToLower();

            return employees.Where(e => e.Name.ToLower().Contains(lowerCaseTerm));
        }

        public static IQueryable<Employee> Sort(this IQueryable<Employee> employees, string orderByQueryString)
        {
            /*
             * For example what we accepting
             * https://localhost:5001/api/companies/c9d4c053-49b6-410c-bc78-2d54a9991870/employees?orderBy=name,age desc
             * orderByQueryString: name,age desc
             */
            if (string.IsNullOrWhiteSpace(orderByQueryString))
                return employees.OrderBy(e => e.Name);

            var orderQuery = OrderQueryBuilder.CreateOrderQuery<Employee>(orderByQueryString); // Custom method by implementing in Utility folder
            
            if (string.IsNullOrWhiteSpace(orderQuery))
                return employees.OrderBy(e => e.Name);

            return employees.OrderBy(orderQuery); // This is possible thanks to System.Linq.Dynamic.Core Library.
        }
    }
}