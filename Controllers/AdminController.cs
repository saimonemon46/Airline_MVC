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

        
    }
}
