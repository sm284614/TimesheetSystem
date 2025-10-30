using Microsoft.AspNetCore.Mvc;
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
        public ValidationResult<int> AddEntry(TimesheetEntry entry, int currentUserId)
        {
            try
            {
                if (entry.UserId != currentUserId)
                {
                    return ValidationResult<int>.Failure("Not authorised to add entries for other users");
                }
                var validation = ValidateTimesheetEntry(entry);
                if (!validation.IsSuccess)
                {
                    return ValidationResult<int>.Failure(validation.ErrorMessage);
                }
                var newId = _dataStore.Add(entry);
                return ValidationResult<int>.Success(newId);
            }
            catch (Exception ex)
            {
                return ValidationResult<int>.Failure($"Error adding timesheet entry: {ex.Message}");
            }
        }
        public ValidationResult<bool> EditEntry(TimesheetEntry entry, int currentUserId)
        {
            try
            {
                var existingEntry = _dataStore.GetById(entry.Id);
                if (existingEntry == null)
                {
                    return ValidationResult<bool>.Failure("Timesheet entry not found");
                }
                if (existingEntry.UserId != entry.UserId || entry.UserId != currentUserId)
                {
                    return ValidationResult<bool>.Failure("Not authorised to edit entries for other users");
                }
                var validation = ValidateTimesheetEntry(entry);
                if (!validation.IsSuccess)
                {
                    return ValidationResult<bool>.Failure(validation.ErrorMessage);
                }
                bool updated = _dataStore.Update(entry);
                if (!updated)
                {
                    return ValidationResult<bool>.Failure("Failed to update timesheet entry");
                }
                return ValidationResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ValidationResult<bool>.Failure($"Error updating timesheet entry: {ex.Message}");
            }
        }
        public ValidationResult<bool> DeleteEntry(int entryId, int currentUserId)
        {
            try
            {
                var entry = _dataStore.GetById(entryId);
                if (entry == null)
                {
                    return ValidationResult<bool>.Failure("Timesheet entry not found");
                }
                if (entry.UserId != currentUserId)
                {
                    return ValidationResult<bool>.Failure("Not authorised to delete entries for other users");
                }
                _dataStore.Delete(entryId);
                return ValidationResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ValidationResult<bool>.Failure($"Error deleting timesheet entry: {ex.Message}");
            }

        }
        public TimesheetEntry? GetEntryById(int id)
        {
            return _dataStore.GetById(id);
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
        private ValidationResult<bool> ValidateTimesheetEntry(TimesheetEntry entry)
        {
            // Validate user exists
            var user = _userServices.GetUserById(entry.UserId);
            if (user == null)
                return ValidationResult<bool>.Failure("User not found");

            // Validate project exists
            var project = _projectServices.GetProjectById(entry.ProjectId);
            if (project == null)
                return ValidationResult<bool>.Failure("Project not found");

            // Check project is assigned to user
            var userProjects = _dataStore.UserProjects(entry.UserId);
            if (!userProjects.Any(p => p.Id == entry.ProjectId))
                return ValidationResult<bool>.Failure("Project is not assigned to the user");

            // Validate date
            if (entry.Date > DateTime.Today)
                return ValidationResult<bool>.Failure("Date must not be in the future");

            // Validate hours range
            if (entry.Hours <= 0 || entry.Hours > 24)
                return ValidationResult<bool>.Failure("Hours must be between 0.1 and 24.0");

            return ValidationResult<bool>.Success(true);
        }

    }
}
