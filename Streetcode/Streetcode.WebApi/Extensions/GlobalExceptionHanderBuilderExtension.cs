using Streetcode.WebApi.Middlewares;

namespace Streetcode.WebApi.Extensions
{
    public static class GlobalExceptionHanderBuilderExtension
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<GlobalExceptionHandlingMidlleware>();
        }
    }
}
