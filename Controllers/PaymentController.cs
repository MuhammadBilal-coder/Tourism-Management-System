using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourismManagementSystem.Data;
using TourismManagementSystem.Models;
using TourismManagementSystem.Models.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TourismManagementSystem.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _db;

        public PaymentController(ApplicationDbContext db)
        {
            _db = db;
        }

        // ✅ SHOW PAYMENT PAGE
        [HttpGet]
        public async Task<IActionResult> ProcessPayment(int packageId)
        {
            // 1. Get user session
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userName = HttpContext.Session.GetString("UserName");
            var userId = HttpContext.Session.GetInt32("UserId");

            if (string.IsNullOrEmpty(userEmail) || userId == null)
            {
                TempData["Error"] = "Please login first! ";
                return RedirectToAction("Login", "Account");
            }

            PaymentVM vm;

            // 2. CHECK IF CUSTOM PACKAGE
            if (packageId == 0)
            {
                // CUSTOM PACKAGE
                var customPackageType = TempData["CustomPackageType"] as string;
                var customDestinationName = TempData["CustomDestinationName"] as string;
                var customTotalAmountStr = TempData["CustomTotalAmount"] as string;

                TempData.Keep("CustomPackageType");
                TempData.Keep("CustomDestinationName");
                TempData.Keep("CustomTotalAmount");
                TempData.Keep("CustomHotelId");
                TempData.Keep("CustomTransportId");
                TempData.Keep("CustomDestinationId");

                if (string.IsNullOrEmpty(customTotalAmountStr) || string.IsNullOrEmpty(customDestinationName))
                {
                    TempData["Error"] = "Invalid custom package data!";
                    return RedirectToAction("Index", "Destinations");
                }

                double customTotalAmount = double.Parse(customTotalAmountStr);

                vm = new PaymentVM
                {
                    PackageId = 0,
                    PackageType = customPackageType ?? "Custom Package",
                    TotalAmount = customTotalAmount,
                    UserId = userId.Value,
                    UserName = userName,
                    UserEmail = userEmail,
                    DestinationName = customDestinationName
                };
            }
            else
            {
                // PREDEFINED PACKAGE
                var package = await _db.Packages
                    .Include(p => p.Destination)
                    .FirstOrDefaultAsync(p => p.PackageId == packageId);

                if (package == null)
                {
                    return NotFound();
                }

                vm = new PaymentVM
                {
                    PackageId = package.PackageId,
                    PackageType = package.Type,
                    TotalAmount = package.Price,
                    UserId = userId.Value,
                    UserName = userName,
                    UserEmail = userEmail,
                    DestinationName = package.Destination.Name
                };
            }

            return View(vm);
        }

        // ✅ PROCESS PAYMENT (UPDATED - EXCLUDE CANCELLED)
        [HttpPost]
        public async Task<IActionResult> ConfirmPayment(PaymentVM model)
        {
            // 1. Get user session
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                TempData["Error"] = "Session expired! Please login again.";
                return RedirectToAction("Login", "Account");
            }

            int hotelId;
            int transportId;

            // 2. CUSTOM OR PREDEFINED
            if (model.PackageId == 0)
            {
                // ✅ CUSTOM PACKAGE
                var customHotelIdStr = TempData["CustomHotelId"] as string;
                var customTransportIdStr = TempData["CustomTransportId"] as string;

                if (string.IsNullOrEmpty(customHotelIdStr) || string.IsNullOrEmpty(customTransportIdStr))
                {
                    TempData["Error"] = "Custom package data expired!";
                    return RedirectToAction("Index", "Destinations");
                }

                hotelId = int.Parse(customHotelIdStr);
                transportId = int.Parse(customTransportIdStr);

                // ✅ Double-check: Same custom combo already booked? (IGNORE CANCELLED)
                var existingCustom = await _db.Bookings
                    .FirstOrDefaultAsync(b =>
                        b.UserId == userId.Value &&
                        b.PackageId == null &&
                        b.HotelId == hotelId &&
                        b.TransportId == transportId &&
                        b.Status != "Cancelled"
                    );

                if (existingCustom != null)
                {
                    TempData["Error"] = $"You have already booked this custom package!  Booking ID: #{existingCustom.BookingId}";
                    return RedirectToAction("Index", "Destinations");
                }
            }
            else
            {
                // ✅ PREDEFINED PACKAGE (IGNORE CANCELLED)
                var existingBooking = await _db.Bookings
                    .FirstOrDefaultAsync(b =>
                        b.UserId == userId.Value &&
                        b.PackageId == model.PackageId &&
                        b.Status != "Cancelled"
                    );

                if (existingBooking != null)
                {
                    TempData["Error"] = $"You have already booked this package!  Booking ID: #{existingBooking.BookingId}";
                    return RedirectToAction("BookNow", "Packages", new { packageId = model.PackageId });
                }

                var hotel = await _db.Hotels.FirstOrDefaultAsync();
                var transport = await _db.Transports.FirstOrDefaultAsync();

                if (hotel == null || transport == null)
                {
                    TempData["Error"] = "Hotel or Transport not available!";
                    return RedirectToAction("ProcessPayment", new { packageId = model.PackageId });
                }

                hotelId = hotel.HotelId;
                transportId = transport.TransportId;
            }

            // 3. CREATE BOOKING
            var booking = new Booking
            {
                UserId = userId.Value,
                PackageId = model.PackageId == 0 ? null : model.PackageId,
                HotelId = hotelId,
                TransportId = transportId,
                BookingDate = DateTime.Now,
                Status = "Confirmed"
            };

            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync();

            // 4. CREATE PAYMENT
            var payment = new Payment
            {
                BookingId = booking.BookingId,
                Amount = model.TotalAmount,
                PaymentMethod = model.PaymentMethod,
                PaymentStatus = "Completed",
                TransactionId = GenerateTransactionId(),
                PaymentDate = DateTime.Now
            };

            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            // 5. SUCCESS
            TempData["Success"] = $"Payment successful! Booking ID:  {booking.BookingId}";
            return RedirectToAction("PaymentSuccess", new { bookingId = booking.BookingId });
        }

        // ✅ SUCCESS PAGE (FIXED QUERY STRING + NULL CHECKS)
        [HttpGet]
        public async Task<IActionResult> PaymentSuccess(int? bookingId) // ✅ Make nullable
        {
            if (bookingId == null || bookingId == 0)
            {
                TempData["Error"] = "Invalid booking ID!";
                return RedirectToAction("MyBookings", "Bookings");
            }

            var booking = await _db.Bookings
                .Include(b => b.User)
                .Include(b => b.Hotel)
                .Include(b => b.Transport)
                .Include(b => b.Package)
                    .ThenInclude(p => p.Destination)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId.Value);

            if (booking == null)
            {
                TempData["Error"] = "Booking not found!";
                return RedirectToAction("MyBookings", "Bookings");
            }

            var payment = await _db.Payments
                .FirstOrDefaultAsync(p => p.BookingId == bookingId.Value);

            if (payment == null)
            {
                TempData["Error"] = "Payment details not found!";
                return RedirectToAction("MyBookings", "Bookings");
            }

            ViewBag.Booking = booking;
            ViewBag.Payment = payment;

            return View();
        }

        // ✅ TRANSACTION ID
        private string GenerateTransactionId()
        {
            return $"TXN{DateTime.Now:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";
        }
    }
}
