using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Social_Media_Chatting_APP_Service.Common.Email;
using Social_Media_Chatting_APP_Service.Common.MappingProfiles;
using Social_Media_Chatting_APP_Service.Features.Authentication;
using Social_Media_Chatting_APP_Service.FluentValidationMiddleWare;
using Social_Media_Chatting_APP_ServiceAbstraction;

namespace Social_Media_Chatting_APP_Service.Common
{
    public static class ServicesRegistration
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services
            , IConfiguration configuration)
        {
            var assembly = typeof(ServicesRegistration).Assembly;


            #region Email Service

            // Bind EmailSettings from appsettings.json
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

            // Register the email service
            services.AddTransient<IEmailService, EmailService>();


            // Register the background queue as Singleton (must survive the app lifetime)
            services.AddSingleton<BackgroundEmailQueue>();

            // Register the hosted worker
            services.AddHostedService<EmailSenderBackgroundService>();

            #endregion


            services.AddScoped<IAuthService, AuthService>();
            //services.AddScoped<IEmailService, EmailService>();

            services.AddAutoMapper(assembly);

            // Registers ALL handlers in this assembly automatically
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(assembly));

            // Registers ALL validators in this assembly automatically
            // CreateProductCommandValidator gets picked up here without you doing anything
            services.AddValidatorsFromAssembly(assembly);

            // Register the pipeline — runs for EVERY command/query
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            //using transient because we want a new instance for each request and its stateless nature, and change in each request.
            //If we used singleton, it would be shared across all requests, which could lead to issues in a multi-threaded environment.
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            return services;
        }
    }
}