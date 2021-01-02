using System;
using System.Collections.Generic;
using System.Text;


internal static class HostEnvironment
{
    public static bool IsDevelopment =>
        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

    public static bool IsProduction =>
        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production";

    public static bool IsStaging =>
        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Staging";
}

