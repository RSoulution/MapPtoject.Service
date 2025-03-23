using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using TechnicalTask.Service.Console.Hubs;
using TechnicalTask.Service.DAL;
using TechnicalTask.Entities;
using TechnicalTask.Service.Stub;

namespace TechnicalTask.Service.Console
{
    public class SignalRNotifier : BackgroundService //Фоновий сервіс, який відправляє всі об'єкти в відповідні групи згідно БД
    {
        private readonly IHubContext<MainHub> _hubContext;
        private readonly DatabaseService _databaseService;
        private readonly ILogger<SignalRNotifier> _logger;
        private List<EntObject> objs;

        public SignalRNotifier(IHubContext<MainHub> hubContext, DatabaseService databaseService, ILogger<SignalRNotifier> logger)
        {
            _hubContext = hubContext;
            _databaseService = databaseService;
            _logger = logger;
            objs = databaseService.GetAllObjects() ?? new List<EntObject>();
            var keys = _databaseService.GetAllKeysValue();
            _logger.LogInformation("Keys {0}", keys.Count);
            foreach (var key in keys)
            {
                _logger.LogInformation("{0}", key);
            }

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                objs = _databaseService.GetAllObjects();
                _logger.LogInformation("Trying send objects... Objs {0}, Groups {1}", objs.Count, MainHub._groups.Count);
                try
                {
                    foreach (EntObject obj in objs) //Для кожного об'єкту
                    {
                        if(ConsoleReader.ints.Contains(obj.Id)) //Пропускаємо об'єкт, якщо він відмічений в списку на пропуск
                            continue;
                        var keys = _databaseService.GetKeysByObj(obj); //Отримуємо всі ключі пов'язані з об'єктом
                        foreach (var key in keys)
                        {
                            if(MainHub._groups.ContainsKey(key.Value)) //Якщо група з назвою ключа існує, тоді відправляємо
                                await _hubContext.Clients.Group(key.Value).SendAsync("ReceiveData", obj);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while sending group messages");
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); //Раз в 10 секунд
            }
        }
    }
}
