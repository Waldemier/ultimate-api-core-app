using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts;
using Entities;
using Entities.DataTransferObjects;
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

        [HttpGet("{Id:Guid}")]
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
    }
}
