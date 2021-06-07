using Contracts;
using Microsoft.AspNetCore.Mvc;

namespace UltimateWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ILoggerManager logger;
        public TestController(ILoggerManager _logger)
        {
            this.logger = _logger;
        }
        
        [HttpGet]
        public ActionResult<string> Index()
        {
            this.logger.LogInfo("LogInfo message");
            this.logger.LogWarn("LogWarn message");
            this.logger.LogError("LogError message");
            this.logger.LogDebug("LogDebug message");
            return "Method result";
        }
    }
}