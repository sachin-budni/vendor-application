namespace vendor_api.Models.Entities;

public class Discipline
{
    public int DisciplineId { get; set; }
    public string DisciplineCode { get; set; } = string.Empty;
    public string DisciplineName { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<Resource> Resources { get; set; } = new List<Resource>();
}
