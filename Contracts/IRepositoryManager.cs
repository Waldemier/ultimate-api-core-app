using System;
using System.Threading.Tasks;
using Contracts.SpecificRepositoryInterfaces;

namespace Contracts
{
    public interface IRepositoryManager
    {
        ICompanyRepository Company { get; }
        IEmployeeRepository Employee { get; }
        Task SaveChangesAsync();
    }
}
