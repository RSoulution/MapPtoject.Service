using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using TechnicalTask.Entities;
using Microsoft.Extensions.Options;
using TechnicalTask.Service.DAL.Settings;

namespace TechnicalTask.Service.DAL
{
    public class DatabaseService //Class for working with the database
    {
        private string ConnectionString = "Data Source=SQLiteDB.db";
        private readonly ILogger<DatabaseService> _logger;

        public DatabaseService(ILogger<DatabaseService> logger, IOptions<SQLiteSettings> options)
        {
            _logger = logger;
            ConnectionString = options.Value.ConnectionString;

            EnsureDatabaseCreated();
        }

        private void EnsureDatabaseCreated()  //Checking and creating tables
        {
            ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS Keys (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Value TEXT NOT NULL UNIQUE
                );
                CREATE TABLE IF NOT EXISTS Objects (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Azimuth REAL NOT NULL,
                    Latitude REAL NOT NULL,
                    Longitude REAL NOT NULL
                );
                CREATE TABLE IF NOT EXISTS KeysToObjs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Id_Key INTEGER NOT NULL,
                    Id_Obj INTEGER NOT NULL
                );");
        }

        public bool AddKey(string keyValue) //Add key by value
        { 
            return ExecuteNonQuery("INSERT INTO Keys (Value) VALUES (@Value);", (cmd) => cmd.Parameters.AddWithValue("@Value", keyValue)); 
        } 

        public bool AddKey(Key key) => AddKey(key.Value); //Add key by instance (Id is not important, field is Autoincrement)

        public bool AddObject(EntObject entityObject) //Add object by instance (Id is not important, field is Autoincrement)
        {
            if (!IsValidObject(entityObject)) return false;

            return ExecuteNonQuery(@"INSERT INTO Objects (Azimuth, Latitude, Longitude) VALUES (@Azimuth, @Latitude, @Longitude);",
                (cmd) => {
                    cmd.Parameters.AddWithValue("@Azimuth", entityObject.Azimuth);
                    cmd.Parameters.AddWithValue("@Latitude", entityObject.Latitude);
                    cmd.Parameters.AddWithValue("@Longitude", entityObject.Longitude);
                });
        }

        public bool ConnectObjAndKey(int keyId, int objId) //Add a relationship between a key and an object in a related table
        {
            if (ExecuteScalar<int>("SELECT COUNT(*) FROM KeysToObjs WHERE Id_Key = @KeyId AND Id_Obj = @ObjId;",
                (cmd) => {
                    cmd.Parameters.AddWithValue("@KeyId", keyId);
                    cmd.Parameters.AddWithValue("@ObjId", objId);
                }) > 0)
                return false;

            return ExecuteNonQuery("INSERT INTO KeysToObjs (Id_Key, Id_Obj) VALUES (@KeyId, @ObjId);",
                (cmd) => {
                    cmd.Parameters.AddWithValue("@KeyId", keyId);
                    cmd.Parameters.AddWithValue("@ObjId", objId);
                });
        }

        public int GetKeyCount() => ExecuteScalar<int>("SELECT COUNT(*) FROM Keys;"); //Get the number of keys in a table

        public List<Key> GetAllKeys() => ExecuteQuery("SELECT Id, Value FROM Keys;", (reader) => new Key(reader.GetInt32(0), reader.GetString(1))); //Get all keys from a table

        public List<EntObject> GetAllObjects() => ExecuteQuery("SELECT Id, Azimuth, Latitude, Longitude FROM Objects;", //Get all objects
            (reader) => new EntObject(reader.GetInt32(0), reader.GetDouble(1), reader.GetDouble(2), reader.GetDouble(3)));

        public void UpdateObject(EntObject entityObject) //Change object in table
        {
            ExecuteNonQuery(@"UPDATE Objects SET Azimuth = @Azimuth, Latitude = @Latitude, Longitude = @Longitude WHERE Id = @Id;",
                (cmd) => {
                    cmd.Parameters.AddWithValue("@Id", entityObject.Id);
                    cmd.Parameters.AddWithValue("@Azimuth", entityObject.Azimuth);
                    cmd.Parameters.AddWithValue("@Latitude", entityObject.Latitude);
                    cmd.Parameters.AddWithValue("@Longitude", entityObject.Longitude);
                });
        }

        public void ClearTables() //Delete all tables
        {
            ExecuteNonQuery(@"DROP TABLE IF EXISTS Keys; DROP TABLE IF EXISTS Objects; DROP TABLE IF EXISTS KeysToObjs;");
        }

        private bool IsValidObject(EntObject obj) //Check the object for compliance
        {
            return obj.Azimuth >= 0 && obj.Azimuth <= 360 && obj.Longitude >= -180 && obj.Longitude <= 180 && obj.Latitude >= -90 && obj.Latitude <= 90;
        }

        private bool ExecuteNonQuery(string query, Action<SqliteCommand> parameterize = null)
        {
            try
            {
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();
                using var command = new SqliteCommand(query, connection);
                parameterize?.Invoke(command);
                command.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database operation failed.");
                return false;
            }
        }

        private T ExecuteScalar<T>(string query, Action<SqliteCommand> parameterize = null)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            using var command = new SqliteCommand(query, connection);
            parameterize?.Invoke(command);
            return (T)Convert.ChangeType(command.ExecuteScalar(), typeof(T));
        }

        private List<T> ExecuteQuery<T>(string query, Func<SqliteDataReader, T> map, Action<SqliteCommand> parameterize = null)
        {
            var list = new List<T>();
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            using var command = new SqliteCommand(query, connection);
            parameterize?.Invoke(command);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add(map(reader));
            }
            return list;
        }

        public List<Key> GetKeysByObj(int obj_id) //Get all keys associated with an object
        {
            var keys = new List<Key>();
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            string selectQuery = "SELECT c.Id, c.Value FROM Keys c JOIN KeysToObjs d ON c.Id = d.Id_Key WHERE d.Id_Obj = @Id_Obj;";
            using var command = new SqliteCommand(selectQuery, connection);
            command.Parameters.AddWithValue("@Id_Obj", obj_id);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                keys.Add(new Key(reader.GetInt32(0), reader.GetString(1)));
            }
            return keys;
        }

        public List<Key> GetKeysByObj(EntObject entityObject) => GetKeysByObj(entityObject.Id); 

        public List<string> GetAllKeysValue() //Get the values ​​of all keys
        {
            var keys = new List<string>();
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            string selectQuery = "SELECT Value FROM Keys;";
            using var command = new SqliteCommand(selectQuery, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                keys.Add(reader.GetString(0));
            }
            return keys;
        }
    }
}