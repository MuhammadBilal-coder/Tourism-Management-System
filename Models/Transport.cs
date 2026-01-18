using System.ComponentModel.DataAnnotations;

namespace TourismManagementSystem.Models
{
    public class Transport
    {
        [Key]
        public int TransportId { get; set; }

        [Required]
        [MaxLength(100)]
        public string TransportType { get; set; }

        public int Capacity { get; set; }

        public double RatePerDay { get; set; }

        public int? PackageId { get; set; }
    }
}