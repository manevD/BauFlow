using BauFlow.Data;
using BauFlow.Entities;
using BauFlow.Factories;
using BauFlow.Interfaces;
using BauFlow.Middleware;
using BauFlow.Providers;
using BauFlow.Security;
using BauFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
SerilogConfig.Configure(builder);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddHealthChecks();


builder.Services.AddScoped<ITenantProvider, TenantProvider>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<PlanService>();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddScoped<
    IUserClaimsPrincipalFactory<ApplicationUser>,
    CustomClaimsPrincipalFactory>(); 
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("OwnerOnly", p => p.RequireRole("Owner"));
    options.AddPolicy("AdminOrOwner", p => p.RequireRole("Admin", "Owner"));
    options.AddPolicy("MemberAccess", p => p.RequireRole("Member", "Admin", "Owner"));
    options.AddPolicy("TenantActive",
      policy => policy.Requirements.Add(new TenantRequirement()));
    foreach (Plan plan in Enum.GetValues<Plan>())
    {
        options.AddPolicy($"Plan_{plan}", policy =>
            policy.Requirements.Add(new PlanRequirement(plan)));
    }
});
builder.Services.AddScoped<IAuthorizationHandler, TenantHandler>();
builder.Services.AddScoped<IAuthorizationHandler, PlanHandler>();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new {
                name = e.Key,
                status = e.Value.Status.ToString(),
                error = e.Value.Exception?.Message
            })
        });

        await context.Response.WriteAsync(result);
    }
});

app.Run();
