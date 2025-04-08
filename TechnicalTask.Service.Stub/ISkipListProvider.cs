using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechnicalTask.Service.Stub
{
    public interface ISkipListProvider
    {
        bool ShouldSkip(int objectId);
    }

    public interface ISkipListManager : ISkipListProvider
    {
        void Add(int id);
        void Remove(int id);
        List<int> GetAll();
    }
}
