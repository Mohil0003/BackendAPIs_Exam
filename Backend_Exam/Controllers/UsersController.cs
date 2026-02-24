using Backend_Exam.DTOs;
using Backend_Exam.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend_Exam.Controllers
{
    [Route("/users")]
    [ApiController]
    [Authorize(Roles = "MANAGER")]
    public class UsersController : ControllerBase
    {
        private readonly DBContext _context;
        private readonly IValidator<UserDTO> _validator;

        public UsersController(DBContext context, IValidator<UserDTO> validator)
        {
            _context = context;
            _validator = validator;
        }

      
        [HttpPost]
        public async Task<ActionResult> CreateUser([FromBody] UserDTO userDto)
        {
            var validation = _validator.Validate(userDto);
            if (!validation.IsValid)
                return BadRequest(new { status = false, message = "Validation failed", errors = validation.Errors.Select(e => e.ErrorMessage) });

            var exists = await _context.Users.AnyAsync(u => u.Email == userDto.Email);
            if (exists)
                return Conflict(new { status = false, message = "Email already exists" });

        
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == userDto.role_id);
            if (role == null)
                return BadRequest(new { status = false, message = "Invalid role_id. Role does not exist." });

            var user = new User
            {
                Name = userDto.Name,
                Email = userDto.Email,
                Password = userDto.Password,
                RoleId = userDto.role_id,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                status = true,
                message = "User created successfully",
                data = new { user.Id, user.Name, user.Email, user.RoleId, RoleName = role.Name, user.CreatedAt }
            });
        }


        [HttpGet]
        public async Task<ActionResult> GetUsers()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.RoleId,
                    RoleName = u.Role.Name,
                    u.CreatedAt
                })
                .ToListAsync();

            return Ok(new { status = true, message = "Users fetched successfully", data = users });
        }
    }
}
