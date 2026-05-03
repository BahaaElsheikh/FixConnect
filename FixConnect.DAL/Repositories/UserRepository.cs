using FixConnect.DAL.Context;
using FixConnect.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace FixConnect.DAL.Repositories
{
    public class UserRepository : GenericRepository<User>
    {
        // ✅ DI: AppDbContext injected here
        public UserRepository(AppDbContext context) : base(context) { }

        public User? GetByEmail(string email)
            => Context.Users.FirstOrDefault(u => u.Email == email);

        public User? GetByGoogleId(string googleId)
            => Context.Users.FirstOrDefault(u => u.GoogleId == googleId);

        public bool EmailExists(string email)
            => Context.Users.Any(u => u.Email == email);

        public User? GetWithRole(int userId)
            => Context.Users
                .Include(u => u.Customer)
                .Include(u => u.Worker)
                .Include(u => u.Admin)
                .FirstOrDefault(u => u.UserId == userId);
    }
}