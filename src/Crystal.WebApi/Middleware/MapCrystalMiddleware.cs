namespace WebApi.Middleware;

public static class MapCrystalMiddleware
{
    public static IApplicationBuilder UseCrystalMiddlewares(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<CorrelationIdMiddleware>();
        return builder;
    }
}