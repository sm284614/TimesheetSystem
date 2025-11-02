using TimesheetSystem.Models;
using TimesheetSystem.Services;

namespace TimesheetSystem.ViewModels
{
    public class TimesheetEntryFormViewModel 
    {
        public TimesheetNavigationViewModel NavigationData { get; set; } = new();
        public TimesheetEntry TimesheetEntry { get; set; } = new();
        public List<Project> AvailableProjects { get; set; } = [];
    }
}
