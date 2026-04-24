using Microsoft.EntityFrameworkCore;
using Social_Media_Chatting_APP_Persistence.DbContext;

namespace Social_Media_Chatting_APP_Web.Extensions
{
    public static class WebAppRegistrations
    {
        public static async Task<WebApplication> MigrateDatabaseAsync(this WebApplication app)
        {
            await using var scope = app.Services.CreateAsyncScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<Social_Media_Chatting_APP_DbContext>();

            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                await dbContext.Database.MigrateAsync();
            }

            return app;
        }

        //public static async Task<WebApplication> SeedIdentityDataAsync(this WebApplication app)
        //{
        //    await using var scope = app.Services.CreateAsyncScope();
        //    var dataInitializer = scope.ServiceProvider.GetRequiredService<IDataInitializer>(); //to create initial data and auto migrations 
        //    await dataInitializer.Initialize();
        //    return app;
        //}


    }
}
