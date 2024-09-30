using Streedcode.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Streedcode.Identity.Extensions;
using Microsoft.AspNetCore.Identity;
using Streedcode.Identity.Models;
using Streedcode.Identity.Services.Interfaces;
using Streedcode.Identity.Services.Realizations;
using Streedcode.Identity.MessageBroker;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultSqlConnection"));
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRabbitMqSender, RabbitMqSender>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

await app.ApplyMigrations();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
