using FixConnect.DAL.Context;
using FixConnect.DAL.Data.Enums;
using FixConnect.DAL.Models;
using FixConnect.DAL.Repositories;

namespace FixConnect.BLL.Services
{
    public class AuthService
    {
        // ✅ DI: Both injected via constructor
        private readonly UserRepository _userRepo;
        private readonly AppDbContext _context;

        public AuthService(UserRepository userRepo, AppDbContext context)
        {
            _userRepo = userRepo;
            _context = context;
        }

        // ─────────────────────────────
        // REGISTER (Manual)
        // ─────────────────────────────
        public (bool Success, string Message) Register(
    string fullName,
    string email,
    string password,
    string phone,
    RoleType role,
    int? specialtyId = null)    // ← بدل string? specialty
        {
            if (_userRepo.EmailExists(email))
                return (false, "Email is already registered.");

            var user = new User
            {
                FullName = fullName,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Phone = phone,
                RoleType = role,
                CreatedAt = DateTime.Now,
                IsActive = true 
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            if (role == RoleType.Customer)
            {
                _context.Customers.Add(new Customer
                {
                    UserId = user.UserId,
                    TotalRequests = 0
                });
            }
            else if (role == RoleType.Worker)
            {
                _context.Workers.Add(new Worker
                {
                    UserId = user.UserId,
                    SpecialtyId = specialtyId,    // ← int? بدل string
                    IsVerified = false,
                    AvailabilityStatus = AvailabilityStatus.Available
                });

                _context.Wallets.Add(new Wallet
                {
                    WorkerId = user.UserId,
                    Balance = 0
                });
            }

            _context.SaveChanges();
            return (true, "Registration successful.");
        }
        // ─────────────────────────────
        // LOGIN (Manual)
        // ─────────────────────────────
        public User? Login(string email, string password)
        {
            var user = _userRepo.GetByEmail(email);
            if (user == null) return null;
            if (string.IsNullOrEmpty(user.PasswordHash)) return null; // Google-only account

            bool valid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

            return valid ? user : null;
        }

        // ─────────────────────────────
        // GOOGLE — Find or Prepare
        // ─────────────────────────────

        // Case A: existing user found by GoogleId or Email
        public User? FindByGoogleId(string googleId)
            => _userRepo.GetByGoogleId(googleId);

        public User? FindByEmail(string email)
            => _userRepo.GetByEmail(email);

        // Case B: new Google user — link GoogleId to existing email
        public void LinkGoogleId(User user, string googleId)
        {
            user.GoogleId = googleId;
            _context.Users.Update(user);
            _context.SaveChanges();
        }

        // Case B: brand new user from Google — complete profile step
        public (bool Success, string Message) RegisterGoogleUser(
     string fullName,
     string email,
     string googleId,
     string phone,
     RoleType role,
     int? specialtyId = null)    // ← بدل string? specialty
        {
            if (_userRepo.EmailExists(email))
                return (false, "Email already registered.");

            var user = new User
            {
                FullName = fullName,
                Email = email,
                PasswordHash = string.Empty,
                Phone = phone,
                RoleType = role,
                GoogleId = googleId,
                CreatedAt = DateTime.Now,
                 IsActive = true
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            if (role == RoleType.Customer)
            {
                _context.Customers.Add(new Customer
                {
                    UserId = user.UserId,
                    TotalRequests = 0
                });
            }
            else if (role == RoleType.Worker)
            {
                _context.Workers.Add(new Worker
                {
                    UserId = user.UserId,
                    SpecialtyId = specialtyId,    // ← int? بدل string
                    IsVerified = false,
                    AvailabilityStatus = AvailabilityStatus.Available
                });
                _context.Wallets.Add(new Wallet
                {
                    WorkerId = user.UserId,
                    Balance = 0
                });
            }

            _context.SaveChanges();
            return (true, "Google registration successful.");
        }

        public List<DAL.Models.Specialty> GetSpecialties()
    => _context.Specialties.OrderBy(s => s.SpecialtyName).ToList();
    }
}