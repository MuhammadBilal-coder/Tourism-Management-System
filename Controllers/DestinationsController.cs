using Microsoft.AspNetCore.Mvc;
using TourismManagementSystem.Data;
using System.Linq;

namespace TourismManagementSystem.Controllers
{
    public class DestinationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DestinationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var destinations = _context.Destinations.ToList();
            return View(destinations);
        }
    }
}