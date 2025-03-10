using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorSaude.Interfaces
{
    public interface ISyncService
    {
        Task SyncDataAsync();
    }
}
