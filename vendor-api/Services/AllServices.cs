using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using vendor_api.Models.DTOs;
using vendor_api.Models.Entities;
using vendor_api.Repositories;

namespace vendor_api.Services;

// ============== Interfaces ==============

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
}

public interface IUserService
{
    Task<PagedResponse<UserResponseDto>> GetAllAsync(PaginationParams paginationParams);
    Task<UserResponseDto?> GetByIdAsync(int id);
    Task<UserResponseDto> CreateAsync(CreateUserDto dto);
    Task<UserResponseDto?> UpdateAsync(int id, UpdateUserDto dto);
    Task<bool> SoftDeleteAsync(int id);
}

public interface IVendorService
{
    Task<IEnumerable<VendorResponseDto>> GetAllAsync(bool? activeOnly = null);
    Task<VendorResponseDto?> GetByIdAsync(int id);
    Task<VendorResponseDto> CreateAsync(CreateVendorDto dto);
    Task<VendorResponseDto?> UpdateAsync(int id, UpdateVendorDto dto);
    Task<bool> SoftDeleteAsync(int id);
}

public interface IResourceService
{
    Task<PagedResponse<ResourceResponseDto>> GetAllAsync(ResourceFilterParams filterParams, int? restrictVendorId = null);
    Task<ResourceResponseDto?> GetByIdAsync(int id, int? restrictVendorId = null);
    Task<ResourceResponseDto> CreateAsync(CreateResourceDto dto, int userId);
    Task<ResourceResponseDto?> UpdateAsync(int id, UpdateResourceDto dto, int userId, int? restrictVendorId = null);
    Task<bool> SoftDeleteAsync(int id, int userId, int? restrictVendorId = null);
}

public interface ISkillLevelService
{
    Task<IEnumerable<SkillLevelResponseDto>> GetAllAsync();
}

public interface IGroupService
{
    Task<IEnumerable<GroupResponseDto>> GetAllAsync(bool? activeOnly = null);
}

public interface IDisciplineService
{
    Task<IEnumerable<DisciplineResponseDto>> GetAllAsync(bool? activeOnly = null);
}

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync(int? vendorId = null);
}

// ============== Implementations ==============

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository userRepository, IConfiguration configuration, ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username);
        if (user == null)
        {
            _logger.LogWarning("Login attempt failed: user '{Username}' not found", request.Username);
            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login attempt failed: invalid password for user '{Username}'", request.Username);
            return null;
        }

        var token = GenerateJwtToken(user);
        var expiration = DateTime.UtcNow.AddMinutes(60);

        _logger.LogInformation("User '{Username}' logged in successfully", user.Username);

        return new LoginResponseDto
        {
            Token = token,
            Username = user.Username,
            Role = user.Role.Name,
            UserId = user.Id,
            VendorId = user.VendorId,
            VendorName = user.Vendor?.VendorName,
            Expiration = expiration
        };
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
        var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.Name),
            new Claim("VendorId", user.VendorId?.ToString() ?? ""),
            new Claim("VendorName", user.Vendor?.VendorName ?? "")
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(60),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<PagedResponse<UserResponseDto>> GetAllAsync(PaginationParams p)
    {
        var (users, totalCount) = await _userRepository.GetAllAsync(
            p.PageNumber, p.PageSize, p.SearchTerm, p.SortBy, p.SortOrder);

        return new PagedResponse<UserResponseDto>
        {
            Data = users.Select(MapToDto),
            PageNumber = p.PageNumber,
            PageSize = p.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<UserResponseDto?> GetByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user == null ? null : MapToDto(user);
    }

    public async Task<UserResponseDto> CreateAsync(CreateUserDto dto)
    {
        if (await _userRepository.UsernameExistsAsync(dto.Username))
            throw new InvalidOperationException("Username already exists");
        if (await _userRepository.EmailExistsAsync(dto.Email))
            throw new InvalidOperationException("Email already exists");

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            RoleId = dto.RoleId,
            VendorId = dto.VendorId,
            IsActive = true,
            Created = DateTime.UtcNow
        };

        var created = await _userRepository.CreateAsync(user);
        _logger.LogInformation("User '{Username}' created with Id {UserId}", created.Username, created.Id);

        // Reload with navigation properties
        var result = await _userRepository.GetByIdAsync(created.Id);
        return MapToDto(result!);
    }

    public async Task<UserResponseDto?> UpdateAsync(int id, UpdateUserDto dto)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return null;

        if (dto.Username != null)
        {
            if (await _userRepository.UsernameExistsAsync(dto.Username, id))
                throw new InvalidOperationException("Username already exists");
            user.Username = dto.Username;
        }
        if (dto.Email != null)
        {
            if (await _userRepository.EmailExistsAsync(dto.Email, id))
                throw new InvalidOperationException("Email already exists");
            user.Email = dto.Email;
        }
        if (dto.Password != null)
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        if (dto.RoleId.HasValue)
            user.RoleId = dto.RoleId.Value;
        if (dto.VendorId.HasValue)
            user.VendorId = dto.VendorId.Value;
        if (dto.IsActive.HasValue)
            user.IsActive = dto.IsActive.Value;

        user.Updated = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        var result = await _userRepository.GetByIdAsync(id);
        return MapToDto(result!);
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return false;

        user.IsActive = false;
        user.Updated = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);
        return true;
    }

    private static UserResponseDto MapToDto(User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Email = user.Email,
        Role = user.Role?.Name ?? "",
        RoleId = user.RoleId,
        VendorId = user.VendorId,
        VendorName = user.Vendor?.VendorName,
        IsActive = user.IsActive,
        Created = user.Created,
        Updated = user.Updated
    };
}

public class VendorService : IVendorService
{
    private readonly IVendorRepository _vendorRepository;
    private readonly IResourceRepository _resourceRepository;
    private readonly ILogger<VendorService> _logger;

    public VendorService(IVendorRepository vendorRepository, IResourceRepository resourceRepository, ILogger<VendorService> logger)
    {
        _vendorRepository = vendorRepository;
        _resourceRepository = resourceRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<VendorResponseDto>> GetAllAsync(bool? activeOnly = null)
    {
        var vendors = await _vendorRepository.GetAllAsync(activeOnly);
        var result = new List<VendorResponseDto>();
        foreach (var v in vendors)
        {
            var (resources, count) = await _resourceRepository.GetAllAsync(1, 1, null, null, "asc", vendorId: v.VendorId, isActive: true);
            result.Add(new VendorResponseDto
            {
                VendorId = v.VendorId,
                VendorName = v.VendorName,
                Created = v.Created,
                IsActive = v.IsActive,
                ResourceCount = count
            });
        }
        return result;
    }

    public async Task<VendorResponseDto?> GetByIdAsync(int id)
    {
        var vendor = await _vendorRepository.GetByIdAsync(id);
        if (vendor == null) return null;

        var (_, count) = await _resourceRepository.GetAllAsync(1, 1, null, null, "asc", vendorId: id, isActive: true);
        return new VendorResponseDto
        {
            VendorId = vendor.VendorId,
            VendorName = vendor.VendorName,
            Created = vendor.Created,
            IsActive = vendor.IsActive,
            ResourceCount = count
        };
    }

    public async Task<VendorResponseDto> CreateAsync(CreateVendorDto dto)
    {
        var vendor = new Vendor
        {
            VendorName = dto.VendorName,
            IsActive = true,
            Created = DateTime.UtcNow
        };
        var created = await _vendorRepository.CreateAsync(vendor);
        return new VendorResponseDto
        {
            VendorId = created.VendorId,
            VendorName = created.VendorName,
            Created = created.Created,
            IsActive = created.IsActive,
            ResourceCount = 0
        };
    }

    public async Task<VendorResponseDto?> UpdateAsync(int id, UpdateVendorDto dto)
    {
        var vendor = await _vendorRepository.GetByIdAsync(id);
        if (vendor == null) return null;

        if (dto.VendorName != null) vendor.VendorName = dto.VendorName;
        if (dto.IsActive.HasValue) vendor.IsActive = dto.IsActive.Value;

        await _vendorRepository.UpdateAsync(vendor);
        return await GetByIdAsync(id);
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        var vendor = await _vendorRepository.GetByIdAsync(id);
        if (vendor == null) return false;

        vendor.IsActive = false;
        await _vendorRepository.UpdateAsync(vendor);
        return true;
    }
}

public class ResourceService : IResourceService
{
    private readonly IResourceRepository _resourceRepository;
    private readonly IVendorRepository _vendorRepository;
    private readonly ISkillLevelRepository _skillLevelRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IDisciplineRepository _disciplineRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<ResourceService> _logger;

    public ResourceService(
        IResourceRepository resourceRepository,
        IVendorRepository vendorRepository,
        ISkillLevelRepository skillLevelRepository,
        IGroupRepository groupRepository,
        IDisciplineRepository disciplineRepository,
        IUserRepository userRepository,
        IEmailService emailService,
        ILogger<ResourceService> logger)
    {
        _resourceRepository = resourceRepository;
        _vendorRepository = vendorRepository;
        _skillLevelRepository = skillLevelRepository;
        _groupRepository = groupRepository;
        _disciplineRepository = disciplineRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<PagedResponse<ResourceResponseDto>> GetAllAsync(ResourceFilterParams p, int? restrictVendorId = null)
    {
        var effectiveVendorId = restrictVendorId ?? p.VendorId;

        var (resources, totalCount) = await _resourceRepository.GetAllAsync(
            p.PageNumber, p.PageSize, p.SearchTerm, p.SortBy, p.SortOrder,
            effectiveVendorId, p.GroupId, p.SkillLevelId, p.IsActive);

        return new PagedResponse<ResourceResponseDto>
        {
            Data = resources.Select(MapToDto),
            PageNumber = p.PageNumber,
            PageSize = p.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ResourceResponseDto?> GetByIdAsync(int id, int? restrictVendorId = null)
    {
        var resource = await _resourceRepository.GetByIdAsync(id);
        if (resource == null) return null;
        if (restrictVendorId.HasValue && resource.VendorId != restrictVendorId.Value) return null;
        return MapToDto(resource);
    }

    public async Task<ResourceResponseDto> CreateAsync(CreateResourceDto dto, int userId)
    {
        // Validate foreign keys
        if (!await _vendorRepository.ExistsAsync(dto.VendorId))
            throw new InvalidOperationException("Invalid VendorId");
        if (!await _skillLevelRepository.ExistsAsync(dto.SkillLevelId))
            throw new InvalidOperationException("Invalid SkillLevelId");
        if (!await _groupRepository.ExistsAsync(dto.GroupId))
            throw new InvalidOperationException("Invalid GroupId");
        if (dto.DisciplineId.HasValue && !await _disciplineRepository.ExistsAsync(dto.DisciplineId.Value))
            throw new InvalidOperationException("Invalid DisciplineId");

        var resource = new Resource
        {
            VendorId = dto.VendorId,
            DisciplineId = dto.DisciplineId,
            EngineerName = dto.EngineerName,
            AvailableFromDate = dto.AvailableFromDate,
            TotalExperienceYears = dto.TotalExperienceYears,
            RelevantExperienceYears = dto.RelevantExperienceYears,
            SkillLevelId = dto.SkillLevelId,
            Remarks = dto.Remarks,
            CurrentProjectName = dto.CurrentProjectName,
            ManagerName = dto.ManagerName,
            GroupId = dto.GroupId,
            IsActive = false,
            CreatedByUserId = userId,
            CreatedOn = DateTime.UtcNow
        };

        var created = await _resourceRepository.CreateAsync(resource);
        var resourceEntity = await _resourceRepository.GetByIdAsync(created.ResourceId);
        var result = MapToDto(resourceEntity!);

        // Send Email Notification if created by a vendor
        var creator = await _userRepository.GetByIdAsync(userId);
        if (creator != null && creator.Role.Name == "vendor")
        {
            var adminEmails = await _userRepository.GetAdminEmailsAsync();
            if (adminEmails.Any())
            {
                var subject = $"New Resource Created: {result.EngineerName} ({result.VendorName})";
                var body = $@"
                    <h3>New Resource Notification</h3>
                    <p>A new resource has been added to the system by <b>{creator.Username}</b> from <b>{result.VendorName}</b>.</p>
                    <ul>
                        <li><b>Engineer Name:</b> {result.EngineerName}</li>
                        <li><b>Discipline:</b> {result.DisciplineName}</li>
                        <li><b>Skill Level:</b> {result.SkillName} ({result.SkillCode})</li>
                        <li><b>Total Experience:</b> {result.TotalExperienceYears} years</li>
                        <li><b>Currently Inactive:</b> Yes (Awaiting activation)</li>
                    </ul>
                    <p>Please log in to the admin portal to review and activate this resource.</p>";

                var adminTo = string.Join(",", adminEmails);
                await _emailService.SendEmailAsync(adminTo, subject, body, creator.Email);
            }
        }

        return result;
    }

    public async Task<ResourceResponseDto?> UpdateAsync(int id, UpdateResourceDto dto, int userId, int? restrictVendorId = null)
    {
        var resource = await _resourceRepository.GetByIdAsync(id);
        if (resource == null) return null;
        if (restrictVendorId.HasValue && resource.VendorId != restrictVendorId.Value) return null;

        if (dto.VendorId.HasValue)
        {
            if (!await _vendorRepository.ExistsAsync(dto.VendorId.Value))
                throw new InvalidOperationException("Invalid VendorId");
            resource.VendorId = dto.VendorId.Value;
        }
        if (dto.SkillLevelId.HasValue)
        {
            if (!await _skillLevelRepository.ExistsAsync(dto.SkillLevelId.Value))
                throw new InvalidOperationException("Invalid SkillLevelId");
            resource.SkillLevelId = dto.SkillLevelId.Value;
        }
        if (dto.GroupId.HasValue)
        {
            if (!await _groupRepository.ExistsAsync(dto.GroupId.Value))
                throw new InvalidOperationException("Invalid GroupId");
            resource.GroupId = dto.GroupId.Value;
        }
        if (dto.DisciplineId.HasValue)
        {
            if (!await _disciplineRepository.ExistsAsync(dto.DisciplineId.Value))
                throw new InvalidOperationException("Invalid DisciplineId");
            resource.DisciplineId = dto.DisciplineId.Value;
        }

        if (dto.EngineerName != null) resource.EngineerName = dto.EngineerName;
        if (dto.AvailableFromDate.HasValue) resource.AvailableFromDate = dto.AvailableFromDate;
        if (dto.TotalExperienceYears.HasValue) resource.TotalExperienceYears = dto.TotalExperienceYears.Value;
        if (dto.RelevantExperienceYears.HasValue) resource.RelevantExperienceYears = dto.RelevantExperienceYears.Value;
        if (dto.Remarks != null) resource.Remarks = dto.Remarks;
        if (dto.CurrentProjectName != null) resource.CurrentProjectName = dto.CurrentProjectName;
        if (dto.ManagerName != null) resource.ManagerName = dto.ManagerName;
        if (dto.IsActive.HasValue) resource.IsActive = dto.IsActive.Value;

        resource.UpdatedByUserId = userId;
        resource.UpdatedOn = DateTime.UtcNow;

        await _resourceRepository.UpdateAsync(resource);
        var result = await _resourceRepository.GetByIdAsync(id);
        return MapToDto(result!);
    }

    public async Task<bool> SoftDeleteAsync(int id, int userId, int? restrictVendorId = null)
    {
        var resource = await _resourceRepository.GetByIdAsync(id);
        if (resource == null) return false;
        if (restrictVendorId.HasValue && resource.VendorId != restrictVendorId.Value) return false;

        resource.IsActive = false;
        resource.UpdatedByUserId = userId;
        resource.UpdatedOn = DateTime.UtcNow;
        await _resourceRepository.UpdateAsync(resource);
        return true;
    }

    private static ResourceResponseDto MapToDto(Resource r) => new()
    {
        ResourceId = r.ResourceId,
        VendorId = r.VendorId,
        VendorName = r.Vendor?.VendorName ?? "",
        DisciplineId = r.DisciplineId,
        DisciplineCode = r.Discipline?.DisciplineCode ?? "",
        DisciplineName = r.Discipline?.DisciplineName ?? "",
        EngineerName = r.EngineerName,
        AvailableFromDate = r.AvailableFromDate,
        TotalExperienceYears = r.TotalExperienceYears,
        RelevantExperienceYears = r.RelevantExperienceYears,
        SkillLevelId = r.SkillLevelId,
        SkillCode = r.SkillLevel?.SkillCode ?? "",
        SkillName = r.SkillLevel?.SkillName ?? "",
        Remarks = r.Remarks,
        CurrentProjectName = r.CurrentProjectName,
        ManagerName = r.ManagerName,
        GroupId = r.GroupId,
        GroupCode = r.Group?.GroupCode ?? "",
        GroupName = r.Group?.GroupName ?? "",
        IsActive = r.IsActive,
        CreatedByUserId = r.CreatedByUserId,
        CreatedOn = r.CreatedOn,
        UpdatedByUserId = r.UpdatedByUserId,
        UpdatedOn = r.UpdatedOn
    };
}

public class SkillLevelService : ISkillLevelService
{
    private readonly ISkillLevelRepository _repository;

    public SkillLevelService(ISkillLevelRepository repository) => _repository = repository;

    public async Task<IEnumerable<SkillLevelResponseDto>> GetAllAsync()
    {
        var skills = await _repository.GetAllAsync();
        return skills.Select(s => new SkillLevelResponseDto
        {
            SkillLevelId = s.SkillLevelId,
            SkillCode = s.SkillCode,
            SkillName = s.SkillName,
            RankOrder = s.RankOrder
        });
    }
}

public class GroupService : IGroupService
{
    private readonly IGroupRepository _repository;

    public GroupService(IGroupRepository repository) => _repository = repository;

    public async Task<IEnumerable<GroupResponseDto>> GetAllAsync(bool? activeOnly = null)
    {
        var groups = await _repository.GetAllAsync(activeOnly);
        return groups.Select(g => new GroupResponseDto
        {
            GroupId = g.GroupId,
            GroupCode = g.GroupCode,
            GroupName = g.GroupName,
            IsActive = g.IsActive
        });
    }
}

public class DisciplineService : IDisciplineService
{
    private readonly IDisciplineRepository _repository;

    public DisciplineService(IDisciplineRepository repository) => _repository = repository;

    public async Task<IEnumerable<DisciplineResponseDto>> GetAllAsync(bool? activeOnly = null)
    {
        var disciplines = await _repository.GetAllAsync(activeOnly);
        return disciplines.Select(d => new DisciplineResponseDto
        {
            DisciplineId = d.DisciplineId,
            DisciplineCode = d.DisciplineCode,
            DisciplineName = d.DisciplineName,
            SortOrder = d.SortOrder,
            IsActive = d.IsActive
        });
    }
}

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _repository;

    public DashboardService(IDashboardRepository repository) => _repository = repository;

    public async Task<DashboardStatsDto> GetStatsAsync(int? vendorId = null)
    {
        var totalResources = await _repository.GetTotalResourcesAsync(vendorId);
        var activeResources = await _repository.GetActiveResourcesAsync(vendorId);
        var totalVendors = await _repository.GetTotalVendorsAsync();
        var totalUsers = await _repository.GetTotalUsersAsync();
        var vendorWise = await _repository.GetVendorWiseCountAsync(vendorId);
        var groupWise = await _repository.GetGroupWiseCountAsync(vendorId);
        var skillWise = await _repository.GetSkillWiseCountAsync(vendorId);

        return new DashboardStatsDto
        {
            TotalResources = totalResources,
            ActiveResources = activeResources,
            TotalVendors = totalVendors,
            TotalUsers = totalUsers,
            VendorWiseCount = vendorWise.Select(x => new VendorResourceCount { VendorName = x.VendorName, Count = x.Count }),
            GroupWiseCount = groupWise.Select(x => new GroupResourceCount { GroupName = x.GroupName, Count = x.Count }),
            SkillWiseCount = skillWise.Select(x => new SkillResourceCount { SkillName = x.SkillName, Count = x.Count })
        };
    }
}
