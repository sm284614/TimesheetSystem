using TimesheetSystem.Common;
using TimesheetSystem.Models;

namespace TimesheetSystem.Services
{
    public interface ITimesheetServices
    {
        ValidationResult<int> AddEntry(TimesheetEntry entry);
        ValidationResult<bool> EditEntry(TimesheetEntry entry);
        ValidationResult<bool> DeleteEntry(int id, int userId);
        TimesheetEntry? GetEntryById(int id);
        IEnumerable<TimesheetEntry> GetEntriesForUserAndWeek(int userId, DateTime weekStart);
        Dictionary<int, decimal> GetTotalHoursPerProject(int userId, DateTime weekStart);

    }
}
