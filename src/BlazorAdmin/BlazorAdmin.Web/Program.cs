using BlazorAdmin.Servers.Core;
using BlazorAdmin.Servers.Core.Auth;
using BlazorAdmin.Servers.Core.Chat;
using BlazorAdmin.Servers.Core.Data;
using BlazorAdmin.Servers.Core.Helper;
using BlazorAdmin.Servers.Core.Modules;
using BlazorAdmin.Servers.Core.Services;
using BlazorAdmin.Web.Components;
using Cropper.Blazor.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.FileProviders;
using MudBlazor;
using MudBlazor.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// log
Environment.CurrentDirectory = AppContext.BaseDirectory;
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));

// 从AdditionalAssemblies中取得IModule接口的实现类
var types = Routes.AdditionalAssemblies.SelectMany(a => a.GetTypes()).Where(t => t.IsClass && t.GetInterface(nameof(IModule)) != null).ToList();
var moduleList = new List<IModule>();
foreach (var type in types)
{
    var module = Activator.CreateInstance(type) as IModule;
    if (module is not null)
    {
        moduleList.Add(module);
    }
}

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
    config.SnackbarConfiguration.PreventDuplicates = false;
});
builder.Services.AddMudMarkdownServices();
builder.Services.AddCropper();

var dbDirectory = Path.Combine(AppContext.BaseDirectory, "DB");
if (!Directory.Exists(dbDirectory))
{
    Directory.CreateDirectory(dbDirectory);
}

// dbcontext
builder.AddDatabase();

// messagesender
builder.Services.AddSingleton<MessageSender>();

// custom auth state provider
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, BlazorAuthorizationMiddlewareResultHandler>();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthStateProvider>();
builder.Services.AddScoped<ExternalAuthService>();

// jwt authentication
builder.Services.AddSingleton<IAuthorizationHandler, ApiAuthorizeHandler>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiAuthorizePolicy", policy =>
    {
        policy.RequireAuthenticatedUser(); // 要求用户已登录
        policy.Requirements.Add(new ApiAuthorizeRequirement());
    });
});
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(JwtOptionsExtension.InitialJwtOptions);

// some service
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IAccessService, AccessService>();

// locallization
builder.Services.AddLocalization();

// get ip and agent only for record login log 
builder.Services.AddHttpContextAccessor();

// jwt helper
builder.Services.AddScoped<JwtHelper>();
builder.Services.AddScoped<AiHelper>();
builder.Services.AddScoped<NotificationHelper>();

builder.Services.AddControllers();

// modules
moduleList.ForEach(m => m.Add(builder.Services));

var app = builder.Build();

CurrentApplication.Application = app;
app.InitialDatabase();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
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

app.MapStaticAssets();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(Routes.AdditionalAssemblies.ToArray()); // rcl的page无法在浏览器上直接route https://github.com/dotnet/aspnetcore/issues/49313

moduleList.ForEach(m => m.Use(app));

app.Run();
