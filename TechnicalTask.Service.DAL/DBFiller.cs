using TechnicalTask.Service.DAL;
using TechnicalTask.Entities;

namespace TechnicalTask.Service.Stub
{
    public class DBFiller //Class for automatic database filling
    {
        DatabaseService databaseService;
        Random random = new Random();
        public DBFiller(DatabaseService databaseService)
        {
            this.databaseService = databaseService;
        }

        public void FillTables(int countKeys, int countObjs, int minKeys, int maxKeys)
        {
            
            for (int i = 0; i < countKeys; i++)
            {
                if (i == 0) //The first key will have all the objects
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
                    for (int j = 0; j < random.Next(minKeys, maxKeys); j++) //You can set the number of objects per key
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

        string GenerateRandomString(int length) //Generating random keys
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
