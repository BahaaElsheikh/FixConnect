using FixConnect.DAL.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FixConnect.DAL.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PasswordHash { get; set; } = string.Empty!;

        public string? GoogleId { get; set; }           // ← ADD THIS

        public bool IsActive { get; set; } /// For Admin

        public bool IsEmailConfirmed { get; set; } = false;   // ← ADDED

        public string Phone { get; set; } = null!;
        public RoleType RoleType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation (TPT children)
        public Customer? Customer { get; set; }
        public Worker? Worker { get; set; }
        public Admin? Admin { get; set; }
    }
}
