using TimesheetSystem.Models;

namespace TimesheetSystem.Data
{
    public static class TestData
    {
        public static readonly List<User> Users =
        [
            new () { UserId = 1, Name = "Alice Ahmed" },
            new () { UserId = 2, Name = "Bobby Brown" },
            new() { UserId = 3, Name = "Clara Clark" },
            new() { UserId = 4, Name = "Donny Darko" }
        ];
        public static readonly List<Project> Projects =
        [
            new () { ProjectId = 1, Name = "Stadium" },
            new () { ProjectId = 2, Name = "Library" },
            new() { ProjectId = 3, Name = "Market" },
            new() { ProjectId = 4, Name = "Station" }
        ];
    }
}
