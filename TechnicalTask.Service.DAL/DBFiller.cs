using TechnicalTask.Service.DAL;
using TechnicalTask.Entities;

namespace TechnicalTask.Service.Stub
{
    public class DBFiller //Клас для автоматичного заповнення бази данних
    {
        DatabaseService databaseService;
        Random random = new Random();
        public DBFiller(DatabaseService databaseService)
        {
            this.databaseService = databaseService;
        }

        public void FillTables(int countKeys, int countObjs)
        {
            
            for (int i = 0; i < countKeys; i++)
            {
                if (i == 0) //Перший ключ буде мати всі об'єкти
                {
                    databaseService.AddKey(GenerateRandomString(8));
                    for (int j = 0; j < countObjs; j++)
                    {
                        if (!databaseService.ConnectObjAndKey(i+1, j+1))
                            j--;
                    }
                }
                else
                {
                    if (!databaseService.AddKey(GenerateRandomString(8)))
                    {
                        i--;
                        continue;
                    }
                    for (int j = 0; j < random.Next(3, 11); j++) //Можна задати кількість об'єктів на один ключ
                    {
                        if (!databaseService.ConnectObjAndKey(i+1, random.Next(1, countObjs+1)))
                            j--;
                    }
                }
            }
            for (int i = 0; i < countObjs; i++)
            {
                databaseService.AddObject(new EntObject(0, random.NextDouble()*360, (- 90 + random.NextDouble()*180), (-180 + random.NextDouble()*360)));
            }

        }

        string GenerateRandomString(int length) //Створення рандомних ключів
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
