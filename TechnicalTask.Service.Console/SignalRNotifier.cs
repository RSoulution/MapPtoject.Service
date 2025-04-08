using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using TechnicalTask.Service.Console.Hubs;
using TechnicalTask.Service.DAL;
using TechnicalTask.Entities;
using TechnicalTask.Service.Stub;
using Microsoft.Extensions.Options;
using TechnicalTask.Service.Console.Settings;
using TechnicalTask.Service.Stub.Settings;

namespace TechnicalTask.Service.Console
{
    public class SignalRNotifier : BackgroundService //Background service that sends all objects to the appropriate groups according to the database
    {
        private readonly IHubContext<MainHub> _hubContext;
        private readonly DatabaseService _databaseService;
        private readonly ILogger<SignalRNotifier> _logger;
        private List<EntObject> _objs;
        ISkipListProvider _skipListProvider;
        private readonly IOptions<AppSettings> _options;

        public SignalRNotifier(IHubContext<MainHub> hubContext, DatabaseService databaseService, ILogger<SignalRNotifier> logger, ISkipListProvider skipListProvider, IOptions<AppSettings> options)
        {
            _hubContext = hubContext;
            _databaseService = databaseService;
            _logger = logger;
            _objs = databaseService.GetAllObjects() ?? new List<EntObject>();
            var keys = _databaseService.GetAllKeysValue();
            _logger.LogInformation("Keys {0}", keys.Count);
            foreach (var key in keys)
            {
                _logger.LogInformation("{0}", key);
            }
            _skipListProvider = skipListProvider;
            _options = options;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _objs = _databaseService.GetAllObjects();
                _logger.LogInformation("Trying send objects... Objs {0}", _objs.Count);
                try
                {
                    foreach (EntObject obj in _objs) //For each object
                    {
                        if(_skipListProvider.ShouldSkip(obj.Id)) //Skip an object if it is marked in the skip list
                            continue;
                        var keys = _databaseService.GetKeysByObj(obj); //Get all keys associated with an object
                        foreach (var key in keys)
                        {
                             await _hubContext.Clients.Group(key.Value).SendAsync("ReceiveData", obj);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while sending group messages");
                }

                await Task.Delay(TimeSpan.FromSeconds(_options.Value.UpdateIntervalSeconds), stoppingToken); //Every 10 seconds
            }
        }
    }
}
