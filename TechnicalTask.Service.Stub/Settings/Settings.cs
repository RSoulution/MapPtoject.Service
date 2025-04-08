using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechnicalTask.Service.Stub.Settings
{
    public class AppSettings
    {
        public int UpdateIntervalSeconds { get; set; } = 10;
    }
    public class MoveSettings
    {
        public int DistanceMin { get; set; } = 10;
        public int DistanceMax { get; set; } = 20;
    }
}
