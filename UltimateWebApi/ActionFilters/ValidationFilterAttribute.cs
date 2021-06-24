using System.Linq;
using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace UltimateWebApi.ActionFilters
{
    /// <summary>
    /// This class are implementing the actions argument filtering.
    /// He checks if the action argument model is not null and all model property are valid.
    /// </summary>
    public class ValidationFilterAttribute: IActionFilter
    {
        private readonly ILoggerManager _logger;
        public ValidationFilterAttribute(ILoggerManager logger)
        {
            this._logger = logger;
        }
        
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var controller = context.RouteData.Values["controller"];
            var action = context.RouteData.Values["action"];

            var param = context.ActionArguments.SingleOrDefault(x => 
                x.Value.ToString().Contains("Dto")).Value; // check the name of attribute type

            if (param == null)
            {
                this._logger.LogError($"Object sent from client is null. Controller: {controller}, action: {action}");
                context.Result = new BadRequestObjectResult($"Object is null. Controller: {controller}, action: {action}");
                return;
            }

            if (!context.ModelState.IsValid)
            {
                this._logger.LogError($"Invalid model state for the object. Controller: {controller}, action: {action}");
                context.Result = new UnprocessableEntityObjectResult(context.ModelState);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            //throw new System.NotImplementedException();
        }
    }
}