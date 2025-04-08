using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.FileIO;
using TechnicalTask.Entities;
using TechnicalTask.Service.DAL;
using TechnicalTask.Service.Stub.Settings;

namespace TechnicalTask.Service.Stub
{
    public class ObjectsMover : BackgroundService //Background service for automatic object movement
    {
        Random random = new Random();
        DatabaseService _databaseService;
        ILogger<ObjectsMover> _logger;
        private readonly IOptions<AppSettings> _AppOptions;
        private readonly IOptions<MoveSettings> _MoveOptions;
        List<EntObject> _objs;
        public ObjectsMover(DatabaseService databaseService, ILogger<ObjectsMover> logger, IOptions<AppSettings> options, IOptions<MoveSettings> options1)
        {
            _databaseService = databaseService;
            _logger = logger;
            _AppOptions = options;
            _MoveOptions = options1;
            _objs = databaseService.GetAllObjects();
        }

        private const double EarthRadius = 6371.0;

        public EntObject MoveObject(EntObject entityObject, double distance) //We list the coordinates of the object according to its direction and distance
        {
            double azimuthRad = ToRadians(entityObject.Azimuth);

            double latitudeRad = ToRadians(entityObject.Latitude);
            double longitudeRad = ToRadians(entityObject.Longitude);

            double newLatitudeRad = latitudeRad + (distance / EarthRadius) * Math.Cos(azimuthRad);

            double newLongitudeRad = longitudeRad + (distance / EarthRadius) * Math.Sin(azimuthRad) / Math.Cos(newLatitudeRad);

            entityObject.Latitude = ToDegrees(newLatitudeRad);
            if (entityObject.Latitude < -90) entityObject.Latitude += 180;
            if (entityObject.Latitude > 90) entityObject.Latitude -= 180;
            entityObject.Longitude = ToDegrees(newLongitudeRad);
            if (entityObject.Longitude < -180) entityObject.Longitude += 360;
            if (entityObject.Longitude > 180) entityObject.Longitude -= 360;

            entityObject.Azimuth += -30+random.NextDouble()*60;
            if (entityObject.Azimuth < 0) entityObject.Azimuth += 360.0;
            if (entityObject.Azimuth > 360) entityObject.Azimuth -= 360.0;

            return entityObject;
        }
        private static double ToRadians(double degrees) //Convert to radians
        {
            return degrees * Math.PI / 180.0;
        }
        private static double ToDegrees(double radians) //Convert to degrees
        {
            return radians * 180.0 / Math.PI;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken) //Process workflow
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                for (int i = 0; i < _objs.Count; i++)
                {
                    _objs[i] = MoveObject(_objs[i], _MoveOptions.Value.DistanceMin + random.NextDouble() * (_MoveOptions.Value.DistanceMax - _MoveOptions.Value.DistanceMin)); //Here you can change the distance traveled. I set it to a lot so it's clearly visible from the satellite.
                    _databaseService.UpdateObject(_objs[i]);
                }
                await Task.Delay(TimeSpan.FromSeconds(_AppOptions.Value.UpdateIntervalSeconds), stoppingToken); //Every 10 seconds
            }
        }
    }
}
