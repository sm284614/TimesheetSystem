using TimesheetSystem.Data;
using TimesheetSystem.Models;

namespace TimesheetSystem.Services
{
    public class UserServices : IUserServices
    {
        private readonly ITimesheetDataStore _dataStore;
        public UserServices(ITimesheetDataStore dataStore)
        {
            _dataStore = dataStore;
        }
        public IEnumerable<User> GetAllUsers()
        {
            return _dataStore.Users;
        }
        public User? GetUserById(int userId)
        {
            return _dataStore.Users.FirstOrDefault(u => u.Id == userId);
        }
    }
}
