using Microsoft.AspNetCore.Mvc;
using TimesheetSystem.Common;
using TimesheetSystem.Data;
using TimesheetSystem.Models;
using TimesheetSystem.Services;
using TimesheetSystem.ViewModels;

namespace TimesheetSystem.Controllers
{
    //I don't think I want to split this into multiple controllers for this task: everything is still timesheets
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
                NavigationData = new TimesheetNavigationViewModel
                {
                    SelectedUserId = selectedUserId,
                    WeekStartDate = selectedWeekStart,
                    AvailableUsers = _userServices.GetAllUsers().ToList()
                },
            };
            // Only load entries or calculate totals if a user is selected 
            if (selectedUserId > 0)
            {
                //all user entries
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
        public IActionResult Add(int? userId, DateTime? weekStart)
        {
            var selectedWeekStart = weekStart ?? DateHelper.GetMondayOfWeek(DateTime.Today); //weekstart should already be Monday!
            var selectedUserId = userId ?? 0;
            var projects = _projectServices.GetProjectsByUserId(selectedUserId) ?? [];
            var viewModel = new TimesheetEntryFormViewModel
            {
                NavigationData = new TimesheetNavigationViewModel
                {
                    SelectedUserId = selectedUserId,
                    WeekStartDate = selectedWeekStart,
                    AvailableUsers = _userServices.GetAllUsers().ToList()
                },
                TimesheetEntry = new TimesheetEntry()
                {
                    UserId = selectedUserId,
                    Date = selectedWeekStart,
                    AvailableProjects = projects.ToList(),
                }
            };

            return View(viewModel);
        }
        [HttpPost]
        public IActionResult AddEntry(TimesheetEntryFormViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                RepopulateFormData(viewModel);
                return View("Add", viewModel);
            }
            var result = _timesheetServices.AddEntry(viewModel.TimesheetEntry, viewModel.NavigationData.SelectedUserId);
            if (!result.IsSuccess)
            {
                RepopulateFormData(viewModel);
                TempData["Error"] = result.ErrorMessage;
                return View("Add", viewModel);
            }
            TempData["Success"] = "Timesheet entry added successfully.";
            // redirect back to index
            return RedirectToAction(
                "Index",
                new { userId = viewModel.NavigationData.SelectedUserId, weekStart = viewModel.NavigationData.WeekStartDate.ToString("yyyy-MM-dd") }
            );
        }
        public IActionResult Edit(int timesheetEntryId, int? userId, DateTime? weekStart)
        {
            // Get the existing entry
            var existingEntry = _timesheetServices.GetEntryById(timesheetEntryId);

            if (existingEntry == null)
            {
                TempData["Error"] = "Timesheet entry not found.";
                return RedirectToAction("Index");
            }

            var selectedWeekStart = weekStart ?? DateHelper.GetMondayOfWeek(existingEntry.Date);
            var selectedUserId = userId ?? existingEntry.UserId;

            var projects = _projectServices.GetProjectsByUserId(selectedUserId) ?? [];

            var viewModel = new TimesheetEntryFormViewModel
            {
                NavigationData = new TimesheetNavigationViewModel
                {
                    SelectedUserId = selectedUserId,
                    WeekStartDate = selectedWeekStart,
                    AvailableUsers = _userServices.GetAllUsers().ToList()
                },
                TimesheetEntry = existingEntry
            };

            viewModel.TimesheetEntry.AvailableProjects = projects.ToList();

            return View("Add", viewModel); // Reuse the Add view
        }
        [HttpPost]
        public IActionResult EditEntry(TimesheetEntryFormViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                RepopulateFormData(viewModel);
                return View("Add", viewModel);
            }

            var result = _timesheetServices.EditEntry(viewModel.TimesheetEntry, viewModel.NavigationData.SelectedUserId);

            if (!result.IsSuccess)
            {
                RepopulateFormData(viewModel);
                TempData["Error"] = result.ErrorMessage;
                return View("Add", viewModel);
            }

            TempData["Success"] = "Timesheet entry updated successfully.";
            return RedirectToAction(
                "Index",
                new
                {
                    userId = viewModel.NavigationData.SelectedUserId,
                    weekStart = viewModel.NavigationData.WeekStartDate.ToString("yyyy-MM-dd")
                }
            );
        }
        [HttpGet]
        public IActionResult DeleteEntry(int id, int userId, DateTime weekStart)
        {
            var result = _timesheetServices.DeleteEntry(id, userId);
            if (!result.IsSuccess)
            {
                TempData["Error"] = "Failed to delete timesheet entry.";
            }
            else
            {
                TempData["Success"] = "Timesheet entry deleted successfully.";
            }
            // redirect back to index
            return RedirectToAction(
                "Index",
                new
                {
                    userId,
                    weekStart = weekStart.ToString("yyyy-MM-dd")
                }
            );
        }
        public IActionResult ProjectTotals()
        {
            return View();
        }
        public IActionResult UserWeek()
        {
            return View();
        }
        private void RepopulateFormData(TimesheetEntryFormViewModel viewModel)
        {
            viewModel.TimesheetEntry.AvailableProjects = _projectServices.GetProjectsByUserId(viewModel.TimesheetEntry.UserId).ToList();
            viewModel.NavigationData.AvailableUsers = _userServices.GetAllUsers().ToList();
        }
    }
}
