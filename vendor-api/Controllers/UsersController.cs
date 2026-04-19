using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vendor_api.Models.DTOs;
using vendor_api.Services;

namespace vendor_api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams paginationParams)
    {
        var result = await _userService.GetAllAsync(paginationParams);
        return Ok(ApiResponse<PagedResponse<UserResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _userService.GetByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
        return Ok(ApiResponse<UserResponseDto>.SuccessResponse(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        var result = await _userService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<UserResponseDto>.SuccessResponse(result, "User created successfully"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
    {
        var result = await _userService.UpdateAsync(id, dto);
        if (result == null)
            return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
        return Ok(ApiResponse<UserResponseDto>.SuccessResponse(result, "User updated successfully"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _userService.SoftDeleteAsync(id);
        if (!result)
            return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
        return Ok(ApiResponse<object>.SuccessResponse(new { }, "User deactivated successfully"));
    }
}
