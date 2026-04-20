using Microsoft.EntityFrameworkCore;
using vendor_api.Data;
using vendor_api.Models.Entities;

namespace vendor_api.Repositories;

// ============== Interfaces ==============

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByIdAsync(int id);
    Task<(IEnumerable<User> Users, int TotalCount)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, string? sortBy, string sortOrder);
    Task<User> CreateAsync(User user);
    Task UpdateAsync(User user);
    Task<bool> UsernameExistsAsync(string username, int? excludeId = null);
    Task<bool> EmailExistsAsync(string email, int? excludeId = null);
    Task<IEnumerable<string>> GetAdminEmailsAsync();
}

public interface IVendorRepository
{
    Task<IEnumerable<Vendor>> GetAllAsync(bool? activeOnly = null);
    Task<Vendor?> GetByIdAsync(int id);
    Task<Vendor> CreateAsync(Vendor vendor);
    Task UpdateAsync(Vendor vendor);
    Task<bool> ExistsAsync(int id);
}

public interface IResourceRepository
{
    Task<(IEnumerable<Resource> Resources, int TotalCount)> GetAllAsync(
        int pageNumber, int pageSize, string? searchTerm, string? sortBy, string sortOrder,
        int? vendorId = null, int? groupId = null, int? skillLevelId = null, bool? isActive = null,
        int? disciplineId = null, string? engineerName = null, string? currentProjectName = null,
        string? managerName = null);
    Task<Resource?> GetByIdAsync(int id);
    Task<Resource> CreateAsync(Resource resource);
    Task UpdateAsync(Resource resource);
    Task<bool> ExistsAsync(int id);
}

public interface ISkillLevelRepository
{
    Task<IEnumerable<SkillLevel>> GetAllAsync();
    Task<bool> ExistsAsync(int id);
}

public interface IGroupRepository
{
    Task<IEnumerable<Group>> GetAllAsync(bool? activeOnly = null);
    Task<bool> ExistsAsync(int id);
}

public interface IDisciplineRepository
{
    Task<IEnumerable<Discipline>> GetAllAsync(bool? activeOnly = null);
    Task<bool> ExistsAsync(int id);
}

public interface IDashboardRepository
{
    Task<int> GetTotalResourcesAsync(int? vendorId = null);
    Task<int> GetActiveResourcesAsync(int? vendorId = null);
    Task<int> GetTotalVendorsAsync();
    Task<int> GetTotalUsersAsync();
    Task<IEnumerable<(string VendorName, int Count)>> GetVendorWiseCountAsync(int? vendorId = null);
    Task<IEnumerable<(string GroupName, int Count)>> GetGroupWiseCountAsync(int? vendorId = null);
    Task<IEnumerable<(string SkillName, int Count)>> GetSkillWiseCountAsync(int? vendorId = null);
}

// ============== Implementations ==============

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Vendor)
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Vendor)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<(IEnumerable<User> Users, int TotalCount)> GetAllAsync(
        int pageNumber, int pageSize, string? searchTerm, string? sortBy, string sortOrder)
    {
        var query = _context.Users
            .Include(u => u.Role)
            .Include(u => u.Vendor)
            .AsQueryable();

        // Search
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(u =>
                u.Username.ToLower().Contains(term) ||
                u.Email.ToLower().Contains(term) ||
                u.Role.Name.ToLower().Contains(term) ||
                (u.Vendor != null && u.Vendor.VendorName.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync();

        // Sort
        query = (sortBy?.ToLower()) switch
        {
            "username" => sortOrder == "desc" ? query.OrderByDescending(u => u.Username) : query.OrderBy(u => u.Username),
            "email" => sortOrder == "desc" ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
            "role" => sortOrder == "desc" ? query.OrderByDescending(u => u.Role.Name) : query.OrderBy(u => u.Role.Name),
            "created" => sortOrder == "desc" ? query.OrderByDescending(u => u.Created) : query.OrderBy(u => u.Created),
            _ => query.OrderByDescending(u => u.Created)
        };

        var users = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (users, totalCount);
    }

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> UsernameExistsAsync(string username, int? excludeId = null)
    {
        return await _context.Users.AnyAsync(u =>
            u.Username == username && (excludeId == null || u.Id != excludeId));
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
    {
        return await _context.Users.AnyAsync(u =>
            u.Email == email && (excludeId == null || u.Id != excludeId));
    }

    public async Task<IEnumerable<string>> GetAdminEmailsAsync()
    {
        return await _context.Users
            .Where(u => u.Role.Name == "admin" && u.IsActive)
            .Select(u => u.Email)
            .ToListAsync();
    }
}

public class VendorRepository : IVendorRepository
{
    private readonly ApplicationDbContext _context;

    public VendorRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Vendor>> GetAllAsync(bool? activeOnly = null)
    {
        var query = _context.Vendors.AsQueryable();
        if (activeOnly == true)
            query = query.Where(v => v.IsActive);
        return await query.OrderBy(v => v.VendorName).ToListAsync();
    }

    public async Task<Vendor?> GetByIdAsync(int id)
    {
        return await _context.Vendors.FindAsync(id);
    }

    public async Task<Vendor> CreateAsync(Vendor vendor)
    {
        _context.Vendors.Add(vendor);
        await _context.SaveChangesAsync();
        return vendor;
    }

    public async Task UpdateAsync(Vendor vendor)
    {
        _context.Vendors.Update(vendor);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Vendors.AnyAsync(v => v.VendorId == id);
    }
}

public class ResourceRepository : IResourceRepository
{
    private readonly ApplicationDbContext _context;

    public ResourceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<Resource> Resources, int TotalCount)> GetAllAsync(
        int pageNumber, int pageSize, string? searchTerm, string? sortBy, string sortOrder,
        int? vendorId = null, int? groupId = null, int? skillLevelId = null, bool? isActive = null,
        int? disciplineId = null, string? engineerName = null, string? currentProjectName = null,
        string? managerName = null)
    {
        var query = _context.Resources
            .Include(r => r.Vendor)
            .Include(r => r.Discipline)
            .Include(r => r.SkillLevel)
            .Include(r => r.Group)
            .AsQueryable();

        // Filters
        if (vendorId.HasValue)
            query = query.Where(r => r.VendorId == vendorId.Value);
        if (groupId.HasValue)
            query = query.Where(r => r.GroupId == groupId.Value);
        if (skillLevelId.HasValue)
            query = query.Where(r => r.SkillLevelId == skillLevelId.Value);
        if (isActive.HasValue)
            query = query.Where(r => r.IsActive == isActive.Value);
        if (disciplineId.HasValue)
            query = query.Where(r => r.DisciplineId == disciplineId.Value);

        if (!string.IsNullOrWhiteSpace(engineerName))
            query = query.Where(r => r.EngineerName.ToLower().Contains(engineerName.ToLower()));
        if (!string.IsNullOrWhiteSpace(currentProjectName))
            query = query.Where(r => r.CurrentProjectName != null && r.CurrentProjectName.ToLower().Contains(currentProjectName.ToLower()));
        if (!string.IsNullOrWhiteSpace(managerName))
            query = query.Where(r => r.ManagerName != null && r.ManagerName.ToLower().Contains(managerName.ToLower()));

        // Search
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(r =>
                r.EngineerName.ToLower().Contains(term) ||
                r.Vendor.VendorName.ToLower().Contains(term) ||
                (r.CurrentProjectName != null && r.CurrentProjectName.ToLower().Contains(term)) ||
                (r.ManagerName != null && r.ManagerName.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync();

        // Sort
        query = (sortBy?.ToLower()) switch
        {
            "engineername" => sortOrder == "desc" ? query.OrderByDescending(r => r.EngineerName) : query.OrderBy(r => r.EngineerName),
            "vendorname" => sortOrder == "desc" ? query.OrderByDescending(r => r.Vendor.VendorName) : query.OrderBy(r => r.Vendor.VendorName),
            "totalexperience" => sortOrder == "desc" ? query.OrderByDescending(r => r.TotalExperienceYears) : query.OrderBy(r => r.TotalExperienceYears),
            "skillname" => sortOrder == "desc" ? query.OrderByDescending(r => r.SkillLevel.RankOrder) : query.OrderBy(r => r.SkillLevel.RankOrder),
            "groupname" => sortOrder == "desc" ? query.OrderByDescending(r => r.Group.GroupName) : query.OrderBy(r => r.Group.GroupName),
            "createdon" => sortOrder == "desc" ? query.OrderByDescending(r => r.CreatedOn) : query.OrderBy(r => r.CreatedOn),
            _ => query.OrderByDescending(r => r.CreatedOn)
        };

        var resources = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (resources, totalCount);
    }

    public async Task<Resource?> GetByIdAsync(int id)
    {
        return await _context.Resources
            .Include(r => r.Vendor)
            .Include(r => r.Discipline)
            .Include(r => r.SkillLevel)
            .Include(r => r.Group)
            .FirstOrDefaultAsync(r => r.ResourceId == id);
    }

    public async Task<Resource> CreateAsync(Resource resource)
    {
        _context.Resources.Add(resource);
        await _context.SaveChangesAsync();
        return resource;
    }

    public async Task UpdateAsync(Resource resource)
    {
        _context.Resources.Update(resource);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Resources.AnyAsync(r => r.ResourceId == id);
    }
}

public class SkillLevelRepository : ISkillLevelRepository
{
    private readonly ApplicationDbContext _context;

    public SkillLevelRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SkillLevel>> GetAllAsync()
    {
        return await _context.SkillLevels.OrderBy(s => s.RankOrder).ToListAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.SkillLevels.AnyAsync(s => s.SkillLevelId == id);
    }
}

public class GroupRepository : IGroupRepository
{
    private readonly ApplicationDbContext _context;

    public GroupRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Group>> GetAllAsync(bool? activeOnly = null)
    {
        var query = _context.Groups.AsQueryable();
        if (activeOnly == true)
            query = query.Where(g => g.IsActive);
        return await query.OrderBy(g => g.GroupName).ToListAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Groups.AnyAsync(g => g.GroupId == id);
    }
}

public class DisciplineRepository : IDisciplineRepository
{
    private readonly ApplicationDbContext _context;

    public DisciplineRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Discipline>> GetAllAsync(bool? activeOnly = null)
    {
        var query = _context.Disciplines.AsQueryable();
        if (activeOnly == true)
            query = query.Where(d => d.IsActive);
        return await query.OrderBy(d => d.SortOrder).ToListAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Disciplines.AnyAsync(d => d.DisciplineId == id);
    }
}

public class DashboardRepository : IDashboardRepository
{
    private readonly ApplicationDbContext _context;

    public DashboardRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetTotalResourcesAsync(int? vendorId = null)
    {
        var query = _context.Resources.AsQueryable();
        if (vendorId.HasValue) query = query.Where(r => r.VendorId == vendorId.Value);
        return await query.CountAsync();
    }

    public async Task<int> GetActiveResourcesAsync(int? vendorId = null)
    {
        var query = _context.Resources.Where(r => r.IsActive);
        if (vendorId.HasValue) query = query.Where(r => r.VendorId == vendorId.Value);
        return await query.CountAsync();
    }

    public async Task<int> GetTotalVendorsAsync()
    {
        return await _context.Vendors.Where(v => v.IsActive).CountAsync();
    }

    public async Task<int> GetTotalUsersAsync()
    {
        return await _context.Users.Where(u => u.IsActive).CountAsync();
    }

    public async Task<IEnumerable<(string VendorName, int Count)>> GetVendorWiseCountAsync(int? vendorId = null)
    {
        var query = _context.Resources.Where(r => r.IsActive).AsQueryable();
        if (vendorId.HasValue) query = query.Where(r => r.VendorId == vendorId.Value);

        return await query
            .GroupBy(r => r.Vendor.VendorName)
            .Select(g => new { VendorName = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Select(x => ValueTuple.Create(x.VendorName, x.Count))
            .ToListAsync();
    }

    public async Task<IEnumerable<(string GroupName, int Count)>> GetGroupWiseCountAsync(int? vendorId = null)
    {
        var query = _context.Resources.Where(r => r.IsActive).AsQueryable();
        if (vendorId.HasValue) query = query.Where(r => r.VendorId == vendorId.Value);

        return await query
            .GroupBy(r => r.Group.GroupName)
            .Select(g => new { GroupName = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Select(x => ValueTuple.Create(x.GroupName, x.Count))
            .ToListAsync();
    }

    public async Task<IEnumerable<(string SkillName, int Count)>> GetSkillWiseCountAsync(int? vendorId = null)
    {
        var query = _context.Resources.Where(r => r.IsActive).AsQueryable();
        if (vendorId.HasValue) query = query.Where(r => r.VendorId == vendorId.Value);

        return await query
            .GroupBy(r => r.SkillLevel.SkillName)
            .Select(g => new { SkillName = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Select(x => ValueTuple.Create(x.SkillName, x.Count))
            .ToListAsync();
    }
}
