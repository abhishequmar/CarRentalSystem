namespace CarRentalSystem.Models
{

    public class BookingReceipt
    {
        public DateTime BookingDate { get; set; }
        public decimal Price { get; set; }
        public string CustomerName { get; set; }
        public Car VehicleDetails { get; set; }
        public int RentalDays { get; set; }
    }
}
