using Streetcode.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Streetcode.Identity.Extensions;
using Microsoft.AspNetCore.Identity;
using Streetcode.Identity.Models;
using Streetcode.Identity.Services.Interfaces;
using Streetcode.Identity.Services.Realizations;
using Streedcode.Identity.Extensions;
using Streetcode.Identity.Models.Mapper;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.ConfigureJwt(builder);
builder.Services.AddJwtAuthentication(builder);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(UsersProfile));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

await app.SeedDataAsync();

await app.ApplyMigrations();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
