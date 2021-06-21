using System;
using System.Collections.Generic;
using System.Linq;
using Contracts.SpecificRepositoryInterfaces;
using Entities;
using Entities.Models;

namespace Repository.SpecificRepositories
{
    public class EmployeeRepository: RepositoryBase<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(RepositoryContext repositoryContext): base(repositoryContext)
        {
        }

        public IEnumerable<Employee> GetEmployees(Guid companyId, bool trackChanges) =>
            FindByCondition(e => e.CompanyId.Equals(companyId), trackChanges)
            .ToList();

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
