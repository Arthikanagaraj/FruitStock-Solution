using System.ComponentModel.DataAnnotations;

namespace FruitsInventorySystem.Models
{
    public class Employee
    {
        public int Id { get; set; }

        public string? Username { get; set; }
        public string? Password { get; set; }

    }
}
