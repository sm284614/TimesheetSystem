using TimesheetSystem.Models;

namespace TimesheetSystem.ViewModels
{
    public class WeeklyTimesheetViewModel
    {
        public int SelectedUserId { get; set; }
        public DateTime WeekStartDate { get; set; }
        public List<TimesheetEntry> Entries { get; set; } = [];
        public List<ProjectHoursViewModel> ProjectTotals { get; set; } = [];
        public List<User> AvailableUsers { get; set; } = [];
    }
}
