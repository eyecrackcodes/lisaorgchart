using Microsoft.AspNetCore.SignalR;
using LuminaryLife.Common.Systems.OrgChart.Queries;

namespace LuminaryLife.Common.Systems.OrgChart.Hubs;

/// <summary>
/// SignalR Hub for real-time org chart updates
/// Enables live synchronization during dynamic filtering and data changes
/// </summary>
public class OrgChartHub : Hub
{
    private readonly IGraphTraversalService _graphService;

    public OrgChartHub(IGraphTraversalService graphService)
    {
        _graphService = graphService;
    }

    /// <summary>
    /// Subscribe to updates for a specific node (and its subtree)
    /// </summary>
    public async Task SubscribeToNode(string nodeId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"node:{nodeId}");
        await Clients.Caller.SendAsync("Subscribed", nodeId);
    }

    /// <summary>
    /// Unsubscribe from a node's updates
    /// </summary>
    public async Task UnsubscribeFromNode(string nodeId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"node:{nodeId}");
    }

    /// <summary>
    /// Subscribe to all org chart updates
    /// </summary>
    public async Task SubscribeToAll()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "all");
        await Clients.Caller.SendAsync("SubscribedAll");
    }

    /// <summary>
    /// Request children for a node (progressive disclosure)
    /// Returns data and subscribes to updates
    /// </summary>
    public async Task<List<GraphNodeProjection>> RequestChildren(string nodeId, GraphQuery query)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"node:{nodeId}");
        return await _graphService.GetNodeChildrenAsync(nodeId, query);
    }

    /// <summary>
    /// Request node detail (for detail panel)
    /// </summary>
    public async Task<GraphNodeDetail?> RequestDetail(string nodeId)
    {
        return await _graphService.GetNodeDetailAsync(nodeId);
    }

    /// <summary>
    /// Request hierarchy overview
    /// </summary>
    public async Task<List<GraphNodeProjection>> RequestHierarchy(GraphQuery query)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "all");
        return await _graphService.GetHierarchyOverviewAsync(query);
    }

    /// <summary>
    /// Search nodes
    /// </summary>
    public async Task<List<GraphNodeProjection>> SearchNodes(string searchTerm)
    {
        return await _graphService.SearchNodesAsync(searchTerm);
    }

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "all");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "all");
        await base.OnDisconnectedAsync(exception);
    }
}

/// <summary>
/// Interface for broadcasting org chart changes to connected clients
/// </summary>
public interface IOrgChartNotifier
{
    Task NotifyNodeUpdated(string nodeId, GraphNodeProjection node);
    Task NotifyNodeDeleted(string nodeId);
    Task NotifyNodeCreated(string parentId, GraphNodeProjection node);
    Task NotifyFilterApplied(GraphQuery query, List<GraphNodeProjection> results);
    Task NotifyStatsUpdated(string nodeId, GraphNodeStats stats);
}

/// <summary>
/// Implementation of org chart notifier using SignalR
/// </summary>
public class OrgChartNotifier : IOrgChartNotifier
{
    private readonly IHubContext<OrgChartHub> _hubContext;

    public OrgChartNotifier(IHubContext<OrgChartHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyNodeUpdated(string nodeId, GraphNodeProjection node)
    {
        // Notify all clients subscribed to this node or its parent
        await _hubContext.Clients.Group($"node:{nodeId}").SendAsync("NodeUpdated", node);
        
        if (node.ParentId != null)
        {
            await _hubContext.Clients.Group($"node:{node.ParentId}").SendAsync("ChildUpdated", node);
        }
        
        // Also notify all subscribers
        await _hubContext.Clients.Group("all").SendAsync("NodeUpdated", node);
    }

    public async Task NotifyNodeDeleted(string nodeId)
    {
        await _hubContext.Clients.Group($"node:{nodeId}").SendAsync("NodeDeleted", nodeId);
        await _hubContext.Clients.Group("all").SendAsync("NodeDeleted", nodeId);
    }

    public async Task NotifyNodeCreated(string parentId, GraphNodeProjection node)
    {
        await _hubContext.Clients.Group($"node:{parentId}").SendAsync("NodeCreated", node);
        await _hubContext.Clients.Group("all").SendAsync("NodeCreated", node);
    }

    public async Task NotifyFilterApplied(GraphQuery query, List<GraphNodeProjection> results)
    {
        // Send filtered results to all connected clients
        await _hubContext.Clients.All.SendAsync("FilterApplied", new { query, results });
    }

    public async Task NotifyStatsUpdated(string nodeId, GraphNodeStats stats)
    {
        await _hubContext.Clients.Group($"node:{nodeId}").SendAsync("StatsUpdated", new { nodeId, stats });
        await _hubContext.Clients.Group("all").SendAsync("StatsUpdated", new { nodeId, stats });
    }
}
