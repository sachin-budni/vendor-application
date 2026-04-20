using Microsoft.EntityFrameworkCore;
using vendor_api.Models.Entities;

namespace vendor_api.Data;

public static class DbInitializer
{
    public static void Initialize(ApplicationDbContext context)
    {
        // 1. Ensure schema is up to date and tables are created
        // This will also apply any seed data defined in OnModelCreating
        context.Database.Migrate();
    }
}
