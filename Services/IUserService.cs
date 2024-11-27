using System.Threading.Tasks;
using CarRentalSystem.Models;

namespace CarRentalSystem.Services
{
    public interface IUserService
    {
        Task<string> RegisterUserAsync(User user); // Asynchronous registration
        Task<string> AuthenticateUserAsync(string email, string password); // Asynchronous authentication
        //bool VerifyPassword(string enteredPassword, string storedPassword);
    }
}
