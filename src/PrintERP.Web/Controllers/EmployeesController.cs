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
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = await employeeService.BuildCreatePageAsync(cancellationToken: cancellationToken);
        return View(model);
    }

    [HttpPost]
    [Authorize(Policy = "EmployeeCreate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] EmployeeCreatePageViewModel pageModel, [FromForm] string submitAction, CancellationToken cancellationToken)
    {
        if (!TryValidateModel(pageModel.Employee, nameof(pageModel.Employee)))
        {
            var invalidModel = await employeeService.BuildCreatePageAsync(pageModel.Employee, cancellationToken);
            return View(invalidModel);
        }

        var result = await employeeService.CreateAsync(pageModel.Employee, User, cancellationToken);
        if (!result.Success)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError($"{nameof(pageModel.Employee)}.{error.Key}", error.Value);
            }

            var errorModel = await employeeService.BuildCreatePageAsync(pageModel.Employee, cancellationToken);
            return View(errorModel);
        }

        TempData["SuccessMessage"] = "Tạo nhân viên thành công.";
        if (string.Equals(submitAction, "save-new", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(Create));
        }

        return RedirectToAction(nameof(Details), new { id = result.EmployeeId });
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
