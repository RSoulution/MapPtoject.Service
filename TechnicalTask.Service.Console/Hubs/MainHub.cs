using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using TechnicalTask.Service.DAL;

namespace TechnicalTask.Service.Console.Hubs
{
    public class MainHub : Hub //Наш головний Хаб SignalR
    {
        private List<string> ValidKeys;
        private readonly ILogger<MainHub> _logger;
        DatabaseService _databaseService;
        public static readonly ConcurrentDictionary<string, HashSet<string>> _groups = new(); //Даємо доступ до активних груп для інших класів
        public MainHub(ILogger<MainHub> logger, DatabaseService databaseService)
        {
            _logger = logger;
            _databaseService = databaseService;
            ValidKeys = _databaseService.GetAllKeysValue();
        }

        public override async Task OnConnectedAsync() //Лог конекту до сервера
        {
            _logger.LogInformation("User connected: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception) //Відловлюємо дісконект і видаляємо з групи
        {
            _logger.LogInformation("User disconnected: {ConnectionId}. Reason: {Exception}",
                Context.ConnectionId, exception?.Message);
            var connectionId = Context.ConnectionId;
            var groupName = _groups.FirstOrDefault(g => g.Value.Contains(connectionId)).Key;

            if (groupName != null)
            {
                if (_groups.TryGetValue(groupName, out var connections))
                {
                    connections.Remove(connectionId);
                    if (connections.Count == 0)
                        _groups.TryRemove(groupName, out _);
                }
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            }
            await base.OnDisconnectedAsync(exception);
        }

        
        public async Task JoinGroup(string key) // Метод для приєднання до групи
        { 
            if (ValidKeys.Contains(key)) // Перевірка ключа
            {
                var connectionId = Context.ConnectionId;

                _groups.AddOrUpdate(key,
                _ => new HashSet<string> { connectionId },
                (_, connections) =>
                {
                    connections.Add(connectionId);
                    return connections;
                });
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

        public async Task LeaveGroup() // Метод для покидання до групи
        {
            var connectionId = Context.ConnectionId;
            var groupName = _groups.FirstOrDefault(g => g.Value.Contains(connectionId)).Key;

            if (groupName != null)
            {
                if (_groups.TryGetValue(groupName, out var connections))
                {
                    connections.Remove(connectionId);
                    if (connections.Count == 0)
                        _groups.TryRemove(groupName, out _);
                }
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
                _logger.LogInformation("User {ConnectionId} disconnected from group {GroupName}.", Context.ConnectionId, groupName);
                await Clients.Caller.SendAsync("JoinFailed", "You leaved group {GroupName}");
            }
        }
    }
}
