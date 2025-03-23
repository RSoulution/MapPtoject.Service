using TechnicalTask.Service.Console;
using TechnicalTask.Service.Console.Hubs;
using TechnicalTask.Service.DAL;
using TechnicalTask.Service.Stub;

    //�� ������� ����������������� ���� ��� ��� ���������� ������� �� 100 ��'����
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
        builder.Services.AddSignalR().AddJsonProtocol(options => //��������� SignalR �� ������ �����������, ��� �������� ������ ����� ��'����
        {
            options.PayloadSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            options.PayloadSerializerOptions.WriteIndented = true;
        }); ;
        builder.Services.AddLogging(logging => //������ ���������
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.AddDebug();
        });
        builder.Services.AddSingleton<DatabaseService>(); //��������� ������ � ��
        builder.Services.AddHostedService<SignalRNotifier>(); //������� ������ ��� ���������� ��� ��'���� �� ������ 
        builder.Services.AddHostedService<ConsoleReader>(); //������ ��� ����� � ������ ��'���� �� �������
        builder.Services.AddHostedService<ObjectsMover>(); //������ ��� ���������� ��'����
        var app = builder.Build();


        app.MapHub<MainHub>("/main"); //����� � ��� 


        app.Run();
        //ϳ��� ������� ����� ������ ����� ����� � �������, ��� ConsoleReader ������� ��� ���� � ��� ��������� ��� ����������� SignalR �������
    }
}