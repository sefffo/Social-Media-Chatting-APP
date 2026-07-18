using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Social_Media_Chatting_APP_SharedLibrary.Settings;

namespace Social_Media_Chatting_APP_Web.Extensions;

public static class AuthRegistration
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSettings = configuration
            .GetSection("JwtSettings")
            .Get<JwtSettings>()!;

        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        services.AddAuthentication(options =>
            {
                // JWT is the default scheme for API endpoints
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            })
            // Cookie scheme — Google middleware uses this to temporarily store
            // the user's claims after the OAuth exchange, before our callback reads them.
            .AddCookie()
            // Google OAuth — handles the redirect to Google and the code exchange.
            // ClientId and ClientSecret come from App settings .json > Google section.
            .AddGoogle(options =>
            {
                options.ClientId = configuration["Google:ClientId"]!;
                options.ClientSecret = configuration["Google:ClientSecret"]!;
                // After Google redirects back, claims are stored in the Cookie scheme above.
                options.SignInScheme = Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults
                    .AuthenticationScheme;
            });

        return services;
    }
}