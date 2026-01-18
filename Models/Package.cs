using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourismManagementSystem.Models
{
    public class Package
    {
        [Key]
        public int PackageId { get; set; }

        [ForeignKey("Destination")]
        public int DestinationId { get; set; }

        // ✅ NAVIGATION PROPERTY - Required for . Include()
        public Destination Destination { get; set; }

        [Required]
        [MaxLength(100)]
        public string Type { get; set; }

        public double Price { get; set; }

        public string Activities { get; set; }

        public string HotelOptions { get; set; }

        public string TransportOptions { get; set; }
    }
}