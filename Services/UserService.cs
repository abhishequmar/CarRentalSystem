using System;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CarRentalSystem.Models;
using CarRentalSystem.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CarRentalSystem.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public UserService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }



        public async Task<string> RegisterUserAsync(User user)
        {
            var existingUser = await _userRepository.GetUserByEmailAsync(user.Email);
            if (existingUser != null)
                return null;

            user.Password = HashPassword(user.Password); // Example password hashing
            await _userRepository.AddUserAsync(user);
            return await GenerateJwtTokenAsync(user);
        }


        public async Task<string> AuthenticateUserAsync(string email, string password)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null || !VerifyPassword(password, user.Password))
                return null;

            return await GenerateJwtTokenAsync(user);
        }


        private async Task<string> GenerateJwtTokenAsync(User user)

        {

            return await Task.Run(() =>
            {
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var cred = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var _claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Name.ToString()),
                    new Claim("role", user.Role),
                    new Claim(JwtRegisteredClaimNames.Jti, user.Id.ToString()),
                };

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: _claims,
                    expires: DateTime.Now.AddHours(1),
                    signingCredentials: cred
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            });
        }


        private string HashPassword(string password)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(password));
        }

        private bool VerifyPassword(string enteredPassword, string storedPassword)
        {
            return storedPassword == Convert.ToBase64String(Encoding.UTF8.GetBytes(enteredPassword));
        }
    }
}
