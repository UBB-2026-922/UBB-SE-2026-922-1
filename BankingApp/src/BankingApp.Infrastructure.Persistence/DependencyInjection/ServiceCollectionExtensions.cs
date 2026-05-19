namespace BankingApp.Infrastructure.Persistence.DependencyInjection;

using System.Text;
using Application.Common.Notifications;
using Application.Common.Security;
using Application.Shared.Clock;
using Application.Shared.Persistence;
using Domain.Repositories;
using Common.Clock;
using Common.Notifications;
using Common.Security;
using Data;
using Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("BankingAppDb")
                                  ?? throw new InvalidOperationException(
                                      "Configuration value 'ConnectionStrings:BankingAppDb' is missing.");
        string jwtSecret = configuration["Jwt:Secret"]
                           ?? throw new InvalidOperationException("Configuration value 'Jwt:Secret' is missing.");
        string otpSecret = configuration["Otp:Secret"]
                           ?? throw new InvalidOperationException("Configuration value 'Otp:Secret' is missing.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                };
            });
        services.AddAuthorization();

        services.Configure<SmtpSettings>(settings =>
        {
            IConfigurationSection section = configuration.GetSection("Email");
            settings.SmtpHost = section["SmtpHost"] ?? string.Empty;
            settings.SmtpPort = int.TryParse(section["SmtpPort"], out int port) ? port : 587;
            settings.SmtpUser = section["SmtpUser"] ?? string.Empty;
            settings.SmtpPass = section["SmtpPass"] ?? string.Empty;
            settings.FromAddress = section["FromAddress"] ?? string.Empty;
        });
        services.AddMemoryCache();
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString)
                .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning)));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IIdentityRepository, IdentityRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IBeneficiaryRepository, BeneficiaryRepository>();
        services.AddScoped<IBillerRepository, BillerRepository>();
        services.AddScoped<IBillPaymentRepository, BillPaymentRepository>();
        services.AddScoped<ITransferRepository, TransferRepository>();
        services.AddScoped<IForexRepository, ForexTransactionRepository>();
        services.AddScoped<IRateAlertRepository, RateAlertRepository>();
        services.AddScoped<IRecurringPaymentRepository, RecurringPaymentRepository>();
        services.AddScoped<ISavedBillerRepository, SavedBillerRepository>();

        services.AddSingleton<ISystemClock, SystemClock>();
        services.AddSingleton<IOtpAttemptTracker, OtpAttemptTracker>();
        services.AddSingleton<IOtpService>(_ => new OtpService(otpSecret));
        services.AddScoped<IHashService, HashService>();
        services.AddScoped<IJsonWebTokenService>(_ => new JsonWebTokenService(jwtSecret));
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}
