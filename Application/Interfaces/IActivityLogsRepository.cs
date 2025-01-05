using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IActivityLogsRepository
    {
        Task<IEnumerable<ActivityLog>> GetAllLogsByDateRangeAsync(DateTime dateStart, DateTime dateEnd);
    }
}
