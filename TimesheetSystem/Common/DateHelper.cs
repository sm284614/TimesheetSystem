namespace TimesheetSystem.Common
{
    public static class DateHelper
    {
        /// <summary>
        /// Get the date of the Monday in the week of a given date
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime GetMondayOfWeek(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }
        /// <summary>
        /// Calculates the start and end dates of the week containing the specified date, assuming the week starts on
        /// Monday and ends on Sunday.
        /// </summary>
        /// <param name="date">The date for which to calculate the week range.</param>
        /// <returns>A tuple containing the start date (Monday) and end date (Sunday) of the week.</returns>
        public static (DateTime startDate, DateTime endDate) GetWeekRange(DateTime date)
        {
            // Calculate week range (Monday to Sunday)
            int daysFromMonday = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            DateTime monday = date.Date.AddDays(-daysFromMonday);
            DateTime sunday = monday.AddDays(6);

            return (monday, sunday);
        }
    }
}
