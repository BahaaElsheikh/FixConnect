using FixConnect.DAL.Context;
using FixConnect.DAL.Data.Enums;
using FixConnect.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace FixConnect.DAL.Data.Seed
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext context)
        {
            // لو الداتابيز فيها يوزر خالص، متعملش Seed تاني
            if (context.Users.Any()) return;

            // ============================
            // Regions
            // ============================
            var regions = new List<Region>
            {
                new Region { RegionName = "Cairo" },
                new Region { RegionName = "Giza" },
                new Region { RegionName = "Alexandria" },
                new Region { RegionName = "Port Said" },
                new Region { RegionName = "Mansoura" }
            };
            context.Regions.AddRange(regions);
            context.SaveChanges();

            // ============================
            // Admin User
            // ============================
            var adminUser = new User
            {
                FullName = "System Admin",
                Email = "admin@fixconnect.com",
                PasswordHash = HashPassword("Admin@1234"),
                Phone = "01000000000",
                RoleType = RoleType.Admin,
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
            // Worker User
            // ============================
            var workerUser = new User
            {
                FullName = "Ahmed Mostafa",
                Email = "ahmed.worker@fixconnect.com",
                PasswordHash = HashPassword("Worker@1234"),
                Phone = "01111111111",
                RoleType = RoleType.Worker,
                CreatedAt = DateTime.Now
            };
            context.Users.Add(workerUser);
            context.SaveChanges();

            var worker = new Worker
            {
                UserId = workerUser.UserId,
                Specialty = "Plumbing",
                Bio = "5 years of experience in plumbing and pipe repair.",
                IsVerified = true,
                AvgRating = 4.5m,
                AvailabilityStatus = AvailabilityStatus.Available
            };
            context.Workers.Add(worker);
            context.SaveChanges();

            // Worker Wallet
            context.Wallets.Add(new Wallet
            {
                WorkerId = worker.UserId,
                Balance = 0
            });

            // Worker Regions
            context.WorksAt.Add(new WorksAt
            {
                UserId = worker.UserId,
                RegionId = regions[0].RegionId  // Cairo
            });
            context.WorksAt.Add(new WorksAt
            {
                UserId = worker.UserId,
                RegionId = regions[1].RegionId  // Giza
            });
            context.SaveChanges();

            // ============================
            // Customer User
            // ============================
            var customerUser = new User
            {
                FullName = "Sara Hassan",
                Email = "sara.customer@fixconnect.com",
                PasswordHash = HashPassword("Customer@1234"),
                Phone = "01222222222",
                RoleType = RoleType.Customer,
                CreatedAt = DateTime.Now
            };
            context.Users.Add(customerUser);
            context.SaveChanges();

            context.Customers.Add(new Customer
            {
                UserId = customerUser.UserId,
                Address = "10 Nile Street, Cairo",
                TotalRequests = 0
            });
            context.SaveChanges();
        }

        // ============================
        // Password Hashing Helper
        // ============================
        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}