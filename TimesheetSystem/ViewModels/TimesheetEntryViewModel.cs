using TimesheetSystem.Models;

namespace TimesheetSystem.ViewModels
{
    public class TimesheetEntryViewModel
    {
        public TimesheetEntry TimesheetEntry {get; set;}
        public List<Project> Projects {get; set; }
        public List<User> Users {get; set; }
        public TimesheetEntryViewModel()
        {
            TimesheetEntry = new TimesheetEntry();
            Projects = [];
            Users = [];
            TimesheetEntry.Date = DateTime.Today;
        }
    }
}
