using FluentValidation;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Streetcode.BLL.Services.BlobStorageService;
using Streetcode.BLL.ValidationBehavior;
using Streetcode.DAL.Entities.Users;
using Streetcode.DAL.Persistence;
using Streetcode.WebApi.Extensions;
using Streetcode.WebApi.Utils;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureApplication();


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://localhost:6000";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ClientIdPolicy", policy =>
        policy.RequireClaim("client_id", "streedcodeClient"));
});


builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddSwaggerServices();
builder.Services.AddCustomServices();
builder.Services.ConfigureBlob(builder);
//builder.Services.ConfigureJwt(builder);
builder.Services.ConfigurePayment(builder);
builder.Services.ConfigureInstagram(builder);
builder.Services.ConfigureSerilog(builder);
//builder.Services.AddJwtAuthentication(builder);

var app = builder.Build();

if (app.Environment.EnvironmentName == "Local")
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPIv5 v1"));
}
else
{
    app.UseHsts();
}

await app.ApplyMigrations();

await app.SeedDataAsync(); // uncomment for seeding data in local
app.UseCors();
app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/dash");

app.UseGlobalExceptionHandler();

if (app.Environment.EnvironmentName != "Local")
{
    BackgroundJob.Schedule<WebParsingUtils>(
    wp => wp.ParseZipFileFromWebAsync(), TimeSpan.FromMinutes(1));
    RecurringJob.AddOrUpdate<WebParsingUtils>(
        wp => wp.ParseZipFileFromWebAsync(), Cron.Monthly);
    RecurringJob.AddOrUpdate<BlobService>(
        b => b.CleanBlobStorage(), Cron.Monthly);
}

app.MapControllers();

await app.RunAsync();
public partial class Program
{
}