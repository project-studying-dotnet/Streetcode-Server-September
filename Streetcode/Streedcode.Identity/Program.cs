using Streetcode.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Streetcode.Identity.Extensions;
using Microsoft.AspNetCore.Identity;
using Streetcode.Identity.Models;
using Streetcode.Identity.Services.Interfaces;
using Streetcode.Identity.Services.Realizations;
using Streedcode.Identity.Extensions;
using Streetcode.Identity.Models.Mapper;
using Streetcode.Identity.Repository;
using Hangfire;
using Microsoft.IdentityModel.JsonWebTokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IRefreshRepository, RefreshRepository>();    
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddSingleton<JsonWebTokenHandler>();

builder.Services.ConfigureJwt(builder);
builder.Services.ConfigureRefreshToken(builder);
builder.Services.AddJwtAuthentication(builder);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerServices();
builder.Services.AddAutoMapper(typeof(UsersProfile));

builder.Services.AddHangfire(config =>
{ 
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddHangfireServer();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "Streetcode.Identity_";
});

builder.Services.AddSingleton<IAzureBusService, AzureBusService>();

var app = builder.Build();
var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

await app.ApplyMigrations();

await app.SeedDataAsync();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseHangfireDashboard("/hangfire");

app.MapControllers();

recurringJobManager.AddOrUpdate<JwtService>(
    "DeleteInvalidTokens",
    service => service.DeleteInvalidTokensAsync(),
    Cron.Daily);

await app.RunAsync();
