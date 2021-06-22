using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Models;
using Entities.RequestFeatures;

namespace Contracts.SpecificRepositoryInterfaces
{
    public interface IEmployeeRepository
    {
        Task<PagedList<Employee>> GetEmployeesAsync(Guid companyId, EmployeeParameters employeeParameters, bool trackChanges);
        Employee GetEmployee(Guid companyId, Guid Id, bool trackChanges);
        void CreateEmployeeForCompany(Guid companyId, Employee employee);
        void DeleteEmployee(Employee employee);
    }
}
