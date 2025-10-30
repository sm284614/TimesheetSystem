namespace TimesheetSystem.ViewModels
{
    public class ProjectHoursViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public decimal TotalHours { get; set; }
    }
}
