using DotNetProject.Balzor.Components;
using DotNetProject.Balzor.Components.Shared;
using DotNetProject.Database.AppDbModels;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Database context for direct database user authentication and data access
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Server=localhost;Database=MyFlixDb;User Id=sa;Password=sasa@123;TrustServerCertificate=True;"));

builder.Services.AddHttpClient("Api", client =>
{
    var baseAddress = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7283/";
    client.BaseAddress = new Uri(baseAddress);
});
builder.Services.AddHttpClient();
builder.Services.AddScoped<HttpClientService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<WatchLaterService>();
builder.Services.AddScoped<FavoriteService>();
builder.Services.AddScoped<WatchHistoryService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

var videoProviders = new List<Microsoft.Extensions.FileProviders.IFileProvider>();
var dramaFolders = new[]
{
    @"D:\AppStorage\videos",
    @"D:\Alchemy Of Soule S1+S2",
    @"D:\KingDomS1+S2",
    @"D:\Daung With You",
    @"D:\KhemJira",
    @"D:\Fool In Love"
};

foreach (var folder in dramaFolders)
{
    if (Directory.Exists(folder))
    {
        videoProviders.Add(new Microsoft.Extensions.FileProviders.PhysicalFileProvider(folder));
    }
}

if (videoProviders.Count > 0)
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.CompositeFileProvider(videoProviders),
        RequestPath = "/videos"
    });
}

var dramaRouteMap = new Dictionary<string, string>
{
    { "/videos/fool-in-love", @"D:\Fool In Love" },
    { "/videos/kingdom", @"D:\KingDomS1+S2" },
    { "/videos/daung-with-you", @"D:\Daung With You" },
    { "/videos/khemjira", @"D:\KhemJira" },
    { "/videos/alchemy-of-souls", @"D:\Alchemy Of Soule S1+S2" }
};

foreach (var kvp in dramaRouteMap)
{
    if (Directory.Exists(kvp.Value))
    {
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(kvp.Value),
            RequestPath = kvp.Key
        });
    }
}

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
