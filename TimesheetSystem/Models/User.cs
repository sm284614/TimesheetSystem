using System.ComponentModel.DataAnnotations;

namespace TimesheetSystem.Models
{
    //we're not creating users, so I won't validate data here
    public class User
    {
        public int UserId { get; set; }
        public string Name { get; set; } = "";
    }
}
