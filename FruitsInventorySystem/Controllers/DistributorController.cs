using FruitsInventorySystem.Data;
using FruitsInventorySystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using FruitsInventorySystem.Hubs;


public class DistributorController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<StockHub> _hubContext;

    public DistributorController(ApplicationDbContext context,
                             IHubContext<StockHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public IActionResult Distributor()
    {
        var today = DateTime.Today;

        var stockList = _context.Suppliers
            .Where(s => s.Date == today)
            .GroupBy(s => new { s.FruitName, s.Quality })
            .Select(g => new
            {
                FruitName = g.Key.FruitName,
                Quality = g.Key.Quality,

                SupplierTotal = g.Sum(x => x.BoxCount),

                DistributorTotal = _context.Distributors
                    .Where(d => d.Date == today &&
                                d.FruitName == g.Key.FruitName &&
                                d.Quality == g.Key.Quality)
                    .Sum(d => (int?)d.BoxCount) ?? 0
            })
            .ToList()
            .Select(x => new
            {
                x.FruitName,
                x.Quality,
                Available = x.SupplierTotal - x.DistributorTotal
            })
            .Where(x => x.Available > 0)   // 🔥 Only show available stock
            .ToList();

        ViewBag.StockList = stockList;

        return View();
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
    string PaymentType,
    string ReturnUrl)
    {
        // 🔹 NAME VALIDATION
        if (string.IsNullOrWhiteSpace(DistributorName) || DistributorName.Trim().Length < 3)
        {
            TempData["Error"] = "Distributor name must be at least 3 letters.";
            return RedirectToAction("Distributor");
        }

        // 🔹 PHONE VALIDATION
        if (string.IsNullOrWhiteSpace(PhoneNumber) ||
            PhoneNumber.Length != 10 ||
            !PhoneNumber.All(char.IsDigit))
        {
            TempData["Error"] = "Phone number must be exactly 10 digits.";
            return RedirectToAction("Distributor");
        }

        using (var transaction = _context.Database.BeginTransaction())
        {
            try
            {
                // 🔥 DELETE OLD DATA (EDIT FIX)
                var existing = _context.Distributors
                    .Where(x => x.DistributorId == DistributorId)
                    .ToList();

                if (existing.Any())
                {
                    _context.Distributors.RemoveRange(existing);
                    _context.SaveChanges();
                }

                // 🔥 SAVE NEW DATA
                for (int i = 0; i < FruitId.Count; i++)
                {
                    Distributor d = new Distributor
                    {
                        DistributorId = DistributorId,
                        DistributorName = DistributorName.Trim(),
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
                transaction.Commit();

                // ✅ RETURN URL CONDITION (SAFE)
                if (!string.IsNullOrEmpty(ReturnUrl))
                    return Redirect(ReturnUrl);


                // 🔥 DEFAULT REDIRECT
                return RedirectToAction("DistributorDashboard");
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }

    // DASHBOARD (TODAY + LIFO)
    public IActionResult DistributorDashboard(string distributorName, string searchDate)
    {
        var data = _context.Distributors.AsQueryable();

        // 🔥 NO SEARCH → SHOW TODAY DATA ONLY
        if (string.IsNullOrWhiteSpace(distributorName) && string.IsNullOrWhiteSpace(searchDate))
        {
            DateTime today = DateTime.Today;

            data = data.Where(x =>
                x.Date.HasValue &&
                x.Date.Value.Date == today);
        }
        else
        {
            // 🔥 DATE FILTER (if selected)
            if (!string.IsNullOrWhiteSpace(searchDate))
            {
                DateTime filterDate = DateTime.Parse(searchDate).Date;

                data = data.Where(x =>
                    x.Date.HasValue &&
                    x.Date.Value.Date == filterDate);
            }

            // 🔥 NAME FILTER
            if (!string.IsNullOrWhiteSpace(distributorName))
            {
                data = data.Where(x =>
                    x.DistributorName.ToLower()
                    .Contains(distributorName.ToLower()));
            }
        }

        var result = data
            .OrderByDescending(x => x.Id)
            .ToList();

        return View(result);
    }

    // EDIT
    public IActionResult EditDistributor(string id)
    {
        // 🔹 Get distributor data
        var data = _context.Distributors
            .Where(x => x.DistributorId == id)
            .ToList();

        if (!data.Any())
            return RedirectToAction("DistributorDashboard");

        // 🔹 Record date (IMPORTANT)
        DateTime recordDate = data.First().Date ?? DateTime.Today;

        // 🔥 STOCK CALCULATION (FINAL FIXED)
        var supplierStock = _context.Suppliers
            .Where(x => x.Date.HasValue &&
                        x.Date.Value.Date == recordDate.Date)
            .GroupBy(x => new { x.FruitName, x.Quality })
            .Select(g => new
            {
                FruitName = g.Key.FruitName,
                Quality = g.Key.Quality,

                // ✅ Total supplier boxes
                SupplierTotal = g.Sum(x => (int?)x.BoxCount) ?? 0,

                // ✅ OTHER distributors மட்டும் (exclude current)
                OtherDistributorTotal = _context.Distributors
                    .Where(d => d.Date.HasValue &&
                                d.Date.Value.Date == recordDate.Date &&
                                d.DistributorId != id &&
                                d.FruitName == g.Key.FruitName &&
                                d.Quality == g.Key.Quality)
                    .Sum(d => (int?)d.BoxCount) ?? 0
            })
            .ToList()
            .Select(x => new StockViewModel
            {
                FruitName = x.FruitName,
                Quality = x.Quality,

                // 🔥 FINAL CORRECT (NO + current)
                Available = x.SupplierTotal - x.OtherDistributorTotal
            })
            .Where(x => x.Available > 0) // optional but நல்லது
            .ToList();

        // 🔹 Send to view
        ViewBag.StockList = supplierStock;

        return View("Distributor", data);
    }
    public JsonResult GetDistributorById(string id)
    {
        var data = _context.Distributors
            .Where(x => x.DistributorId == id)
            .ToList();

        return Json(data);
    }
    
    [HttpGet]
    public JsonResult GetAvailableStock(string fruit, string quality, string distributorId)
    {
        var record = _context.Distributors
            .FirstOrDefault(x => x.DistributorId == distributorId);

        DateTime date = record?.Date ?? DateTime.Today;

        // ✅ Supplier total
        int supplierTotal = _context.Suppliers
            .Where(x => x.Date.HasValue &&
                        x.Date.Value.Date == date.Date &&
                        x.FruitName == fruit &&
                        x.Quality == quality)
            .Sum(x => (int?)x.BoxCount) ?? 0;

        // ✅ OTHER distributors மட்டும்
        int other = _context.Distributors
            .Where(x => x.Date.HasValue &&
                        x.Date.Value.Date == date.Date &&
                        x.DistributorId != distributorId &&
                        x.FruitName == fruit &&
                        x.Quality == quality)
            .Sum(x => (int?)x.BoxCount) ?? 0;

        // ❌ REMOVE THIS (THIS IS CAUSING 400)
        // int current = ...

        // 🔥 FINAL CORRECT
        int available = supplierTotal - other;

        return Json(new { available });
    }
    public PartialViewResult DistributorTable(string distributorName, string searchDate)
    {
        var data = _context.Distributors.AsQueryable();

        // 🔥 NO SEARCH → TODAY DATA
        if (string.IsNullOrWhiteSpace(distributorName) && string.IsNullOrWhiteSpace(searchDate))
        {
            DateTime today = DateTime.Today;

            data = data.Where(x =>
                x.Date.HasValue &&
                x.Date.Value.Date == today);
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(searchDate))
            {
                DateTime filterDate = DateTime.Parse(searchDate).Date;

                data = data.Where(x =>
                    x.Date.HasValue &&
                    x.Date.Value.Date == filterDate);
            }

            if (!string.IsNullOrWhiteSpace(distributorName))
            {
                data = data.Where(x =>
                    x.DistributorName.ToLower()
                    .Contains(distributorName.ToLower()));
            }
        }

        var result = data
            .OrderByDescending(x => x.Id)
            .ToList();

        return PartialView("_DistributorTable", result);
    }
}
