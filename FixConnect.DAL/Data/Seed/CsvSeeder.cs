using CsvHelper;
using CsvHelper.Configuration;
using FixConnect.DAL.Context;
using FixConnect.DAL.Data.Enums;
using FixConnect.DAL.Models;
using System.Globalization;

namespace FixConnect.DAL.Data.Seed
{
    public static class CsvSeeder
    {
        public static void Seed(AppDbContext context, string csvFolderPath)
        {
            if (context.Users.Any()) return;

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim
            };

            // ============================
            // 1. Seed Users
            // ============================
            var usersPath = Path.Combine(csvFolderPath, "Seed_Users_Base.csv");
            using (var reader = new StreamReader(usersPath))
            using (var csv = new CsvReader(reader, config))
            {
                var records = csv.GetRecords<UserCsvRecord>().ToList();
                foreach (var r in records)
                {
                    context.Users.Add(new User
                    {
                        FullName = r.FullName,
                        Email = r.Email,
                        PasswordHash = r.PasswordHash,
                        Phone = r.PhoneNumber,
                        RoleType = (RoleType)r.RoleType,
                        CreatedAt = r.CreatedAt
                    });
                }
                context.SaveChanges();
            }

            // ============================
            // 2. Seed Customers
            // ============================
            var customersPath = Path.Combine(csvFolderPath, "Seed_Customers_Extension.csv");
            using (var reader = new StreamReader(customersPath))
            using (var csv = new CsvReader(reader, config))
            {
                var records = csv.GetRecords<CustomerCsvRecord>().ToList();
                foreach (var r in records)
                {
                    context.Customers.Add(new Customer
                    {
                        UserId = r.UserId,
                        Address = r.Address,
                        TotalRequests = r.TotalRequests
                    });
                }
                context.SaveChanges();
            }

            // ============================
            // 3. Seed Workers
            // ============================
            var workersPath = Path.Combine(csvFolderPath, "Seed_Workers_Extension.csv");
            using (var reader = new StreamReader(workersPath))
            using (var csv = new CsvReader(reader, config))
            {
                var records = csv.GetRecords<WorkerCsvRecord>().ToList();
                foreach (var r in records)
                {
                    context.Workers.Add(new Worker
                    {
                        UserId = r.UserId,
                        Specialty = r.Specialty,
                        Bio = r.Bio,
                        IsVerified = r.IsVerified,
                        AvailabilityStatus = (AvailabilityStatus)r.AvailabilityStatus,
                        AvgRating = r.AvgRating
                    });

                    // Wallet لكل Worker
                    context.Wallets.Add(new Wallet
                    {
                        WorkerId = r.UserId,
                        Balance = 0
                    });
                }
                context.SaveChanges();
            }
        }

        // ============================
        // CSV Record Models
        // ============================
        private class UserCsvRecord
        {
            public int Id { get; set; }
            public string FullName { get; set; } = null!;
            public string Email { get; set; } = null!;
            public string PasswordHash { get; set; } = null!;
            public string PhoneNumber { get; set; } = null!;
            public int RoleType { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        private class CustomerCsvRecord
        {
            public int UserId { get; set; }
            public string? Address { get; set; }
            public int TotalRequests { get; set; }
        }

        private class WorkerCsvRecord
        {
            public int UserId { get; set; }
            public string? Specialty { get; set; }
            public string? Bio { get; set; }
            public bool IsVerified { get; set; }
            public int AvailabilityStatus { get; set; }
            public decimal AvgRating { get; set; }
        }
    }
}