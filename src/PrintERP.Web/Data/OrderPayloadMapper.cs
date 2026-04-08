using System.Text.Json;
using PrintERP.Web.Models.ViewModels;

namespace PrintERP.Web.Data;

public static class OrderPayloadMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public static OrderDetailViewModel? Deserialize(string payload)
        => JsonSerializer.Deserialize<OrderDetailViewModel>(payload, JsonOptions);

    public static string Serialize(OrderDetailViewModel payload)
        => JsonSerializer.Serialize(payload, JsonOptions);
}
