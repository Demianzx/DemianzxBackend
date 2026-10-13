using DemianzxBackend.Application.Common.Interfaces;
using DemianzxBackend.Domain.Constants;
using DemianzxBackend.Infrastructure.Common;
using DemianzxBackend.Infrastructure.Data;
using DemianzxBackend.Infrastructure.Data.Interceptors;
using DemianzxBackend.Infrastructure.Identity;
using DemianzxBackend.Infrastructure.Blob;
using DemianzxBackend.Infrastructure.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DemianzxBackendDb");
        Guard.Against.Null(connectionString, message: "Connection string 'DemianzxBackendDb' not found.");

        builder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
        builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
        builder.Services.AddScoped<ISlugService, SlugService>();

        var dbProvider = builder.Configuration.GetValue<string>("DbProvider")?.ToLower() ?? "sqlserver";

        builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());

            if (dbProvider == "postgres")
            {
                options.UseNpgsql(connectionString,
                    npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
            }
            else // Por defecto, usar SQL Server
            {
                options.UseSqlServer(connectionString);
            }
        });

        builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        builder.Services.AddScoped<ApplicationDbContextInitialiser>();

        builder.Services.AddAuthentication()
            .AddBearerToken(IdentityConstants.BearerScheme);

        builder.Services.AddAuthorizationBuilder();

        builder.Services
            .AddIdentityCore<ApplicationUser>(options => {
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddApiEndpoints();

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddTransient<IIdentityService, IdentityService>();
        builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
            options.TokenLifespan = TimeSpan.FromHours(1));
        builder.Services.Configure<PasswordRecoveryOptions>(
            builder.Configuration.GetSection(PasswordRecoveryOptions.SectionName));
        builder.Services.Configure<SmtpEmailOptions>(
            builder.Configuration.GetSection(SmtpEmailOptions.SectionName));
        builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();
        builder.Services.AddTransient<PasswordRecoveryService>();
        builder.Services.AddTransient<IPasswordRecoveryService>(sp => sp.GetRequiredService<PasswordRecoveryService>());
        builder.Services.AddSingleton<PasswordRecoveryQueue>();
        builder.Services.AddHostedService<PasswordRecoveryWorker>();

        var dataProtection = builder.Services.AddDataProtection().SetApplicationName("DemianzxBackend");
        var keysPath = builder.Configuration["DataProtection:KeysPath"];
        if (!string.IsNullOrWhiteSpace(keysPath))
        {
            dataProtection.PersistKeysToFileSystem(new DirectoryInfo(keysPath));
        }

        builder.Services.AddAuthorization(options =>
            options.AddPolicy(Policies.CanPurge, policy => policy.RequireRole(Roles.Administrator)));
    }
}
