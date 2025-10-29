using System.ComponentModel.DataAnnotations;

namespace TimesheetSystem.Models
{
    public class Project
    {
        public int ProjectId { get; set; }
        public string Name { get; set; } = "";
    }
}
