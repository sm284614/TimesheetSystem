using TimesheetSystem.Models;

namespace TimesheetSystem.Data
{
    public class TimesheetDataStore : ITimesheetDataStore
    {
        private int _nextId = 1;
        public IEnumerable<User> Users => TestData.Users;
        public IEnumerable<Project> Projects => TestData.Projects;
        public IEnumerable<Project> UserProjects(int userId)
        {
            return from up in TestData.UserProjects
                   join p in TestData.Projects on up.ProjectId equals p.Id
                   where up.UserId == userId
                   select p;
        }

        public List<TimesheetEntry> TimesheetEntries = [];

        public int Add(TimesheetEntry entry)
        {
            entry.Id = _nextId++;
            TimesheetEntries.Add(entry);
            return entry.Id;
        }
        public bool Edit(TimesheetEntry entry)
        {
            TimesheetEntry? existingEntry = GetById(entry.Id);
            if (existingEntry == null)
            {
                return false;
            }
            existingEntry.UserId = entry.UserId;
            existingEntry.ProjectId = entry.ProjectId;
            existingEntry.Date = entry.Date;
            existingEntry.Hours = entry.Hours;
            existingEntry.Description = entry.Description;
            return true;
        }
        public bool Delete(int id)
        {
            TimesheetEntry? existingEntry = GetById(id);
            if (existingEntry == null)
            {
                return false;
            }
            TimesheetEntries.Remove(existingEntry);
            return true;
        }
        public TimesheetEntry? GetById(int id)
        {
           return TimesheetEntries.FirstOrDefault(e => e.Id == id);
        }
        public IEnumerable<TimesheetEntry> GetByUserAndWeek(int userId, DateTime weekStart)
        {
            throw new NotImplementedException();
        }
        public bool ExistsForUserProjectDate(int userId, int projectId, DateTime date)
        {
            return TimesheetEntries.Any(e =>
                e.UserId == userId &&
                e.ProjectId == projectId &&
                e.Date == date);
        }


    }
}
