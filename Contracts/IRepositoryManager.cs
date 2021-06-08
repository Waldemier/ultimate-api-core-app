using System;
using Contracts.SpecificRepositoryInterfaces;

namespace Contracts
{
    public interface IRepositoryManager
    {
        ICompanyRepository Company { get; }
        IEmployeeRepository Employee { get; }
        void SaveChanges();
    }
}
