using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace SmartFarmSEP490.API.Helpers;

public static class ControllerExtensions
{
    public static Guid GetUserId(this ControllerBase controller)
    {
        var claim = controller.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? controller.User.FindFirst("sub")?.Value
                 ?? controller.User.FindFirst("id")?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    public static string? GetUserRole(this ControllerBase controller)
        => controller.User.FindFirst(ClaimTypes.Role)?.Value;

    public static bool IsInRole(this ControllerBase controller, string role)
        => controller.User.IsInRole(role);
}
