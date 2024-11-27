using System.Collections.Generic;
using System.Threading.Tasks;
using CarRentalSystem.Models;

namespace CarRentalSystem.Repositories
{
    public interface ICarRepository
    {
        Task AddCarAsync(Car car);
        Task<Car> GetCarByIdAsync(int id);
        Task<IEnumerable<Car>> GetAvailableCarsAsync();
        Task UpdateCarAvailabilityAsync(int id, bool isAvailable);
    }
}
