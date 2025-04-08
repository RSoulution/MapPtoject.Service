using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechnicalTask.Service.Stub
{
    public class ConsoleInputSkipList : ISkipListManager
    {
        public readonly List<int> _ints = new();

        public void Add(int id) => _ints.Add(id);

        public void Remove(int id) => _ints.Remove(id);

        public bool ShouldSkip(int objectId) => _ints.Contains(objectId);

        public List<int> GetAll() => _ints.ToList();
    }
}
