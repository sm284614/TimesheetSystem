using TimesheetSystem.Models;

namespace TimesheetSystem.Services
{
    public interface IUserServices
    {
        IEnumerable<User> GetAllUsers();
        User? GetUserById(int userId);
    }
}
