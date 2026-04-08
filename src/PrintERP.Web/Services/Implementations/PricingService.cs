using Microsoft.EntityFrameworkCore;
using PrintERP.Web.Data;
using PrintERP.Web.Models.ViewModels;
using PrintERP.Web.Services.Interfaces;

namespace PrintERP.Web.Services.Implementations;

public class PricingService(AppDbContext dbContext) : IPricingService
{
    public async Task<OrderItemEstimateResultViewModel> EstimateItemAsync(OrderItemEstimateRequest request, CancellationToken cancellationToken = default)
    {
        if (!request.ProductCategoryId.HasValue || request.Quantity <= 0)
        {
            return new OrderItemEstimateResultViewModel
            {
                Success = false,
                Message = "Không tìm thấy cấu hình giá phù hợp. Vui lòng nhập đơn giá thủ công."
            };
        }

        var category = await dbContext.ProductCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.ProductCategoryId.Value, cancellationToken);
        if (category is null)
        {
            return new OrderItemEstimateResultViewModel
            {
                Success = false,
                Message = "Không tìm thấy cấu hình giá phù hợp. Vui lòng nhập đơn giá thủ công."
            };
        }

        var area = category.UseAreaPricing
            ? Math.Round((request.Width ?? 0) * (request.Height ?? 0) * request.Quantity, 3)
            : 0;

        var basePrice = category.Name switch
        {
            "In bạt" => 75000m,
            "In decal" => 95000m,
            "Bảng alu" => 320000m,
            "Mica" => 280000m,
            "Name card" => 80000m,
            "Tờ rơi" => 65000m,
            _ => 100000m
        };

        var dimensionFactor = category.UseAreaPricing
            ? Math.Max(1m, area / Math.Max(1m, request.Quantity))
            : 1m;

        var estimatedUnitPrice = Math.Round(basePrice * dimensionFactor, 0);
        var estimatedLineTotal = Math.Round(estimatedUnitPrice * request.Quantity, 0);
        var estimatedCost = Math.Round(estimatedLineTotal * 0.72m, 0);
        var estimatedProfit = estimatedLineTotal - estimatedCost;

        return new OrderItemEstimateResultViewModel
        {
            Success = true,
            Area = area,
            EstimatedUnitPrice = estimatedUnitPrice,
            EstimatedLineTotal = estimatedLineTotal,
            EstimatedCost = estimatedCost,
            EstimatedProfit = estimatedProfit,
            PricingNote = $"Estimate theo rule mặc định {category.Name}"
        };
    }
}
