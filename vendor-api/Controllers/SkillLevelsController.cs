using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vendor_api.Models.DTOs;
using vendor_api.Services;

namespace vendor_api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SkillLevelsController : ControllerBase
{
    private readonly ISkillLevelService _skillLevelService;

    public SkillLevelsController(ISkillLevelService skillLevelService)
    {
        _skillLevelService = skillLevelService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _skillLevelService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<SkillLevelResponseDto>>.SuccessResponse(result));
    }
}
