using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.SpecificRepositoryInterfaces;
using Entities;
using Entities.Models;
using Entities.RequestFeatures;
using Microsoft.EntityFrameworkCore;
using Repository.Extensions;

namespace Repository.SpecificRepositories
{
    public class EmployeeRepository: RepositoryBase<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(RepositoryContext repositoryContext): base(repositoryContext)
        {
        }

        public async Task<PagedList<Employee>> GetEmployeesAsync(Guid companyId, EmployeeParameters employeeParameters,
            bool trackChanges)
        {
            var employees = await FindByCondition(e => e.CompanyId.Equals(companyId), trackChanges)
                .FilterEmployees(employeeParameters.MinAge, employeeParameters.MaxAge) // custom option implemented in Repository Extension folder
                .Search(employeeParameters.SearchTerm)
                .Sort(employeeParameters.OrderBy)
                //.OrderBy(e => e.Name) // sorting by name
                //.Skip((employeeParameters.PageNumber - 1) * employeeParameters.PageSize) // 2-1 * 2 = 2 - skip, take 2 next (because page size = 2 for example)
                //.Take(employeeParameters.PageSize)  
                .ToListAsync();
            
            return PagedList<Employee>.ToPagedList(employees, employeeParameters.PageNumber,
                employeeParameters.PageSize);
        }
            
        public Employee GetEmployee(Guid companyId, Guid Id, bool trackChanges) =>
            FindByCondition(e => e.CompanyId.Equals(companyId) && e.Id.Equals(Id), trackChanges)
            .SingleOrDefault();

        public void CreateEmployeeForCompany(Guid companyId, Employee employee)
        {
            employee.CompanyId = companyId;
            Create(employee);
        }

        public void DeleteEmployee(Employee employee) => Delete(employee);
    }
}
