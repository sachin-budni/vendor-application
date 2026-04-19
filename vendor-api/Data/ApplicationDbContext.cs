using Microsoft.EntityFrameworkCore;
using vendor_api.Models.Entities;

namespace vendor_api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<User> Users => Set<User>();
    public DbSet<SkillLevel> SkillLevels => Set<SkillLevel>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<Discipline> Disciplines => Set<Discipline>();
    public DbSet<Resource> Resources => Set<Resource>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Role
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
            entity.HasData(
                new Role { Id = 1, Name = "vendor" },
                new Role { Id = 2, Name = "admin" }
            );
        });

        // Vendor
        modelBuilder.Entity<Vendor>(entity =>
        {
            entity.HasKey(e => e.VendorId);
            entity.Property(e => e.VendorName).HasMaxLength(100);
            entity.Property(e => e.Created).HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.HasData(
                new Vendor { VendorId = 1, VendorName = "Rockwell", IsActive = true, Created = new DateTime(2025, 1, 1) },
                new Vendor { VendorId = 2, VendorName = "Esskay", IsActive = true, Created = new DateTime(2025, 1, 1) },
                new Vendor { VendorId = 3, VendorName = "Emerson", IsActive = true, Created = new DateTime(2025, 1, 1) },
                new Vendor { VendorId = 4, VendorName = "ABB", IsActive = true, Created = new DateTime(2025, 1, 1) },
                new Vendor { VendorId = 5, VendorName = "Schneider", IsActive = true, Created = new DateTime(2025, 1, 1) },
                new Vendor { VendorId = 6, VendorName = "Yokogawa", IsActive = true, Created = new DateTime(2025, 1, 1) }
            );
        });

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Created).HasDefaultValueSql("GETDATE()");

            entity.HasOne(e => e.Role)
                  .WithMany(r => r.Users)
                  .HasForeignKey(e => e.RoleId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Vendor)
                  .WithMany(v => v.Users)
                  .HasForeignKey(e => e.VendorId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Seed users with BCrypt hashed passwords
            entity.HasData(
                new User
                {
                    Id = 1,
                    Username = "ram",
                    Email = "ram@company.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("ram123"),
                    RoleId = 1,
                    VendorId = 1,
                    IsActive = true,
                    Created = new DateTime(2025, 1, 1)
                },
                new User
                {
                    Id = 2,
                    Username = "shiva",
                    Email = "shiva@company.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    RoleId = 2,
                    VendorId = null,
                    IsActive = true,
                    Created = new DateTime(2025, 1, 1)
                }
            );
        });

        // SkillLevel
        modelBuilder.Entity<SkillLevel>(entity =>
        {
            entity.HasKey(e => e.SkillLevelId);
            entity.Property(e => e.SkillCode).HasMaxLength(10);
            entity.Property(e => e.SkillName).HasMaxLength(100);
            entity.HasData(
                new SkillLevel { SkillLevelId = 1, SkillCode = "L0", SkillName = "Can Not Do", RankOrder = 0 },
                new SkillLevel { SkillLevelId = 2, SkillCode = "L1", SkillName = "Can Do Under Supervision", RankOrder = 1 },
                new SkillLevel { SkillLevelId = 3, SkillCode = "L2", SkillName = "Can Do Independently", RankOrder = 2 },
                new SkillLevel { SkillLevelId = 4, SkillCode = "L3", SkillName = "Can Supervise Others", RankOrder = 3 },
                new SkillLevel { SkillLevelId = 5, SkillCode = "L4", SkillName = "Subject Matter Expert", RankOrder = 4 }
            );
        });

        // Group
        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.GroupId);
            entity.Property(e => e.GroupCode).HasMaxLength(20);
            entity.Property(e => e.GroupName).HasMaxLength(100);
            entity.HasData(
                new Group { GroupId = 1, GroupCode = "SE", GroupName = "SE", IsActive = true },
                new Group { GroupId = 2, GroupCode = "ES", GroupName = "ES", IsActive = true },
                new Group { GroupId = 3, GroupCode = "LSS", GroupName = "LSS", IsActive = true },
                new Group { GroupId = 4, GroupCode = "AS", GroupName = "AS", IsActive = true },
                new Group { GroupId = 5, GroupCode = "DFE", GroupName = "DFE", IsActive = true }
            );
        });

        // Discipline
        modelBuilder.Entity<Discipline>(entity =>
        {
            entity.HasKey(e => e.DisciplineId);
            entity.Property(e => e.DisciplineCode).HasMaxLength(20);
            entity.Property(e => e.DisciplineName).HasMaxLength(150);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.HasData(
                new Discipline { DisciplineId = 1, DisciplineCode = "D01", DisciplineName = "Control Software", SortOrder = 1, IsActive = true },
                new Discipline { DisciplineId = 2, DisciplineCode = "D02", DisciplineName = "Safety Software", SortOrder = 2, IsActive = true },
                new Discipline { DisciplineId = 3, DisciplineCode = "D03", DisciplineName = "Control Hardware", SortOrder = 3, IsActive = true },
                new Discipline { DisciplineId = 4, DisciplineCode = "D04", DisciplineName = "Safety Resource", SortOrder = 4, IsActive = true },
                new Discipline { DisciplineId = 5, DisciplineCode = "D05", DisciplineName = "RTU Resource", SortOrder = 5, IsActive = true },
                new Discipline { DisciplineId = 6, DisciplineCode = "D06", DisciplineName = "HMI Graphics", SortOrder = 6, IsActive = true },
                new Discipline { DisciplineId = 7, DisciplineCode = "D07", DisciplineName = "ACAD & MicroStation", SortOrder = 7, IsActive = true },
                new Discipline { DisciplineId = 8, DisciplineCode = "D08", DisciplineName = "CNV", SortOrder = 8, IsActive = true },
                new Discipline { DisciplineId = 9, DisciplineCode = "D09", DisciplineName = "TSI", SortOrder = 9, IsActive = true },
                new Discipline { DisciplineId = 10, DisciplineCode = "D10", DisciplineName = "F & G", SortOrder = 10, IsActive = true },
                new Discipline { DisciplineId = 11, DisciplineCode = "D11", DisciplineName = "TAS", SortOrder = 11, IsActive = true },
                new Discipline { DisciplineId = 12, DisciplineCode = "D12", DisciplineName = "FI", SortOrder = 12, IsActive = true },
                new Discipline { DisciplineId = 13, DisciplineCode = "D13", DisciplineName = "PHD", SortOrder = 13, IsActive = true },
                new Discipline { DisciplineId = 14, DisciplineCode = "D14", DisciplineName = "OTS", SortOrder = 14, IsActive = true },
                new Discipline { DisciplineId = 15, DisciplineCode = "D15", DisciplineName = "APM", SortOrder = 15, IsActive = true },
                new Discipline { DisciplineId = 16, DisciplineCode = "D16", DisciplineName = "HC900", SortOrder = 16, IsActive = true },
                new Discipline { DisciplineId = 17, DisciplineCode = "D17", DisciplineName = "Analytics", SortOrder = 17, IsActive = true },
                new Discipline { DisciplineId = 18, DisciplineCode = "D18", DisciplineName = "Experion HS / LX", SortOrder = 18, IsActive = true },
                new Discipline { DisciplineId = 19, DisciplineCode = "D19", DisciplineName = "Rockwell, Siemens, ABB, Schneider, Yokogawa", SortOrder = 19, IsActive = true },
                new Discipline { DisciplineId = 20, DisciplineCode = "D20", DisciplineName = "Skid", SortOrder = 20, IsActive = true },
                new Discipline { DisciplineId = 21, DisciplineCode = "D21", DisciplineName = "ES-RPMO", SortOrder = 21, IsActive = true },
                new Discipline { DisciplineId = 22, DisciplineCode = "D22", DisciplineName = "e-Learning", SortOrder = 22, IsActive = true },
                new Discipline { DisciplineId = 23, DisciplineCode = "D23", DisciplineName = "Misc. LSS", SortOrder = 23, IsActive = true },
                new Discipline { DisciplineId = 24, DisciplineCode = "D24", DisciplineName = "Misc. HCP", SortOrder = 24, IsActive = true },
                new Discipline { DisciplineId = 25, DisciplineCode = "D25", DisciplineName = "TFMS", SortOrder = 25, IsActive = true },
                new Discipline { DisciplineId = 26, DisciplineCode = "D26", DisciplineName = "Other", SortOrder = 26, IsActive = true }
            );
        });

        // Resource
        modelBuilder.Entity<Resource>(entity =>
        {
            entity.HasKey(e => e.ResourceId);
            entity.Property(e => e.EngineerName).HasMaxLength(150);
            entity.Property(e => e.TotalExperienceYears).HasColumnType("decimal(5,2)");
            entity.Property(e => e.RelevantExperienceYears).HasColumnType("decimal(5,2)");
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.CurrentProjectName).HasMaxLength(200);
            entity.Property(e => e.ManagerName).HasMaxLength(150);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");

            entity.HasOne(e => e.Vendor)
                  .WithMany(v => v.Resources)
                  .HasForeignKey(e => e.VendorId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Discipline)
                  .WithMany(d => d.Resources)
                  .HasForeignKey(e => e.DisciplineId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.SkillLevel)
                  .WithMany(s => s.Resources)
                  .HasForeignKey(e => e.SkillLevelId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Group)
                  .WithMany(g => g.Resources)
                  .HasForeignKey(e => e.GroupId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
