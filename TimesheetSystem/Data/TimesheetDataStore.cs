using TimesheetSystem.Models;

namespace TimesheetSystem.Data
{
    public class TimesheetDataStore : ITimesheetDataStore
    {
        public IEnumerable<User> Users => throw new NotImplementedException();

        public IEnumerable<Project> Projects => throw new NotImplementedException();

        public int Add(TimesheetEntry entry)
        {
            throw new NotImplementedException();
        }

        public bool Delete(int id)
        {
            throw new NotImplementedException();
        }

        public bool ExistsForUserProjectDate(int userId, int projectId, DateTime date)
        {
            throw new NotImplementedException();
        }

        public TimesheetEntry? GetById(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TimesheetEntry> GetByUserAndWeek(int userId, DateTime weekStart)
        {
            throw new NotImplementedException();
        }

        public bool Update(TimesheetEntry entry)
        {
            throw new NotImplementedException();
        }
    }
}
