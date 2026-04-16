using FruitsInventorySystem.Data;
using FruitsInventorySystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class EmployeeController : Controller
{
    private readonly ApplicationDbContext _context;

    public EmployeeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Dashboard()
    {
        return View();
    }

    public IActionResult SupplierDashboard(string supplierName, string searchDate)
    {
        var data = _context.Suppliers.AsQueryable();

        // 🔥 NO SEARCH → TODAY DATA ONLY
        if (string.IsNullOrWhiteSpace(supplierName) && string.IsNullOrWhiteSpace(searchDate))
        {
            DateTime today = DateTime.Today;

            data = data.Where(x =>
                x.Date.HasValue &&
                x.Date.Value.Date == today);
        }
        else
        {
            // 🔥 DATE FILTER
            if (!string.IsNullOrWhiteSpace(searchDate))
            {
                DateTime filterDate = DateTime.Parse(searchDate).Date;

                data = data.Where(x =>
                    x.Date.HasValue &&
                    x.Date.Value.Date == filterDate);
            }

            // 🔥 NAME FILTER
            if (!string.IsNullOrWhiteSpace(supplierName))
            {
                data = data.Where(x =>
                    x.SupplierName.ToLower()
                    .Contains(supplierName.ToLower()));
            }
        }

        var result = data
            .OrderByDescending(x => x.Id)
            .ToList();

        return View(result);
    }



    public IActionResult Supplier()
    {
        return View();
    }
    [HttpPost]
    public IActionResult SaveSupplier(
string SupplierId,
string SupplierName,
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
        var oldRows = _context.Suppliers
            .Where(x => x.SupplierId == SupplierId)
            .ToList();

        if (oldRows.Any())
        {
            _context.Suppliers.RemoveRange(oldRows);
            _context.SaveChanges();
        }

        for (int i = 0; i < FruitId.Count; i++)
        {
            Supplier s = new Supplier();

            s.SupplierId = SupplierId;
            s.SupplierName = SupplierName;
            s.PhoneNumber = PhoneNumber;
            s.Date = Date;

            s.FruitId = FruitId[i];
            s.FruitName = FruitName[i];
            s.Quality = Quality[i];
            s.BoxCount = BoxCount[i];
            s.PricePerBox = PricePerBox[i];
            s.Total = Total[i];

            s.GrandTotal = GrandTotal;
            s.PaidAmount = PaidAmount;
            s.RemainingAmount = RemainingAmount;
            s.PaymentType = PaymentType;

            _context.Suppliers.Add(s);
        }

        _context.SaveChanges();

        if(!string.IsNullOrEmpty(ReturnUrl))
    return Redirect(ReturnUrl);

        return RedirectToAction("SupplierDashboard");
    }


    public IActionResult EditSupplier(string id)
    {
        var data = _context.Suppliers
            .Where(x => x.SupplierId == id)
            .ToList();

        return View("Supplier", data);
    }

    public JsonResult GetSupplierById(string id)
    {
        var data = _context.Suppliers
            .Where(x => x.SupplierId == id)
            .ToList(); // 🔥 IMPORTANT (no Select)

        return Json(data);
    }
    public PartialViewResult SupplierTable(string supplierName, string searchDate)
    {
        var data = _context.Suppliers.AsQueryable();

        // 🔥 NO SEARCH → TODAY DATA
        if (string.IsNullOrWhiteSpace(supplierName) && string.IsNullOrWhiteSpace(searchDate))
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

            if (!string.IsNullOrWhiteSpace(supplierName))
            {
                data = data.Where(x =>
                    x.SupplierName.ToLower()
                    .Contains(supplierName.ToLower()));
            }
        }

        var result = data
            .OrderByDescending(x => x.Id)
            .ToList();

        return PartialView("_SupplierTable", result);
    }
}
