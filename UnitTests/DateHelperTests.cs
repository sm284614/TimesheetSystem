using TimesheetSystem.Common;
namespace TimesheetSystem.Tests.Common
{
    public class DateHelperTests
    {
        [Theory]
        [InlineData("2025-10-27", "2025-10-27")] // Monday => Monday
        [InlineData("2025-10-28", "2025-10-27")] // Tuesdat => Monday
        [InlineData("2025-10-29", "2025-10-27")] 
        [InlineData("2025-10-30", "2025-10-27")] 
        [InlineData("2025-10-31", "2025-10-27")] 
        [InlineData("2025-11-01", "2025-10-27")] 
        [InlineData("2025-11-02", "2025-10-27")]
        public void GetMondayOfWeek_ReturnsCorrectMonday(string inputDate, string expectedMonday)
        {
            // Arrange
            var date = DateTime.Parse(inputDate);
            var expected = DateTime.Parse(expectedMonday);
            // Act
            var result = DateHelper.GetMondayOfWeek(date);
            // Assert
            Assert.Equal(expected, result);
        }
        [Theory]
        [InlineData("2025-01-06", "2025-01-06", "2025-01-12")] // Monday to Sunday
        [InlineData("2025-01-09", "2025-01-06", "2025-01-12")] // Thursday
        [InlineData("2025-01-12", "2025-01-06", "2025-01-12")] // Sunday
        public void GetWeekRange_ReturnsCorrectRange(string inputDate, string expectedStart, string expectedEnd)
        {
            // Arrange
            var date = DateTime.Parse(inputDate);
            var expectedStartDate = DateTime.Parse(expectedStart);
            var expectedEndDate = DateTime.Parse(expectedEnd);
            // Act
            var (startDate, endDate) = DateHelper.GetWeekRange(date);
            // Assert
            Assert.Equal(expectedStartDate, startDate);
            Assert.Equal(expectedEndDate, endDate);
        }
    }
}