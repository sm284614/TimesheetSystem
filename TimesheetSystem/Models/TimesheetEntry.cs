using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata.Ecma335;
using TimesheetSystem.Services;

namespace TimesheetSystem.Models
{
    public class TimesheetEntry
    {
        public int Id { get; set; }

        [Display(Name = "User")]
        [Required(ErrorMessage = "User is required")]
        public int UserId { get; set; }

        [Display(Name = "Project")]
        [Required(ErrorMessage = "Project is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Project is required")]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Date is required")]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(TimesheetEntry), nameof(ValidateDateNoFuture))]
        public DateTime Date { get; set; }

        [Display(Name = "Hours")]
        [Required(ErrorMessage = "Hours is required")]
        [Range(0.01, 24.0, ErrorMessage = "Hours must be between 0.01 and 24.0")] //assuming max 24 hours per day, ignoring crazy daylight-savings erors?
        public decimal Hours { get; set; } //decimal should prevent float rounding drift

        [Display(Name = "Description (Optional)")]
        [StringLength(255, ErrorMessage = "Description cannot exceed 255 characters")]
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
