using Microsoft.EntityFrameworkCore;
using FruitsInventorySystem.Models;

namespace FruitsInventorySystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }

        // 🔥 THIS LINE WAS MISSING

        public DbSet<Supplier> Suppliers { get; set; }

        public DbSet<Distributor> Distributors { get; set; }



    }
}
