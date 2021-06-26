using System.Threading.Tasks;
using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UltimateWebApi.ActionFilters;

namespace UltimateWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly ILoggerManager _loggerManager;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly IAuthenticationManager _authenticationManager;
        
        public AuthenticationController(ILoggerManager loggerManager, IMapper mapper, UserManager<User> userManager, IAuthenticationManager authenticationManager)
        {
            this._loggerManager = loggerManager;
            this._mapper = mapper;
            this._userManager = userManager;
            this._authenticationManager = authenticationManager;
        }

        [HttpPost]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> RegisterUser([FromBody] UserForRegistrationDto userRegisterDto)
        {
            /*
             * To Input:
             * {
                    "firstname": "Wolodymyr",
                    "lastname": "Didukh",
                    "username": "WDidukh",
                    "password": "1234567@Vv",
                    "email": "www@gmail.com",
                    "phonenumber": "068-300-60-28",
                    "roles": [
                        "Manager"
                    ]
                }
             */
            var user = this._mapper.Map<User>(userRegisterDto);
            var result = await this._userManager.CreateAsync(user, userRegisterDto.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                }
                return BadRequest(ModelState);
            }
            
            await this._userManager.AddToRolesAsync(user, userRegisterDto.Roles);
            
            return StatusCode(201);
        }

        [HttpPost("login")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> Authenticate([FromBody] UserForAuthenticationDto userAuthDto)
        {
            /*
                 {
                    "username": "KDidukh",
                    "password": "1234567@Vv"
                }
            */
            if (!await this._authenticationManager.ValidateUser(userAuthDto))
            {
                this._loggerManager.LogWarn($"{nameof(Authenticate)}: Authentication failed. Wrong username or password.");
                return Unauthorized();
            }

            var jwtToken = await this._authenticationManager.CreateTokenAsync();
            
            return Ok(new { Token = jwtToken });
        }
    }
}