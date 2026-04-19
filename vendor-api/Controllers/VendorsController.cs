using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vendor_api.Models.DTOs;
using vendor_api.Services;

namespace vendor_api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VendorsController : ControllerBase
{
    private readonly IVendorService _vendorService;

    public VendorsController(IVendorService vendorService)
    {
        _vendorService = vendorService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? activeOnly)
    {
        var result = await _vendorService.GetAllAsync(activeOnly);
        return Ok(ApiResponse<IEnumerable<VendorResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _vendorService.GetByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Vendor not found"));
        return Ok(ApiResponse<VendorResponseDto>.SuccessResponse(result));
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Create([FromBody] CreateVendorDto dto)
    {
        var result = await _vendorService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.VendorId },
            ApiResponse<VendorResponseDto>.SuccessResponse(result, "Vendor created successfully"));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateVendorDto dto)
    {
        var result = await _vendorService.UpdateAsync(id, dto);
        if (result == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Vendor not found"));
        return Ok(ApiResponse<VendorResponseDto>.SuccessResponse(result, "Vendor updated successfully"));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _vendorService.SoftDeleteAsync(id);
        if (!result)
            return NotFound(ApiResponse<object>.ErrorResponse("Vendor not found"));
        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Vendor deactivated successfully"));
    }
}
