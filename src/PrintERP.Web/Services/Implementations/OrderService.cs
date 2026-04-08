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

        var customer = InMemoryOrderDataStore.Customers.FirstOrDefault(x => x.Id == model.CustomerId);
        if (customer is null)
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
        var salesEmployee = InMemoryOrderDataStore.Employees.FirstOrDefault(x => x.Id == model.SalesEmployeeId)?.Name ?? "-";
        var assignedEmployee = model.AssignedEmployeeId.HasValue
            ? InMemoryOrderDataStore.Employees.FirstOrDefault(x => x.Id == model.AssignedEmployeeId.Value)?.Name
            : null;

        var detail = new OrderDetailViewModel
        {
            Id = orderId,
            OrderCode = model.OrderCode,
            OrderDate = model.OrderDate,
            DeliveryDate = model.DeliveryDate,
            Status = "NEW",
            PaymentStatus = "UNPAID",
            CustomerId = customer.Id,
            CustomerName = customer.Name,
            CustomerPhone = customer.Phone,
            CustomerEmail = customer.Email,
            CustomerAddress = customer.Address,
            SalesEmployeeName = salesEmployee,
            AssignedEmployeeName = assignedEmployee,
            SubtotalAmount = model.SubtotalAmount,
            DiscountAmount = model.DiscountAmount,
            TaxAmount = model.TaxAmount,
            TotalAmount = model.TotalAmount,
            PaidAmount = 0,
            RemainingAmount = model.TotalAmount,
            Note = model.Note,
            Items = model.Items.Select((item, index) => new OrderDetailItemViewModel
            {
                Id = index + 1,
                ProductCategoryName = InMemoryOrderDataStore.ProductCategories.FirstOrDefault(x => x.Id == item.ProductCategoryId)?.Name,
                ItemName = item.ItemName,
                Description = item.Description,
                Width = item.Width,
                Height = item.Height,
                Unit = item.Unit,
                Quantity = item.Quantity,
                Area = item.Area,
                MaterialDescription = item.MaterialDescription,
                PrintType = item.PrintType,
                FinishingDescription = item.FinishingDescription,
                EstimatedUnitPrice = item.EstimatedUnitPrice,
                EstimatedLineTotal = item.EstimatedLineTotal,
                EstimatedCost = item.EstimatedCost,
                EstimatedProfit = item.EstimatedProfit,
                UnitPrice = item.UnitPrice,
                LineTotal = item.LineTotal,
                PricingNote = item.PricingNote,
                Note = item.Note
            }).ToList(),
            StatusHistories =
            [
                new OrderStatusHistoryViewModel
                {
                    ChangedAt = DateTime.Now,
                    NewStatus = "NEW",
                    ChangedBy = salesEmployee,
                    Note = "Tạo đơn hàng"
                }
            ]
        };

        InMemoryOrderDataStore.AddOrder(detail);

        return Task.FromResult((Success: true, Error: (string?)null, OrderId: (long?)orderId));
    }

    public Task<OrderDetailViewModel?> GetDetailAsync(long id, ClaimsPrincipal user, CancellationToken cancellationToken = default)
    {
        var order = InMemoryOrderDataStore.Orders.FirstOrDefault(x => x.Id == id);
        if (order is null)
        {
            return Task.FromResult<OrderDetailViewModel?>(null);
        }

        order.PaidAmount = order.Payments.Sum(x => x.Amount);
        order.RemainingAmount = Math.Max(0, order.TotalAmount - order.PaidAmount);
        order.PaymentStatus = order.RemainingAmount <= 0 ? "PAID" : order.PaidAmount > 0 ? "PARTIAL" : "UNPAID";

        var hasPermission = (string permission) => user.Claims.Any(c => c.Type == "Permission" && c.Value == permission);
        var isLocked = string.Equals(order.Status, "DELIVERED", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(order.Status, "CANCELLED", StringComparison.OrdinalIgnoreCase);

        order.CanEdit = hasPermission(OrderPermissions.Edit) && !isLocked;
        order.CanUpdateStatus = hasPermission(OrderPermissions.StatusUpdate) && !string.Equals(order.Status, "CANCELLED", StringComparison.OrdinalIgnoreCase);
        order.CanRecordPayment = hasPermission(PaymentPermissions.Create) && order.RemainingAmount > 0 && !string.Equals(order.Status, "CANCELLED", StringComparison.OrdinalIgnoreCase);
        order.CanCreateStockOut = hasPermission(StockOutPermissions.Create) && !string.Equals(order.Status, "CANCELLED", StringComparison.OrdinalIgnoreCase);
        order.CanExportPdf = hasPermission(OrderPermissions.View);

        order.Payments = order.Payments.OrderByDescending(x => x.PaymentDate).ToList();

        return Task.FromResult<OrderDetailViewModel?>(order);
    }
}
