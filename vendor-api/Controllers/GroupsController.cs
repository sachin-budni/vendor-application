using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vendor_api.Models.DTOs;
using vendor_api.Services;

namespace vendor_api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GroupsController : ControllerBase
{
    private readonly IGroupService _groupService;

    public GroupsController(IGroupService groupService)
    {
        _groupService = groupService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? activeOnly)
    {
        var result = await _groupService.GetAllAsync(activeOnly);
        return Ok(ApiResponse<IEnumerable<GroupResponseDto>>.SuccessResponse(result));
    }
}
