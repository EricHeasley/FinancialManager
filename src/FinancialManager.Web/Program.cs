using System.Globalization;
using FinancialManager.Application;
using FinancialManager.Infrastructure;
using FinancialManager.Infrastructure.Persistence;
using FinancialManager.Web.Components;
using FinancialManager.Web.Formatting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<DisplayOptions>()
    .Bind(builder.Configuration.GetSection(DisplayOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddApplication(builder.Configuration)
    .AddInfrastructure(builder.Configuration);

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

var displayCulture = builder.Configuration.GetSection(DisplayOptions.SectionName).Get<DisplayOptions>()?.CultureName
    ?? throw new InvalidOperationException($"'{DisplayOptions.SectionName}:{nameof(DisplayOptions.CultureName)}' is not configured.");
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo(displayCulture);
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo(displayCulture);
app.UseRequestLocalization(displayCulture);

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.Services.InitializeDatabaseAsync();
await app.RunAsync();
