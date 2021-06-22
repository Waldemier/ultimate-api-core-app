using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Models;

namespace Contracts.SpecificRepositoryInterfaces
{
    public interface ICompanyRepository
    {
        Task<IEnumerable<Company>> GetAllCompaniesAsync(bool trackChanges);
        Task<Company> GetCompanyAsync(Guid Id, bool trackChanges);
        void CreateCompany(Company company);
        Task<IEnumerable<Company>> GetByIdsAsync(IEnumerable<Guid> Ids, bool trachChanges);
        void DeleteCompany(Company company);
    }
}
