using TimesheetSystem.Models;

namespace TimesheetSystem.Services
{
    public interface IProjectServices
    {
        IEnumerable<Project> GetAllProjects();
        Project? GetProjectById(int projectId);
        IEnumerable<Project> GetProjectsByUserId(int userId);
    }
}
