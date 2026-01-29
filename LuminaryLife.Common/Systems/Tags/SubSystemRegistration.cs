using LuminaryLife.Common.Systems.OrgChart;
using LuminaryLife.Common.Systems.Tags.Repositories;
using LuminaryLife.Common.Systems.Tags.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LuminaryLife.Common.Systems.Tags;

/// <summary>
/// Registers Tag system services with the DI container
/// </summary>
public class SubSystemRegistration : ISubSystemRegistration
{
    public void Register(IServiceCollection services, string environmentName, IConfiguration configuration)
    {
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<ITagService, TagService>();
    }
}
