using System;
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
    }
}
