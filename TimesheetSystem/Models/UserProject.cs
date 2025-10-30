namespace TimesheetSystem.Models
{
    public class UserProject
    {
        public int UserId { get; private set; }
        public int ProjectId { get; private set; }
        public UserProject(int userId, int projectId)
        {
            UserId = userId;
            ProjectId = projectId;
        }
    }
}
