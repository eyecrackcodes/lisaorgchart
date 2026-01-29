using LuminaryLife.Common.Systems.OrgChart.Hubs;
using LuminaryLife.Common.Systems.OrgChart.Queries;
using LuminaryLife.Common.Systems.OrgChart.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LuminaryLife.Common.Systems.OrgChart;

/// <summary>
/// Registers OrgChart system services with the DI container
/// </summary>
public class SubSystemRegistration : ISubSystemRegistration
{
    public void Register(IServiceCollection services, string environmentName, IConfiguration configuration)
    {
        // Original services
        services.AddScoped<IOrgChartService, OrgChartService>();
        services.AddScoped<IOrgChartTagEngine, OrgChartTagEngine>();
        
        // CQRS Query services - optimized for reads
        services.AddScoped<IGraphTraversalService, GraphTraversalService>();
        
        // SignalR notifier for real-time updates
        services.AddScoped<IOrgChartNotifier, OrgChartNotifier>();
    }
}

/// <summary>
/// Interface for subsystem registration
/// </summary>
public interface ISubSystemRegistration
{
    void Register(IServiceCollection services, string environmentName, IConfiguration configuration);
}
