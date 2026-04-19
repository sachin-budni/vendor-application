using System.ComponentModel.DataAnnotations;

namespace vendor_api.Models.DTOs;

// ============== Auth DTOs ==============

public class LoginRequestDto
{
    [Required(ErrorMessage = "Username is required")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int UserId { get; set; }
    public int? VendorId { get; set; }
    public string? VendorName { get; set; }
    public DateTime Expiration { get; set; }
}

// ============== User DTOs ==============

public class UserResponseDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public int? VendorId { get; set; }
    public string? VendorName { get; set; }
    public bool IsActive { get; set; }
    public DateTime Created { get; set; }
    public DateTime? Updated { get; set; }
}

public class CreateUserDto
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(100, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "RoleId is required")]
    public int RoleId { get; set; }

    public int? VendorId { get; set; }
}

public class UpdateUserDto
{
    [StringLength(100, MinimumLength = 3)]
    public string? Username { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(150)]
    public string? Email { get; set; }

    [StringLength(100, MinimumLength = 6)]
    public string? Password { get; set; }

    public int? RoleId { get; set; }
    public int? VendorId { get; set; }
    public bool? IsActive { get; set; }
}

// ============== Vendor DTOs ==============

public class VendorResponseDto
{
    public int VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public bool IsActive { get; set; }
    public int ResourceCount { get; set; }
}

public class CreateVendorDto
{
    [Required(ErrorMessage = "Vendor name is required")]
    [StringLength(100, MinimumLength = 2)]
    public string VendorName { get; set; } = string.Empty;
}

public class UpdateVendorDto
{
    [StringLength(100, MinimumLength = 2)]
    public string? VendorName { get; set; }
    public bool? IsActive { get; set; }
}

// ============== Resource DTOs ==============

public class ResourceResponseDto
{
    public int ResourceId { get; set; }
    public int VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public int? DisciplineId { get; set; }
    public string? DisciplineCode { get; set; }
    public string? DisciplineName { get; set; }
    public string EngineerName { get; set; } = string.Empty;
    public DateTime? AvailableFromDate { get; set; }
    public decimal TotalExperienceYears { get; set; }
    public decimal RelevantExperienceYears { get; set; }
    public int SkillLevelId { get; set; }
    public string SkillCode { get; set; } = string.Empty;
    public string SkillName { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public string? CurrentProjectName { get; set; }
    public string? ManagerName { get; set; }
    public int GroupId { get; set; }
    public string GroupCode { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime CreatedOn { get; set; }
    public int? UpdatedByUserId { get; set; }
    public DateTime? UpdatedOn { get; set; }
}

public class CreateResourceDto
{
    [Required]
    public int VendorId { get; set; }

    public int? DisciplineId { get; set; }

    [Required(ErrorMessage = "Engineer name is required")]
    [StringLength(150, MinimumLength = 2)]
    public string EngineerName { get; set; } = string.Empty;

    public DateTime? AvailableFromDate { get; set; }

    [Range(0, 99.99)]
    public decimal TotalExperienceYears { get; set; }

    [Range(0, 99.99)]
    public decimal RelevantExperienceYears { get; set; }

    [Required]
    public int SkillLevelId { get; set; }

    [StringLength(500)]
    public string? Remarks { get; set; }

    [StringLength(200)]
    public string? CurrentProjectName { get; set; }

    [StringLength(150)]
    public string? ManagerName { get; set; }

    [Required]
    public int GroupId { get; set; }
}

public class UpdateResourceDto
{
    public int? VendorId { get; set; }
    public int? DisciplineId { get; set; }

    [StringLength(150, MinimumLength = 2)]
    public string? EngineerName { get; set; }

    public DateTime? AvailableFromDate { get; set; }

    [Range(0, 99.99)]
    public decimal? TotalExperienceYears { get; set; }

    [Range(0, 99.99)]
    public decimal? RelevantExperienceYears { get; set; }

    public int? SkillLevelId { get; set; }

    [StringLength(500)]
    public string? Remarks { get; set; }

    [StringLength(200)]
    public string? CurrentProjectName { get; set; }

    [StringLength(150)]
    public string? ManagerName { get; set; }

    public int? GroupId { get; set; }
    public bool? IsActive { get; set; }
}

// ============== SkillLevel DTOs ==============

public class SkillLevelResponseDto
{
    public int SkillLevelId { get; set; }
    public string SkillCode { get; set; } = string.Empty;
    public string SkillName { get; set; } = string.Empty;
    public int RankOrder { get; set; }
}

// ============== Group DTOs ==============

public class GroupResponseDto
{
    public int GroupId { get; set; }
    public string GroupCode { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

// ============== Discipline DTOs ==============

public class DisciplineResponseDto
{
    public int DisciplineId { get; set; }
    public string DisciplineCode { get; set; } = string.Empty;
    public string DisciplineName { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

// ============== Pagination ==============

public class PaginationParams
{
    private int _pageSize = 10;
    private const int MaxPageSize = 100;

    public int PageNumber { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value < 1 ? 1 : value;
    }

    public string? SortBy { get; set; }
    public string SortOrder { get; set; } = "asc";
    public string? SearchTerm { get; set; }
}

public class ResourceFilterParams : PaginationParams
{
    public int? VendorId { get; set; }
    public int? GroupId { get; set; }
    public int? SkillLevelId { get; set; }
    public int? DisciplineId { get; set; }
    public bool? IsActive { get; set; }
}

public class PagedResponse<T>
{
    public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public IEnumerable<string>? Errors { get; set; }

    public static ApiResponse<T> SuccessResponse(T data, string message = "Success")
    {
        return new ApiResponse<T> { Success = true, Message = message, Data = data };
    }

    public static ApiResponse<T> ErrorResponse(string message, IEnumerable<string>? errors = null)
    {
        return new ApiResponse<T> { Success = false, Message = message, Errors = errors };
    }
}

// ============== Dashboard DTOs ==============

public class DashboardStatsDto
{
    public int TotalResources { get; set; }
    public int ActiveResources { get; set; }
    public int TotalVendors { get; set; }
    public int TotalUsers { get; set; }
    public IEnumerable<VendorResourceCount> VendorWiseCount { get; set; } = Enumerable.Empty<VendorResourceCount>();
    public IEnumerable<GroupResourceCount> GroupWiseCount { get; set; } = Enumerable.Empty<GroupResourceCount>();
    public IEnumerable<SkillResourceCount> SkillWiseCount { get; set; } = Enumerable.Empty<SkillResourceCount>();
}

public class VendorResourceCount
{
    public string VendorName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class GroupResourceCount
{
    public string GroupName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class SkillResourceCount
{
    public string SkillName { get; set; } = string.Empty;
    public int Count { get; set; }
}
