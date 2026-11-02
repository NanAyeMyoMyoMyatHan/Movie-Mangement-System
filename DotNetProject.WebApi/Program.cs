using DotNetProject.Database.AppDbModels;
using DotNetProject.Domain.Features.Contents;
using DotNetProject.Domain.Features.Movies;
using DotNetProject.Domain.Features.Ratings;
using DotNetProject.Domain.Features.Users;
using DotNetProject.Domain.Features.WatchList;
using DotNetProject.WebApi.Features.Contents;
using DotNetProject.WebApi.Features.Movies;
using DotNetProject.WebApi.Features.Ratings;
using DotNetProject.WebApi.Features.Users;
using DotNetProject.WebApi.Features.WatchList;
using DotNetProject.WebApi.Features.Thai;
using DotNetProject.Domain.Features.Thai;
using DotNetProject.WebApi.Features.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Allow large file uploads (up to 600 MB for video files)
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 600L * 1024 * 1024; // 600 MB
});
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 600L * 1024 * 1024; // 600 MB
});

builder.Services.AddControllers();
builder.Services.AddScoped<IMoviesInterface, MovieServices>();
builder.Services.AddScoped<IUserInterface, UserServices>();
builder.Services.AddScoped<IWatchListInterface, WatchListServices>();
builder.Services.AddScoped<IRatingInterface, RatingServices>();
builder.Services.AddScoped<IContent, ContentsServices>();
builder.Services.AddScoped<IThaiInterface, ThaiServices>();

// File storage service (singleton — stateless, holds only the configured root path)
builder.Services.AddSingleton<FileStorageService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Program.cs ထဲမှာ အပေါ်ပိုင်း (builder.Build() မတိုင်ခင်) မှာ ထည့်ပါ
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ── Serve uploaded files as static files ──────────────────────────────────────
// GET /images/a7b9f3.jpg  → D:\AppStorage\images\a7b9f3.jpg
// GET /videos/e1c2d3.mp4  → D:\AppStorage\videos\e1c2d3.mp4 & D:\Alchemy Of Soule S1+S2
var storagePath = builder.Configuration["FileStorage:RootPath"]
                  ?? Path.Combine(Directory.GetCurrentDirectory(), "AppStorage");

var imageDir = Path.Combine(storagePath, "images");
if (!Directory.Exists(imageDir)) Directory.CreateDirectory(imageDir);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(imageDir),
    RequestPath = "/images"
});

var videoProviders = new List<Microsoft.Extensions.FileProviders.IFileProvider>();
var videosDir = Path.Combine(storagePath, "videos");
if (!Directory.Exists(videosDir)) Directory.CreateDirectory(videosDir);
videoProviders.Add(new Microsoft.Extensions.FileProviders.PhysicalFileProvider(videosDir));

var extraFolders = new[]
{
    @"D:\Alchemy Of Soule S1+S2",
    @"D:\KingDomS1+S2",
    @"D:\Daung With You",
    @"D:\KhemJira",
    @"D:\Fool In Love"
};

foreach (var folder in extraFolders)
{
    if (Directory.Exists(folder))
    {
        videoProviders.Add(new Microsoft.Extensions.FileProviders.PhysicalFileProvider(folder));
    }
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.CompositeFileProvider(videoProviders),
    RequestPath = "/videos"
});
// ─────────────────────────────────────────────────────────────────────────────

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
