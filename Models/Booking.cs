using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourismManagementSystem.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }

        // ✅ NULLABLE - Custom bookings can have null PackageId
        [ForeignKey("Package")]
        public int? PackageId { get; set; }
        public Package Package { get; set; }

        [ForeignKey("Hotel")]
        public int HotelId { get; set; }
        public Hotel Hotel { get; set; }

        [ForeignKey("Transport")]
        public int TransportId { get; set; }
        public Transport Transport { get; set; }

        public DateTime BookingDate { get; set; }

        [MaxLength(50)]
        public string Status { get; set; }
    }
}