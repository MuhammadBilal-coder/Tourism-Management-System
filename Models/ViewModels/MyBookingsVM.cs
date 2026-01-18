using System;

namespace TourismManagementSystem.Models.ViewModels
{
    public class MyBookingsVM
    {
        public int BookingId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;  // ✅ Default value
        public string UserEmail { get; set; } = string.Empty;  // ✅ Default value

        // Package Details (null for custom)
        public int? PackageId { get; set; }
        public string PackageType { get; set; } = "Custom Package";  // ✅ Default value
        public string DestinationName { get; set; } = "Custom Destination";  // ✅ Default value

        // Hotel Details
        public int HotelId { get; set; }
        public string HotelName { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;

        // Transport Details
        public int TransportId { get; set; }
        public string TransportType { get; set; } = string.Empty;
        public int TransportCapacity { get; set; }

        // Booking Details
        public DateTime BookingDate { get; set; }
        public string Status { get; set; } = "Confirmed";

        // Payment Details
        public int PaymentId { get; set; }
        public double Amount { get; set; }  // ✅ Changed to double
        public string PaymentMethod { get; set; } = "N/A";
        public string PaymentStatus { get; set; } = "Pending";
        public string TransactionId { get; set; } = "N/A";
        public DateTime PaymentDate { get; set; }

        // Helper
        public bool IsCustomPackage => PackageId == null;
        public bool CanCancel => Status == "Confirmed";
    }
}