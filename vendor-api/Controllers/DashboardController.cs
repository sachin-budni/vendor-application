using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using vendor_api.Models.DTOs;
using vendor_api.Services;

namespace vendor_api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        int? vendorId = null;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        if (role == "vendor")
        {
            var vendorClaim = User.FindFirst("VendorId")?.Value;
            if (!string.IsNullOrEmpty(vendorClaim))
                vendorId = int.Parse(vendorClaim);
        }

        var result = await _dashboardService.GetStatsAsync(vendorId);
        return Ok(ApiResponse<DashboardStatsDto>.SuccessResponse(result));
    }
}
