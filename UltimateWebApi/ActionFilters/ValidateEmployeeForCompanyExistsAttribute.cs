using System;
using System.Threading.Tasks;
using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace UltimateWebApi.ActionFilters
{
    public class ValidateEmployeeForCompanyExistsAttribute: IAsyncActionFilter
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;

        public ValidateEmployeeForCompanyExistsAttribute(IRepositoryManager repository, ILoggerManager logger)
        {
            this._repository = repository;
            this._logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var method = context.HttpContext.Request.Method;
            var trackChanges = (method.Equals("PUT") || method.Equals("PATCH")) ? true : false;

            var companyId = (Guid)context.ActionArguments["companyId"];
            var company = this._repository.Company.GetCompanyAsync(companyId, trackChanges: false);
            
            if (company == null)
            {
                this._logger.LogInfo($"Company with Id: {companyId} doesn't exist.");
                context.Result = new NotFoundResult();
                return;
            }

            var employeeId = (Guid)context.ActionArguments["Id"];
            var employee = this._repository.Employee.GetEmployee(companyId, employeeId, trackChanges);

            if (employee == null)
            {
                this._logger.LogInfo($"Employee with id: {employeeId} doesn't exist in the database.");
                context.Result = new NotFoundResult();
            }
            else
            {
                context.HttpContext.Items.Add("employee", employee);
                await next();
            }
        }
    }
}