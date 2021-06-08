using System;
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
    }
}
