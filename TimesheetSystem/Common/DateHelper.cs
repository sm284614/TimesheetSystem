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
            // logic check: DayofWeek enum has Sunday = 0
            // here: e.g. Thurs = 3 => (7 + (3 - 1)) % 7 = 2
            // date + (-1 * 2) is 2 days back from Thurs (3) to Mon (1)
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
            //find monday, add 6, ship it off
            DateTime monday = GetMondayOfWeek(date);
            DateTime sunday = monday.AddDays(6);
            return (monday, sunday);
        }
    }
}
