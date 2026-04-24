using Microsoft.Extensions.DependencyInjection;
using Social_Media_Chatting_APP_Domain.Interfaces;
using Social_Media_Chatting_APP_Persistence.Repositories;

namespace Social_Media_Chatting_APP_Persistence.DI
{
    public static class PersistenceServicesRegistration
    {
        public static IServiceCollection AddPersistenceServicesRegistration(this IServiceCollection services)
        {

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;


        }
    }
}
