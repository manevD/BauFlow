
using Serilog;
using Serilog.Sinks.MSSqlServer;

namespace BauFlow.Services
{

    public static class SerilogConfig
    {
        public static void Configure(WebApplicationBuilder builder)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                // Console Logs
                .WriteTo.Console()

                //SQL Server Logs   
                .WriteTo.MSSqlServer(
                    connectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
                    sinkOptions: new MSSqlServerSinkOptions
                    {
                        TableName = "Logs",
                        AutoCreateSqlTable = true
                    })

                // File Logs
                .WriteTo.File(
                    "Logs/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30)

                .CreateLogger();

            builder.Host.UseSerilog();
        }
    }
}
