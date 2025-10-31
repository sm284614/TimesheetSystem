using System.ComponentModel.DataAnnotations;
using TimesheetSystem.Common;

namespace TimesheetSystem.Models
{
    //notes: there's lots to add here in reality (approved, billable, invoiced, etc) but out of scope for this task
    public class TimesheetEntry
    {
        public int Id { get; set; }

        [Display(Name = "User")]
        [Required(ErrorMessage = ErrorMessages.UserNotSelected)]
        public int UserId { get; set; }

        [Display(Name = "Project")]
        [Required(ErrorMessage = ErrorMessages.ProjectNotSelected)]
        [Range(1, int.MaxValue, ErrorMessage = ErrorMessages.ProjectNotSelected)] 
        public int ProjectId { get; set; }

        [Required(ErrorMessage = ErrorMessages.DateNotSelected)]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(TimesheetEntry), nameof(ValidateDateNoFuture))]
        public DateTime Date { get; set; }

        [Display(Name = "Hours")]
        [Required(ErrorMessage = "Hours is required")]
        [Range(0.05, 23.0, ErrorMessage = ErrorMessages.InvalidHours)] //assuming max 24 hours per day, ignoring crazy daylight-savings erors?
        public decimal Hours { get; set; } //decimal should prevent float rounding drift

        [Display(Name = "Description (Optional)")]
        [StringLength(255, ErrorMessage = ErrorMessages.DescriptionExceedsMaximumLength)]
        public string? Description { get; set; } //if this isn't nullable, it's made required by default and interferes with validation (don't set to = "")
        //TODO: AvailablePorjects isn't part of the model: move to viewbag?
        public List<Project> AvailableProjects { get; set; } = []; 
        public TimesheetEntry()
        {           
            Date = DateTime.Today;
        }
        public static ValidationResult? ValidateDateNoFuture(DateTime date, ValidationContext context)
        {
            //defaults to today: potential issue with timezones (australians live in the future...)
            if (date > DateTime.Today)
            {
                return new ValidationResult(Common.ErrorMessages.DateMustNotBeInTheFuture);
            }
            return ValidationResult.Success;
        }
    }
}
