using TimesheetSystem.Common;
using TimesheetSystem.Data;
using TimesheetSystem.Models;

namespace TimesheetSystem.Services
{
    public class TimesheetServices : ITimesheetServices
    {
        private readonly ITimesheetDataStore _dataStore;
        private readonly IUserServices _userServices;
        private readonly IProjectServices _projectServices;
        public TimesheetServices(ITimesheetDataStore dataStore, IUserServices userService, IProjectServices projectService)
        {
            _dataStore = dataStore;
            _userServices = userService;
            _projectServices = projectService;
        }
        public ValidationResult<int> AddEntry(TimesheetEntry entry)
        {
            // Validate user exists
            var user = _userServices.GetUserById(entry.UserId);
            if (user == null)
                return ValidationResult<int>.Failure("User not found");

            // Validate project exists
            var project = _projectServices.GetProjectById(entry.ProjectId);
            if (project == null)
                return ValidationResult<int>.Failure("Project not found");

            // check project is assigned to user
            var userProjects = _dataStore.UserProjects(entry.UserId);
            if (!userProjects.Any(p => p.Id == entry.ProjectId))
                return ValidationResult<int>.Failure("Project is not assigned to the user");

            // Validate hours range
            if (entry.Hours <= 0 || entry.Hours > 24)
                return ValidationResult<int>.Failure("Hours must be between 0.1 and 24.0");

            // Check for duplicate entry (same user, project, date)
            if (_dataStore.ExistsForUserProjectDate(entry.UserId, entry.ProjectId, entry.Date))
                return ValidationResult<int>.Failure("An entry for this user, project, and date already exists");

            // Add entry to repository
            var newId = _dataStore.Add(entry);
            return ValidationResult<int>.Success(newId);
        }
        public ValidationResult<bool> EditEntry(TimesheetEntry entry)
        {
            return ValidationResult<bool>.Success(true);
        }
        public ValidationResult<bool> DeleteEntry(int id)
        {
            return ValidationResult<bool>.Success(true);
        }
        public IEnumerable<TimesheetEntry> GetEntriesForUserAndWeek(int userId, DateTime weekStart)
        {
            // Validate user 
            var user = _userServices.GetUserById(userId);
            if (user == null)
            {
                return [];
            }
            return _dataStore.GetByUserAndWeek(userId, weekStart);
        }
        //should we pass ANY weekday, then get weekstart here, or require weekstart?
        public Dictionary<int, decimal> GetTotalHoursPerProject(int userId, DateTime weekStart)
        {
            var entries = GetEntriesForUserAndWeek(userId, weekStart);

            return entries
                .GroupBy(e => e.ProjectId)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Hours));
        }

    }
}
