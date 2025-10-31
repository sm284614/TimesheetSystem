namespace TimesheetSystem.Common
{
    public static class ErrorMessages
    {
        public const string UserNotFound = "User not found";
        public const string ProjectNotFound = "Project not found";
        public const string TimesheetEntryNotFound = "Timesheet entry not found";
        public const string UnauthorisedAdd = "Not authorised to add entries for other users";
        public const string UnauthorisedEdit = "Not authorised to edit entries for other users";
        public const string UnauthorisedDelete = "Not authorised to delete entries for other users";
        public const string ProjectNotAssignedToUser = "Project is not assigned to the user";
        public const string DateMustNotBeInTheFuture = "Date must not be in the future";
        public const string InvalidHours = "Hours must be between 0.1 and 24.0";
        public const string DatabaseConnectionFailed = "Database connection failed";


    }
}
