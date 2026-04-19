using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using vendor_api.Models.DTOs;
using vendor_api.Services;

namespace vendor_api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ResourcesController : ControllerBase
{
    private readonly IResourceService _resourceService;
    private readonly IVendorService _vendorService;
    private readonly IDisciplineService _disciplineService;

    public ResourcesController(IResourceService resourceService, IVendorService vendorService, IDisciplineService disciplineService)
    {
        _resourceService = resourceService;
        _vendorService = vendorService;
        _disciplineService = disciplineService;
    }

    private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    private string GetRole() => User.FindFirst(ClaimTypes.Role)!.Value;
    private int? GetVendorId()
    {
        var vendorClaim = User.FindFirst("VendorId")?.Value;
        return string.IsNullOrEmpty(vendorClaim) ? null : int.Parse(vendorClaim);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ResourceFilterParams filterParams)
    {
        var restrictVendorId = GetRole() == "vendor" ? GetVendorId() : null;
        var result = await _resourceService.GetAllAsync(filterParams, restrictVendorId);
        return Ok(ApiResponse<PagedResponse<ResourceResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportExcel([FromQuery] ResourceFilterParams filterParams)
    {
        var restrictVendorId = GetRole() == "vendor" ? GetVendorId() : null;
        var pagedResponse = await _resourceService.GetAllAsync(filterParams, restrictVendorId);

        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Resources");

        var currentRow = 1;
        worksheet.Cell(currentRow, 1).Value = "Vendor Name";
        worksheet.Cell(currentRow, 2).Value = "Disc Name";
        worksheet.Cell(currentRow, 3).Value = "Engineer Name";
        worksheet.Cell(currentRow, 4).Value = "Avail. From";
        worksheet.Cell(currentRow, 5).Value = "Total Exp(Y)";
        worksheet.Cell(currentRow, 6).Value = "Rel Exp(Y)";
        worksheet.Cell(currentRow, 7).Value = "Skill Name";
        worksheet.Cell(currentRow, 8).Value = "Remarks";
        worksheet.Cell(currentRow, 9).Value = "Current Proj";
        worksheet.Cell(currentRow, 10).Value = "Manager Name";
        worksheet.Cell(currentRow, 11).Value = "Group Name";

        var headerRange = worksheet.Range(1, 1, 1, 11);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;

        foreach (var res in pagedResponse.Data)
        {
            currentRow++;
            worksheet.Cell(currentRow, 1).Value = res.VendorName;
            worksheet.Cell(currentRow, 2).Value = res.DisciplineName;
            worksheet.Cell(currentRow, 3).Value = res.EngineerName;
            worksheet.Cell(currentRow, 4).Value = res.AvailableFromDate;
            worksheet.Cell(currentRow, 5).Value = res.TotalExperienceYears;
            worksheet.Cell(currentRow, 6).Value = res.RelevantExperienceYears;
            worksheet.Cell(currentRow, 7).Value = res.SkillName;
            worksheet.Cell(currentRow, 8).Value = res.Remarks;
            worksheet.Cell(currentRow, 9).Value = res.CurrentProjectName;
            worksheet.Cell(currentRow, 10).Value = res.ManagerName;
            worksheet.Cell(currentRow, 11).Value = res.GroupName;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new System.IO.MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();

        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Resources.xlsx");
    }

    [HttpGet("export-headcount")]
    public async Task<IActionResult> ExportHeadCountExcel([FromQuery] ResourceFilterParams filterParams)
    {
        var allDisciplines = await _disciplineService.GetAllAsync();
        var disciplines = allDisciplines
            .Where(r => !string.IsNullOrEmpty(r.DisciplineName))
            .Select(r => r.DisciplineName)
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        var restrictVendorId = GetRole() == "vendor" ? GetVendorId() : null;
        var pagedResponse = await _resourceService.GetAllAsync(filterParams, restrictVendorId);
        var resources = pagedResponse.Data;

        var allVendors = await _vendorService.GetAllAsync();
        if (restrictVendorId.HasValue) 
        {
            allVendors = allVendors.Where(v => v.VendorId == restrictVendorId.Value);
        }
        var vendorGroups = allVendors.OrderBy(v => v.VendorName).ToList();

        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Head Count");

        var currentRow = 1;
        worksheet.Cell(currentRow, 1).Value = "Decipline";
        worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
        worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.Yellow;

        for (int i = 0; i < disciplines.Count; i++)
        {
            worksheet.Cell(currentRow, i + 2).Value = "";
            worksheet.Cell(currentRow, i + 2).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.Orange;
        }
        worksheet.Cell(currentRow, disciplines.Count + 2).Value = "Total (All)";
        worksheet.Cell(currentRow, disciplines.Count + 2).Style.Font.Bold = true;
        worksheet.Cell(currentRow, disciplines.Count + 2).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.Yellow;

        currentRow++;
        worksheet.Cell(currentRow, 1).Value = "Contractors / Disciplines";
        worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
        for (int i = 0; i < disciplines.Count; i++)
        {
            worksheet.Cell(currentRow, i + 2).Value = disciplines[i];
            worksheet.Cell(currentRow, i + 2).Style.Font.Bold = true;
        }

        int grandTotal = 0;
        var disciplineTotals = new int[disciplines.Count];

        foreach (var vendor in vendorGroups)
        {
            currentRow++;
            worksheet.Cell(currentRow, 1).Value = vendor.VendorName;
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            
            int vendorTotal = 0;
            for (int i = 0; i < disciplines.Count; i++)
            {
                var count = resources.Count(r => r.VendorId == vendor.VendorId && r.DisciplineName == disciplines[i]);
                if (count > 0)
                {
                    worksheet.Cell(currentRow, i + 2).Value = count;
                }
                vendorTotal += count;
                disciplineTotals[i] += count;
            }
            worksheet.Cell(currentRow, disciplines.Count + 2).Value = vendorTotal;
            worksheet.Cell(currentRow, disciplines.Count + 2).Style.Font.Bold = true;
            worksheet.Cell(currentRow, disciplines.Count + 2).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightYellow;
            grandTotal += vendorTotal;
        }

        currentRow++;
        worksheet.Cell(currentRow, 1).Value = "Total Headcount";
        worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
        
        for (int i = 0; i < disciplines.Count; i++)
        {
            worksheet.Cell(currentRow, i + 2).Value = disciplineTotals[i];
            worksheet.Cell(currentRow, i + 2).Style.Font.Bold = true;
        }
        worksheet.Cell(currentRow, disciplines.Count + 2).Value = grandTotal;
        worksheet.Cell(currentRow, disciplines.Count + 2).Style.Font.Bold = true;
        worksheet.Cell(currentRow, disciplines.Count + 2).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.Yellow;

        worksheet.Columns().AdjustToContents();

        using var stream = new System.IO.MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();

        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "HeadCount.xlsx");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var restrictVendorId = GetRole() == "vendor" ? GetVendorId() : null;
        var result = await _resourceService.GetByIdAsync(id, restrictVendorId);
        if (result == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Resource not found"));
        return Ok(ApiResponse<ResourceResponseDto>.SuccessResponse(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateResourceDto dto)
    {
        var role = GetRole();
        var vendorId = GetVendorId();

        // Vendor users can only create resources for their own vendor
        if (role == "vendor" && vendorId.HasValue && dto.VendorId != vendorId.Value)
            return Forbid();

        var result = await _resourceService.CreateAsync(dto, GetUserId());
        return CreatedAtAction(nameof(GetById), new { id = result.ResourceId },
            ApiResponse<ResourceResponseDto>.SuccessResponse(result, "Resource created successfully"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateResourceDto dto)
    {
        var restrictVendorId = GetRole() == "vendor" ? GetVendorId() : null;
        var result = await _resourceService.UpdateAsync(id, dto, GetUserId(), restrictVendorId);
        if (result == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Resource not found or access denied"));
        return Ok(ApiResponse<ResourceResponseDto>.SuccessResponse(result, "Resource updated successfully"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var restrictVendorId = GetRole() == "vendor" ? GetVendorId() : null;
        var result = await _resourceService.SoftDeleteAsync(id, GetUserId(), restrictVendorId);
        if (!result)
            return NotFound(ApiResponse<object>.ErrorResponse("Resource not found or access denied"));
        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Resource deactivated successfully"));
    }
}
