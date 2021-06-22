using System;
using System.Threading.Tasks;
using Contracts;
using Contracts.SpecificRepositoryInterfaces;
using Entities;
using Repository.SpecificRepositories;

namespace Repository
{
    public class RepositoryManager : IRepositoryManager
    {
        private readonly RepositoryContext _repositoryContext;
        private readonly ICompanyRepository _companyRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public RepositoryManager(RepositoryContext repositoryContext)
        {
            this._repositoryContext = repositoryContext;
        }

        public ICompanyRepository Company
        {
            get => this._companyRepository ?? new CompanyRepository(this._repositoryContext);
        }

        public IEmployeeRepository Employee {
            get => this._employeeRepository ?? new EmployeeRepository(this._repositoryContext);
        }

        public Task SaveChangesAsync() => this._repositoryContext.SaveChangesAsync();
    }
}
