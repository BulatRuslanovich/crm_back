namespace CrmBack.Core.Utils;

public static class DeviceInfoHelper
{
    public static DeviceInfo ParseDeviceInfo(HttpContext context)
    {
        var userAgent = context.Request.Headers.UserAgent.FirstOrDefault() ?? "";

        return new DeviceInfo
        {
            UserAgent = userAgent,
            Browser = GetBrowser(userAgent),
            OperatingSystem = GetOperatingSystem(userAgent),
            DeviceType = GetDeviceType(userAgent),
            IsMobile = IsMobileDevice(userAgent),
            IsBot = IsBot(userAgent)
        };
    }

    private static string GetBrowser(string userAgent)
    {
        if (userAgent.Contains("Chrome") && !userAgent.Contains("Edg")) return "Chrome";
        if (userAgent.Contains("Firefox")) return "Firefox";
        if (userAgent.Contains("Safari") && !userAgent.Contains("Chrome")) return "Safari";
        if (userAgent.Contains("Edg")) return "Edge";
        if (userAgent.Contains("Opera")) return "Opera";
        return "Unknown";
    }

    private static string GetOperatingSystem(string userAgent)
    {
        if (userAgent.Contains("Windows NT 10.0")) return "Windows 10";
        if (userAgent.Contains("Windows NT 6.3")) return "Windows 8.1";
        if (userAgent.Contains("Windows NT 6.2")) return "Windows 8";
        if (userAgent.Contains("Windows NT 6.1")) return "Windows 7";
        if (userAgent.Contains("Windows NT")) return "Windows";
        if (userAgent.Contains("Mac OS X")) return "macOS";
        if (userAgent.Contains("Linux")) return "Linux";
        if (userAgent.Contains("Android")) return "Android";
        if (userAgent.Contains("iPhone") || userAgent.Contains("iPad")) return "iOS";
        return "Unknown";
    }

    private static string GetDeviceType(string userAgent)
    {
        if (userAgent.Contains("Mobile")) return "Mobile";
        if (userAgent.Contains("Tablet")) return "Tablet";
        return "Desktop";
    }

    private static bool IsMobileDevice(string userAgent)
    {
        return userAgent.Contains("Mobile") ||
               userAgent.Contains("Android") ||
               userAgent.Contains("iPhone") ||
               userAgent.Contains("iPad");
    }

    private static bool IsBot(string userAgent)
    {
        var botKeywords = new[] { "bot", "crawler", "spider", "scraper", "curl", "wget", "python-requests", "postman" };
        return botKeywords.Any(keyword => userAgent.Contains(keyword, StringComparison.CurrentCultureIgnoreCase));
    }
}

public class DeviceInfo
{
    public string UserAgent { get; set; } = string.Empty;
    public string Browser { get; set; } = string.Empty;
    public string OperatingSystem { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public bool IsMobile { get; set; }
    public bool IsBot { get; set; }

    public string ToJsonString()
    {
        return System.Text.Json.JsonSerializer.Serialize(this);
    }
}
