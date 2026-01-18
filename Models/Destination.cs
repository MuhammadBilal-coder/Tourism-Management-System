using System.ComponentModel.DataAnnotations;

namespace TourismManagementSystem.Models
{
    public class Destination
    {
        [Key]
        public int DestinationId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(200)]
        public string Location { get; set; }

        public string Description { get; set; }

        [MaxLength(500)]
        public string ImagePath { get; set; }
    }
}