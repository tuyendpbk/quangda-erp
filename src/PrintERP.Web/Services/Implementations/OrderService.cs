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


    public Task<OrderListViewModel> GetListAsync(OrderFilterViewModel filter, ClaimsPrincipal user, CancellationToken cancellationToken = default)
    {
        var normalizedFilter = NormalizeFilter(filter);
        var today = DateTime.Today;
        const int nearDueDays = 3;

        var hasPermission = (string permission) => user.Claims.Any(c => c.Type == "Permission" && c.Value == permission);

        IEnumerable<OrderDetailViewModel> query = InMemoryOrderDataStore.Orders;

        if (!string.IsNullOrWhiteSpace(normalizedFilter.Keyword))
        {
            var keyword = normalizedFilter.Keyword.Trim();
            query = query.Where(x =>
                x.OrderCode.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || x.CustomerName.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || (x.CustomerPhone?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false)
                || x.Items.Any(i => i.ItemName.Contains(keyword, StringComparison.OrdinalIgnoreCase)));
        }

        if (!string.IsNullOrWhiteSpace(normalizedFilter.Status))
        {
            query = query.Where(x => string.Equals(x.Status, normalizedFilter.Status, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(normalizedFilter.PaymentStatus))
        {
            query = query.Where(x => string.Equals(x.PaymentStatus, normalizedFilter.PaymentStatus, StringComparison.OrdinalIgnoreCase));
        }

        if (normalizedFilter.CustomerId.HasValue)
        {
            query = query.Where(x => x.CustomerId == normalizedFilter.CustomerId.Value);
        }

        if (normalizedFilter.SalesEmployeeId.HasValue)
        {
            var salesEmployeeName = InMemoryOrderDataStore.Employees.FirstOrDefault(x => x.Id == normalizedFilter.SalesEmployeeId.Value)?.Name;
            if (!string.IsNullOrWhiteSpace(salesEmployeeName))
            {
                query = query.Where(x => string.Equals(x.SalesEmployeeName, salesEmployeeName, StringComparison.OrdinalIgnoreCase));
            }
        }

        if (normalizedFilter.AssignedEmployeeId.HasValue)
        {
            var assignedEmployeeName = InMemoryOrderDataStore.Employees.FirstOrDefault(x => x.Id == normalizedFilter.AssignedEmployeeId.Value)?.Name;
            if (!string.IsNullOrWhiteSpace(assignedEmployeeName))
            {
                query = query.Where(x => string.Equals(x.AssignedEmployeeName, assignedEmployeeName, StringComparison.OrdinalIgnoreCase));
            }
        }

        if (normalizedFilter.OrderDateFrom.HasValue)
        {
            query = query.Where(x => x.OrderDate.Date >= normalizedFilter.OrderDateFrom.Value.Date);
        }

        if (normalizedFilter.OrderDateTo.HasValue)
        {
            query = query.Where(x => x.OrderDate.Date <= normalizedFilter.OrderDateTo.Value.Date);
        }

        if (normalizedFilter.DeliveryDateFrom.HasValue)
        {
            query = query.Where(x => x.DeliveryDate.HasValue && x.DeliveryDate.Value.Date >= normalizedFilter.DeliveryDateFrom.Value.Date);
        }

        if (normalizedFilter.DeliveryDateTo.HasValue)
        {
            query = query.Where(x => x.DeliveryDate.HasValue && x.DeliveryDate.Value.Date <= normalizedFilter.DeliveryDateTo.Value.Date);
        }

        if (normalizedFilter.IsOverdueOnly)
        {
            query = query.Where(x => x.DeliveryDate.HasValue
                                     && x.DeliveryDate.Value.Date < today
                                     && !string.Equals(x.Status, "DELIVERED", StringComparison.OrdinalIgnoreCase)
                                     && !string.Equals(x.Status, "CANCELLED", StringComparison.OrdinalIgnoreCase));
        }

        if (normalizedFilter.IsNearDueOnly)
        {
            var nearDueDate = today.AddDays(nearDueDays);
            query = query.Where(x => x.DeliveryDate.HasValue
                                     && x.DeliveryDate.Value.Date >= today
                                     && x.DeliveryDate.Value.Date <= nearDueDate
                                     && !string.Equals(x.Status, "DELIVERED", StringComparison.OrdinalIgnoreCase)
                                     && !string.Equals(x.Status, "CANCELLED", StringComparison.OrdinalIgnoreCase));
        }

        var projected = query
            .Select(x =>
            {
                var paidAmount = x.Payments.Sum(p => p.Amount);
                var remainingAmount = Math.Max(0, x.TotalAmount - paidAmount);
                var paymentStatus = remainingAmount <= 0 ? "PAID" : paidAmount > 0 ? "PARTIAL" : "UNPAID";
                var isCancelled = string.Equals(x.Status, "CANCELLED", StringComparison.OrdinalIgnoreCase);
                var isDelivered = string.Equals(x.Status, "DELIVERED", StringComparison.OrdinalIgnoreCase);
                var isOverdue = x.DeliveryDate.HasValue
                                && x.DeliveryDate.Value.Date < today
                                && !isDelivered
                                && !isCancelled;
                var isNearDue = x.DeliveryDate.HasValue
                                && x.DeliveryDate.Value.Date >= today
                                && x.DeliveryDate.Value.Date <= today.AddDays(nearDueDays)
                                && !isDelivered
                                && !isCancelled;

                return new OrderListItemViewModel
                {
                    Id = x.Id,
                    OrderCode = x.OrderCode,
                    OrderDate = x.OrderDate,
                    DeliveryDate = x.DeliveryDate,
                    CustomerName = x.CustomerName,
                    CustomerPhone = x.CustomerPhone,
                    SalesEmployeeName = x.SalesEmployeeName,
                    AssignedEmployeeName = x.AssignedEmployeeName,
                    Status = x.Status,
                    PaymentStatus = paymentStatus,
                    SubtotalAmount = x.SubtotalAmount,
                    DiscountAmount = x.DiscountAmount,
                    TaxAmount = x.TaxAmount,
                    TotalAmount = x.TotalAmount,
                    PaidAmount = paidAmount,
                    RemainingAmount = remainingAmount,
                    ItemCount = x.Items.Count,
                    Note = x.Note,
                    IsOverdue = isOverdue,
                    IsNearDue = isNearDue,
                    CanView = hasPermission(OrderPermissions.View),
                    CanEdit = hasPermission(OrderPermissions.Edit) && !isDelivered && !isCancelled,
                    CanUpdateStatus = hasPermission(OrderPermissions.StatusUpdate) && !isCancelled,
                    CanRecordPayment = hasPermission(PaymentPermissions.Create) && remainingAmount > 0 && !isCancelled,
                    CanCreateStockOut = hasPermission(StockOutPermissions.Create) && !isCancelled,
                    CanExportPdf = hasPermission(OrderPermissions.View)
                };
            });

        projected = ApplySort(projected, normalizedFilter.SortBy, normalizedFilter.SortDirection);

        var totalRecords = projected.Count();
        var totalOrderAmount = projected.Sum(x => x.TotalAmount);
        var totalPaidAmount = projected.Sum(x => x.PaidAmount);
        var totalRemainingAmount = projected.Sum(x => x.RemainingAmount);
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalRecords / (double)normalizedFilter.PageSize));
        var currentPage = Math.Min(normalizedFilter.Page, totalPages);

        var items = projected
            .Skip((currentPage - 1) * normalizedFilter.PageSize)
            .Take(normalizedFilter.PageSize)
            .ToList();

        normalizedFilter.Page = currentPage;

        var model = new OrderListViewModel
        {
            Filter = normalizedFilter,
            Items = items,
            TotalRecords = totalRecords,
            TotalPages = totalPages,
            TotalOrderAmount = totalOrderAmount,
            TotalPaidAmount = totalPaidAmount,
            TotalRemainingAmount = totalRemainingAmount,
            Customers = InMemoryOrderDataStore.Customers,
            Employees = InMemoryOrderDataStore.Employees,
            CanCreateOrder = hasPermission(OrderPermissions.Create),
            CanExport = hasPermission(OrderPermissions.View)
        };

        return Task.FromResult(model);
    }

    private static OrderFilterViewModel NormalizeFilter(OrderFilterViewModel filter)
    {
        var normalizedPageSize = filter.PageSize is 10 or 20 or 50 or 100 ? filter.PageSize : 20;

        return new OrderFilterViewModel
        {
            Keyword = filter.Keyword?.Trim(),
            Status = NormalizeValue(filter.Status),
            PaymentStatus = NormalizeValue(filter.PaymentStatus),
            CustomerId = filter.CustomerId,
            SalesEmployeeId = filter.SalesEmployeeId,
            AssignedEmployeeId = filter.AssignedEmployeeId,
            OrderDateFrom = filter.OrderDateFrom,
            OrderDateTo = filter.OrderDateTo,
            DeliveryDateFrom = filter.DeliveryDateFrom,
            DeliveryDateTo = filter.DeliveryDateTo,
            IsOverdueOnly = filter.IsOverdueOnly,
            IsNearDueOnly = filter.IsNearDueOnly,
            SortBy = string.IsNullOrWhiteSpace(filter.SortBy) ? "orderDate" : filter.SortBy,
            SortDirection = string.Equals(filter.SortDirection, "asc", StringComparison.OrdinalIgnoreCase) ? "asc" : "desc",
            Page = filter.Page <= 0 ? 1 : filter.Page,
            PageSize = normalizedPageSize
        };
    }

    private static string? NormalizeValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || string.Equals(value, "ALL", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return value.Trim().ToUpperInvariant();
    }

    private static IEnumerable<OrderListItemViewModel> ApplySort(IEnumerable<OrderListItemViewModel> query, string? sortBy, string? sortDirection)
    {
        var isAsc = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        var sortKey = (sortBy ?? "orderDate").Trim().ToLowerInvariant();

        return (sortKey, isAsc) switch
        {
            ("ordercode", true) => query.OrderBy(x => x.OrderCode),
            ("ordercode", false) => query.OrderByDescending(x => x.OrderCode),
            ("orderdate", true) => query.OrderBy(x => x.OrderDate),
            ("orderdate", false) => query.OrderByDescending(x => x.OrderDate),
            ("deliverydate", true) => query.OrderBy(x => x.DeliveryDate),
            ("deliverydate", false) => query.OrderByDescending(x => x.DeliveryDate),
            ("customer", true) => query.OrderBy(x => x.CustomerName),
            ("customer", false) => query.OrderByDescending(x => x.CustomerName),
            ("status", true) => query.OrderBy(x => x.Status),
            ("status", false) => query.OrderByDescending(x => x.Status),
            ("totalamount", true) => query.OrderBy(x => x.TotalAmount),
            ("totalamount", false) => query.OrderByDescending(x => x.TotalAmount),
            ("paidamount", true) => query.OrderBy(x => x.PaidAmount),
            ("paidamount", false) => query.OrderByDescending(x => x.PaidAmount),
            ("remainingamount", true) => query.OrderBy(x => x.RemainingAmount),
            ("remainingamount", false) => query.OrderByDescending(x => x.RemainingAmount),
            _ => query.OrderByDescending(x => x.OrderDate)
        };
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
