using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using TourismManagementSystem.Data;
using TourismManagementSystem.Models;

namespace TourismManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.PasswordHash == password);
            if (user != null)
            {
                // ✅ SET SESSION - UserId as Int32, others as String
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("UserName", user.Name);
                HttpContext.Session.SetString("UserEmail", user.Email);

                // DEBUG - Check if session is working
                System.Diagnostics.Debug.WriteLine($"✅ Session Set - UserId:  {user.UserId}, Name: {user.Name}, Email: {user.Email}");

                return RedirectToAction("Index", "Destinations");
            }
            ViewBag.Error = "Invalid email or password";
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string name, string email, string password, string confirmPassword)
        {
            // ✅ VALIDATION 1: Check if all fields are filled
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                ViewBag.Error = "All fields are required!";
                return View();
            }

            // ✅ VALIDATION 2: Name minimum 3 characters
            if (name.Length < 3)
            {
                ViewBag.Error = "Name must be at least 3 characters! ";
                return View();
            }

            // ✅ VALIDATION 3: Email format validation
            if (!IsValidEmail(email))
            {
                ViewBag.Error = "Invalid email format!  Please enter a valid email address.";
                return View();
            }

            // ✅ VALIDATION 4: Password minimum 8 characters
            if (password.Length < 8)
            {
                ViewBag.Error = "Password must be at least 8 characters!";
                return View();
            }

            // ✅ VALIDATION 5: Password strength (at least 1 number and 1 letter)
            if (!Regex.IsMatch(password, @"^(?=.*[A-Za-z])(?=.*\d).+$"))
            {
                ViewBag.Error = "Password must contain at least one letter and one number!";
                return View();
            }

            // ✅ VALIDATION 6: Password match
            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match! ";
                return View();
            }

            // ✅ VALIDATION 7: Check if email already exists
            var existingUser = _context.Users.FirstOrDefault(u => u.Email == email);
            if (existingUser != null)
            {
                ViewBag.Error = "Email already registered!";
                return View();
            }

            // ✅ CREATE USER
            var user = new User
            {
                Name = name,
                Email = email,
                PasswordHash = password
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            // ✅ AUTO LOGIN - SET SESSION
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetString("UserEmail", user.Email);

            System.Diagnostics.Debug.WriteLine($"✅ New User Registered & Logged In - UserId: {user.UserId}, Name: {user.Name}");

            // ✅ REDIRECT TO SUCCESS PAGE
            return RedirectToAction("RegisterSuccess");
        }

        // ✅ NEW ACTION - REGISTER SUCCESS PAGE WITH AUTO REDIRECT
        [HttpGet]
        public IActionResult RegisterSuccess()
        {
            var userName = HttpContext.Session.GetString("UserName");

            // If no session, redirect to login
            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Login");
            }

            ViewBag.UserName = userName;
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ✅ HELPER METHOD: Email validation
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Check email format using regex
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        // ✅ FORGOT PASSWORD - GET (Show email input form)
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // ✅ FORGOT PASSWORD - POST (Generate reset token)
        [HttpPost]
        public IActionResult ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            {
                ViewBag.Error = "Please enter a valid email address!";
                return View();
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                ViewBag.Error = "No account found with this email!";
                return View();
            }

            // Generate reset token
            string resetToken = Guid.NewGuid().ToString();
            user.ResetToken = resetToken;
            user.ResetTokenExpiry = DateTime.Now.AddHours(1); // Token valid for 1 hour

            _context.Users.Update(user);
            _context.SaveChanges();

            // Store token in TempData to show reset link
            TempData["ResetToken"] = resetToken;
            TempData["UserEmail"] = email;

            return RedirectToAction("ForgotPasswordConfirmation");
        }

        // ✅ FORGOT PASSWORD CONFIRMATION (Show reset link)
        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        // ✅ RESET PASSWORD - GET (Show new password form)
        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("Login");
            }

            var user = _context.Users.FirstOrDefault(u => u.ResetToken == token && u.ResetTokenExpiry > DateTime.Now);
            if (user == null)
            {
                ViewBag.Error = "Invalid or expired reset link! ";
                return View("Error");
            }

            ViewBag.Token = token;
            return View();
        }

        // ✅ RESET PASSWORD - POST (Update password)
        [HttpPost]
        public IActionResult ResetPassword(string token, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                ViewBag.Error = "Invalid reset link! ";
                return View();
            }

            if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                ViewBag.Error = "All fields are required!";
                ViewBag.Token = token;
                return View();
            }

            if (newPassword.Length < 8)
            {
                ViewBag.Error = "Password must be at least 8 characters!";
                ViewBag.Token = token;
                return View();
            }

            if (!Regex.IsMatch(newPassword, @"^(?=.*[A-Za-z])(?=.*\d).+$"))
            {
                ViewBag.Error = "Password must contain at least one letter and one number!";
                ViewBag.Token = token;
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match!";
                ViewBag.Token = token;
                return View();
            }

            var user = _context.Users.FirstOrDefault(u => u.ResetToken == token && u.ResetTokenExpiry > DateTime.Now);
            if (user == null)
            {
                ViewBag.Error = "Invalid or expired reset link!";
                return View();
            }

            // Update password
            user.PasswordHash = newPassword;
            user.ResetToken = null;
            user.ResetTokenExpiry = null;

            _context.Users.Update(user);
            _context.SaveChanges();

            TempData["Success"] = "Password reset successful! Please login with your new password.";
            return RedirectToAction("Login");
        }
    }
}