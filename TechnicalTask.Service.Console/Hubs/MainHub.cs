using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using TechnicalTask.Service.DAL;

namespace TechnicalTask.Service.Console.Hubs
{
    public class MainHub : Hub //Our main SignalR Hub
    {
        private List<string> ValidKeys;
        private readonly ILogger<MainHub> _logger;
        DatabaseService _databaseService;
        public MainHub(ILogger<MainHub> logger, DatabaseService databaseService)
        {
            _logger = logger;
            _databaseService = databaseService;
            ValidKeys = _databaseService.GetAllKeysValue();
        }

        public override async Task OnConnectedAsync() //Server connection log
        {
            _logger.LogInformation("User connected: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }
        
        public async Task JoinGroup(string key) //Method for joining a group
        { 
            if (ValidKeys.Contains(key)) //Key verification
            {
                var connectionId = Context.ConnectionId;
                _logger.LogInformation("User {ConnectionId} joined group {GroupName}", Context.ConnectionId, key);
                await Groups.AddToGroupAsync(Context.ConnectionId, key);
                await Clients.Caller.SendAsync("JoinSuccess", $"You have joined the group: {key}");
            }
            else
            {
                _logger.LogInformation("User {ConnectionId} could not join, unknown key.", Context.ConnectionId);
                await Clients.Caller.SendAsync("JoinFailed", "This key does not exist.");
            }
        }

        public async Task LeaveGroup(string groupName) //Method for leaving the group
        {
            var connectionId = Context.ConnectionId;

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation("User {ConnectionId} disconnected from group {GroupName}.", Context.ConnectionId, groupName);
            await Clients.Caller.SendAsync("JoinFailed", "You leaved group");
        }
    }
}
