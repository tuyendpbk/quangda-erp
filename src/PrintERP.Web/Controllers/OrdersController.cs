using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrintERP.Web.Models.ViewModels;
using PrintERP.Web.Services.Interfaces;

namespace PrintERP.Web.Controllers;

[Authorize]
public class OrdersController(IOrderService orderService, IPricingService pricingService, ICustomerService customerService, ILogger<OrdersController> logger) : Controller
{

    [HttpGet]
    [Authorize(Policy = "OrderView")]
    public async Task<IActionResult> Index([FromQuery] OrderFilterViewModel filter, CancellationToken cancellationToken)
    {
        if (!TryValidateModel(filter))
        {
            TempData["ErrorMessage"] = "Điều kiện tìm kiếm không hợp lệ.";
        }

        var model = await orderService.GetListAsync(filter, User, cancellationToken);
        ViewBag.ErrorMessage = TempData["ErrorMessage"]?.ToString();
        return View(model);
    }
    [HttpGet]
    [Authorize(Policy = "OrderCreate")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = await orderService.BuildCreatePageAsync(User, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [Authorize(Policy = "OrderCreate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OrderCreatePageViewModel pageModel, string submitAction, CancellationToken cancellationToken)
    {
        pageModel.Customers = await customerService.GetCustomersAsync(cancellationToken);
        var createPageOptions = await orderService.BuildCreatePageAsync(User, cancellationToken);
        pageModel.Employees = createPageOptions.Employees;
        pageModel.ProductCategories = createPageOptions.ProductCategories;

        var model = pageModel.Order;

        if (!ModelState.IsValid)
        {
            return View(pageModel);
        }

        var result = await orderService.CreateAsync(model, cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Không thể lưu đơn hàng. Vui lòng thử lại.");
            return View(pageModel);
        }

        TempData["SuccessMessage"] = $"Tạo đơn {model.OrderCode} thành công.";

        if (string.Equals(submitAction, "save-new", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(Create));
        }

        return RedirectToAction(nameof(Details), new { id = result.OrderId });
    }

    [HttpPost]
    [Authorize(Policy = "OrderCreate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EstimateItem([FromBody] OrderItemEstimateRequest request, CancellationToken cancellationToken)
    {
        var result = await pricingService.EstimateItemAsync(request, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Json(result);
    }

    [HttpPost]
    [Authorize(Policy = "OrderCreate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickCreateCustomer([FromBody] CustomerQuickCreateViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
            return BadRequest(new { message = string.Join("; ", errors) });
        }

        try
        {
            var customer = await customerService.QuickCreateAsync(model, cancellationToken);
            return Json(customer);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Quick create customer failed at {NowUtc}", DateTime.UtcNow);
            return StatusCode(500, new { message = "Không thể tạo khách hàng. Vui lòng thử lại." });
        }
    }


    [HttpGet]
    [Authorize(Policy = "OrderStatusUpdate")]
    public async Task<IActionResult> UpdateStatus(long id, CancellationToken cancellationToken)
    {
        var model = await orderService.BuildStatusUpdateAsync(id, User, cancellationToken);
        if (model is null)
        {
            return NotFound("Không tìm thấy đơn hàng.");
        }

        return View(model);
    }

    [HttpPost]
    [Authorize(Policy = "OrderStatusUpdate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(OrderStatusUpdateViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var fallback = await orderService.BuildStatusUpdateAsync(model.OrderId, User, cancellationToken);
            if (fallback is null)
            {
                return NotFound("Không tìm thấy đơn hàng.");
            }

            model.OrderCode = fallback.OrderCode;
            model.CustomerName = fallback.CustomerName;
            model.OrderDate = fallback.OrderDate;
            model.DeliveryDate = fallback.DeliveryDate;
            model.CurrentStatus = fallback.CurrentStatus;
            model.AvailableStatuses = fallback.AvailableStatuses;
            model.HasPayments = fallback.HasPayments;
            model.HasStockOut = fallback.HasStockOut;
            model.IsLocked = fallback.IsLocked;

            return View(model);
        }

        var result = await orderService.UpdateStatusAsync(model, User, cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Không thể cập nhật trạng thái đơn hàng. Vui lòng thử lại sau.");

            var fallback = await orderService.BuildStatusUpdateAsync(model.OrderId, User, cancellationToken);
            if (fallback is not null)
            {
                model.OrderCode = fallback.OrderCode;
                model.CustomerName = fallback.CustomerName;
                model.OrderDate = fallback.OrderDate;
                model.DeliveryDate = fallback.DeliveryDate;
                model.CurrentStatus = fallback.CurrentStatus;
                model.AvailableStatuses = fallback.AvailableStatuses;
                model.HasPayments = fallback.HasPayments;
                model.HasStockOut = fallback.HasStockOut;
                model.IsLocked = fallback.IsLocked;
            }

            return View(model);
        }

        TempData["SuccessMessage"] = "Cập nhật trạng thái đơn hàng thành công.";
        return RedirectToAction(nameof(Details), new { id = model.OrderId });
    }

    [HttpGet]
    [Authorize(Policy = "OrderView")]
    public async Task<IActionResult> Details(long id, CancellationToken cancellationToken)
    {
        var model = await orderService.GetDetailAsync(id, User, cancellationToken);
        if (model is null)
        {
            return NotFound("Không tìm thấy đơn hàng.");
        }

        if (model.RemainingAmount < 0)
        {
            logger.LogWarning("Invalid payment summary for order {OrderId}", id);
            return BadRequest("Dữ liệu thanh toán không hợp lệ");
        }

        ViewBag.SuccessMessage = TempData["SuccessMessage"]?.ToString();
        return View(model);
    }

    [HttpGet]
    [Authorize(Policy = "OrderView")]
    public Task<IActionResult> Detail(long id, CancellationToken cancellationToken) => Details(id, cancellationToken);
}
