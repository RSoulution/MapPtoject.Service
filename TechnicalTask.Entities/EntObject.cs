namespace TechnicalTask.Entities
{
    public class EntObject //сутність об'єкту з БД
    {
        public int Id { get; set; }
        public double Azimuth { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

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
