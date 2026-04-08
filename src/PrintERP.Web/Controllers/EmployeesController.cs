using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrintERP.Web.Models.ViewModels;
using PrintERP.Web.Services.Interfaces;

namespace PrintERP.Web.Controllers;

[Authorize]
public class EmployeesController(IEmployeeService employeeService) : Controller
{
    [HttpGet]
    [Authorize(Policy = "EmployeeView")]
    public async Task<IActionResult> Index([FromQuery] EmployeeFilterViewModel filter, CancellationToken cancellationToken)
    {
        if (!TryValidateModel(filter))
        {
            TempData["ErrorMessage"] = "Điều kiện tìm kiếm không hợp lệ.";
        }

        var model = await employeeService.GetListAsync(filter, User, cancellationToken);
        ViewBag.ErrorMessage = TempData["ErrorMessage"]?.ToString();
        ViewBag.SuccessMessage = TempData["SuccessMessage"]?.ToString();
        return View(model);
    }

    [HttpGet]
    [Authorize(Policy = "EmployeeView")]
    public IActionResult Details(long id)
    {
        TempData["SuccessMessage"] = $"Điều hướng tới chi tiết nhân viên #{id} (placeholder).";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Policy = "EmployeeEdit")]
    public IActionResult Edit(long id)
    {
        TempData["SuccessMessage"] = $"Điều hướng tới sửa nhân viên #{id} (placeholder).";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Policy = "EmployeeCreate")]
    public IActionResult Create()
    {
        TempData["SuccessMessage"] = "Điều hướng tới thêm mới nhân viên (placeholder).";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Policy = "EmployeeEdit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(long id, [FromForm] EmployeeFilterViewModel filter, CancellationToken cancellationToken)
    {
        var result = await employeeService.ToggleStatusAsync(id, User, cancellationToken);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Error ?? "Không thể khóa tài khoản này.";
            return RedirectToAction(nameof(Index), filter);
        }

        TempData["SuccessMessage"] = "Cập nhật trạng thái nhân viên thành công.";
        return RedirectToAction(nameof(Index), filter);
    }
}
