using Air.Data;
using Air.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Air.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Dashboard
        public IActionResult Dashboard()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (email == null || email.ToLower() != "saimonemon46@gmail.com")
                return RedirectToAction("Login", "Account");

            return View("AdminDashboard");
        }

        public IActionResult Profile()
        {
            // Example: assuming admin is stored in Users table with Role = "Admin"
            var admin = _context.Users.FirstOrDefault(u => u.Username == "Admin");

            if (admin == null)
                return View(); // or show some error

            return View(admin); // pass the model to the view
        }


        // GET: /Admin/AddAirplane
        public IActionResult AddAirplane()
        {
            return View();
        }

        // POST: /Admin/AddAirplane
        [HttpPost]
        public IActionResult AddAirplane(Airplane model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Optional: dynamically calculate seats if needed
            model.UpdateSeats();

            _context.Airplanes.Add(model);
            _context.SaveChanges();

            TempData["Message"] = "Airplane added successfully!";
            return RedirectToAction("SeeAirplanes");
        }

        // GET: /Admin/SeeAirplanes
        public IActionResult SeeAirplanes()
        {
            var airplanes = _context.Airplanes
                .Include(a => a.Bookings) // include bookings for AvailableSeats
                .ToList();

            return View(airplanes);
        }

        // GET: /Admin/SeeUsers
        public IActionResult SeeUsers()
        {
            var users = _context.Users.ToList();
            return View(users);
        }

        // GET: /Admin/SeeBookings
        public IActionResult SeeBookings()
        {
            var bookings = _context.Bookings
                .Include(b => b.Airplane) 
                .Include(b => b.User)
                .ToList();

            return View(bookings);
        }

        // GET: /Admin/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}
