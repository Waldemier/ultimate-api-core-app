using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts;
using Entities;
using Entities.DataTransferObjects;
using Entities.Models;
using Entities.RequestFeatures;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using UltimateWebApi.ActionFilters;
using UltimateWebApi.Utility;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UltimateWebApi.Controllers
{
    [ApiController]
    [Route("api/companies/{companyId}/employees")]
    public class EmployeesController : ControllerBase
    {
        /*
         * https://localhost:5001/api/companies/c9d4c053-49b6-410c-bc78-2d54a9991870/employees?
         *                          pageNumber=1&pageSize=4&minAge=23&maxAge=40&searchTerm=A&orderBy=name,age desc&fields=name,age
         */
        private readonly IRepositoryManager _repositoryManager;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        private readonly IDataShaper<EmployeeDto> _dataShaper;
        private readonly EmployeeLinks _employeeLinks;
        
        public EmployeesController(IRepositoryManager repositoryManager, ILoggerManager logger, IMapper mapper, IDataShaper<EmployeeDto> dataShaper, EmployeeLinks employeeLinks)
        {
            this._repositoryManager = repositoryManager;
            this._logger = logger;
            this._mapper = mapper;
            this._dataShaper = dataShaper;
            this._employeeLinks = employeeLinks;
        }

        [HttpGet]
        [ServiceFilter(typeof(ValidateMediaTypeAttribute))]
        public async Task<IActionResult> GetEmployees(Guid companyId, 
            [FromQuery] EmployeeParameters employeeParameters)
        {
            if (!employeeParameters.ValidAgeRange)
                return BadRequest("Max age cant be less than min age.");
            
            var company = await this._repositoryManager.Company.GetCompanyAsync(companyId, trackChanges: false);
            if (company == null)
            {
                this._logger.LogInfo($"Company with {companyId} Id does not exist.");
                return NotFound();
            }
            
            var employees = await this._repositoryManager.Employee.GetEmployeesAsync(companyId, 
                employeeParameters, trackChanges: false);
            
            // X-Pagination: {"CurrentPage":2,"TotalPages":5,"PageSize":2,"TotalCount":9,"HasPrevious":true,"HasNext":true}
            HttpContext.Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(employees.MetaData)); 
            
            var employeesDto = this._mapper.Map<IEnumerable<EmployeeDto>>(employees);

            var links = this._employeeLinks.TryGenerateLinks(employeesDto, employeeParameters.Fields, companyId, HttpContext);
            
            // used before 
            // return Ok(this._dataShaper.ShapeData(employeesDto, employeeParameters.Fields));
            return links.HasLinks ? Ok(links.LinkedEntities) : Ok(links.ShapedEntities);
        }

        
        [HttpGet("{Id:Guid}", Name = "GetEmployeeForCompany")]
        public async Task<IActionResult> GetEmployee(Guid companyId, Guid Id)
        {
            var company = await this._repositoryManager.Company.GetCompanyAsync(companyId, trackChanges: false);
            if(company == null)
            {
                this._logger.LogInfo($"Company with {companyId} Id does not exist.");
                return NotFound();
            }

            var employee = this._repositoryManager.Employee.GetEmployee(companyId, Id, trackChanges: false);
            if(employee == null)
            {
                this._logger.LogInfo($"Employee with {Id} Id does not exist.");
                return NotFound();
            }
            var employeeDto = this._mapper.Map<EmployeeDto>(employee);

            return Ok(employeeDto);
        }

        
        [HttpPost]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreateEmployee(Guid companyId, [FromBody] EmployeeForCreateDto employeeForCreateDto)
        {
            // Used before we created a custom ValidationFilterAttribute
            // if(employeeForCreateDto == null)
            // {
            //     this._logger.LogError("EmployeeForCreateDto object sent from client is null.");
            //     return BadRequest("EmployeeForCreateDto object is null");
            // }

            if (!ModelState.IsValid)
            {
                this._logger.LogError("Invalid model state for the EmployeeForCreateDto object");
                return UnprocessableEntity(ModelState);
            }

            var company = await this._repositoryManager.Company.GetCompanyAsync(companyId, trackChanges: false);
            if(company == null)
            {
                this._logger.LogInfo($"Company with {companyId} Id does not exists.");
                return NotFound();
            }

            var employee = this._mapper.Map<Employee>(employeeForCreateDto);
            
            this._repositoryManager.Employee.CreateEmployeeForCompany(companyId, employee);
            await this._repositoryManager.SaveChangesAsync();

            var employeeDtoToView = this._mapper.Map<EmployeeDto>(employee);

            return CreatedAtRoute("GetEmployeeForCompany", new { companyId, Id = employeeDtoToView.Id }, employeeDtoToView);
        }

        
        [HttpPut("{Id:Guid}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> UpdateEmployeeForCompany(Guid companyId, Guid Id,
            [FromBody] EmployeeForUpdateDto employeeUpdateDto)
        {
            // Used before we created a custom ValidationFilterAttribute
            // if(employeeUpdateDto == null)
            // {
            //     this._logger.LogError("EmployeeForUpdateDto object sent from client is null.");
            //     return BadRequest("EmployeeForUpdateDto object is null.");
            // }
            
            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state for the EmployeeForUpdateDto object");
                return UnprocessableEntity(ModelState);
            }
            
            // Used before created ValidateEmployeeForCompanyExistsAttribute
            
            // var company = await this._repositoryManager.Company.GetCompanyAsync(companyId, trackChanges: false);
            //
            // if(company == null)
            // {
            //     this._logger.LogInfo($"Company with {companyId} Id does not exist.");
            //     return NotFound();
            // }
            //
            // var employeeEntity = this._repositoryManager.Employee.GetEmployee(companyId, Id, trackChanges: true);
            //
            // if(employeeEntity == null)
            // {
            //     this._logger.LogInfo($"Employee with {Id} Id does not exist.");
            //     return NotFound();
            // }

            var employeeEntity = HttpContext.Items["employee"] as Employee;

            this._mapper.Map(employeeUpdateDto, employeeEntity);
            await this._repositoryManager.SaveChangesAsync();

            return NoContent();
        }

        
        [HttpPatch("{Id:Guid}")]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> PartiallyUpdateEmployeeForCompany(Guid companyId, Guid Id,
            [FromBody] JsonPatchDocument<EmployeeForUpdateDto> patchDoc)
        {
            /* Content-Type: application/json-patch+json (Recommended by convension) OR JUST application/json */
            /*
             * example to send:
                [
                    {
                        "op": "replace",
                        "path": "/age",
                        "value": 28
                    }
                ]
            OR
                [
                    {
                        "op": "remove",
                        "path": "/age",
                    }
                ]
            OR
                [
                    {
                        "op": "add",
                        "path": "/age",
                        "value": 28
                    }
                ]
            */
            if (patchDoc == null)
            {
                this._logger.LogError("patchDoc object sent from client is null.");
                return BadRequest("patchDoc object is null");
            }
            
            // Used before created ValidateEmployeeForCompanyExistsAttribute
            
            //var companyEntity = await this._repositoryManager.Company.GetCompanyAsync(companyId, trackChanges: false);
            // if(companyEntity == null)
            // {
            //     this._logger.LogInfo($"Company with {companyId} Id does not exist.");
            //     return NotFound();
            // }
            //
            // var employeeEntity = this._repositoryManager.Employee.GetEmployee(companyId, Id, trackChanges: true);
            //
            // if(employeeEntity == null)
            // {
            //     this._logger.LogInfo($"Employee with {Id} Id does not exist.");
            //     return NotFound();
            // }

            var employeeEntity = HttpContext.Items["employee"] as Employee;
            
            var employeeUpdateDto = this._mapper.Map<EmployeeForUpdateDto>(employeeEntity);
            
            // For validate functionality we need to provide a second parameter as ModelState.
            patchDoc.ApplyTo(employeeUpdateDto, ModelState); // The type of json patch document must be the same as type of object who applied

            TryValidateModel(employeeUpdateDto); // Implemented for checking if the any property did not be removed. Because check in below validation applies only for patchDoc (request operations by patch)
            
            // patch validation located in this place, because manipulation with model is provided above
            if(!ModelState.IsValid)
            {
                /*What we get*/
                /*
                    {
                        "EmployeeForUpdateDto": [
                            "The target location specified by path segment 'agee' was not found."
                        ]
                    }
                */
                this._logger.LogError("Invalid model state for the patch document");
                return UnprocessableEntity(ModelState);
            }
            
            this._mapper.Map(employeeUpdateDto, employeeEntity); // from update dto to entity
            await this._repositoryManager.SaveChangesAsync();

            return NoContent();
        }

        
        [HttpDelete("{Id:Guid}")]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> DeleteEmployeeForCompany(Guid companyId, Guid Id)
        {
            // Used before created ValidateEmployeeForCompanyExistsAttribute
            
            // var company = await this._repositoryManager.Company.GetCompanyAsync(companyId, trackChanges: false);
            // if (company == null)
            // {
            //     _logger.LogInfo($"Company with {companyId} Id does not exist.");
            //     return NotFound();
            // }
            // var employee = this._repositoryManager.Employee.GetEmployee(companyId, Id, trackChanges: false);
            // if (employee == null)
            // {
            //     _logger.LogInfo($"Employee with {Id} Id does not exist.");
            //     return NotFound();
            // }

            var employee = HttpContext.Items["employee"] as Employee;
            
            this._repositoryManager.Employee.DeleteEmployee(employee);
            await this._repositoryManager.SaveChangesAsync();

            return NoContent();
        }
    }
}
