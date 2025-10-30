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
            return from userProjects in TestData.UserProjects
                   join project in TestData.Projects on userProjects.ProjectId equals project.Id
                   where userProjects.UserId == userId
                   select project;

            // SELECT * AS project FROM userProjects JOIN Projects P ON userProjects.ProjectId = P.Id WHERE userProjects.UserId = userId
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
            var weekEnd = weekStart.AddDays(7);
            return TimesheetEntries.Where(e =>
                e.UserId == userId &&
                e.Date >= weekStart &&
                e.Date < weekEnd)
                .OrderBy(e => e.Date)
                .ToList();
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
