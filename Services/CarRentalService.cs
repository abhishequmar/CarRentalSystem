using System.Threading.Tasks;
using Azure.Core;
using CarRentalSystem.Models;
using CarRentalSystem.Repositories;

namespace CarRentalSystem.Services
{
    public class CarRentalService : ICarRentalService
    {
        private readonly ICarRepository _carRepository;
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;

        public CarRentalService(ICarRepository carRepository, IUserRepository userRepository, IEmailService emailService)
        {
            _carRepository = carRepository;
            _userRepository = userRepository;
            _emailService = emailService;
        }

        public async Task<BookingReceipt> RentCarAsync(int carId, int userId, int days, string userEmail)
        {
            var car = await _carRepository.GetCarByIdAsync(carId);
            if (car == null || !car.IsAvailable)
                return null;

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return null;

            // Mark the car as unavailable
            car.IsAvailable = false;
            await _carRepository.UpdateCarAvailabilityAsync(carId, false);

            // Calculate rental cost
            var rentalCost = car.PricePerDay * days;

            // Create a receipt object
            var receipt = new BookingReceipt
            {
                BookingDate = DateTime.UtcNow,
                Price = rentalCost,
                CustomerName = user.Name,
                VehicleDetails = new Car
                {
                    Id = car.Id,
                    Model = car.Model,
                    Make = car.Make,
                    Year = car.Year,
                    PricePerDay = car.PricePerDay
                },
                RentalDays = days,

                
            };

            // Send confirmation email
            //string bookingDetails = $@"Dear {receipt.CustomerName},\n\nThank you for choosing our car rental service. We are excited to confirm your booking. Below are the details of your reservation:\n\n--- Booking Details ---\nBooking Date: {receipt.BookingDate:yyyy-MM-dd HH:mm:ss} UTC\nCustomer Name: {receipt.CustomerName}\nVehicle: {receipt.VehicleDetails.Make} {receipt.VehicleDetails.Model} ({receipt.VehicleDetails.Id})\nRental Period: {receipt.RentalDays} day(s)\nTotal Price: {receipt.Price:C}\n\n--- Vehicle Details ---\nCar ID: {receipt.VehicleDetails.Id}\nBrand: {receipt.VehicleDetails.Make}\nModel: {receipt.VehicleDetails.Model}\nLicense Plate: {receipt.VehicleDetails.Id}\n\nIf you have any questions or need assistance, please don't hesitate to contact our support team at support@carrentals.com or call us at +1 (800) 555-1234.\n\nWe look forward to serving you. Have a great day!\n\nBest regards,\nThe Car Rental Team\nCarRentalSystem Inc.\nwww.carrentals.com\nSupport: support@carrentals.com\nPhone: +1 (800) 555-1234\n";
            string bookingDetails = @"<html><body><p>Dear {receipt.CustomerName},</p><p>Thank you for choosing our car rental service. We are excited to confirm your booking. Below are the details of your reservation:</p><h3>--- Booking Details ---</h3><p><strong>Booking Date:</strong> {receipt.BookingDate:yyyy-MM-dd HH:mm:ss} UTC<br><strong>Customer Name:</strong> {receipt.CustomerName}<br><strong>Vehicle:</strong> {receipt.VehicleDetails.Make} {receipt.VehicleDetails.Model} ({receipt.VehicleDetails.Id})<br><strong>Rental Period:</strong> {receipt.RentalDays} day(s)<br><strong>Total Price:</strong> {receipt.Price:C}<br></p><h3>--- Vehicle Details ---</h3><p><strong>Car ID:</strong> {receipt.VehicleDetails.Id}<br><strong>Brand:</strong> {receipt.VehicleDetails.Make}<br><strong>Model:</strong> {receipt.VehicleDetails.Model}<br><strong>License Plate:</strong> {receipt.VehicleDetails.Id}<br></p><p>If you have any questions or need assistance, please don't hesitate to contact our support team at <a href='mailto:support@carrentals.com'>support@carrentals.com</a> or call us at +1 (800) 555-1234.</p><p>We look forward to serving you. Have a great day!</p><p>Best regards,<br>The Car Rental Team<br>CarRentalSystem Inc.<br><a href='https://www.carrentals.com'>www.carrentals.com</a><br>Support: <a href='mailto:support@carrentals.com'>support@carrentals.com</a><br>Phone: +1 (800) 555-1234</p></body></html>";

            await _emailService.SendEmailAsync(userEmail, "Booking Confirmation", bookingDetails);

            return receipt;
        }

        

        


        public async Task<bool> CheckCarAvailabilityAsync(int carId)
        {
            var car = await _carRepository.GetCarByIdAsync(carId);
            return car != null && car.IsAvailable;
        }
    }
}
