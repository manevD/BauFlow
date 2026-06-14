using BauFlow.Data;
using BauFlow.Entities;
using BauFlow.Factories;
using BauFlow.Interfaces;
using BauFlow.Middleware;
using BauFlow.Models;
using BauFlow.Providers;
using BauFlow.Security;
using BauFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------
// Logging
// -----------------------------
//SerilogConfig.Configure(builder);

// -----------------------------
// Database
// -----------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString,
        sql => sql.EnableRetryOnFailure()));

// -----------------------------
// Identity
// -----------------------------
builder.Services
.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;

    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddErrorDescriber<MacedonianIdentityErrors>(); // 🔥 MKD errors

// -----------------------------
// 🔥 ONLY MKD CULTURE
// -----------------------------
var culture = new CultureInfo("en-US");


CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(culture);
    options.SupportedCultures = new[] { culture };
    options.SupportedUICultures = new[] { culture };
});

// -----------------------------
// Tokens / Cookies
// -----------------------------
builder.Services.Configure<DataProtectionTokenProviderOptions>(o =>
{
    o.TokenLifespan = TimeSpan.FromHours(24);
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});
builder.Services.AddSession();

// -----------------------------
// MVC
// -----------------------------
builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

// -----------------------------
// Tenant System
// -----------------------------
builder.Services.AddScoped<ITenantProvider, TenantProvider>();
builder.Services.AddHttpContextAccessor();

// -----------------------------
// Application Services
// -----------------------------
builder.Services.AddScoped<PlanService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<EmailTemplateService>();
builder.Services.AddScoped<NumberService>();
builder.Services.AddScoped<EmailEncryptionService>();

builder.Services.AddDataProtection();
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

// -----------------------------
// Custom Claims
// -----------------------------
builder.Services.AddScoped<
    IUserClaimsPrincipalFactory<ApplicationUser>,
    CustomClaimsPrincipalFactory>();

// -----------------------------
// Authorization Policies
// -----------------------------
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

// -----------------------------
// Health Checks
// -----------------------------
builder.Services.AddHealthChecks();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// -----------------------------
// Build App
// -----------------------------
var app = builder.Build();

// -----------------------------
// Middleware
// -----------------------------
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();

//app.UseSerilogRequestLogging();

// -----------------------------
// Environment
// -----------------------------
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// -----------------------------
// HTTP Pipeline
// -----------------------------
app.UseHttpsRedirection();
app.UseStaticFiles();

// 🔥 АКТИВИРАЈ MKD LOCALIZATION
var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(locOptions.Value);

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// -----------------------------
// Routes
// -----------------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// -----------------------------
// Health Endpoint
// -----------------------------
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                error = e.Value.Exception?.Message
            })
        });

        await context.Response.WriteAsync(result);
    }
});
app.UseSession();
app.Run();