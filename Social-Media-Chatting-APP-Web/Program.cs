using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Social_Media_APP_Web.CustomMiddlewares;
using Social_Media_Chatting_APP_Persistence.DbContext;
using Social_Media_Chatting_APP_Persistence.DI;
using Social_Media_Chatting_APP_Service.Common;
using Social_Media_Chatting_APP_Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


#region Database Connection 
builder.Services.AddDbContext<Social_Media_Chatting_APP_DbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
#endregion

#region Data Intializer


#endregion

#region Application Services
builder.Services.AddApplicationServices();
builder.Services.AddHttpContextAccessor();
#endregion
#region Persistence
builder.Services.AddPersistenceServicesRegistration();
#endregion





var app = builder.Build();


//adding custom middlewares for database migration and data seeding during application startup

app.UseMiddleware<ExceptionHandlerMiddleware>();

//await app.MigrateDatabaseAsync();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // UI → /scalar/v1

}


app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


