namespace TimesheetSystem.Common
{
    public static class ErrorMessages
    {
        public const string UserNotSelected = "User is required";
        public const string ProjectNotSelected = "Project is required";
        public const string DateNotSelected = "Date is required";
        public const string UserNotFound = "User not found";
        public const string ProjectNotFound = "Project not found";
        public const string TimesheetEntryNotFound = "Timesheet entry not found";
        public const string UnauthorisedAdd = "Not authorised to add entries for other users";
        public const string UnauthorisedEdit = "Not authorised to edit entries for other users";
        public const string UnauthorisedDelete = "Not authorised to delete entries for other users";
        public const string ProjectNotAssignedToUser = "Project is not assigned to the user";
        public const string DateMustNotBeInTheFuture = "Date must not be in the future";
        public const string InvalidHours = "Hours must be between 0.05 and 23.0";
        public const string DescriptionExceedsMaximumLength = "Description cannot exceed 255 characters";
        public const string DatabaseConnectionFailed = "Database connection failed";
        public const string ErrorAddingTimesheetEntry = "Error adding timesheet entry";
        public const string ErrorUpdatingTimesheetEntry = "Error updating timesheet entry";
        public const string ErrorDeletingTimesheetEntry = "Error deleting timesheet entry";

    }
}
