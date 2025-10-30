using Microsoft.AspNetCore.Mvc;
using TimesheetSystem.Common;
using TimesheetSystem.Data;
using TimesheetSystem.Services;
using TimesheetSystem.ViewModels;

namespace TimesheetSystem.Controllers
{
    public class TimesheetController : Controller
    {
        private readonly ITimesheetServices _timesheetServices;
        private readonly IUserServices _userServices;
        private readonly IProjectServices _projectServices;
        public TimesheetController(ITimesheetServices timesheetServices, IUserServices userServices, IProjectServices projectServices)
        {
            _timesheetServices = timesheetServices;
            _userServices = userServices;
            _projectServices = projectServices;
        }
        public IActionResult Index(int? userId, DateTime? weekStart)
        {
            var selectedWeekStart = weekStart ?? DateHelper.GetMondayOfWeek(DateTime.Today);
            var selectedUserId = userId ?? 0;

            var viewModel = new WeeklyTimesheetViewModel
            {
                SelectedUserId = selectedUserId,
                WeekStartDate = selectedWeekStart,
                AvailableUsers = _userServices.GetAllUsers().ToList()
            };
            // Only load entries if a user is selected
            if (selectedUserId > 0)
            {
                viewModel.Entries = _timesheetServices.GetEntriesForUserAndWeek(selectedUserId, selectedWeekStart).ToList();

                // Calculate project totals
                var projectHours = _timesheetServices.GetTotalHoursPerProject(selectedUserId, selectedWeekStart);
                var projects = _projectServices.GetAllProjects().ToList();

                viewModel.ProjectTotals = projectHours.Select(ph => new ProjectHoursViewModel
                {
                    ProjectId = ph.Key,
                    ProjectName = projects.FirstOrDefault(p => p.Id == ph.Key)?.Name ?? "Unknown",
                    TotalHours = ph.Value
                }).ToList();
            }
            return View(viewModel);
        }
        public IActionResult Create(int? userId, DateTime? weekStart)
        {
            var viewModel = new TimesheetEntryViewModel
            {

            };
            return View(viewModel);
        }
        public IActionResult ProjectTotals()
        {
            return View();
        }
        public IActionResult UserWeek()
        {
            return View();
        }

    }
}
