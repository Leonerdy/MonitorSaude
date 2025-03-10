using Google.Apis.Fitness.v1;
using Google.Apis.Fitness.v1.Data;
using MonitorSaude.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorSaude.Interfaces
{
    public interface IUserProfileMapper
    {
        Task<UserProfile> Map(ListDataSourcesResponse response, FitnessService fitnessService);
    }
}
