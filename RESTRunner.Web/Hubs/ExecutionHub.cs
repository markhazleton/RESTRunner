using Microsoft.AspNetCore.SignalR;
using RESTRunner.Web.Models;

namespace RESTRunner.Web.Hubs;

/// <summary>
/// SignalR hub for real-time execution updates
/// </summary>
public class ExecutionHub : Hub
{
    private readonly ILogger<ExecutionHub> _logger;

    public ExecutionHub(ILogger<ExecutionHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Join an execution tracking group
    /// </summary>
    /// <param name="executionId">Execution ID to track</param>
    public async Task JoinExecution(string executionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GetExecutionGroup(executionId));
        _logger.LogInformation("Client {ConnectionId} joined execution {ExecutionId}", Context.ConnectionId, executionId);
    }

    /// <summary>
    /// Leave an execution tracking group
    /// </summary>
    /// <param name="executionId">Execution ID to stop tracking</param>
    public async Task LeaveExecution(string executionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetExecutionGroup(executionId));
        _logger.LogInformation("Client {ConnectionId} left execution {ExecutionId}", Context.ConnectionId, executionId);
    }

    /// <summary>
    /// Send progress update to execution group
    /// </summary>
    /// <param name="executionId">Execution ID</param>
    /// <param name="execution">Updated execution state</param>
    public async Task SendProgressUpdate(string executionId, TestExecution execution)
    {
        await Clients.Group(GetExecutionGroup(executionId)).SendAsync("ProgressUpdate", execution);
    }

    /// <summary>
    /// Send execution completed notification
    /// </summary>
    /// <param name="executionId">Execution ID</param>
    /// <param name="history">Completed execution history</param>
    public async Task SendExecutionCompleted(string executionId, ExecutionHistory history)
    {
        await Clients.Group(GetExecutionGroup(executionId)).SendAsync("ExecutionCompleted", history);
    }

    /// <summary>
    /// Send execution failed notification
    /// </summary>
    /// <param name="executionId">Execution ID</param>
    /// <param name="errorMessage">Error message</param>
    public async Task SendExecutionFailed(string executionId, string errorMessage)
    {
        await Clients.Group(GetExecutionGroup(executionId)).SendAsync("ExecutionFailed", new
        {
            ExecutionId = executionId,
            ErrorMessage = errorMessage,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Send execution cancelled notification
    /// </summary>
    /// <param name="executionId">Execution ID</param>
    public async Task SendExecutionCancelled(string executionId)
    {
        await Clients.Group(GetExecutionGroup(executionId)).SendAsync("ExecutionCancelled", new
        {
            ExecutionId = executionId,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Broadcast to all connected clients that a new execution started
    /// </summary>
    /// <param name="execution">New execution</param>
    public async Task BroadcastExecutionStarted(TestExecution execution)
    {
        await Clients.All.SendAsync("ExecutionStarted", execution);
    }

    private static string GetExecutionGroup(string executionId) => $"execution_{executionId}";

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception != null)
        {
            _logger.LogError(exception, "Client disconnected with error: {ConnectionId}", Context.ConnectionId);
        }
        else
        {
            _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        }
        await base.OnDisconnectedAsync(exception);
    }
}
