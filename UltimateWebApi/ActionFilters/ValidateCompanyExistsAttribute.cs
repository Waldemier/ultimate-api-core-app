using System;
using System.Threading.Tasks;
using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace UltimateWebApi.ActionFilters
{
    public class ValidateCompanyExistsAttribute: IAsyncActionFilter
    {
        private readonly ILoggerManager _logger;
        private readonly IRepositoryManager _repository;

        public ValidateCompanyExistsAttribute(ILoggerManager logger, IRepositoryManager repository)
        {
            this._logger = logger;
            this._repository = repository;
        }
        
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var trackChanges = context.HttpContext.Request.Method.Equals("PUT"); // if it only PUT method that returns true.
            var Id = (Guid) context.ActionArguments["Id"]; // gets from action attributes, not from query string
            var company = await this._repository.Company.GetCompanyAsync(Id, trackChanges);

            if (company == null)
            { 
                this._logger.LogInfo($"Company with Id: {Id} doesn't exist.");
                context.Result = new NotFoundResult();
            }
            else
            {
                context.HttpContext.Items.Add("company", company);
                await next();
            }
        }
    }
}