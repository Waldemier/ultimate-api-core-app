using System.Threading.Tasks;
using Contracts;
using Microsoft.AspNetCore.Mvc;

namespace UltimateWebApi.Controllers
{
    /// <summary>
    /// Can call by query string: https://localhost:5001/api/companies?api-version=2.0
    /// Can call by route url: https://localhost:5001/api/2.0/companies
    /// Can call by HTTP Header (but before that we need configure that in startup)
    /// </summary>
    
    //[ApiVersion("2.0", Deprecated = true)] // Deprecated = true повідомляє нам про те, що ця версія застаріла (але ми все рівно можемо нею користуватися)
    [Route("api/companies")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v2")]
    public class CompaniesV2Controller : ControllerBase
    {
        private readonly IRepositoryManager _repository;
        public CompaniesV2Controller(IRepositoryManager repository)
        {
            this._repository = repository;
        }
        [HttpGet]
        public async Task<IActionResult> GetCompanies()
        {
            var companies = await _repository.Company.GetAllCompaniesAsync(trackChanges: false);
            return Ok(companies);
        }
    }
}