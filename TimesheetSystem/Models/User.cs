using System.ComponentModel.DataAnnotations;

namespace TimesheetSystem.Models
{
    //we're not creating users, so I won't validate data here
    public class User
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public User(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
