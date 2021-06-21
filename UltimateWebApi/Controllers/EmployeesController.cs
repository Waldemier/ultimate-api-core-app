using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts;
using Entities;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UltimateWebApi.Controllers
{
    [ApiController]
    [Route("api/companies/{companyId}/employees")]
    public class EmployeesController : ControllerBase
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;

        public EmployeesController(IRepositoryManager repositoryManager, ILoggerManager logger, IMapper mapper)
        {
            this._repositoryManager = repositoryManager;
            this._logger = logger;
            this._mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetEmployees(Guid companyId)
        {
            var company = this._repositoryManager.Company.GetCompany(companyId, trackChanges: false);
            if (company == null)
            {
                this._logger.LogInfo($"Company with {companyId} Id does not exist.");
                return NotFound();
            }
            var employees = this._repositoryManager.Employee.GetEmployees(companyId, trackChanges: false);
            var employeesDto = this._mapper.Map<IEnumerable<EmployeeDto>>(employees);

            return Ok(employeesDto);
        }

        [HttpGet("{Id:Guid}", Name = "GetEmployeeForCompany")]
        public IActionResult GetEmployee(Guid companyId, Guid Id)
        {
            var company = this._repositoryManager.Company.GetCompany(companyId, trackChanges: false);
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
        public IActionResult CreateEmployee(Guid companyId, [FromBody] EmployeeForCreateDto employeeForCreateDto)
        {
            if(employeeForCreateDto == null)
            {
                this._logger.LogError("EmployeeForCreateDto object sent from client is null.");
                return BadRequest("EmployeeForCreateDto object is null");
            }

            if (!ModelState.IsValid)
            {
                this._logger.LogError("Invalid model state for the EmployeeForCreateDto object");
                return UnprocessableEntity(ModelState);
            }

            var company = this._repositoryManager.Company.GetCompany(companyId, trackChanges: false);
            if(company == null)
            {
                this._logger.LogInfo($"Company with {companyId} Id does not exists.");
                return NotFound();
            }

            var employee = this._mapper.Map<Employee>(employeeForCreateDto);
            this._repositoryManager.Employee.CreateEmployeeForCompany(companyId, employee);
            this._repositoryManager.SaveChanges();

            var employeeDtoToView = this._mapper.Map<EmployeeDto>(employee);

            return CreatedAtRoute("GetEmployeeForCompany", new { companyId, Id = employeeDtoToView.Id }, employeeDtoToView);
        }

        [HttpPut("{Id:Guid}")]
        public IActionResult UpdateEmployeeForCompany(Guid companyId, Guid Id,
            [FromBody] EmployeeForUpdateDto employeeUpdateDto)
        {
            if(employeeUpdateDto == null)
            {
                this._logger.LogError("EmployeeForUpdateDto object sent from client is null.");
                return BadRequest("EmployeeForUpdateDto object is null.");
            }
            
            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state for the EmployeeForUpdateDto object");
                return UnprocessableEntity(ModelState);
            }
            
            var company = this._repositoryManager.Company.GetCompany(companyId, trackChanges: false);

            if(company == null)
            {
                this._logger.LogInfo($"Company with {companyId} Id does not exist.");
                return NotFound();
            }

            var employeeEntity = this._repositoryManager.Employee.GetEmployee(companyId, Id, trackChanges: true);

            if(employeeEntity == null)
            {
                this._logger.LogInfo($"Employee with {Id} Id does not exist.");
                return NotFound();
            }

            this._mapper.Map(employeeUpdateDto, employeeEntity);
            this._repositoryManager.SaveChanges();

            return NoContent();
        }

        [HttpPatch("{Id:Guid}")]
        public IActionResult PartiallyUpdateEmployeeForCompany(Guid companyId, Guid Id,
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

            var companyEntity = this._repositoryManager.Company.GetCompany(companyId, trackChanges: false);

            if(companyEntity == null)
            {
                this._logger.LogInfo($"Company with {companyId} Id does not exist.");
                return NotFound();
            }

            var employeeEntity = this._repositoryManager.Employee.GetEmployee(companyId, Id, trackChanges: true);

            if(employeeEntity == null)
            {
                this._logger.LogInfo($"Employee with {Id} Id does not exist.");
                return NotFound();
            }

            var employeeUpdateDto = this._mapper.Map<EmployeeForUpdateDto>(employeeEntity);
            // For validating we need to provide a second parameter as ModelState.
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
            this._repositoryManager.SaveChanges();

            return NoContent();
        }

        [HttpDelete("{Id:Guid}")]
        public IActionResult DeleteEmployeeForCompany(Guid companyId, Guid Id)
        {
            var company = this._repositoryManager.Company.GetCompany(companyId, trackChanges: false);
            if (company == null)
            {
                _logger.LogInfo($"Company with {companyId} Id does not exist.");
                return NotFound();
            }
            var employee = this._repositoryManager.Employee.GetEmployee(companyId, Id, trackChanges: false);
            if (employee == null)
            {
                _logger.LogInfo($"Employee with {Id} Id does not exist.");
                return NotFound();
            }
            this._repositoryManager.Employee.DeleteEmployee(employee);
            this._repositoryManager.SaveChanges();

            return NoContent();
        }
    }
}
