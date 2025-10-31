using TimesheetSystem.Models;

namespace TimesheetSystem.ViewModels
{
    public class TimesheetNavigationViewModel
    {
        public int SelectedUserId { get; set; }
        public string SelectedUserName { get; set; } = "User";
        public DateTime WeekStartDate { get; set; }
        public List<User> AvailableUsers { get; set; } = [];
    }
}
