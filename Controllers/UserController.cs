using Microsoft.AspNetCore.Mvc;
using CarRentalSystem.Models;
using CarRentalSystem.Services;
using System.Threading.Tasks;
using CarRentalSystem.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace CarRentalSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public UserController(IUserService userService, IUserRepository userRepository)
        {
            _userService = userService;
            _userRepository = userRepository;
        }

        // POST: api/users/register
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] User user)
        {


            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = await _userRepository.GetUserByEmailAsync(user.Email);
            if (existingUser != null)
                return Conflict("Email is already in use.");
            string? isRegistered=null;

            try
            {
                isRegistered = await _userService.RegisterUserAsync(user);
            }
            catch (Exception ex) { 
                return BadRequest(ex);
            }

            return CreatedAtAction(nameof(RegisterUser), new { id = user.Id }, user);
        }


        //POST: api/users/login
       [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var token = await _userService.AuthenticateUserAsync(request.Email, request.Password);

            if (token == null)
                return Unauthorized(new { message = "Invalid email or password." });

            return Ok(new { Token = token });
        }

        // Define the LoginRequest class
        public class LoginRequest
        {
            [Required(ErrorMessage = "Email is required.")]
            [RegularExpression(
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                ErrorMessage = "Enter a valid email address.")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Password is required.")]
            public string Password { get; set; }
        }


    }
}
