using System;
using System.Collections.Generic;
using System.Text;


internal static class AppEnvironment
{
    public static bool IsDevelopment =>
        Environment.GetEnvironmentVariable("NETCOREAPP_ENVIRONMENT") == "Development";

    public static bool IsProduction =>
        Environment.GetEnvironmentVariable("NETCOREAPP_ENVIRONMENT") == "Production";

    public static bool IsStaging =>
        Environment.GetEnvironmentVariable("NETCOREAPP_ENVIRONMENT") == "Staging";
}

