using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourismManagementSystem.Data;
using TourismManagementSystem.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace TourismManagementSystem.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public BookingsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // ✅ MY BOOKINGS PAGE
        [HttpGet]
        public async Task<IActionResult> MyBookings()
        {
            // 1. Check if user logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            var userName = HttpContext.Session.GetString("UserName");
            var userEmail = HttpContext.Session.GetString("UserEmail");

            if (userId == null)
            {
                TempData["Error"] = "Please login first! ";
                return RedirectToAction("Login", "Account");
            }

            // 2. Get all bookings for this user
            var bookings = await _db.Bookings
                .Include(b => b.Package)
                    .ThenInclude(p => p.Destination)
                .Include(b => b.Hotel)
                .Include(b => b.Transport)
                .Where(b => b.UserId == userId.Value)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            // 3. Get payment details for each booking
            var bookingVMs = new System.Collections.Generic.List<MyBookingsVM>();

            foreach (var booking in bookings)
            {
                var payment = await _db.Payments
                    .FirstOrDefaultAsync(p => p.BookingId == booking.BookingId);

                // ✅ FIX: Get destination name for custom bookings
                string destinationName = "Custom Destination";

                if (booking.Package != null && booking.Package.Destination != null)
                {
                    // Predefined package
                    destinationName = booking.Package.Destination.Name;
                }
                else
                {
                    // ✅ Custom booking: Get destination from hotel
                    var hotel = await _db.Hotels
                        .Include(h => h.Destination)
                        .FirstOrDefaultAsync(h => h.HotelId == booking.HotelId);

                    if (hotel?.Destination != null)
                    {
                        destinationName = hotel.Destination.Name;
                    }
                }

                var vm = new MyBookingsVM
                {
                    BookingId = booking.BookingId,
                    UserId = booking.UserId,
                    UserName = userName,
                    UserEmail = userEmail,

                    // Package details
                    PackageId = booking.PackageId,
                    PackageType = booking.Package?.Type ?? "Custom Package",
                    DestinationName = destinationName,  // ✅ Fixed

                    // Hotel details
                    HotelId = booking.HotelId,
                    HotelName = booking.Hotel.HotelName,
                    RoomType = booking.Hotel.RoomType,

                    // Transport details
                    TransportId = booking.TransportId,
                    TransportType = booking.Transport.TransportType,
                    TransportCapacity = booking.Transport.Capacity,

                    // Booking details
                    BookingDate = booking.BookingDate,
                    Status = booking.Status,

                    // Payment details
                    PaymentId = payment?.PaymentId ?? 0,
                    Amount = payment?.Amount ?? 0,
                    PaymentMethod = payment?.PaymentMethod ?? "N/A",
                    PaymentStatus = payment?.PaymentStatus ?? "Pending",
                    TransactionId = payment?.TransactionId ?? "N/A",
                    PaymentDate = payment?.PaymentDate ?? booking.BookingDate
                };

                bookingVMs.Add(vm);
            }

            return View(bookingVMs);
        }

        // ✅ CANCEL BOOKING
        [HttpPost]
        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            // 1. Check user
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                TempData["Error"] = "Please login first! ";
                return RedirectToAction("Login", "Account");
            }

            // 2. Find booking
            var booking = await _db.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.UserId == userId.Value);

            if (booking == null)
            {
                TempData["Error"] = "Booking not found!";
                return RedirectToAction("MyBookings");
            }

            // 3. Check if already cancelled
            if (booking.Status == "Cancelled")
            {
                TempData["Error"] = "This booking is already cancelled!";
                return RedirectToAction("MyBookings");
            }

            // 4. Update status to Cancelled (soft delete)
            booking.Status = "Cancelled";
            _db.Bookings.Update(booking);
            await _db.SaveChangesAsync();

            // 5. Update payment status
            var payment = await _db.Payments.FirstOrDefaultAsync(p => p.BookingId == bookingId);
            if (payment != null)
            {
                payment.PaymentStatus = "Refunded";
                _db.Payments.Update(payment);
                await _db.SaveChangesAsync();
            }

            TempData["Success"] = $"Booking #{bookingId} has been cancelled successfully!";
            return RedirectToAction("MyBookings");
        }
    }
}