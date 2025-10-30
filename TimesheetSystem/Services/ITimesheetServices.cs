using TimesheetSystem.Common;
using TimesheetSystem.Models;

namespace TimesheetSystem.Services
{
    public interface ITimesheetServices
    {
        ValidationResult<int> AddEntry(TimesheetEntry entry, int currentUserId);
        ValidationResult<bool> EditEntry(TimesheetEntry entry, int currentUserId);
        ValidationResult<bool> DeleteEntry(int id, int currentUserId);
        TimesheetEntry? GetEntryById(int id);
        IEnumerable<TimesheetEntry> GetEntriesForUserAndWeek(int userId, DateTime weekStart);
        Dictionary<int, decimal> GetTotalHoursPerProject(int userId, DateTime weekStart);

    }
}
