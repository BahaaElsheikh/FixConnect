using FixConnect.DAL.Context;
using FixConnect.DAL.Data.Enums;
using FixConnect.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace FixConnect.DAL.Data.Seed
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext context)
        {
            // لو التيست يوزرز موجودين خالص، متضيفش تاني
            if (context.Users.Any(u => u.Email == "admin@test.com")) return;

            // ============================
            // Test Admin
            // ============================
            var adminUser = new User
            {
                FullName = "Test Admin",
                Email = "admin@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@1234"),
                Phone = "01000000001",
                RoleType = RoleType.Admin,
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            context.Users.Add(adminUser);
            context.SaveChanges();

            context.Admins.Add(new Admin
            {
                UserId = adminUser.UserId,
                PermissionsLevel = 1
            });
            context.SaveChanges();

            // ============================
            // Test Worker
            // ============================
            var workerUser = new User
            {
                FullName = "Test Worker",
                Email = "worker@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Worker@1234"),
                Phone = "01000000002",
                RoleType = RoleType.Worker,
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            context.Users.Add(workerUser);
            context.SaveChanges();

            context.Workers.Add(new Worker
            {
                UserId = workerUser.UserId,
                Specialty = "Plumbing",
                Bio = "Test worker account.",
                IsVerified = false,
                AvailabilityStatus = AvailabilityStatus.Available,
                AvgRating = 0
            });

            context.Wallets.Add(new Wallet
            {
                WorkerId = workerUser.UserId,
                Balance = 0
            });

            context.SaveChanges();

            // ============================
            // Test Customer
            // ============================
            var customerUser = new User
            {
                FullName = "Test Customer",
                Email = "customer@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Customer@1234"),
                Phone = "01000000003",
                RoleType = RoleType.Customer,
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            context.Users.Add(customerUser);
            context.SaveChanges();

            context.Customers.Add(new Customer
            {
                UserId = customerUser.UserId,
                Address = "Test Address, Cairo",
                TotalRequests = 0
            });

            context.SaveChanges();
        }
    }
}