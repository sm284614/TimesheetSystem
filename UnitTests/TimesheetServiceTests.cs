using Moq;
using TimesheetSystem.Common;
using TimesheetSystem.Data;
using TimesheetSystem.Models;
using TimesheetSystem.Services;
using Xunit;

namespace TimesheetSystem.UnitTests
{
    public class TimesheetServicesTests
    {
        //we're effectively using the same test data as the system is for mocking
        private readonly Mock<ITimesheetDataStore> _mockDataStore;
        private readonly Mock<IUserServices> _mockUserServices;
        private readonly Mock<IProjectServices> _mockProjectServices;
        private readonly TimesheetServices _timesheetServices;

        public TimesheetServicesTests()
        {
            // Setup mocks
            _mockDataStore = new Mock<ITimesheetDataStore>();
            _mockUserServices = new Mock<IUserServices>();
            _mockProjectServices = new Mock<IProjectServices>();

            // Create service with mocked dependencies
            _timesheetServices = new TimesheetServices(
                _mockDataStore.Object,
                _mockUserServices.Object,
                _mockProjectServices.Object
            );
        }

        [Fact]
        public void AddEntry_ValidEntry_ReturnsSuccess()
        {
            // Arrange
            var currentUserId = 1;
            var entry = new TimesheetEntry
            {
                UserId = 1,
                ProjectId = 1,
                Date = DateTime.Today,
                Hours = 8.0m,
                Description = "Test work"
            };

            // Mock user, project, & project access 
            _mockUserServices.Setup(x => x.GetUserById(1)).Returns(new User(1, "Test User"));
            _mockProjectServices.Setup(x => x.GetProjectById(1)).Returns(new Project(1, "Library"));
            _mockDataStore.Setup(x => x.UserProjects(1)).Returns(new List<Project> { new Project(1, "Test Project")});
            _mockDataStore.Setup(x => x.Add(It.IsAny<TimesheetEntry>())).Returns(1);

            // Act
            var result = _timesheetServices.AddEntry(entry, currentUserId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(1, result.Value);
        }

        [Fact]
        public void AddEntry_UnauthorizedUser_ReturnsFailure()
        {
            // Arrange
            var currentUserId = 1;
            var entry = new TimesheetEntry
            {
                UserId = 2, // Different user!
                ProjectId = 1,
                Date = DateTime.Today,
                Hours = 8.0m
            };

            // Act
            var result = _timesheetServices.AddEntry(entry, currentUserId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Not authorised to add entries for other users", result.ErrorMessage);
        }   

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(24.1)]
        [InlineData(100)]
        public void AddEntry_InvalidHours_ReturnsFailure(decimal hours)
        {
            // Arrange
            var currentUserId = 1;
            var entry = new TimesheetEntry
            {
                UserId = 1,
                ProjectId = 1,
                Date = DateTime.Today,
                Hours = hours // Invalid hours!
            };

            _mockUserServices.Setup(x => x.GetUserById(1)).Returns(new User(1, "Test User"));
            _mockProjectServices.Setup(x => x.GetProjectById(1)).Returns(new Project(1, "Library"));
            _mockDataStore.Setup(x => x.UserProjects(1)).Returns(new List<Project> { new Project(1, "Test Project") });

            // Act
            var result = _timesheetServices.AddEntry(entry, currentUserId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Hours must be between 0.1 and 24.0", result.ErrorMessage);
        }

        [Fact]
        public void EditEntry_UserTriesToEditAnotherUsersEntry_ReturnsFailure()
        {
            // Arrange
            var currentUserId = 1;  // Logged in as user 1
            var existingEntry = new TimesheetEntry
            {
                Id = 1,
                UserId = 2,  // Entry belongs to user 2
                ProjectId = 1,
                Date = DateTime.Today.AddDays(-1),
                Hours = 8.0m
            };

            var editedEntry = new TimesheetEntry
            {
                Id = 1,
                UserId = 2,  // Trying to edit user 2's entry
                ProjectId = 1,
                Date = DateTime.Today.AddDays(-1),
                Hours = 10.0m  // Changed hours
            };

            // Mock the existing entry in the datastore
            _mockDataStore.Setup(x => x.GetById(1)).Returns(existingEntry);

            // Act
            var result = _timesheetServices.EditEntry(editedEntry, currentUserId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Not authorised to edit entries for other users", result.ErrorMessage);
        }

        [Fact]
        public void EditEntry_UserTriesToChangeEntryOwnership_ReturnsFailure()
        {
            // Arrange
            var currentUserId = 1;  // Logged in as user 1
            var existingEntry = new TimesheetEntry
            {
                Id = 1,
                UserId = 1,  // Entry belongs to user 1
                ProjectId = 1,
                Date = DateTime.Today.AddDays(-1),
                Hours = 8.0m
            };

            var editedEntry = new TimesheetEntry
            {
                Id = 1,
                UserId = 2,  // Trying to change ownership to user 2!
                ProjectId = 1,
                Date = DateTime.Today.AddDays(-1),
                Hours = 8.0m
            };

            // Mock the existing entry
            _mockDataStore.Setup(x => x.GetById(1)).Returns(existingEntry);

            // Act
            var result = _timesheetServices.EditEntry(editedEntry, currentUserId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Not authorised to edit entries for other users", result.ErrorMessage);
        }

    }
}