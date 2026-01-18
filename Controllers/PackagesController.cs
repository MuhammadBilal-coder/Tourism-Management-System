using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourismManagementSystem.Data;
using TourismManagementSystem.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace TourismManagementSystem.Controllers
{
    public class PackagesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public PackagesController(ApplicationDbContext db)
        {
            _db = db;
        }

        // =========================
        // 1) SHOW PACKAGES (GET)
        // =========================
        [HttpGet]
        public async Task<IActionResult> BookPackage(int destinationId)
        {
            var destination = await _db.Destinations
                .FirstOrDefaultAsync(d => d.DestinationId == destinationId);

            if (destination == null)
                return NotFound();

            var packages = await _db.Packages
                .Where(p => p.DestinationId == destinationId)
                .AsNoTracking()
                .ToListAsync();

            var vm = new BookPackageVM
            {
                DestinationId = destinationId,
                DestinationName = destination.Name,
                Packages = packages,
                Hotels = await _db.Hotels.AsNoTracking().ToListAsync(),
                Transports = await _db.Transports.AsNoTracking().ToListAsync()
            };

            return View("BookPackage", vm);
        }

        // =========================
        // 2) BOOK PREDEFINED PACKAGE (GET)
        // =========================
        [HttpGet]
        public async Task<IActionResult> BookNow(int packageId)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userName = HttpContext.Session.GetString("UserName");
            var userId = HttpContext.Session.GetInt32("UserId");

            if (string.IsNullOrEmpty(userEmail) || userId == null)
            {
                TempData["Error"] = "Please login first to book a package!";
                return RedirectToAction("Login", "Account");
            }

            // ✅ CHECK IF USER ALREADY BOOKED THIS PACKAGE (IGNORE CANCELLED)
            var existingBooking = await _db.Bookings
                .FirstOrDefaultAsync(b =>
                    b.UserId == userId.Value &&
                    b.PackageId == packageId &&
                    b.Status != "Cancelled"   // ✅ Ignore cancelled bookings
                );

            if (existingBooking != null)
            {
                TempData["Error"] =
                    $"⚠️ You have already booked this package! Booking ID: #{existingBooking.BookingId}. Please cancel it if you want to book again.";

                var pkg = await _db.Packages.FirstOrDefaultAsync(p => p.PackageId == packageId);
                return RedirectToAction("BookPackage", new { destinationId = pkg?.DestinationId ?? 0 });
            }

            var package = await _db.Packages
                .Include(p => p.Destination)
                .FirstOrDefaultAsync(p => p.PackageId == packageId);

            if (package == null)
                return NotFound();

            var vm = new BookingConfirmationVM
            {
                PackageId = package.PackageId,
                PackageType = package.Type,
                PackagePrice = package.Price,
                Activities = package.Activities,
                HotelOptions = package.HotelOptions,
                TransportOptions = package.TransportOptions,
                DestinationName = package.Destination.Name,
                DestinationLocation = package.Destination.Location,
                UserId = userId.Value,
                UserName = userName,
                UserEmail = userEmail,
                AlreadyBooked = false,
                ExistingBookingId = null
            };

            return View("BookingConfirmation", vm);
        }

        // =========================
        // 3) BOOK CUSTOM PACKAGE (POST)
        // =========================
        [HttpPost]
        public async Task<IActionResult> BookCustom(int DestinationId, int SelectedHotelId, int SelectedTransportId)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userName = HttpContext.Session.GetString("UserName");
            var userId = HttpContext.Session.GetInt32("UserId");

            if (string.IsNullOrEmpty(userEmail) || userId == null)
            {
                TempData["Error"] = "Please login first to book a package!";
                return RedirectToAction("Login", "Account");
            }

            // ✅ CHECK IF USER ALREADY BOOKED SAME CUSTOM PACKAGE (IGNORE CANCELLED)
            var existingCustomBooking = await _db.Bookings
                .FirstOrDefaultAsync(b =>
                    b.UserId == userId.Value &&
                    b.PackageId == null &&
                    b.HotelId == SelectedHotelId &&
                    b.TransportId == SelectedTransportId &&
                    b.Status != "Cancelled"   // ✅ Ignore cancelled bookings
                );

            if (existingCustomBooking != null)
            {
                TempData["Error"] =
                    $"⚠️ You have already booked this exact custom package! Booking ID: #{existingCustomBooking.BookingId}. Please cancel it if you want to book again.";

                return RedirectToAction("BookPackage", new { destinationId = DestinationId });
            }

            var destination = await _db.Destinations
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DestinationId == DestinationId);

            var hotel = await _db.Hotels
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.HotelId == SelectedHotelId);

            var transport = await _db.Transports
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TransportId == SelectedTransportId);

            if (destination == null || hotel == null || transport == null)
                return NotFound();

            // Calculate custom price
            var totalPrice = hotel.PricePerNight + transport.RatePerDay;

            // Store in TempData
            TempData["CustomPackageType"] = "Custom Package";
            TempData["CustomDestinationName"] = destination.Name;
            TempData["CustomTotalAmount"] = totalPrice.ToString();
            TempData["CustomHotelId"] = SelectedHotelId.ToString();
            TempData["CustomTransportId"] = SelectedTransportId.ToString();
            TempData["CustomDestinationId"] = DestinationId.ToString();

            // Redirect to payment
            return RedirectToAction("ProcessPayment", "Payment", new { packageId = 0 });
        }
    }
}
