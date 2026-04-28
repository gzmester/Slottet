using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain.Entities;
using Application.DTOs.Authorization;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<Employee> _userManager;
        private readonly IConfiguration _config;

        public AuthController(UserManager<Employee> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            // 1. Search for the employee using the passcode as their username
            var user = await _userManager.FindByNameAsync(model.Passcode);

            // 2. In this specific "Passcode-only" setup, if the user exists, 
            // they are authenticated (since the passcode IS the credential).
            if (user != null)
            {
                var token = GenerateJwtToken(user);
                return Ok(new { Token = token });
            }

            return Unauthorized("Invalid Passcode.");
        }

        private string GenerateJwtToken(Employee user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // The token still carries the Employee's name so the UI can greet them
            var claims = new[] {
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddHours(12), // Longer session for internal app
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
