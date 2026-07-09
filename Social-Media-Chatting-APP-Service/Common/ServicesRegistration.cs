using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Social_Media_Chatting_APP_Service.Common.Email;
using Social_Media_Chatting_APP_Service.Common.MappingProfiles;
using Social_Media_Chatting_APP_Service.Features.Authentication;
using Social_Media_Chatting_APP_Service.FluentValidationMiddleWare;
using Social_Media_Chatting_APP_ServiceAbstraction;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Social_Media_Chatting_APP_Service.Common.Upload;
using Social_Media_Chatting_APP_SharedLibrary.Settings;

namespace Social_Media_Chatting_APP_Service.Common
{
    public static class ServicesRegistration
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services
            , IConfiguration configuration)
        {
            var assembly = typeof(ServicesRegistration).Assembly;

            #region Real Time Notifier Service

            services.AddScoped<IRealtimeNotifier, RealTimeNotifierService>();

            #endregion


            #region Upload Service

            services.Configure<CloudinarySettings>(configuration.GetSection("Cloudinary"));
            services.AddScoped<IUploadService, UploadService>();
            // 2 — Initialize Cloudinary SDK and register as singleton
            var cloudinarySettings = configuration
                .GetSection("Cloudinary")
                .Get<CloudinarySettings>();

            var account = new Account(
                cloudinarySettings.CloudName,
                cloudinarySettings.ApiKey,
                cloudinarySettings.ApiSecret
            );

            var cloudinary = new Cloudinary(account);
            cloudinary.Api.Secure = true; // always use https

            services.AddSingleton(cloudinary);
            
            services.AddScoped<IUploadService, UploadService>();
            
            //background queue for uploading files
            services.AddSingleton<BackgroundUploadQueue>();
            services.AddHostedService<UploadBackgroundService>();

            #endregion


            #region Rate Limiting

            services.AddRateLimiter(options =>
            {
                // Global rejection handler — returns 429 with a consistent JSON body
                // WHY custom handler: default ASP.NET rate limit response is plain text
                // We want the same JSON envelope shape as the rest of our API
                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.ContentType = "application/json";
                    await context.HttpContext.Response.WriteAsync(
                        """{"isSuccess":false,"errors":[{"code":"RateLimit.Exceeded","message":"Too many requests. Please slow down and try again later."}]}""",
                        cancellationToken);
                };

                // ── STRICT: login, verify-otp, reset-password ──────────────────────
                // 5 requests per minute per IP
                // WHY Fixed Window and not Sliding Window?
                // Fixed window is slightly more lenient (burst allowed at window boundary)
                // but cheaper on memory — sliding window stores per-request timestamps.
                // For auth endpoints fixed window is sufficient protection.
                options.AddFixedWindowLimiter("StrictAuth", limiterOptions =>
                {
                    limiterOptions.PermitLimit = 5;
                    limiterOptions.Window = TimeSpan.FromMinutes(1);
                    limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiterOptions.QueueLimit = 0; // no queuing — reject immediately
                });

                // ── GENEROUS: register, resend-otp, forgot-password, refresh, 2fa ──
                // 20 requests per minute per IP
                options.AddFixedWindowLimiter("GenerousAuth", limiterOptions =>
                {
                    limiterOptions.PermitLimit = 20;
                    limiterOptions.Window = TimeSpan.FromMinutes(1);
                    limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiterOptions.QueueLimit = 0;
                });
            });

            #endregion


            #region Email Service

            // Bind EmailSettings from appsettings.json
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.Configure<AppSettings>(configuration.GetSection("URLS"));
            // Register the email service
            services.AddTransient<IEmailService, EmailService>();


            // Register the background queue as Singleton (must survive the app lifetime)
            services.AddSingleton<BackgroundEmailQueue>();

            // Register the hosted worker
            services.AddHostedService<EmailSenderBackgroundService>();

            #endregion

            services.AddScoped<IOtpService, OtpService>();
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