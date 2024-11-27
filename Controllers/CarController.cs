using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using CarRentalSystem.Models;
using CarRentalSystem.Services;
using CarRentalSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using CarRentalSystem.Services;

namespace CarRentalSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarController : ControllerBase
    {
        private readonly ICarRentalService _carRentalService;
        private readonly ICarRepository _carRepository;

        public CarController(ICarRentalService carRentalService, ICarRepository carRepository)
        {
            _carRentalService = carRentalService;
            _carRepository = carRepository;
        }

        // GET: api/cars
        [HttpGet]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetAvailableCars()
        {
            var availableCars = await _carRepository.GetAvailableCarsAsync();
            return Ok(availableCars);
        }

        // POST: api/cars
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddCar([FromBody] Car car)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _carRepository.AddCarAsync(car);
            return CreatedAtAction(nameof(GetAvailableCars), new { id = car.Id }, car);
        }

        // PUT: api/cars/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCar(int id, [FromBody] Car car)
        {
            if (id != car.Id)
                return BadRequest("Car ID mismatch.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingCar = await _carRepository.GetCarByIdAsync(id);
            if (existingCar == null)
                return NotFound();

            // Update the car details
            existingCar.Make = car.Make;
            existingCar.Model = car.Model;
            existingCar.Year = car.Year;
            existingCar.PricePerDay = car.PricePerDay;
            existingCar.IsAvailable = car.IsAvailable;

            await _carRepository.UpdateCarAvailabilityAsync(id, car.IsAvailable);
            return NoContent();  
        }

        // DELETE: api/cars/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCar(int id)
        {
            var car = await _carRepository.GetCarByIdAsync(id);
            if (car == null)
                return NotFound();

            await _carRepository.UpdateCarAvailabilityAsync(id, false);
            return NoContent();  
        }


        [HttpPost("rent")]
        public async Task<IActionResult> RentCar([FromBody] RentCarRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var car = await _carRepository.GetCarByIdAsync(request.CarId);
            if (car == null)
                return NotFound("Car not found.");
            if (!car.IsAvailable)
                return BadRequest("Car is not available.");

            var receipt = await _carRentalService.RentCarAsync(request.CarId, request.UserId, request.Days, request.UserEmail);
            if (receipt == null)
                return BadRequest("Unable to rent car. Please try again.");

            

            // Return the receipt
            return Ok(receipt);
        }

    }

    public class RentCarRequest
    {
        public int UserId { get; set; }
        public string UserEmail { get; set; }
        public int CarId { get; set; }
        public int Days { get; set; }
        public string CarModel { get ; set; }   
    }
}

