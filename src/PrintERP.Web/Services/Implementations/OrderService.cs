using System.Security.Claims;
using PrintERP.Web.Models.ViewModels;
using PrintERP.Web.Services.Interfaces;

namespace PrintERP.Web.Services.Implementations;

public class OrderService(ICustomerService customerService) : IOrderService
{
    public async Task<OrderCreatePageViewModel> BuildCreatePageAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default)
    {
        var customers = await customerService.GetCustomersAsync(cancellationToken);
        var orderDate = DateTime.Today;
        var order = new OrderCreateViewModel
        {
            OrderCode = InMemoryOrderDataStore.NextOrderCode(),
            OrderDate = orderDate,
            Items =
            [
                new OrderItemCreateViewModel
                {
                    Quantity = 1,
                    Unit = "CÁI"
                }
            ]
        };

        var userRole = user.FindFirstValue(ClaimTypes.Role);
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.Equals(userRole, DashboardRoles.Sales, StringComparison.OrdinalIgnoreCase)
            && long.TryParse(userId, out var salesEmployeeId))
        {
            order.SalesEmployeeId = salesEmployeeId;
        }

        return new OrderCreatePageViewModel
        {
            Order = order,
            Customers = customers,
            Employees = InMemoryOrderDataStore.Employees,
            ProductCategories = InMemoryOrderDataStore.ProductCategories
        };
    }

    public Task<(bool Success, string? Error, long? OrderId)> CreateAsync(OrderCreateViewModel model, CancellationToken cancellationToken = default)
    {
        if (model.Items.Count == 0)
        {
            return Task.FromResult((Success: false, Error: (string?)"Đơn hàng phải có ít nhất 1 hạng mục", OrderId: (long?)null));
        }

        var customerExists = InMemoryOrderDataStore.Customers.Any(x => x.Id == model.CustomerId);
        if (!customerExists)
        {
            return Task.FromResult((Success: false, Error: (string?)"Vui lòng chọn khách hàng", OrderId: (long?)null));
        }

        var subtotal = model.Items.Sum(x => x.LineTotal);
        if (subtotal < 0)
        {
            return Task.FromResult((Success: false, Error: (string?)"Dữ liệu thành tiền không hợp lệ", OrderId: (long?)null));
        }

        if (model.DiscountAmount > subtotal)
        {
            return Task.FromResult((Success: false, Error: (string?)"Giảm giá không hợp lệ", OrderId: (long?)null));
        }

        model.SubtotalAmount = subtotal;
        model.TotalAmount = subtotal - model.DiscountAmount + model.TaxAmount;

        var orderId = InMemoryOrderDataStore.NextOrderId();
        InMemoryOrderDataStore.Orders.Add(new
        {
            OrderId = orderId,
            model.OrderCode,
            Status = "NEW",
            model.CustomerId,
            model.OrderDate,
            model.DeliveryDate,
            model.SalesEmployeeId,
            model.AssignedEmployeeId,
            model.SubtotalAmount,
            model.DiscountAmount,
            model.TaxAmount,
            model.TotalAmount,
            Items = model.Items.Count,
            CreatedAt = DateTime.UtcNow
        });

        return Task.FromResult((Success: true, Error: (string?)null, OrderId: (long?)orderId));
    }
}
