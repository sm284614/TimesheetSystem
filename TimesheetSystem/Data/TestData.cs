using TimesheetSystem.Models;

namespace TimesheetSystem.Data
{
    public static class TestData
    {
        public static readonly List<User> Users =
        [
            new (1, "Alice Ahmed" ),
            new (2, "Bobby Brown"),
            new(3, "Clara Clark"),
            new(4, "Donny Darko")
        ];
        public static readonly List<Project> Projects =
        [
            new (1, "Stadium"),
            new (2, "Library"),
            new(3, "Market"),
            new(4, "Station")
        ];
        public static readonly List<UserProject> UserProjects =
        [
            new (1, 1),
            new (1, 2),
            new (1, 3),
            new (1, 4),
            new (2, 1),
            new (2, 2),
            new (3, 2),
            new (3, 4),
        ];
    }
}
