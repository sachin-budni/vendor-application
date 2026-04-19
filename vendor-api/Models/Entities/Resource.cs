namespace vendor_api.Models.Entities;

public class Resource
{
    public int ResourceId { get; set; }
    public int VendorId { get; set; }
    public int? DisciplineId { get; set; }
    public string EngineerName { get; set; } = string.Empty;
    public DateTime? AvailableFromDate { get; set; }
    public decimal TotalExperienceYears { get; set; }
    public decimal RelevantExperienceYears { get; set; }
    public int SkillLevelId { get; set; }
    public string? Remarks { get; set; }
    public string? CurrentProjectName { get; set; }
    public string? ManagerName { get; set; }
    public int GroupId { get; set; }
    public bool IsActive { get; set; } = true;
    public int CreatedByUserId { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public int? UpdatedByUserId { get; set; }
    public DateTime? UpdatedOn { get; set; }

    // Navigation
    public Vendor Vendor { get; set; } = null!;
    public Discipline? Discipline { get; set; }
    public SkillLevel SkillLevel { get; set; } = null!;
    public Group Group { get; set; } = null!;
}
