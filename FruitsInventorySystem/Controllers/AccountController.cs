using Microsoft.AspNetCore.Mvc;
using FruitsInventorySystem.Data;
using FruitsInventorySystem.Models;
using System.Linq;

namespace FruitsInventorySystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // HOME
        public IActionResult Index()
        {
            return View();
        }

        // REGISTER - GET
        public IActionResult Register()
        {
            return View();
        }

        // REGISTER - POST
        [HttpPost]
        public IActionResult Register(Employee emp)
        {
            if (_context.Employees.Any(e => e.Username == emp.Username))
            {
                ViewBag.Error = "Username already exists";
                return View();
            }

            _context.Employees.Add(emp);
            _context.SaveChanges();

            // ✅ REGISTER SUCCESS → LOGIN PAGE
            return RedirectToAction("EmployeeLogin");
        }

        // LOGIN - GET
        public IActionResult EmployeeLogin()
        {
            return View();
        }

        // LOGIN - POST
        [HttpPost]
        public IActionResult EmployeeLogin(string username, string password)
        {
            var emp = _context.Employees
                .FirstOrDefault(e => e.Username == username && e.Password == password);

            if (emp != null)
            {
                return RedirectToAction("Dashboard", "Employee");
            }

            ViewBag.Error = "Invalid Login Details";
            return View();
        }
    }
}
