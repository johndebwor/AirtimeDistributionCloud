using System.Globalization;
using AirtimeDistributionCloud.Application;
using AirtimeDistributionCloud.Core.Entities;
using AirtimeDistributionCloud.Core.Interfaces;
using AirtimeDistributionCloud.Infrastructure;
using AirtimeDistributionCloud.Infrastructure.Data;
using AirtimeDistributionCloud.Web.Components;
using AirtimeDistributionCloud.Web.Components.Account;
using AirtimeDistributionCloud.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using MudBlazor.Services;

// Configure SSP (South Sudanese Pound) currency with 6 decimal places
var culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
culture.NumberFormat.CurrencySymbol = "SSP ";
culture.NumberFormat.CurrencyDecimalDigits = 6;
culture.NumberFormat.CurrencyGroupSeparator = ",";
culture.NumberFormat.CurrencyDecimalSeparator = ".";
culture.NumberFormat.NumberDecimalDigits = 6;
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

var builder = WebApplication.CreateBuilder(args);

// Add Blazor services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add MudBlazor
builder.Services.AddMudServices();

// Add Clean Architecture layers
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// Add Authentication / Authorization
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddIdentityCookies();

builder.Services.AddAuthorization();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.UseStaticFiles(); // Serve runtime-uploaded files (e.g. /uploads/)
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();

// Seed data
await SeedData.InitializeAsync(app.Services);

app.Run();
