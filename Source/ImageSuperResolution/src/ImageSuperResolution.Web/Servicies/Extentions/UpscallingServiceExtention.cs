using Microsoft.Extensions.DependencyInjection;

namespace ImageSuperResolution.Web.Servicies.Extentions
{
    public static class UpscallingServiceExtention
    {
        public static IServiceCollection AddUpscallingService(this IServiceCollection services, bool shallClearData = false)
        {
            return services.AddSingleton<IUpscallingService>(new UpscallingService(shallClearData));
        }
    }
}