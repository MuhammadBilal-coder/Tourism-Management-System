namespace TourismManagementSystem.Models.ViewModels
{
    public class BookingConfirmationVM
    {
        // Package Info
        public int PackageId { get; set; }
        public string PackageType { get; set; }
        public double PackagePrice { get; set; }
        public string Activities { get; set; }
        public string HotelOptions { get; set; }
        public string TransportOptions { get; set; }

        // Destination Info
        public string DestinationName { get; set; }
        public string DestinationLocation { get; set; }

        // User Info
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }

        // Booking Check
        public bool AlreadyBooked { get; set; }
        public int? ExistingBookingId { get; set; }
    }
}