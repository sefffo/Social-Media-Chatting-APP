using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Interfaces;
using Social_Media_Chatting_APP_Persistence.DbContext;
using Social_Media_Chatting_APP_Persistence.Repositories;

namespace Social_Media_Chatting_APP_Persistence.DI
{
    public static class PersistenceServicesRegistration
    {
        public static IServiceCollection AddPersistenceServicesRegistration(this IServiceCollection services)
        {
            services.AddIdentityCore<AppUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
            })
            // AddRoles registers IUserRoleStore and IRoleStore so that
            // userManager.GetRolesAsync() works. Without this, calling
            // GetRolesAsync throws NotSupportedException at runtime.
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<Social_Media_Chatting_APP_DbContext>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IMessageReadStatusRepository, MessageReadStatusRepository>();

            return services;
        }
    }
}
