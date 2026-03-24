using Microsoft.Extensions.DependencyInjection;
using SpotBoardingHouse.Application.Interfaces.Services;
using SpotBoardingHouse.Application.Services;

namespace SpotBoardingHouse.Application
{
    public static class IServiceCollectionExtension
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Services
            services.AddScoped<IRoleService, RoleService>();
            return services;
        }
    }
}
