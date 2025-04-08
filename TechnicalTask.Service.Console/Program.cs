using TechnicalTask.Service.Console;
using TechnicalTask.Service.Console.Hubs;
using TechnicalTask.Service.Console.Settings;
using TechnicalTask.Service.DAL;
using TechnicalTask.Service.DAL.Settings;
using TechnicalTask.Service.Stub;
using TechnicalTask.Service.Stub.Settings;

//This part was used once to populate a table of 100 objects.
//var log = LoggerFactory.Create(builder =>
//{
//    builder.SetMinimumLevel(LogLevel.Debug)
//           .AddConsole(); 
//});
//DatabaseService databaseService = new DatabaseService(log.CreateLogger<DatabaseService>());
////databaseService.ClearTables();
//DBFiller dBFiller = new DBFiller(databaseService);
//dBFiller.FillTables(7, 100, 3, 11);

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddSignalR().AddJsonProtocol(options => //Declare SignalR and add Serialization to send an entire object class
        {
            options.PayloadSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            options.PayloadSerializerOptions.WriteIndented = true;
        }); ;
        builder.Services.AddLogging(logging => //Adding logging
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.AddDebug();
        });

        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        builder.Services.Configure<SignalRSettings>(builder.Configuration.GetSection("SignalR"));
        builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
        builder.Services.Configure<SQLiteSettings>(builder.Configuration.GetSection("SQLite"));
        builder.Services.Configure<MoveSettings>(builder.Configuration.GetSection("MoveSettings"));

        builder.Services.AddSingleton<DatabaseService>(); //Announcing work with the database
        builder.Services.AddSingleton<ConsoleInputSkipList>();
        builder.Services.AddSingleton<ISkipListProvider>(provider => provider.GetRequiredService<ConsoleInputSkipList>());
        builder.Services.AddSingleton<ISkipListManager>(provider => provider.GetRequiredService<ConsoleInputSkipList>());
        builder.Services.AddHostedService<ConsoleReader>(); //Process for entering objects from the console on a pass
        builder.Services.AddHostedService<SignalRNotifier>(); //Background process for sending all objects to groups
        builder.Services.AddHostedService<ObjectsMover>(); //Process for moving objects
        builder.WebHost.UseUrls("http://localhost:5034");
        var app = builder.Build();


        app.MapHub<MainHub>("/main"); //Channel and Hub


        app.Run();

        //After starting, you need to enter some number into the console so that ConsoleReader completes its cycle and allows the SignalR server to continue running.
    }
}