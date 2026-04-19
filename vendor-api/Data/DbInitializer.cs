using Microsoft.EntityFrameworkCore;
using vendor_api.Models.Entities;

namespace vendor_api.Data;

public static class DbInitializer
{
    public static void Initialize(ApplicationDbContext context)
    {
        // 1. Ensure schema is up to date and tables are created
        context.Database.Migrate();

        // 2. Seed Roles
        if (!context.Roles.Any())
        {
            context.Roles.AddRange(
                new Role { Name = "vendor" },
                new Role { Name = "admin" }
            );
            context.SaveChanges();
        }

        // 3. Seed Vendors
        if (!context.Vendors.Any())
        {
            context.Vendors.AddRange(
                new Vendor { VendorName = "Alpha Tech", IsActive = true, Created = DateTime.UtcNow },
                new Vendor { VendorName = "Beta Solutions", IsActive = true, Created = DateTime.UtcNow },
                new Vendor { VendorName = "Gamma Systems", IsActive = true, Created = DateTime.UtcNow },
                new Vendor { VendorName = "Rockwell", IsActive = true, Created = DateTime.UtcNow }
            );
            context.SaveChanges();
        }

        // 4. Seed Users
        if (!context.Users.Any())
        {
            var adminRole = context.Roles.FirstOrDefault(r => r.Name == "admin");
            var vendorRole = context.Roles.FirstOrDefault(r => r.Name == "vendor");
            var alphaVendor = context.Vendors.FirstOrDefault(v => v.VendorName == "Alpha Tech");

            context.Users.AddRange(
                new User { Username = "shiva", Email = "admin@test.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), RoleId = adminRole!.Id, IsActive = true, Created = DateTime.UtcNow },
                new User { Username = "vendor1", Email = "v1@test.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass123!"), RoleId = vendorRole!.Id, VendorId = alphaVendor?.VendorId, IsActive = true, Created = DateTime.UtcNow }
            );
            context.SaveChanges();
        }

        // 5. Seed Metadata Lookups
        if (!context.SkillLevels.Any())
        {
            context.SkillLevels.AddRange(
                new SkillLevel { SkillCode = "L0", SkillName = "Basic", RankOrder = 0 },
                new SkillLevel { SkillCode = "L1", SkillName = "Intermediate", RankOrder = 1 },
                new SkillLevel { SkillCode = "L2", SkillName = "Advanced", RankOrder = 2 }
            );
        }

        if (!context.Groups.Any())
        {
            context.Groups.AddRange(
                new Group { GroupCode = "SE", GroupName = "Software Engineering", IsActive = true },
                new Group { GroupCode = "ES", GroupName = "Embedded Systems", IsActive = true }
            );
        }

        if (!context.Disciplines.Any())
        {
            context.Disciplines.AddRange(
                new Discipline { DisciplineCode = "D01", DisciplineName = "Control Software", SortOrder = 1, IsActive = true },
                new Discipline { DisciplineCode = "D02", DisciplineName = "Safety Software", SortOrder = 2, IsActive = true }
            );
        }

        context.SaveChanges();
    }
}
