using System.Text;
using Archdraw.Domain.Interfaces.UnitOfWork;
using Archdraw.Domain.Modules.Users.Entities;
using Archdraw.Infrastructure.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Archdraw.CrossCutting.IoC.Infrastructure.Identity;

public static class IdentityDependencyInjection
{
    public static void AddIdentityDependencyInjection(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentity<AppUser, IdentityRole<Guid>>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 8;
            options.User.RequireUniqueEmail = true;
            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        var key = configuration["Jwt:key"];
        if (string.IsNullOrWhiteSpace(key) || key.Length < 32)
            throw new InvalidOperationException("Jwt:key deve ser configurada com pelo menos 32 caracteres.");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.FromSeconds(30),
                ValidIssuer = configuration["TokenConfiguration:Issuer"],
                ValidAudience = configuration["TokenConfiguration:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
            };
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    var userId = context.Principal?.FindFirst("Id")?.Value;
                    var sessionId = context.Principal?.FindFirst("sid")?.Value;
                    if (!Guid.TryParse(userId, out var uid) || !Guid.TryParse(sessionId, out var sid))
                    {
                        context.Fail("Sessão ausente no token.");
                        return;
                    }

                    try
                    {
                        var unitOfWork = context.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>();
                        if (!await unitOfWork.UserSessionRepository.IsActiveAsync(sid, uid, context.HttpContext.RequestAborted))
                            context.Fail("Sessão revogada ou expirada.");
                    }
                    catch (OperationCanceledException)
                    {
                        context.Fail("Requisição cancelada.");
                    }
                }
            };
        });
    }
}
