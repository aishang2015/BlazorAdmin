using BlazorAdmin.Web.Components;
using MudBlazor.Services;
using MudBlazor;
using Serilog;
using BlazorAdmin.Core.Auth;
using BlazorAdmin.Core.Helper;
using BlazorAdmin.Core.Services;
using Microsoft.AspNetCore.Components.Authorization;
using BlazorAdmin.Web.Data.States;
using BlazorAdmin.Data;
using Microsoft.EntityFrameworkCore;
using BlazorAdmin.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Authorization;
using Cropper.Blazor.Extensions;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// log
Environment.CurrentDirectory = AppContext.BaseDirectory;
builder.Host.UseSerilog((context, services, configuration) => configuration
	.ReadFrom.Configuration(context.Configuration)
	.ReadFrom.Services(services)
	.WriteTo.Console());

// Add services to the container.
builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();

// mudblazor
builder.Services.AddMudServices(config =>
{
	config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopCenter;

	config.SnackbarConfiguration.VisibleStateDuration = 3000;
	config.SnackbarConfiguration.HideTransitionDuration = 200;
	config.SnackbarConfiguration.ShowTransitionDuration = 200;
});
builder.Services.AddMudMarkdownServices();
builder.Services.AddCropper();

builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, BlazorAuthorizationMiddlewareResultHandler>();

// dbcontext
builder.Services.AddDbContextFactory<BlazorAdminDbContext>(b =>
{
	b.UseSqlite(builder.Configuration.GetConnectionString("Rbac"));
});
builder.Services.AddHostedService<DbContextInitialBackgroundService>();

// jwt helper
builder.Services.AddScoped<JwtHelper>();

// custom auth state provider
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthStateProvider>();
builder.Services.AddScoped<ExternalAuthService>();

// some service
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IAccessService, AccessService>();

// some state
builder.Services.AddScoped<ThemeState>();

// locallization
builder.Services.AddLocalization();

// get ip and agent only for record login log 
builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseRequestLocalization(new RequestLocalizationOptions()
	.AddSupportedCultures(new[] { "zh-CN", "en-US" })
	.AddSupportedUICultures(new[] { "zh-CN", "en-US" }));

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
           Path.Combine(AppContext.BaseDirectory, "Avatars")),
    RequestPath = "/Avatars"
});

app.UseStaticFiles();
app.UseAntiforgery();

app.MapControllers();

app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode()
	.AddAdditionalAssemblies(Routes.AdditionalAssemblies.ToArray()); // rcl的page无法在浏览器上直接route https://github.com/dotnet/aspnetcore/issues/49313

app.Run();

// https://github.com/dotnet/aspnetcore/issues/52063
// AuthorizeRouteView 不起作用
public class BlazorAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
	public Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
	{
		return next(context);
	}
}