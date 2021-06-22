using System;
using System.Collections.Generic;
using System.Linq;
using Contracts.SpecificRepositoryInterfaces;
using Entities;
using Entities.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Repository.SpecificRepositories
{
    public class CompanyRepository: RepositoryBase<Company>, ICompanyRepository
    {
        public CompanyRepository(RepositoryContext repositoryContext)
            :base(repositoryContext)
        {
        }

        public async Task<IEnumerable<Company>> GetAllCompaniesAsync(bool trackChanges) =>
            await FindAll(trackChanges)
            .OrderBy(c => c.Name)
            .ToListAsync();

        public async Task<Company> GetCompanyAsync(Guid Id, bool trackChanges) =>
            await FindByCondition(c => c.Id.Equals(Id), trackChanges)
            .SingleOrDefaultAsync();

        public void CreateCompany(Company company) => Create(company);

        public async Task<IEnumerable<Company>> GetByIdsAsync(IEnumerable<Guid> Ids, bool trachChanges) =>
            await FindByCondition(x => Ids.Contains(x.Id), trachChanges)
            .ToListAsync();

        public void DeleteCompany(Company company) => Delete(company);
    }
}
