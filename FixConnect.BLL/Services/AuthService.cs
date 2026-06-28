using FixConnect.DAL.Context;
using FixConnect.DAL.Data.Enums;
using FixConnect.DAL.Models;
using FixConnect.DAL.Repositories;

namespace FixConnect.BLL.Services
{
    public class AuthService
    {
        // ✅ DI: All three injected via constructor
        private readonly UserRepository _userRepo;
        private readonly AppDbContext _context;
        private readonly EmailSender _emailSender;

        public AuthService(UserRepository userRepo, AppDbContext context, EmailSender emailSender)
        {
            _userRepo = userRepo;
            _context = context;
            _emailSender = emailSender;
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
            int? specialtyId = null)
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
                IsActive = true,
                IsEmailConfirmed = false
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
                    SpecialtyId = specialtyId,
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
        public User? FindByGoogleId(string googleId)
            => _userRepo.GetByGoogleId(googleId);

        public User? FindByEmail(string email)
            => _userRepo.GetByEmail(email);

        public void LinkGoogleId(User user, string googleId)
        {
            user.GoogleId = googleId;
            _context.Users.Update(user);
            _context.SaveChanges();
        }

        public (bool Success, string Message) RegisterGoogleUser(
            string fullName,
            string email,
            string googleId,
            string phone,
            RoleType role,
            int? specialtyId = null)
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
                IsActive = true,
                IsEmailConfirmed = true // Google already verified this email
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
                    SpecialtyId = specialtyId,
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

        // ─────────────────────────────────────────────
        // EMAIL CONFIRMATION
        // ─────────────────────────────────────────────

        // Creates + stores the token, returns the raw token string.
        // Controller builds the actual URL (via Url.Action) and calls
        // SendConfirmationEmailAsync with that link.
        public string GenerateEmailConfirmationToken(int userId)
        {
            var oldTokens = _context.UserTokens
                .Where(t => t.UserId == userId
                    && t.Type == TokenType.EmailConfirmation
                    && !t.IsUsed)
                .ToList();
            foreach (var old in oldTokens) old.IsUsed = true;

            var token = new UserToken
            {
                UserId = userId,
                Token = GenerateSecureToken(),
                Type = TokenType.EmailConfirmation,
                ExpiryDate = DateTime.Now.AddHours(24),
                IsUsed = false,
                CreatedAt = DateTime.Now
            };

            _context.UserTokens.Add(token);
            _context.SaveChanges();

            return token.Token;
        }

        public async Task SendConfirmationEmailAsync(string toEmail, string fullName, string confirmationLink)
        {
            string subject = "Confirm your FixConnect account";
            string body = $@"
                <div style='font-family:Arial,sans-serif;max-width:480px;margin:auto;'>
                    <h2 style='color:#003d9b;'>Welcome to FixConnect, {fullName}!</h2>
                    <p>Please confirm your email address by clicking the button below:</p>
                    <p style='text-align:center;margin:24px 0;'>
                        <a href='{confirmationLink}' style='background:#003d9b;color:#fff;padding:12px 24px;border-radius:6px;text-decoration:none;'>Confirm Email</a>
                    </p>
                    <p>This link expires in 24 hours.</p>
                    <p>If you didn't create this account, you can ignore this email.</p>
                </div>";

            await _emailSender.SendEmailAsync(toEmail, subject, body);
        }

        public (bool Success, string Message) ConfirmEmail(int userId, string token)
        {
            var record = _context.UserTokens.FirstOrDefault(t =>
                t.UserId == userId &&
                t.Token == token &&
                t.Type == TokenType.EmailConfirmation);

            if (record == null)
                return (false, "Invalid confirmation link.");

            if (record.IsUsed)
                return (false, "This confirmation link has already been used.");

            if (record.ExpiryDate < DateTime.Now)
                return (false, "This confirmation link has expired.");

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
                return (false, "User not found.");

            user.IsEmailConfirmed = true;
            record.IsUsed = true;

            _context.SaveChanges();
            return (true, "Email confirmed successfully.");
        }

        // ─────────────────────────────────────────────
        // FORGOT PASSWORD / RESET PASSWORD
        // ─────────────────────────────────────────────

        // Returns null if no account exists for that email (Controller decides
        // whether to reveal that or show a generic "check your email" message)
        public string? GeneratePasswordResetToken(string email)
        {
            var user = _userRepo.GetByEmail(email);
            if (user == null) return null;

            var oldTokens = _context.UserTokens
                .Where(t => t.UserId == user.UserId
                    && t.Type == TokenType.PasswordReset
                    && !t.IsUsed)
                .ToList();
            foreach (var old in oldTokens) old.IsUsed = true;

            var token = new UserToken
            {
                UserId = user.UserId,
                Token = GenerateSecureToken(),
                Type = TokenType.PasswordReset,
                ExpiryDate = DateTime.Now.AddHours(1), // shorter expiry for security
                IsUsed = false,
                CreatedAt = DateTime.Now
            };

            _context.UserTokens.Add(token);
            _context.SaveChanges();

            return token.Token;
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string fullName, string resetLink)
        {
            string subject = "Reset your FixConnect password";
            string body = $@"
                <div style='font-family:Arial,sans-serif;max-width:480px;margin:auto;'>
                    <h2 style='color:#003d9b;'>Password Reset Request</h2>
                    <p>Hi {fullName}, we received a request to reset your password.</p>
                    <p style='text-align:center;margin:24px 0;'>
                        <a href='{resetLink}' style='background:#003d9b;color:#fff;padding:12px 24px;border-radius:6px;text-decoration:none;'>Reset Password</a>
                    </p>
                    <p>This link expires in 1 hour. If you didn't request this, you can safely ignore this email.</p>
                </div>";

            await _emailSender.SendEmailAsync(toEmail, subject, body);
        }

        // Validates token only (used by GET ResetPassword to show/hide the form)
        public bool ValidatePasswordResetToken(int userId, string token)
        {
            var record = _context.UserTokens.FirstOrDefault(t =>
                t.UserId == userId &&
                t.Token == token &&
                t.Type == TokenType.PasswordReset);

            if (record == null || record.IsUsed || record.ExpiryDate < DateTime.Now)
                return false;

            return true;
        }

        public (bool Success, string Message) ResetPassword(int userId, string token, string newPassword)
        {
            var record = _context.UserTokens.FirstOrDefault(t =>
                t.UserId == userId &&
                t.Token == token &&
                t.Type == TokenType.PasswordReset);

            if (record == null)
                return (false, "Invalid reset link.");

            if (record.IsUsed)
                return (false, "This reset link has already been used.");

            if (record.ExpiryDate < DateTime.Now)
                return (false, "This reset link has expired.");

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
                return (false, "User not found.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            record.IsUsed = true;

            _context.SaveChanges();
            return (true, "Password has been reset successfully.");
        }

        // ─────────────────────────────────────────────
        // HELPERS
        // ─────────────────────────────────────────────
        private static string GenerateSecureToken()
        {
            // URL-safe, long, unguessable
            return Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        }
    }
}