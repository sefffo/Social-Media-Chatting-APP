using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Social_Media_Chatting_APP_Domain.Entities;

namespace Social_Media_Chatting_APP_Persistence.DbContext
{
    public class Social_Media_Chatting_APP_DbContext(DbContextOptions<Social_Media_Chatting_APP_DbContext> options) : IdentityDbContext<AppUser>(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Additional model configurations can be added here
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(Social_Media_Chatting_APP_DbContext).Assembly);

          

        }
    }
}
