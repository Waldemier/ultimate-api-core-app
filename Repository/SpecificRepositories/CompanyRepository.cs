using System;
using System.Collections.Generic;
using System.Linq;
using Contracts.SpecificRepositoryInterfaces;
using Entities;
using Entities.Models;

namespace Repository.SpecificRepositories
{
    public class CompanyRepository: RepositoryBase<Company>, ICompanyRepository
    {
        public CompanyRepository(RepositoryContext repositoryContext)
            :base(repositoryContext)
        {
        }

        public IEnumerable<Company> GetAllCompanies(bool trackChanges) =>
            FindAll(trackChanges)
            .OrderBy(c => c.Name)
            .ToList();

        public Company GetCompany(Guid Id, bool trackChanges) =>
            FindByCondition(c => c.Id.Equals(Id), trackChanges)
            .SingleOrDefault();
    }
}
