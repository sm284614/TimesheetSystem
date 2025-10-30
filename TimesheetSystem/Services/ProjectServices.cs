using TimesheetSystem.Data;
using TimesheetSystem.Models;

namespace TimesheetSystem.Services
{
    public class ProjectServices : IProjectServices
    {
        private readonly ITimesheetDataStore _dataStore;
        public ProjectServices(ITimesheetDataStore dataStore)
        {
            _dataStore = dataStore;
        }
        public IEnumerable<Project> GetAllProjects()
        {
            return _dataStore.Projects;
        }
        public Project? GetProjectById(int projectId)
        {
            return _dataStore.Projects.FirstOrDefault(p => p.Id == projectId);
        }
        public IEnumerable<Project> GetProjectsByUserId(int userId)
        {
           return _dataStore.UserProjects(userId);
        }
    }
}
