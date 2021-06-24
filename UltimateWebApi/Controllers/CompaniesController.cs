using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using UltimateWebApi.ActionFilters;
using UltimateWebApi.ModelBinders;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UltimateWebApi.Controllers
{
    //[ApiVersion("1.0")]
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

        
        [HttpGet(Name = "GetCompanies")]
        public async Task<IActionResult> GetCompanies()
        {
            var companies = await this._repositoryManager.Company.GetAllCompaniesAsync(trackChanges: false); // getting readonly
            //var companiesDto = companies.Select(c => new CompanyDto
            //{
            //    Id = c.Id,
            //    Name = c.Name,
            //    FullAddress = string.Join(' ', c.Address, c.Country)
            //}).ToList();
            var companiesDto = this._mapper.Map<IEnumerable<CompanyDto>>(companies);
            return Ok(companiesDto);
        }

        
        [HttpGet("{Id:Guid}", Name = "CompanyById")]
        [ServiceFilter(typeof(ValidateCompanyExistsAttribute))]
        public IActionResult GetCompany(Guid Id)
        {
            // Used before created custom ValidateCompanyExistAttribute
            // var company = await this._repositoryManager.Company.GetCompanyAsync(Id, trackChanges: false);
            
            var company = HttpContext.Items["company"] as Company;
            
            // Used before created ValidateCompanyExistsAttribute
            // if (company == null)
            // {
            //     this._logger.LogInfo($"Company with {Id} Id does not exist.");
            //     return NotFound();
            // }
            
            var companyDto = this._mapper.Map<CompanyDto>(company);
            return Ok(companyDto);
        }

        
        // Ids:string => ModelBinder => IEnumerable<Guid> Ids
        [HttpGet("collection/({Ids})", Name = "CompanyCollection")]
        public async Task<IActionResult> GetCompanyCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> Ids)
        {
            if (Ids == null)
            {
                this._logger.LogError("Ids parameter is null.");
                return BadRequest("Ids parameter is null");
            }

            var companies = await this._repositoryManager.Company.GetByIdsAsync(Ids, trachChanges: false);
            if (Ids.Count() != companies.Count())
            {
                this._logger.LogError("Some ids are not valid in a collection.");
                return NotFound();
            }

            var compatiesDtoToView = this._mapper.Map<IEnumerable<CompanyDto>>(companies);
            return Ok(compatiesDtoToView);
        }

        
        [HttpPost("collection")]
        public async Task<IActionResult> CreateCompanyCollection([FromBody] IEnumerable<CompanyForCreateDto> collection)
        {
            /*
             * POST:
                [
                    {
                        "name": "Sales all over the world Ltd",
                        "address": "355 Open Street, B 784",
                        "country": "USA"
                    },
                    {
                        "name": "Branding Ltd",
                        "address": "255 Main Street, K 334",
                        "country": "USA"
                    }
                ]
             */
            if (collection == null)
            {
                _logger.LogError("Company collection sent from client is null.");
                return BadRequest("Company collection is null");
            }

            var companies = this._mapper.Map<IEnumerable<Company>>(collection);
            foreach (var company in companies)
            {
                this._repositoryManager.Company.CreateCompany(company);
            }
            await this._repositoryManager.SaveChangesAsync();

            var companiesDtoToView = this._mapper.Map<IEnumerable<CompanyDto>>(companies);
            var Ids = string.Join(",", companiesDtoToView.Select(c => c.Id));

            return CreatedAtRoute("CompanyCollection", new { Ids }, companiesDtoToView);
        }

        
        [HttpPost(Name = "CreateCompany")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreateCompany([FromBody] CompanyForCreateDto companyForCreateDto)
        {
            /* 
             * We can do smth like this and employees will be created:
                {
                    "name":"Electronics Solutions Ltd",
                    "address": "312 Deviever Avenue, K 334",
                    "country": "USA",
                    "employees": [
                        {
                            "name": "Joan Dane",
                            "age": 31,
                            "position": "Manager"
                        },
                        {
                            "name": "Moana Greyn",
                            "age": 27,
                            "position": "Administrator" 
                        }
                    ]
                }
             */
            
            // Used before we created a custom ValidationFilterAttribute
            // if (companyForCreateDto == null)
            // {
            //     this._logger.LogError("CompanyForCreateDto object sent from client is null.");
            //     return BadRequest("CompanyForCreateDto object is null");
            // }

            var company = this._mapper.Map<Company>(companyForCreateDto);
            
            this._repositoryManager.Company.CreateCompany(company);
            await this._repositoryManager.SaveChangesAsync();

            var companyDtoToView = this._mapper.Map<CompanyDto>(company);

            return CreatedAtRoute("CompanyById", new { Id = companyDtoToView.Id }, companyDtoToView);
        }

        
        [HttpPut("{Id:Guid}")] // Fully update a resource
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        [ServiceFilter(typeof(ValidateCompanyExistsAttribute))]
        public async Task<IActionResult> UpdateCompany(Guid Id, [FromBody] CompanyForUpdateDto companyUpdateDto)
        {
            // Used before we created a custom ValidationFilterAttribute
            // if(companyUpdateDto == null)
            // {
            //     this._logger.LogError("CompanyForUpdateDto object sent from client is null.");
            //     return BadRequest("CompanyForUpdateDto object is null");
            // }

            // Used before created custom ValidateCompanyExistAttribute
            // var companyEntity = await this._repositoryManager.Company.GetCompanyAsync(Id, trackChanges: true);

            var companyEntity = HttpContext.Items["company"] as Company;
            
            // Used before created ValidateCompanyExistsAttribute
            // if(companyEntity == null)
            // {
            //     this._logger.LogInfo($"Company with {Id} Id does not exist.");
            //     return NotFound();
            // }

            this._mapper.Map(companyUpdateDto, companyEntity);
            await this._repositoryManager.SaveChangesAsync();

            return NoContent();
        }

        
        [HttpDelete("{Id:Guid}")]
        [ServiceFilter(typeof(ValidateCompanyExistsAttribute))]
        public async Task<IActionResult> DeleteCompany(Guid Id)
        {
            // Used before created ValidateCompanyExistsAttribute
            
            // var company = await this._repositoryManager.Company.GetCompanyAsync(Id, trackChanges: false);
            
            // if(company == null)
            // {
            //     this._logger.LogInfo($"Company with id: {Id} doesn't exist in the database.");
            //     return NotFound();
            // }

            var company = HttpContext.Items["company"] as Company; 
            
            this._repositoryManager.Company.DeleteCompany(company);
            await this._repositoryManager.SaveChangesAsync();

            return NoContent();
        }

        [HttpOptions]
        public IActionResult GetCompaniesOptions()
        {
            HttpContext.Response.Headers.Add("Allow", "GET, OPTIONS, POST, PUT");
            
            return Ok();
        }
    }
}
