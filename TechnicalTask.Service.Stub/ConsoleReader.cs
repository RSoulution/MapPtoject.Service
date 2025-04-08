using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TechnicalTask.Service.DAL;

namespace TechnicalTask.Service.Stub
{
    public class ConsoleReader : BackgroundService //Background service for manually skipping sending certain objects for demonstration purposes
    {
        DatabaseService _databaseService;
        ILogger<ConsoleReader> _logger;
        ISkipListManager _skipListManager;
        public ConsoleReader(DatabaseService databaseService, ILogger<ConsoleReader> logger, ISkipListManager consoleInputSkipList) 
        { 
            _databaseService = databaseService;
            _logger = logger;
            _skipListManager = consoleInputSkipList;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                List<int> ints = _skipListManager.GetAll();
                if (ints.Count == 0)
                    Console.Write("Enter the IDs of the objects whose data does not need to be transferred -> ");
                else
                {
                    Console.WriteLine(string.Join(", ", ints)+" -> ");
                }

                var parts = Console.ReadLine().Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts) {
                    int.TryParse(part.Trim(), out int id);
                    if (_skipListManager.ShouldSkip(id))
                        _skipListManager.Remove(id);
                    else
                        _skipListManager.Add(id);
                }
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
