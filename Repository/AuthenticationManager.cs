using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Repository
{
    public class AuthenticationManager: IAuthenticationManager
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;

        private User _user;
        
        public AuthenticationManager(IConfiguration configuration, UserManager<User> userManager)
        {
            this._configuration = configuration;
            this._userManager = userManager;
        }
        
        public async Task<bool> ValidateUser(UserForAuthenticationDto userAuthDto)
        {
            this._user = await this._userManager.FindByNameAsync(userAuthDto.UserName);
            return (this._user != null && await this._userManager.CheckPasswordAsync(this._user, userAuthDto.Password));
        }

        public async Task<string> CreateTokenAsync()
        {
            var signingCredentials = GetSigningCredentials();
            var claims = await GetClaims();
            var tokenOptions = GenerateTokenOptions(signingCredentials, claims);

            // serializing token options
            return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(tokenOptions);
        }

        /// <summary>
        /// Returns our secret key as a byte array with the security algorithm.
        /// </summary>
        /// <returns></returns>
        private SigningCredentials GetSigningCredentials()
        {
            var key = this._configuration.GetSection("JwtSettings").GetSection("Secret").Value;
            var secret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }

        /// <summary>
        /// Creates a list of claims with the user name inside and all the roles the user belongs to.
        /// </summary>
        /// <returns></returns>
        private async Task<List<Claim>> GetClaims()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, this._user.UserName)
            };

            var roles = await this._userManager.GetRolesAsync(this._user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            return claims;
        }

        /// <summary>
        /// Creates an object of the JwtSecurityToken type with all of the required options.
        /// </summary>
        /// <param name="signingCredentials"></param>
        /// <param name="claims"></param>
        /// <returns></returns>
        private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims)
        {
            var jwtSettings = this._configuration.GetSection("JwtSettings");

            var tokenOptions = new JwtSecurityToken(
                    issuer: jwtSettings.GetSection("ValidIssuer").Value,
                    audience: jwtSettings.GetSection("ValidAudience").Value,
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings.GetSection("Expires").Value)),
                    signingCredentials: signingCredentials
                );
            
            return tokenOptions;
        }
    }
}