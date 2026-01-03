using Air.Data;
using Air.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using System.Linq;

namespace Air.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly StripeSettings _stripeSettings;

        public UserController(ApplicationDbContext context, IOptions<StripeSettings> stripeOptions)
        {
            _context = context;
            _stripeSettings = stripeOptions.Value;
        }

        // Dashboard
        public IActionResult Dashboard()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            int userId = HttpContext.Session.GetInt32("UserId").Value;
            var user = _context.Users.Find(userId);
            return View("UserDashboard", user);
        }
        public IActionResult Profile()
        {
            var username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login");

            var user = _context.Users
                .FirstOrDefault(u => u.Username == username);

            return View(user);
        }

        // Seat Selection
        public IActionResult SelectSeat(int flightId)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var flight = _context.Airplanes
                .Include(a => a.Bookings)
                .FirstOrDefault(a => a.Id == flightId);

            if (flight == null) return NotFound();

            var bookedSeats = flight.Bookings
                .Select(b => $"{b.SeatRow}{b.SeatColumn}")
                .ToList();

            var model = new SeatSelectionViewModel
            {
                FlightId = flight.Id,
                FlightName = flight.AirplaneName,
                SeatRows = flight.SeatRows,
                SeatCols = flight.SeatColumns,
                TicketPrice = flight.TicketPrice,
                BookedSeats = bookedSeats
            };

            return View(model);
        }

        // Book Ticket - Show Flights
        public IActionResult BookTicket()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var flights = _context.Airplanes
                .Include(a => a.Bookings)  // so booked seats are loaded
                .ToList();

            return View(flights);  // Model should be IEnumerable<Airplane>
        }

        // Confirm Seat → Redirect to Payment
        [HttpPost]
        public IActionResult ConfirmBooking(int airplaneId, string seat)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var airplane = _context.Airplanes
                .Include(a => a.Bookings)
                .FirstOrDefault(a => a.Id == airplaneId);

            if (airplane == null) return BadRequest("Selected airplane does not exist.");

            int row = int.Parse(seat[..^1]);
            char col = seat[^1];

            if (airplane.Bookings.Any(b => b.SeatRow == row && b.SeatColumn == col))
                return BadRequest("Seat already booked");

            // Redirect to Payment page with info
            return RedirectToAction("Payment", new { airplaneId, seat });
        }

        // Payment Page
        public IActionResult Payment(int airplaneId, string seat)
        {
            var airplane = _context.Airplanes.Find(airplaneId);
            if (airplane == null) return NotFound();

            int userId = HttpContext.Session.GetInt32("UserId").Value;
            string username = HttpContext.Session.GetString("Username");

            // Create Stripe PaymentIntent
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(airplane.TicketPrice * 100), // cents
                Currency = "usd",
                Metadata = new System.Collections.Generic.Dictionary<string, string>
                {
                    { "AirplaneId", airplaneId.ToString() },
                    { "Seat", seat },
                    { "UserId", userId.ToString() },
                    { "Username", username }
                }
            };
            var service = new PaymentIntentService();
            var paymentIntent = service.Create(options);

            var model = new PaymentViewModel
            {
                AirplaneId = airplaneId,
                SeatRow = int.Parse(seat[..^1]),
                SeatColumn = seat[^1],
                UserId = userId,
                UserName = username,
                Amount = airplane.TicketPrice,
                PublishableKey = _stripeSettings.PublishableKey,
                ClientSecret = paymentIntent.ClientSecret
            };

            return View(model); // Views/User/Payment.cshtml
        }

        // Payment Success → Save Booking
        // Payment Success → Save booking and redirect to MyBookings with receipt
        [HttpGet]
        public IActionResult PaymentSuccess(string paymentIntentId)
        {
            var service = new Stripe.PaymentIntentService();
            var paymentIntent = service.Get(paymentIntentId);

            int airplaneId = int.Parse(paymentIntent.Metadata["AirplaneId"]);
            string seat = paymentIntent.Metadata["Seat"];
            int userId = int.Parse(paymentIntent.Metadata["UserId"]);
            string username = paymentIntent.Metadata["Username"];

            var airplane = _context.Airplanes
                .Include(a => a.Bookings)
                .FirstOrDefault(a => a.Id == airplaneId);

            if (airplane == null) return BadRequest("Airplane not found.");

            int row = int.Parse(seat[..^1]);
            char col = seat[^1];

            if (airplane.Bookings.Any(b => b.SeatRow == row && b.SeatColumn == col))
                return BadRequest("Seat already booked.");

            var booking = new Booking
            {
                AirplaneId = airplaneId,
                UserId = userId,
                UserName = username,
                SeatRow = row,
                SeatColumn = col
            };

            _context.Bookings.Add(booking);
            airplane.UpdateSeats();
            _context.SaveChanges();

            // Redirect to MyBookings and optionally pass a receipt flag
            return RedirectToAction("MyBookings", new { receipt = booking.Id });
        }

        // My Bookings
        public IActionResult MyBookings(int? receipt)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            int userId = HttpContext.Session.GetInt32("UserId").Value;

            var myBookings = _context.Bookings
                .Where(b => b.UserId == userId)
                .Include(b => b.Airplane)
                .ToList();

            ViewBag.ReceiptBookingId = receipt; // pass the receipt info to view
            return View(myBookings);
        }

        // cancel ticket
        [HttpPost]
        public IActionResult CancelBooking(int bookingId)
        {
            var booking = _context.Bookings.Find(bookingId);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                _context.SaveChanges();
            }
            return RedirectToAction("MyBookings");
        }


        // Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        private bool IsLoggedIn() => HttpContext.Session.GetInt32("UserId") != null;
    }
}
