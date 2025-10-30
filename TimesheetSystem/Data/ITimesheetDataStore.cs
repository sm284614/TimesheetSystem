using TimesheetSystem.Models;

namespace TimesheetSystem.Data
{
    public interface ITimesheetDataStore
    {
        TimesheetEntry? GetById(int id);
        IEnumerable<TimesheetEntry> GetByUserAndWeek(int userId, DateTime weekStart);
        IEnumerable<User> Users { get; }
        IEnumerable<Project> Projects { get; }
        IEnumerable<Project> UserProjects(int userId); 
        int Add(TimesheetEntry entry);
        bool Update(TimesheetEntry entry);
        bool Delete(int id);
        bool ExistsForUserProjectDate(int userId, int projectId, DateTime date);
    }
}
