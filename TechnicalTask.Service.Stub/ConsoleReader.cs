using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TechnicalTask.Service.DAL;

namespace TechnicalTask.Service.Stub
{
    public class ConsoleReader : BackgroundService //Фоновий сервіс для ручного пропуску відправки певних об'єктів в демонстраційних цілях
    {
        DatabaseService _databaseService;
        ILogger<ConsoleReader> _logger;
        public static List<int> ints = new List<int>(); //Глобальний список Id об'єктів, які треба пропустити
        public ConsoleReader(DatabaseService databaseService, ILogger<ConsoleReader> logger) 
        { 
            _databaseService = databaseService;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (ints.Count == 0)
                    Console.Write("Enter the IDs of the objects whose data does not need to be transferred -> ");
                else
                {
                    string op = "";
                    foreach (int i in ints)
                        op += i + ", ";
                    Console.Write(op + " -> ");
                }

                try //Відловлюємо помилки вводу в консоль
                {
                    var input = Console.ReadLine().Split(',');
                    foreach (var line in input)
                    {
                        var i = int.Parse(line);
                        if (!ints.Contains(i))
                            ints.Add(i);
                        else
                            ints.Remove(i);
                    }
                    await Task.Delay(1000, stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.Write(ex.ToString());
                }
            }
        }
    }
}
