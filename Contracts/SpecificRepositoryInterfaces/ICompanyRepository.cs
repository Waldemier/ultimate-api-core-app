﻿using System;
using System.Collections.Generic;
using Entities.Models;

namespace Contracts.SpecificRepositoryInterfaces
{
    public interface ICompanyRepository
    {
        IEnumerable<Company> GetAllCompanies(bool trackChanges);
        Company GetCompany(Guid Id, bool trackChanges);
        void CreateCompany(Company company);
        IEnumerable<Company> GetByIds(IEnumerable<Guid> Ids, bool trachChanges);
        void DeleteCompany(Company company);
    }
}
