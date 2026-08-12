using DotNetProject.Database.AppDbModels;
using DotNetProject.WebApi.Features.Movies;
using DotNetProject.WebApi.Features.Ratings;
using DotNetProject.WebApi.Features.Users;
using DotNetProject.WebApi.Features.WatchList;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<IMoviesInterface, MovieServices>();
builder.Services.AddScoped<IUserInterface, UserServices>();
builder.Services.AddScoped<IWatchListInterface, WatchListServices>();
builder.Services.AddScoped<IRatingInterface, RatingServices>();
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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
