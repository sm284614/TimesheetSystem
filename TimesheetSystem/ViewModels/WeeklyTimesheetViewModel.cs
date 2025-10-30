using TimesheetSystem.Models;

namespace TimesheetSystem.ViewModels
{
    public class WeeklyTimesheetViewModel
    {
        public TimesheetNavigationViewModel NavigationData { get; set; } = new();
        public List<TimesheetEntry> Entries { get; set; } = [];
        public List<ProjectHoursViewModel> ProjectTotals { get; set; } = [];
    }
}
