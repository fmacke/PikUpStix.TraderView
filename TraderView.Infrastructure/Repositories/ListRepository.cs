using Microsoft.EntityFrameworkCore;
using TraderView.Domain.Entities;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Specifications.List;
using TraderView.Infrastructure.DbContexts;

namespace TraderView.Infrastructure.Repositories
{
    /// <summary>
    /// EF-backed repository for ListItem
    /// </summary>
    public class ListRepository : EfBaseRepository<ListItem>, IListRepository
    {
        public ListRepository(AppDbContext db) : base(db)
        {
        }

        public async Task<IReadOnlyList<ListItem>> GetByCategoryAsync(string category)
        {
            var spec = new ListItemByCategorySpecification(category);
            return await GetAsync(spec);
        }

        public async Task<int> InsertAsync(string category, string name)
        {
            var entity = new ListItem
            {
                Category = category,
                Name = name,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var added = await AddAsync(entity);
            return added.Id;
        }

        public async Task<bool> UpdateAsync(int id, string category, string name, string? description, bool isActive, DateTime updatedAt)
        {
            var existing = await _db.Set<ListItem>().FindAsync(id);
            if (existing == null)
                return false;

            existing.Category = category;
            existing.Name = name;
            existing.Description = description;
            existing.IsActive = isActive;
            existing.UpdatedAt = updatedAt;

            await UpdateAsync(existing);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _db.Set<ListItem>().FindAsync(id);
            if (existing == null)
                return false;

            await DeleteAsync(existing);
            return true;
        }

        public async Task<IReadOnlyList<string>> GetDistinctCategoriesAsync()
        {
            var list = await _db.Set<ListItem>()
                .Select(li => li.Category ?? string.Empty)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();

            return list;
        }
    }
}
