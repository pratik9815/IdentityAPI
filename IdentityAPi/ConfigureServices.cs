using System.Text;
using System.Text.Json;
using Application.Common;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.DTOs.Auth.Validators;
using Domain.Interfaces;
using FluentValidation;
using FluentValidation.AspNetCore;
using IdentityAPi.ExceptionHandlingMiddleware;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace IdentityAPi;

public static class ConfigureServices
{
    public static IServiceCollection AddIdentityApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        //configure fluent validation globally
        services.AddControllers(options =>
        {
            options.Filters.Add<ValidationFilter>(); // Auto validation handling
        })
        .ConfigureApiBehaviorOptions(options =>
        {
            options.SuppressModelStateInvalidFilter = true; // disable automatic 400
        });
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
        services.AddFluentValidationAutoValidation(); // enables automatic validation
        services.AddFluentValidationClientsideAdapters(); // optional for client-side validation

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // JWT Settings
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();

        // Authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
            options.Events = new JwtBearerEvents
            {
                OnChallenge = context =>
                {
                    // Skip the default behavior
                    context.HandleResponse();
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    var response = ApiResponse<string>.FailureResponse(
                           "Unauthorized: Please provide a valid token.",
                           Application.Common.Enums.OperationType.None,
                           "Unauthorized."
                    );
                    var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                    return context.Response.WriteAsync(json);
                },
                OnForbidden = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";
                    var response = ApiResponse<string>.FailureResponse(
                        "Forbidden: You do not have permission to access this resource.",
                        Application.Common.Enums.OperationType.None,
                        "Forbidden."
                    );
                    var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                    return context.Response.WriteAsync(json);
                }
            };
        });
        services.AddAuthorization();


        // Repository Pattern
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        // Services
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAuditService, AuditService>();

        // MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<RegisterRequestValidator>());

        // Swagger
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Enterprise Auth API", Version = "v1" });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        // CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigins",
                policy => policy
                    .WithOrigins("http://localhost:3000")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
        });

      

        return services;
    }
}
