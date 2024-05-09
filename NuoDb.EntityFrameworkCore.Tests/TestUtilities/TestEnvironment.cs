using Microsoft.Extensions.Configuration;

namespace NuoDb.EntityFrameworkCore.Tests.TestUtilities
{
    public static class TestEnvironment
    {
        public static IConfiguration Config { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("config.json", optional: true)
            .AddJsonFile("config.test.json", optional: true)
            .AddEnvironmentVariables()
            .Build()
            .GetSection("Test:NuoDb");

        public static string DefaultConnection { get; } = Config["DefaultConnection"]
            ?? "Server=localhost;Database=demo;User=dba;Password=dba;Schema=USER";

        //private static readonly string _dataSource = new NuoDbConnectionStringBuilder(DefaultConnection).DataSource;
        //public static bool IsConfigured { get; } = !string.IsNullOrEmpty(_dataSource);

        public static bool IsCI { get; } = Environment.GetEnvironmentVariable("PIPELINE_WORKSPACE") != null;

        public static bool? GetFlag(string key)
            => bool.TryParse(Config[key], out var flag) ? flag : (bool?)null;

        public static int? GetInt(string key)
            => int.TryParse(Config[key], out var value) ? value : (int?)null;

        
    }
}
