using Microsoft.EntityFrameworkCore;
using FruitsInventorySystem.Data;
using FruitsInventorySystem.Hubs;

var builder = WebApplication.CreateBuilder(args);

// 🔥 SERVICES (ADD BEFORE BUILD)
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSignalR();   // ✅ MUST BE HERE

var app = builder.Build();

// 🔥 MIDDLEWARE
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

// ⭐ DEFAULT ROUTE
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Index}/{id?}");

// 🔥 MAP HUB AFTER BUILD
app.MapHub<StockHub>("/stockHub");

app.Run();