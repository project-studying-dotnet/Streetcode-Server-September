using FluentResults;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Streetcode.BLL.Services.JwtService;
using System.Text;

namespace Streetcode.WebApi.Extensions;

public static class AuthenticationServiceExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, WebApplicationBuilder builder)
    {
        var jwtOptions = builder.Services.BuildServiceProvider().GetRequiredService<IOptions<JwtVariables>>();
        JwtVariables environment = jwtOptions.Value;

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.RequireHttpsMetadata = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(environment.Secret)),
                    ValidIssuer = environment.Issuer,
                    ValidAudience = environment.Audience,
                    ClockSkew = TimeSpan.FromMinutes(5) //tolerance to expired tokens
                };
            });

        return services;
    }
}
