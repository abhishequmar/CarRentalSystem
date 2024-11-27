using System.Threading.Tasks;
using CarRentalSystem.Models;

namespace CarRentalSystem.Services
{
    public interface ICarRentalService
    {
        Task<BookingReceipt> RentCarAsync(int carId, int userId, int days, string userEmail);
        Task<bool> CheckCarAvailabilityAsync(int carId);
    }
}
