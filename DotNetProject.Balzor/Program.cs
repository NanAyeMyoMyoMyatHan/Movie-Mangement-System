using DotNetProject.Balzor.Components;
using DotNetProject.Balzor.Components.Shared;
using DotNetProject.Domain.Features.Rbac;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        options.DetailedErrors = builder.Environment.IsDevelopment();
    })
    .AddHubOptions(options =>
    {
        options.MaximumReceiveMessageSize = 10L * 1024 * 1024; // 10 MB
    });

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
builder.Services.AddScoped<AuditLogService>();
builder.Services.AddScoped<TmdbService>();

// Clean startup: delete any leftover session file on server boot so fresh runs always start clean as Guest!
try
{
    var contentRoot = builder.Environment.ContentRootPath;
    var baseDir = AppContext.BaseDirectory;
    var possibleSessionFiles = new[]
    {
        Path.Combine(contentRoot, "Storage", "auth_session.json"),
        Path.Combine(baseDir, "Storage", "auth_session.json")
    };
    foreach (var file in possibleSessionFiles)
    {
        if (File.Exists(file))
        {
            File.Delete(file);
        }
    }
}
catch { }

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

var contentTypeProvider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
contentTypeProvider.Mappings[".vtt"] = "text/vtt; charset=utf-8";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = contentTypeProvider
});

var appStorageRoot = builder.Configuration["FileStorage:RootPath"] ?? @"D:\AppStorage";
var storedImagesDir = Path.Combine(appStorageRoot, "images");
if (!Directory.Exists(storedImagesDir)) Directory.CreateDirectory(storedImagesDir);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(storedImagesDir),
    RequestPath = "/images"
});
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(storedImagesDir),
    RequestPath = "/storage/images"
});

var videoProviders = new List<Microsoft.Extensions.FileProviders.IFileProvider>();
var dramaFolders = new[]
{
    @"D:\AppStorage\videos",
    @"D:\Alchemy Of Soule S1+S2",
    @"D:\KingDomS1+S2",
    @"D:\Daung With You",
    @"D:\KhemJira",
    @"D:\Fool In Love",
    @"D:\Queen Of Tears",
    @"D:\QueenOfTears"
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
    { "/videos/alchemy-of-souls", @"D:\Alchemy Of Soule S1+S2" },
    { "/videos/queen-of-tears", @"D:\Queen Of Tears" },
    { "/videos/queenoftears", @"D:\QueenOfTears" }
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
