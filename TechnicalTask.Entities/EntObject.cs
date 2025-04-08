namespace TechnicalTask.Entities
{
    public class EntObject //entity object from the database
    {
        public int Id { get; set; }
        private double _azimuth;
        public double Azimuth {
            get => _azimuth;
            set => _azimuth = Math.Clamp(value, 0, 360);
        }
        private double _latitude;
        public double Latitude {
            get => _latitude;
            set => _latitude = Math.Clamp(value, -90, 90);
        }
        private double _longitude;
        public double Longitude {
            get => _longitude;
            set => _longitude = Math.Clamp(value, -180, 180);
        }

        public EntObject() { }
        public EntObject(int _id, double _azimuth, double _latitude, double _longitude)
        {
            Id = _id;
            Azimuth = _azimuth;
            Latitude = _latitude;
            Longitude = _longitude;
        }
    }
}
