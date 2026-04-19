namespace vendor_api.Models.Entities;

public class Group
{
    public int GroupId { get; set; }
    public string GroupCode { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<Resource> Resources { get; set; } = new List<Resource>();
}
