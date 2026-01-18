using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourismManagementSystem.Models
{
    public class Hotel
    {
        [Key]
        public int HotelId { get; set; }

        [Required]
        [MaxLength(100)]
        public string HotelName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string RoomType { get; set; } = string.Empty;

        public double PricePerNight { get; set; }

        // ✅ ADD Availability property
        [MaxLength(50)]
        public string Availability { get; set; } = "Available";

        // ✅ Navigation to Destination
        [ForeignKey("Destination")]
        public int? DestinationId { get; set; }
        public Destination? Destination { get; set; }
    }
}