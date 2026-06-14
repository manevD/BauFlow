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


// =============================
// DATABASE
// =============================

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string not found");


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        connectionString,
        sql => sql.EnableRetryOnFailure()
    )
);



// =============================
// IDENTITY
// =============================

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
    .AddErrorDescriber<MacedonianIdentityErrors>();



// =============================
// COOKIE LOGIN FIX
// =============================

builder.Services.ConfigureApplicationCookie(options =>
{
    // 🔥 WICHTIG FÜR AddDefaultIdentity
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";


    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;


    options.SlidingExpiration = true;

    options.ExpireTimeSpan =
        TimeSpan.FromHours(8);
});



// =============================
// SESSION
// =============================

builder.Services.AddSession(options =>
{
    options.IdleTimeout =
        TimeSpan.FromHours(8);

    options.Cookie.HttpOnly = true;

    options.Cookie.IsEssential = true;
});



// =============================
// CULTURE
// =============================

var culture =
    new CultureInfo("en-US");


CultureInfo.DefaultThreadCurrentCulture =
    culture;

CultureInfo.DefaultThreadCurrentUICulture =
    culture;


builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture =
        new RequestCulture(culture);

    options.SupportedCultures =
        new[] { culture };

    options.SupportedUICultures =
        new[] { culture };
});



// =============================
// MVC
// =============================

builder.Services.AddControllersWithViews();


builder.Services
    .AddRazorPages()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();



// =============================
// TENANT
// =============================

builder.Services.AddHttpContextAccessor();


builder.Services.AddScoped
<
    ITenantProvider,
    TenantProvider
>();



// =============================
// SERVICES
// =============================

builder.Services.AddScoped<PlanService>();

builder.Services.AddScoped<EmailService>();

builder.Services.AddScoped<EmailTemplateService>();

builder.Services.AddScoped<NumberService>();

builder.Services.AddScoped<EmailEncryptionService>();



builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings")
);


builder.Services.AddDataProtection();



// =============================
// CLAIMS
// =============================

builder.Services.AddScoped
<
    IUserClaimsPrincipalFactory<ApplicationUser>,
    CustomClaimsPrincipalFactory
>();



// =============================
// AUTHORIZATION
// =============================

builder.Services.AddAuthorization(options =>
{

    options.AddPolicy(
        "OwnerOnly",
        p => p.RequireRole("Owner")
    );


    options.AddPolicy(
        "AdminOrOwner",
        p => p.RequireRole("Admin", "Owner")
    );


    options.AddPolicy(
        "MemberAccess",
        p => p.RequireRole(
            "Member",
            "Admin",
            "Owner"
        )
    );



    options.AddPolicy(
        "TenantActive",
        policy =>
            policy.Requirements.Add(
                new TenantRequirement()
            )
    );



    foreach (var plan in Enum.GetValues<Plan>())
    {

        options.AddPolicy(
            $"Plan_{plan}",
            policy =>
                policy.Requirements.Add(
                    new PlanRequirement(plan)
                )
        );

    }

});



builder.Services.AddScoped
<
    IAuthorizationHandler,
    TenantHandler
>();


builder.Services.AddScoped
<
    IAuthorizationHandler,
    PlanHandler
>();



// =============================
// HEALTH
// =============================

builder.Services.AddHealthChecks();


builder.Services
    .AddDatabaseDeveloperPageExceptionFilter();




// =============================
// APP
// =============================

var app = builder.Build();



// =============================
// ERROR HANDLING
// =============================

if (app.Environment.IsDevelopment())
{

    app.UseMigrationsEndPoint();

}
else
{

    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();

}



// =============================
// PIPELINE
// =============================

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseMiddleware<GlobalExceptionMiddleware>();


app.UseHttpsRedirection();


app.UseStaticFiles();



var locOptions =
    app.Services
    .GetRequiredService<IOptions<RequestLocalizationOptions>>();


app.UseRequestLocalization(
    locOptions.Value
);



app.UseRouting();


// 🔥 richtige Reihenfolge

app.UseSession();


app.UseAuthentication();


app.UseAuthorization();



// =============================
// ROUTES
// =============================

app.MapControllerRoute(
    name: "default",
    pattern:
    "{controller=Home}/{action=Index}/{id?}"
);


app.MapRazorPages();



// =============================
// HEALTH
// =============================

app.MapHealthChecks(
    "/health",
    new HealthCheckOptions
    {
        ResponseWriter =
        async (context, report) =>
        {

            context.Response.ContentType =
                "application/json";


            var result =
                JsonSerializer.Serialize(new
                {

                    status =
                        report.Status.ToString(),

                    checks =
                    report.Entries.Select(e => new
                    {

                        name = e.Key,

                        status =
                            e.Value.Status.ToString(),

                        error =
                            e.Value.Exception?.Message

                    })

                });


            await context.Response
                .WriteAsync(result);

        }
    });

app.Run();