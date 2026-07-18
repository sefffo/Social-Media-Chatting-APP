using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Social_Media_Chatting_APP_Persistence.DbContext;
using Social_Media_Chatting_APP_Persistence.DI;
using Social_Media_Chatting_APP_Presentation.Hubs;
using Social_Media_Chatting_APP_Presentation.Infrastructure.SignalR;
using Social_Media_Chatting_APP_Service.Common;
using Social_Media_Chatting_APP_ServiceAbstraction;
using Social_Media_Chatting_APP_Web.CustomMiddlewares;
using Social_Media_Chatting_APP_Web.Extensions;
using StackExchange.Redis;
using IUserIdProvider = Microsoft.AspNetCore.SignalR.IUserIdProvider;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddApplicationPart(typeof(Social_Media_Chatting_APP_Presentation.Controllers.AuthController).Assembly);

builder.Services.AddOpenApi();


#region Database Connection

builder.Services.AddDbContext<Social_Media_Chatting_APP_DbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

#endregion

#region Data Intializer

#endregion

#region Application Services

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddHttpContextAccessor();

#endregion

#region Persistence

builder.Services.AddPersistenceServicesRegistration();

#endregion


#region SignalR and Redis

var redisConnectionString = builder.Configuration.GetConnectionString("Redis")!;

// Singleton IConnectionMultiplexer — shared across the app (AuthService OTP/session storage)

var redisConnection = builder.Configuration.GetSection("ConnectionStrings")["Redis"];
var options = ConfigurationOptions.Parse(redisConnection);
options.Ssl = true;
options.AbortOnConnectFail = false;
options.ConnectRetry = 5;
options.ConnectTimeout = 10000;

var multiplexer = ConnectionMultiplexer.Connect(options);
builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);

//builder.Services.AddSingleton<IConnectionMultiplexer>(
//  ConnectionMultiplexer.Connect(redisConnectionString));

// SignalR Redis backplane — uses same Redis instance for multi-server broadcasting
builder.Services.AddSignalR()
    .AddStackExchangeRedis(redisConnectionString, opts =>
    {
        opts.Configuration.Ssl = true;
        opts.Configuration.AbortOnConnectFail = false;
    });


builder.Services.AddSingleton<IUserIdProvider, UserIdProvider>();
builder.Services.AddScoped<IRealtimeNotifier, SignalRRealtimeNotifier>();

#endregion


#region Authentication

builder.Services.AddJwtAuthentication(builder.Configuration);

#endregion

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

var app = builder.Build();

app.UseMiddleware<ExceptionHandlerMiddleware>();

//await app.MigrateDatabaseAsync();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();

app.Run();