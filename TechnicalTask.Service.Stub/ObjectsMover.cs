using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TechnicalTask.Entities;
using TechnicalTask.Service.DAL;

namespace TechnicalTask.Service.Stub
{
    public class ObjectsMover : BackgroundService //Фоновий сервіс для автоматичного переміщення об'єктів
    {
        Random random = new Random();
        DatabaseService _databaseService;
        ILogger<ObjectsMover> _logger;
        List<EntObject> _objs;
        public ObjectsMover(DatabaseService databaseService, ILogger<ObjectsMover> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
            _objs = databaseService.GetAllObjects();
        }

        private const double EarthRadius = 6371.0;

        public EntObject MoveObject(EntObject entityObject, double distance) //Перераховуємо координати об'єкту згідно його напрямку та відстані
        {
            double azimuthRad = ToRadians(entityObject.Azimuth);

            double latitudeRad = ToRadians(entityObject.Latitude);
            double longitudeRad = ToRadians(entityObject.Longitude);

            double newLatitudeRad = latitudeRad + (distance / EarthRadius) * Math.Cos(azimuthRad);

            double newLongitudeRad = longitudeRad + (distance / EarthRadius) * Math.Sin(azimuthRad) / Math.Cos(newLatitudeRad);

            entityObject.Latitude = ToDegrees(newLatitudeRad);
            if(entityObject.Latitude < -90) entityObject.Latitude += 180;
            if (entityObject.Latitude > 90) entityObject.Latitude -= 180;
            entityObject.Longitude = ToDegrees(newLongitudeRad);
            if (entityObject.Longitude < -180) entityObject.Longitude += 360;
            if (entityObject.Longitude > 180) entityObject.Longitude -= 360;
            entityObject.Azimuth += -30+random.NextDouble()*60;
            if(entityObject.Azimuth < 0) entityObject.Azimuth += 360.0;
            if (entityObject.Azimuth > 360) entityObject.Azimuth -= 360.0;

            return entityObject;
        }
        private static double ToRadians(double degrees) //Перевести в радіани
        {
            return degrees * Math.PI / 180.0;
        }
        private static double ToDegrees(double radians) //Перевести в градуси
        {
            return radians * 180.0 / Math.PI;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken) //Робочий цикл процесу
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                for (int i = 0; i < _objs.Count; i++)
                {
                    _objs[i] = MoveObject(_objs[i], 300 + random.NextDouble() * 300); //Тут можна змінити пройдену відстань. Я поставив багато, щоб було добре видно зі спутника
                    _databaseService.UpdateObject(_objs[i]);
                }
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); //Раз в 10 секунд
            }
        }
    }
}
