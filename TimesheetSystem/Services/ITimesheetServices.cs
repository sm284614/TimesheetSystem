using TimesheetSystem.Common;
using TimesheetSystem.Models;

namespace TimesheetSystem.Services
{
    public interface ITimesheetServices
    {
        Result<int> AddEntry(TimesheetEntry entry);
        Result<bool> UpdateEntry(TimesheetEntry entry);
        Result<bool> DeleteEntry(int id);
        IEnumerable<TimesheetEntry> GetEntriesForUserAndWeek(int userId, DateTime weekStart);
        Dictionary<int, decimal> GetTotalHoursPerProject(int userId, DateTime weekStart);
    }
}
