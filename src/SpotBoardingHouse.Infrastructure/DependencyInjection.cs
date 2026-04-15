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
            services.AddScoped<IRoomOfferRepository, RoomOfferRepository>();

            services.AddScoped<IAmenityRepository, AmenityRepository>();
            services.AddScoped<IBoardingHouseAmenityRepository, BoardingHouseAmenityRepository>();
            services.AddScoped<IRoomAmenityRepository, RoomAmenityRepository>();

            services.AddScoped<IContactRepository, ContactRepository>();
            services.AddScoped<IMediaRepository, MediaRepository>();
            return services;
        }
    }
}
