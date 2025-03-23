using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using TechnicalTask.Entities;

namespace TechnicalTask.Service.DAL
{
    public class DatabaseService //Клас для роботи з БД
    {
        private const string ConnectionString = "Data Source=SQLiteDB.db";
        private readonly ILogger<DatabaseService> _logger;

        public DatabaseService(ILogger<DatabaseService> logger)
        {
            _logger = logger;
            EnsureDatabaseCreated();
        }

        private void EnsureDatabaseCreated()  //Перевірка та сворення таблиць
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

        public bool AddKey(string keyValue) //Додати ключ за значенням
        { 
            return ExecuteNonQuery("INSERT INTO Keys (Value) VALUES (@Value);", (cmd) => cmd.Parameters.AddWithValue("@Value", keyValue)); 
        } 

        public bool AddKey(Key key) => AddKey(key.Value); //Додати ключ за екземпляром (Id не важливий, поле Автоінкремент)

        public bool AddObject(EntObject entityObject) //Додати об'єкт за екземпляром (Id не важливий, поле Автоінкремент)
        {
            if (!IsValidObject(entityObject)) return false;

            return ExecuteNonQuery(@"INSERT INTO Objects (Azimuth, Latitude, Longitude) VALUES (@Azimuth, @Latitude, @Longitude);",
                (cmd) => {
                    cmd.Parameters.AddWithValue("@Azimuth", entityObject.Azimuth);
                    cmd.Parameters.AddWithValue("@Latitude", entityObject.Latitude);
                    cmd.Parameters.AddWithValue("@Longitude", entityObject.Longitude);
                });
        }

        public bool ConnectObjAndKey(int keyId, int objId) //Додати зв'язок між ключем та об'єктом в суміжну таблицю
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

        public int GetKeyCount() => ExecuteScalar<int>("SELECT COUNT(*) FROM Keys;"); //Отримати кількість ключів в таблиці

        public List<Key> GetAllKeys() => ExecuteQuery("SELECT Id, Value FROM Keys;", (reader) => new Key(reader.GetInt32(0), reader.GetString(1))); //Отримати всі ключі з таблиці

        public List<EntObject> GetAllObjects() => ExecuteQuery("SELECT Id, Azimuth, Latitude, Longitude FROM Objects;", //Отримати всі об'єкти
            (reader) => new EntObject(reader.GetInt32(0), reader.GetDouble(1), reader.GetDouble(2), reader.GetDouble(3)));

        public void UpdateObject(EntObject entityObject) //Змінити об'єкт в таблиці
        {
            ExecuteNonQuery(@"UPDATE Objects SET Azimuth = @Azimuth, Latitude = @Latitude, Longitude = @Longitude WHERE Id = @Id;",
                (cmd) => {
                    cmd.Parameters.AddWithValue("@Id", entityObject.Id);
                    cmd.Parameters.AddWithValue("@Azimuth", entityObject.Azimuth);
                    cmd.Parameters.AddWithValue("@Latitude", entityObject.Latitude);
                    cmd.Parameters.AddWithValue("@Longitude", entityObject.Longitude);
                });
        }

        public void ClearTables() //Видалити всі таблиці
        {
            ExecuteNonQuery(@"DROP TABLE IF EXISTS Keys; DROP TABLE IF EXISTS Objects; DROP TABLE IF EXISTS KeysToObjs;");
        }

        private bool IsValidObject(EntObject obj) //Перевірити об'єкт на відповідність
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

        public List<Key> GetKeysByObj(int obj_id) //Отримати всі ключі, що пов'язані з об'єктом
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

        public List<string> GetAllKeysValue() //Отримати значення всіх ключів
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