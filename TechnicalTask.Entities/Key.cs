namespace TechnicalTask.Entities
{
    public class Key //the essence of the key from the database
    {
        public int Id { get; }
        public string Value {  get; }
        public Key() { }

        public Key(int _id, string _value) { 
            Id = _id;
            Value = _value;
        }
    }
}
