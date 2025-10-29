using TimesheetSystem.Common;
using TimesheetSystem.Models;

namespace TimesheetSystem.Services
{
    public class TimesheetService : ITimesheetServices
    {
        public Result<int> AddEntry(TimesheetEntry entry)
        {
            throw new NotImplementedException();
        }

        public Result<bool> DeleteEntry(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TimesheetEntry> GetEntriesForUserAndWeek(int userId, DateTime weekStart)
        {
            throw new NotImplementedException();
        }

        public Dictionary<int, decimal> GetTotalHoursPerProject(int userId, DateTime weekStart)
        {
            throw new NotImplementedException();
        }

        public Result<bool> UpdateEntry(TimesheetEntry entry)
        {
            throw new NotImplementedException();
        }
    }
}
