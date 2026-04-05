using Microsoft.Extensions.DependencyInjection;
using SpotBoardingHouse.Application.Interfaces.Repositories;
using SpotBoardingHouse.Infrastructure.Repositories;

namespace SpotBoardingHouse.Infrastructure
{
    public static class IServiceCollectionExtension
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IBoardingHouseRepository, BoardingHouseRepository>();
            services.AddScoped<IRoomTypeRepository, RoomTypeRepository>();
            services.AddScoped<IRoomRepository, RoomRepository>();
            return services;
        }
    }
}
