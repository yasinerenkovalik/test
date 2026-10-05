using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Test.API.Common;
using Test.Infrastructure.Identity;

namespace Test.API.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddApiAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwt = JwtSettings.FromConfiguration(configuration);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false; // "sub", "email", "role" olduğu gibi kalsın
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = "sub",
                    RoleClaimType = "role"
                };

                // 401/403 cevapları da standart ApiResponse formatında dönsün.
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        return ApiResponseWriter.WriteAsync(context.HttpContext, StatusCodes.Status401Unauthorized,
                            "Authentication is required or the token is invalid.");
                    },
                    OnForbidden = context => ApiResponseWriter.WriteAsync(context.HttpContext,
                        StatusCodes.Status403Forbidden, "You do not have permission to perform this action.")
                };
            });

        services.AddAuthorization();
        return services;
    }
}
