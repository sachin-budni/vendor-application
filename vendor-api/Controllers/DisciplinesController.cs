using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vendor_api.Models.DTOs;
using vendor_api.Services;

namespace vendor_api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DisciplinesController : ControllerBase
{
    private readonly IDisciplineService _disciplineService;

    public DisciplinesController(IDisciplineService disciplineService)
    {
        _disciplineService = disciplineService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? activeOnly)
    {
        var result = await _disciplineService.GetAllAsync(activeOnly);
        return Ok(ApiResponse<IEnumerable<DisciplineResponseDto>>.SuccessResponse(result));
    }
}
