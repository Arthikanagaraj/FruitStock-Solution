using FruitsInventorySystem.Data;
using FruitsInventorySystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;


public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    // LOGIN
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(string username, string password)
    {
        if (username == "owner" && password == "owner@123")
        {
            return RedirectToAction("Dashboard");
        }

        ViewBag.Error = "Invalid login";
        return View();
    }
    public IActionResult Dashboard(string filterType, string month, DateTime? date)
    {
        DateTime today = DateTime.Today;
        DateTime startDate = today;
        DateTime endDate = today;

        // 🔥 SINGLE DATE
        if (date.HasValue)
        {
            startDate = date.Value.Date;
            endDate = date.Value.Date;
        }

        // 🔥 MONTH FILTER
        else if (!string.IsNullOrEmpty(month))
        {
            var parts = month.Split('-');
            int year = int.Parse(parts[0]);
            int monthValue = int.Parse(parts[1]);

            startDate = new DateTime(year, monthValue, 1);
            endDate = startDate.AddMonths(1).AddDays(-1);
        }

        // 🔥 TODAY
        else if (filterType == "today")
        {
            startDate = today;
            endDate = today;
        }

        // 🔥 3 DAYS = LAST 2 DAYS + TODAY
        else if (filterType == "3days")
        {
            startDate = today.AddDays(-2);
            endDate = today;
        }

        // 🔥 WEEK = SUNDAY → SATURDAY
        else if (filterType == "week")
        {
            int diff = (int)today.DayOfWeek;
            DateTime thisWeekSunday = today.AddDays(-diff);

            startDate = thisWeekSunday.AddDays(-7);
            endDate = startDate.AddDays(6);
        }

        var suppliers = _context.Suppliers
            .Where(x => x.Date >= startDate && x.Date <= endDate)
            .ToList();

        var distributors = _context.Distributors
            .Where(x => x.Date >= startDate && x.Date <= endDate)
            .ToList();

        var inlet = suppliers
            .GroupBy(x => x.FruitName)
            .Select(g => new
            {
                Fruit = g.Key,
                Boxes = g.Sum(x => x.BoxCount)
            }).ToList();

        var outlet = distributors
            .GroupBy(x => x.FruitName)
            .Select(g => new
            {
                Fruit = g.Key,
                Boxes = g.Sum(x => x.BoxCount)
            }).ToList();

        var remaining = inlet
    .Select(i =>
    {
        var outletBoxes = outlet
            .FirstOrDefault(o => o.Fruit == i.Fruit)?.Boxes ?? 0;

        int balance = i.Boxes - outletBoxes;

        return new
        {
            Fruit = i.Fruit,
            Boxes = balance < 0 ? 0 : balance   // 🔥 FIX HERE
        };
    })
    .ToList();

        ViewBag.Inlet = inlet;
        ViewBag.Outlet = outlet;
        ViewBag.Remaining = remaining;

        return View();
    }



    // ---------------- SUPPLIER ----------------

    public IActionResult SupplierDashboard()
    {
        return View();
    }

    public IActionResult StockApproval(
    string supplierName,
    string fruitName,
    string fruitId,
    string date)
    {
        var data = _context.Suppliers.AsQueryable();

        // ⭐ Supplier Name Filter (FIXED)
        if (!string.IsNullOrWhiteSpace(supplierName))
        {
            supplierName = supplierName.Trim().ToLower();

            data = data.Where(x =>
                x.SupplierName.ToLower().Contains(supplierName));
        }

        // Fruit Name
        if (!string.IsNullOrWhiteSpace(fruitName))
        {
            fruitName = fruitName.Trim().ToLower();

            data = data.Where(x =>
                x.FruitName.ToLower().Contains(fruitName));
        }

        // Fruit ID
        if (!string.IsNullOrWhiteSpace(fruitId))
        {
            fruitId = fruitId.Trim();

            data = data.Where(x =>
                x.FruitId.Contains(fruitId));
        }

        // Date
        if (!string.IsNullOrWhiteSpace(date))
        {
            DateTime d = DateTime.Parse(date);
            data = data.Where(x =>
                x.Date.HasValue &&
                x.Date.Value.Date == d.Date);
        }

        // ⭐ LIFO ORDER
        return View(
            data.OrderByDescending(x => x.Id).ToList()
        );
    }


    // Delete Supplier
    public IActionResult DeleteSupplier(string id)
    {
        var data = _context.Suppliers
            .Where(x => x.SupplierId == id)
            .ToList();

        if (data.Any())
        {
            _context.Suppliers.RemoveRange(data);
            _context.SaveChanges();
        }

        return RedirectToAction("StockApproval");
    }

    // ---------------- DISTRIBUTOR ----------------

    // Distributor Dashboard (Admin View)
    public IActionResult DistributorDashboard()
    {
        var data = _context.Distributors
            .OrderByDescending(x => x.Date)
            .ToList();

        return View(data);
    }

    // Distributor Search + Table
    // 🔹 ADMIN – DISTRIBUTOR TABLE VIEW (LIFO)
    public IActionResult Distributor(
        string distributorName,
        string fruitName,
        string fruitId,
        string date)
    {
        var data = _context.Distributors.AsQueryable();

        // Name filter
        if (!string.IsNullOrEmpty(distributorName))
        {
            data = data.Where(x => x.DistributorName.Contains(distributorName));
        }

        // Fruit name filter
        if (!string.IsNullOrEmpty(fruitName))
        {
            data = data.Where(x => x.FruitName.Contains(fruitName));
        }

        // Fruit ID filter
        if (!string.IsNullOrEmpty(fruitId))
        {
            data = data.Where(x => x.FruitId.Contains(fruitId));
        }

        // Date filter
        if (!string.IsNullOrEmpty(date))
        {
            DateTime d = DateTime.Parse(date);
            data = data.Where(x => x.Date.HasValue && x.Date.Value.Date == d.Date);
        }

        // ⭐⭐ LIFO ORDER ⭐⭐
        var result = data
            .OrderByDescending(x => x.Id)   // Latest insert TOP
            .ToList();

        return View(result);
    }

    // Delete Distributor
    public IActionResult DeleteDistributor(string id)
    {
        var rows = _context.Distributors
            .Where(x => x.DistributorId == id)
            .ToList();

        if (rows.Any())
        {
            _context.Distributors.RemoveRange(rows);
            _context.SaveChanges();
        }

        return RedirectToAction("Distributor");
    }

    [HttpGet]
    public JsonResult GetPieChartData(string filterType)
    {
        DateTime startDate = DateTime.Today;
        DateTime endDate = DateTime.Today;

        if (filterType == "3days")
        {
            startDate = DateTime.Today.AddDays(-2);
        }
        else if (filterType == "week")
        {
            DateTime today = DateTime.Today;
            int diff = (int)today.DayOfWeek;
            DateTime thisWeekSunday = today.AddDays(-diff);

            startDate = thisWeekSunday.AddDays(-7);
            endDate = startDate.AddDays(6);
        }

        var supplier = _context.Suppliers
            .Where(x => x.Date >= startDate && x.Date <= endDate)
            .Sum(x => x.BoxCount);

        var distributor = _context.Distributors
            .Where(x => x.Date >= startDate && x.Date <= endDate)
            .Sum(x => x.BoxCount);

        return Json(new { supplier, distributor });
    }

    [HttpGet]
    public JsonResult GetLineChartData(string filterType)
    {
        DateTime startDate;
        DateTime endDate;

        if (filterType == "today")
        {
            startDate = DateTime.Today;
            endDate = DateTime.Today;
        }
        else if (filterType == "3days")
        {
            startDate = DateTime.Today.AddDays(-2);
            endDate = DateTime.Today;
        }
        else // WEEK (Previous Sunday → Saturday)
        {
            var today = DateTime.Today;

            int diff = (int)today.DayOfWeek;
            var currentWeekSunday = today.AddDays(-diff);

            startDate = currentWeekSunday.AddDays(-7);
            endDate = currentWeekSunday.AddDays(-1);
        }

        var dates = Enumerable.Range(0, (endDate - startDate).Days + 1)
                              .Select(d => startDate.AddDays(d))
                              .ToList();

        var supplierData = _context.Suppliers
            .Where(x => x.Date.HasValue &&
                        x.Date.Value.Date >= startDate &&
                        x.Date.Value.Date <= endDate)
            .GroupBy(x => x.Date.Value.Date)
            .Select(g => new
            {
                Date = g.Key,
                Total = g.Sum(x => x.BoxCount)
            })
            .ToList();

        var distributorData = _context.Distributors
            .Where(x => x.Date.HasValue &&
                        x.Date.Value.Date >= startDate &&
                        x.Date.Value.Date <= endDate)
            .GroupBy(x => x.Date.Value.Date)
            .Select(g => new
            {
                Date = g.Key,
                Total = g.Sum(x => x.BoxCount)
            })
            .ToList();

        var labels = new List<string>();
        var supplier = new List<int>();
        var distributor = new List<int>();

        foreach (var date in dates)
        {
            var s = supplierData.FirstOrDefault(x => x.Date == date.Date);
            var d = distributorData.FirstOrDefault(x => x.Date == date.Date);

            int sVal = s?.Total ?? 0;
            int dVal = d?.Total ?? 0;

            // 🔥 SKIP EMPTY DAYS
            if (sVal == 0 && dVal == 0)
                continue;

            labels.Add(date.ToString("dd-MM"));
            supplier.Add(sVal);
            distributor.Add(dVal);
        }

        return Json(new
        {
            labels,
            supplier,
            distributor
        });
    }

    [HttpGet]
    [HttpGet]
    public JsonResult GetSupplierById(string id)
    {
        var data = _context.Suppliers
            .Where(x => x.SupplierId == id)
            .ToList();

        return Json(data);
    }
    [HttpPost]
    public IActionResult SaveDistributor(
    string DistributorId,
    string DistributorName,
    string PhoneNumber,
    DateTime Date,
    List<string> FruitId,
    List<string> FruitName,
    List<string> Quality,
    List<int> BoxCount,
    List<decimal> PricePerBox,
    List<decimal> Total,
    decimal GrandTotal,
    decimal PaidAmount,
    decimal RemainingAmount,
    string PaymentType)
    {
        // 🔥 VALIDATION (VERY IMPORTANT)
        for (int i = 0; i < FruitName.Count; i++)
        {
            int supplierTotal = _context.Suppliers
                .Where(x => x.Date.HasValue &&
                            x.Date.Value.Date == Date.Date &&
                            x.FruitName == FruitName[i] &&
                            x.Quality == Quality[i])
                .Sum(x => (int?)x.BoxCount) ?? 0;

            int distributorUsed = _context.Distributors
                .Where(x => x.Date.HasValue &&
                            x.Date.Value.Date == Date.Date &&
                            x.FruitName == FruitName[i] &&
                            x.Quality == Quality[i])
                .Sum(x => (int?)x.BoxCount) ?? 0;

            // 🔥 CHECK STOCK LIMIT
            if (distributorUsed + BoxCount[i] > supplierTotal)
            {
                TempData["Error"] = $"Only {supplierTotal - distributorUsed} boxes available for {FruitName[i]}";
                return RedirectToAction("DistributorDashboard");
            }
        }

        // 🔥 SAVE DATA
        for (int i = 0; i < FruitName.Count; i++)
        {
            Distributor d = new Distributor()
            {
                DistributorId = DistributorId,
                DistributorName = DistributorName,
                PhoneNumber = PhoneNumber,
                Date = Date,

                FruitId = FruitId[i],
                FruitName = FruitName[i],
                Quality = Quality[i],
                BoxCount = BoxCount[i],
                PricePerBox = PricePerBox[i],
                Total = Total[i],

                GrandTotal = GrandTotal,
                PaidAmount = PaidAmount,
                RemainingAmount = RemainingAmount,
                PaymentType = PaymentType
            };

            _context.Distributors.Add(d);
        }

        _context.SaveChanges();

        return RedirectToAction("DistributorDashboard");
    }
}