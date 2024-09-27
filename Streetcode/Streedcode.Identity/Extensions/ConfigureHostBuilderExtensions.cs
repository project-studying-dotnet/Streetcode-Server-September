using Streetcode.Identity.Models.Additional;

namespace Streetcode.Identity.Extensions;

public static class ConfigureHostBuilderExtensions
{
    public static void ConfigureJwt(this IServiceCollection services, WebApplicationBuilder builder)
    {
        services.Configure<JwtVariables>(builder.Configuration.GetSection("Jwt"));
    }
}
