using System.Collections.Generic;
using TourismManagementSystem.Models;

namespace TourismManagementSystem.Models.ViewModels
{
    public class BookPackageVM
    {
        public int DestinationId { get; set; }

        public string DestinationName { get; set; }

        public int? SelectedHotelId { get; set; }
        public int? SelectedTransportId { get; set; }

        public List<Package> Packages { get; set; } = new();
        public List<Hotel> Hotels { get; set; } = new();
        public List<Transport> Transports { get; set; } = new();
    }
}