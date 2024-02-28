using BlazorAdmin.Core.Auth;
using BlazorAdmin.Core.Chat;
using BlazorAdmin.Core.Data;
using BlazorAdmin.Core.Helper;
using BlazorAdmin.Core.Services;
using BlazorAdmin.Data;
using BlazorAdmin.Data.Constants;
using BlazorAdmin.Im.Backgrounds;
using BlazorAdmin.Im.Events;
using BlazorAdmin.Layout.States;
using BlazorAdmin.Web.Components;
using Cropper.Blazor.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using MudBlazor;
using MudBlazor.Services;
using Serilog;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

// log
Environment.CurrentDirectory = AppContext.BaseDirectory;
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .WriteTo.Console());

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(options =>
    {
        options.MaximumReceiveMessageSize = 320 * 1024;
    })
    .AddInteractiveWebAssemblyComponents();

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

var dbDirectory = Path.Combine(AppContext.BaseDirectory, "DB");
if (!Directory.Exists(dbDirectory))
{
    Directory.CreateDirectory(dbDirectory);
}

// dbcontext
builder.Services.AddDbContextFactory<BlazorAdminDbContext>(b =>
{
    b.UseSqlite(builder.Configuration.GetConnectionString("Rbac"));
}, ServiceLifetime.Scoped);
builder.Services.AddSingleton<BlazroAdminChatDbContextFactory>();
builder.Services.AddHostedService<SendMessageBackgroundService>();

builder.Services.AddSingleton<MessageSender>();

// custom auth state provider
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthStateProvider>();
builder.Services.AddScoped<ExternalAuthService>();

// jwt authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    var connStr = builder.Configuration.GetConnectionString("Rbac");
    using var context = new BlazorAdminDbContext(new DbContextOptionsBuilder<BlazorAdminDbContext>()
        .UseSqlite(builder.Configuration.GetConnectionString("Rbac")).Options, default, default);

    var issuer = context.Settings.FirstOrDefault(s => s.Key == JwtConstant.JwtIssue)!.Value;
    var audience = context.Settings.FirstOrDefault(s => s.Key == JwtConstant.JwtAudience)!.Value;
    var privateKey = context.Settings.FirstOrDefault(s => s.Key == JwtConstant.JwtSigningRsaPrivateKey)!.Value;
    var publicKey = context.Settings.FirstOrDefault(s => s.Key == JwtConstant.JwtSigningRsaPublicKey)!.Value;

    var rsa = RSA.Create();
    rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out int publicReadBytes);
    rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKey), out int privateReadBytes);
    var securityKey = new RsaSecurityKey(rsa);
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidAudience = audience,
        ValidateAudience = false,

        ValidIssuer = issuer,
        ValidateIssuer = false,

        IssuerSigningKey = securityKey,

        ValidateIssuerSigningKey = true,
        ValidateLifetime = true
    };
});

// some service
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IAccessService, AccessService>();

// some state
builder.Services.AddScoped<ThemeState>();
builder.Services.AddScoped<LayoutState>();

// event helper
builder.Services.AddScoped<EventHelper<UpdateNoReadCountEvent>>();

// locallization
builder.Services.AddLocalization();

// get ip and agent only for record login log 
builder.Services.AddHttpContextAccessor();

// jwt helper
builder.Services.AddScoped<JwtHelper>();

builder.Services.AddControllers();


var app = builder.Build();

// initial db
using var scope = app.Services.CreateScope();
var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<BlazorAdminDbContext>>();
using var context = dbContextFactory.CreateDbContext();
context.InitialData();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseRequestLocalization(new RequestLocalizationOptions()
    .AddSupportedCultures(new[] { "zh-CN", "en-US" })
    .AddSupportedUICultures(new[] { "zh-CN", "en-US" }));

var avatarDirectory = Path.Combine(AppContext.BaseDirectory, "Avatars");
if (!Directory.Exists(avatarDirectory))
{
    Directory.CreateDirectory(avatarDirectory);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(avatarDirectory),
    RequestPath = "/Avatars"
});

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(Routes.AdditionalAssemblies.ToArray()); // rcl的page无法在浏览器上直接route https://github.com/dotnet/aspnetcore/issues/49313

app.MapHub<ChatHub>(ChatHub.ChatHubUrl);

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