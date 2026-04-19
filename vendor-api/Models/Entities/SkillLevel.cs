namespace vendor_api.Models.Entities;

public class SkillLevel
{
    public int SkillLevelId { get; set; }
    public string SkillCode { get; set; } = string.Empty;
    public string SkillName { get; set; } = string.Empty;
    public int RankOrder { get; set; }

    // Navigation
    public ICollection<Resource> Resources { get; set; } = new List<Resource>();
}
