using System.ComponentModel.DataAnnotations;

namespace TimesheetSystem.Models
{
    public class TimesheetEntry
    {
        public int TimeSheetEntryId { get; set; }

        [Required(ErrorMessage = "User is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Project is required")]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Date is required")]
        public DateOnly Date { get; set; }

        [Required(ErrorMessage = "Hours is required")]
        [Range(0.01, 24.0, ErrorMessage = "Hours must be between 0.01 and 24.0")] //assuming max 24 hours per day, ignoring crazy daylight-savings erors?
        public decimal Hours { get; set; } //decimal should prevent float rounding drift

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = "";
    }
}
