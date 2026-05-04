using FixConnect.DAL.Context;
using FixConnect.DAL.Models;

namespace FixConnect.BLL.Services
{
    public class PortfolioService
    {
        // ✅ DI: AppDbContext injected
        private readonly AppDbContext _context;
        private const int MaxItems = 10;

        public PortfolioService(AppDbContext context)
        {
            _context = context;
        }

        public List<PortfolioItem> GetItems(int workerId)
            => _context.PortfolioItems
                .Where(p => p.UserId == workerId)
                .ToList();

        public (bool Success, string Message) AddItem(
            int workerId, string title, string? description, string? imageUrl)
        {
            var count = _context.PortfolioItems.Count(p => p.UserId == workerId);
            if (count >= MaxItems)
                return (false, $"Maximum {MaxItems} portfolio items allowed.");

            _context.PortfolioItems.Add(new PortfolioItem
            {
                UserId = workerId,
                Title = title,
                Description = description,
                ImageUrl = imageUrl
            });

            _context.SaveChanges();
            return (true, "Item added successfully.");
        }

        public (bool Success, string Message) UpdateItem(
            int itemId, int workerId, string title, string? description, string? imageUrl)
        {
            var item = _context.PortfolioItems
                .FirstOrDefault(p => p.ItemId == itemId && p.UserId == workerId);

            if (item == null) return (false, "Item not found.");

            item.Title = title;
            item.Description = description;

            if (imageUrl != null)
                item.ImageUrl = imageUrl;

            _context.SaveChanges();
            return (true, "Item updated.");
        }

        public (bool Success, string Message) DeleteItem(int itemId, int workerId)
        {
            var item = _context.PortfolioItems
                .FirstOrDefault(p => p.ItemId == itemId && p.UserId == workerId);

            if (item == null) return (false, "Item not found.");

            // Delete image file from disk
            if (!string.IsNullOrEmpty(item.ImageUrl))
            {
                var fullPath = Path.Combine(
                    Directory.GetCurrentDirectory(), "wwwroot", item.ImageUrl.TrimStart('/'));
                if (File.Exists(fullPath)) File.Delete(fullPath);
            }

            _context.PortfolioItems.Remove(item);
            _context.SaveChanges();
            return (true, "Item deleted.");
        }

        public PortfolioItem? GetItem(int itemId, int workerId)
            => _context.PortfolioItems
                .FirstOrDefault(p => p.ItemId == itemId && p.UserId == workerId);
    }
}