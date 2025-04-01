using TechnicalTask.Service.Console;
using TechnicalTask.Service.Console.Hubs;
using TechnicalTask.Service.DAL;
using TechnicalTask.Service.Stub;

    //Ця частина використовувалась один раз для заповнення таблиці на 100 об'єктів
//var log = LoggerFactory.Create(builder =>
//{
//    builder.SetMinimumLevel(LogLevel.Debug)
//           .AddConsole(); 
//});
//DatabaseService databaseService = new DatabaseService(log.CreateLogger<DatabaseService>());
////databaseService.ClearTables();
//DBFiller dBFiller = new DBFiller(databaseService);
//dBFiller.FillTables(7, 100);

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddSignalR().AddJsonProtocol(options => //Оголошуємо SignalR та додаємо Сереалізацію, для відправки цілого класу об'єкта
        {
            options.PayloadSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            options.PayloadSerializerOptions.WriteIndented = true;
        }); ;
        builder.Services.AddLogging(logging => //Додаємо логування
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.AddDebug();
        });
        builder.Services.AddSingleton<DatabaseService>(); //Оголошуємо роботу з БД
        builder.Services.AddHostedService<ConsoleReader>(); //Процес для вводу з консолі об'єктів на пропуск
        builder.Services.AddHostedService<SignalRNotifier>(); //Фоновий процес для розсилання всіх об'єктів по групам 
        builder.Services.AddHostedService<ObjectsMover>(); //Процес для переміщення об'єктів
        builder.WebHost.UseUrls("http://localhost:5034");
        var app = builder.Build();


        app.MapHub<MainHub>("/main"); //Канал і Хаб 


        app.Run();

        //Після запуску треба ввести якесь число в консоль, щоб ConsoleReader виконав свій цикл і дав можливість далі запуститись SignalR серверу
    }
}