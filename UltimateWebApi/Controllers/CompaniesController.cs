using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Repository;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UltimateWebApi.Controllers
{
    [ApiController]
    [Route("api/companies")]
    public class CompaniesController : ControllerBase
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        public CompaniesController(IRepositoryManager repositoryManager, ILoggerManager logger, IMapper mapper)
        {
            this._repositoryManager = repositoryManager;
            this._logger = logger;
            this._mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetCompanies()
        {
            var companies = this._repositoryManager.Company.GetAllCompanies(trackChanges: false); // getting readonly
            //var companiesDto = companies.Select(c => new CompanyDto
            //{
            //    Id = c.Id,
            //    Name = c.Name,
            //    FullAddress = string.Join(' ', c.Address, c.Country)
            //}).ToList();
            var companiesDto = this._mapper.Map<IEnumerable<CompanyDto>>(companies);
            return Ok(companiesDto);
        }

        [HttpGet("{Id:Guid}")]
        public IActionResult GetCompany(Guid Id)
        {
            var company = this._repositoryManager.Company.GetCompany(Id, trackChanges: false);
            if(company == null)
            {
                this._logger.LogInfo($"Company with {Id} Id does not exist.");
                return NotFound();
            }
            var companyDto = this._mapper.Map<CompanyDto>(company);
            return Ok(companyDto);
        }
    }
}
