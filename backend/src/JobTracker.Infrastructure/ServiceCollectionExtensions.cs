using JobTracker.Application.Activities;
using JobTracker.Application.Common;
using JobTracker.Application.JobApplications;
using JobTracker.Infrastructure.Interceptors;
using JobTracker.Infrastructure.Persistence;
using JobTracker.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JobTracker.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(opts =>
            opts.UseSqlite(configuration.GetConnectionString("Default"))
                .AddInterceptors(new WalModeInterceptor()));

        services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
        services.AddScoped<IActivityRepository, ActivityRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
