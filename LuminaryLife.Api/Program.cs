using LuminaryLife.Common.Data;
using LuminaryLife.Common.Services;
using static LuminaryLife.Common.Data.DatabaseSeeder;
using LuminaryLife.Common.Systems.OrgChart;
using LuminaryLife.Common.Systems.OrgChart.Hubs;
using LuminaryLife.Common.Systems.OrgChart.Queries;
using LuminaryLife.Common.Systems.OrgChart.Services;
using LuminaryLife.Common.Systems.Tags;
using LuminaryLife.Common.Systems.Tags.Repositories;
using LuminaryLife.Common.Systems.Tags.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "LuminaryLife Org Chart API", Version = "v1" });
});

// SignalR for real-time updates
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.MaximumReceiveMessageSize = 102400; // 100KB
});

// Database context
builder.Services.AddDbContext<CoreApiEfDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Data Source=luminarylife.db";
    options.UseSqlite(connectionString);
});

// Cache service
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();

// Register subsystems
var configuration = builder.Configuration;
var environmentName = builder.Environment.EnvironmentName;

// Tags subsystem
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<ITagService, TagService>();

// OrgChart subsystem - Original services
builder.Services.AddScoped<IOrgChartService, OrgChartService>();
builder.Services.AddScoped<IOrgChartTagEngine, OrgChartTagEngine>();

// OrgChart subsystem - CQRS Query services (optimized reads)
builder.Services.AddScoped<IGraphTraversalService, GraphTraversalService>();

// OrgChart subsystem - SignalR notifier
builder.Services.AddScoped<IOrgChartNotifier, OrgChartNotifier>();

// OrgChart subsystem - Reporting service
builder.Services.AddScoped<IReportService, ReportService>();

// CORS - Updated to support SignalR
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Required for SignalR
    });
});

// Authorization (simplified for demo)
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowVueApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Map SignalR hub
app.MapHub<OrgChartHub>("/hubs/org-chart");

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CoreApiEfDbContext>();
    context.Database.EnsureCreated();
    await DatabaseSeeder.SeedAsync(context);
    
    // Sync tags for all seeded entities
    var tagEngine = scope.ServiceProvider.GetRequiredService<IOrgChartTagEngine>();
    await tagEngine.SyncAllTagsAsync();
}

app.Run();
