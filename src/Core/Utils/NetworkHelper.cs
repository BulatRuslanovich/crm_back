namespace CrmBack.Core.Utils;

public static class NetworkHelper
{
    public static string GetClientIpAddress(HttpContext context)
    {
        var cfConnectingIp = context.Request.Headers["CF-Connecting-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(cfConnectingIp) && IsValidIpAddress(cfConnectingIp))
            return cfConnectingIp;

        var xAmznTraceId = context.Request.Headers["X-Amzn-Trace-Id"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xAmznTraceId))
        {
            var xForwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xForwardedFor))
            {
                var ip = xForwardedFor.Split(',')[0].Trim();
                if (IsValidIpAddress(ip))
                    return ip;
            }
        }

        var headers = new[]
        {
            "X-Forwarded-For",
            "X-Real-IP",
            "X-Client-IP",
            "X-Forwarded",
            "Forwarded-For",
            "Forwarded"
        };

        foreach (var header in headers)
        {
            var value = context.Request.Headers[header].FirstOrDefault();
            if (!string.IsNullOrEmpty(value))
            {
                var ip = value.Split(',')[0].Trim();
                if (IsValidIpAddress(ip))
                    return ip;
            }
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private static bool IsValidIpAddress(string ip)
    {
        if (string.IsNullOrEmpty(ip)) return false;

        if (System.Net.IPAddress.TryParse(ip, out var address))
        {
            return address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ||
                   address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6;
        }
        return false;
    }
}
