using Backend_Exam.DTOs;
using Backend_Exam.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Backend_Exam.Controllers
{
    [Route("/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DBContext _context;
        private readonly IConfiguration _config;

        public AuthController(DBContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDTO loginDto)
        {

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null)
                return Unauthorized(new { status = false, message = "Invalid email or password" });

            // Verify password (plain-text comparison — passwords stored as plain text in DB)
            if (user.Password != loginDto.Password)
                return Unauthorized(new { status = false, message = "Invalid email or password" });

           
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.Name)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(25),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                status = true,
                message = "Login successful",
                data = new
                {
                    token = tokenString,
                    user = new { user.Id, user.Name, user.Email, Role = user.Role.Name }
                }
            });
        }
    }
}
