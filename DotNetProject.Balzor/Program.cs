using DotNetProject.Balzor.Components;
using DotNetProject.Balzor.Components.Shared;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient("Api", client =>
{
    var baseAddress = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7283/";
    client.BaseAddress = new Uri(baseAddress);
});
builder.Services.AddHttpClient();
builder.Services.AddScoped<HttpClientService>();

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

var alchemyFolder = @"D:\Alchemy Of Soule S1+S2";
if (Directory.Exists(alchemyFolder))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(alchemyFolder),
        RequestPath = "/videos"
    });
}

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
