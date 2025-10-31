using Moq;
using static TimesheetSystem.Common.ErrorMessages;
using TimesheetSystem.Data;
using TimesheetSystem.Models;
using TimesheetSystem.Services;
using Xunit;
using TimesheetSystem.Common;
using Xunit.Sdk;
namespace TimesheetSystem.UnitTests
{
    public class TimesheetServicesTests
    {
        // note on things I'm not testing:
        // complex time issues:
        // * daylight savings clock-change days and hours worked potentially being max 23/25
        // * timezones: validataion for timesheet entry day being in the future for server, but present for client (e.g. server is East US, client in australia)
        // * dates in the far past (we should probably set a minimum date limit somewhere?)
        // * leapyears/days: should not affect anything because of week-based entry
        // concurrent user operations (e.g. entry deleted between edit and save steps)
        // connection to data source
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
        #region Add Entry Tests
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
            _mockDataStore.Setup(x => x.UserProjects(1)).Returns(new List<Project> { new Project(1, "Test Project") });
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
            Assert.Equal(ErrorMessages.UnauthorisedAdd, result.ErrorMessage);
        }
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(24.01)]
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
            Assert.Equal(ErrorMessages.InvalidHours, result.ErrorMessage);
        }
        [Fact]
        public void AddEntry_UserDoesNotExist_ReturnsFailure()
        {
            // Arrange
            var currentUserId = 1;
            var entry = new TimesheetEntry
            {
                UserId = 1,
                ProjectId = 1,
                Date = DateTime.Today,
                Hours = 8.0m
            };
            // Mock user as not found
            _mockUserServices.Setup(x => x.GetUserById(1)).Returns((User?)null);
            // Act
            var result = _timesheetServices.AddEntry(entry, currentUserId);
            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.UserNotFound, result.ErrorMessage);
        }
        [Fact]
        public void AddEntry_ProjectDoesNotExist_ReturnsFailure()
        {
            // Arrange
            var currentUserId = 1;
            var entry = new TimesheetEntry
            {
                UserId = 1,
                ProjectId = 999, // Non-existent project
                Date = DateTime.Today,
                Hours = 8.0m
            };
            // Mock user exists but project does not
            _mockUserServices.Setup(x => x.GetUserById(1)).Returns(new User(1, "Test User"));
            _mockProjectServices.Setup(x => x.GetProjectById(999)).Returns((Project?)null);
            // Act
            var result = _timesheetServices.AddEntry(entry, currentUserId);
            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.ProjectNotFound, result.ErrorMessage);
        }
        [Fact]
        public void AddEntry_ProjectNotAssignedToUser_ReturnsFailure()
        {
            // Arrange
            var currentUserId = 1;
            var entry = new TimesheetEntry
            {
                UserId = 1,
                ProjectId = 2,
                Date = DateTime.Today,
                Hours = 8.0m
            };
            // Mock user and project exist, but user is not assigned to this project
            _mockUserServices.Setup(x => x.GetUserById(1)).Returns(new User(1, "Test User"));
            _mockProjectServices.Setup(x => x.GetProjectById(2)).Returns(new Project(2, "Other Project"));
            _mockDataStore.Setup(x => x.UserProjects(1)).Returns(new List<Project>
            {
                new Project(1, "Assigned Project") // User only has access to project 1, not 2
            });
            // Act
            var result = _timesheetServices.AddEntry(entry, currentUserId);
            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.ProjectNotAssignedToUser, result.ErrorMessage);
        }
        [Fact]
        public void AddEntry_FutureDate_ReturnsFailure()
        {
            // Arrange
            var currentUserId = 1;
            var entry = new TimesheetEntry
            {
                UserId = 1,
                ProjectId = 1,
                Date = DateTime.Today.AddDays(1), // Future date!
                Hours = 8.0m
            };
            _mockUserServices.Setup(x => x.GetUserById(1)).Returns(new User(1, "Test User"));
            _mockProjectServices.Setup(x => x.GetProjectById(1)).Returns(new Project(1, "Library"));
            _mockDataStore.Setup(x => x.UserProjects(1)).Returns(new List<Project> { new Project(1, "Test Project") });
            // Act
            var result = _timesheetServices.AddEntry(entry, currentUserId);
            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.DateMustNotBeInTheFuture, result.ErrorMessage);
        }
        [Fact]
        public void AddEntry_TodayDate_ReturnsSuccess()
        {
            // Arrange
            var currentUserId = 1;
            var entry = new TimesheetEntry
            {
                UserId = 1,
                ProjectId = 1,
                Date = DateTime.Today, // Today is valid
                Hours = 8.0m
            };
            _mockUserServices.Setup(x => x.GetUserById(1)).Returns(new User(1, "Test User"));
            _mockProjectServices.Setup(x => x.GetProjectById(1)).Returns(new Project(1, "Library"));
            _mockDataStore.Setup(x => x.UserProjects(1)).Returns(new List<Project> { new Project(1, "Test Project") });
            _mockDataStore.Setup(x => x.Add(It.IsAny<TimesheetEntry>())).Returns(1);
            // Act
            var result = _timesheetServices.AddEntry(entry, currentUserId);
            // Assert
            Assert.True(result.IsSuccess);
        }
        [Fact]
        public void AddEntry_PastDate_ReturnsSuccess()
        {
            // Arrange
            var currentUserId = 1;
            var entry = new TimesheetEntry
            {
                UserId = 1,
                ProjectId = 1,
                Date = DateTime.Today.AddDays(-5), // Past date is valid
                Hours = 8.0m
            };
            _mockUserServices.Setup(x => x.GetUserById(1)).Returns(new User(1, "Test User"));
            _mockProjectServices.Setup(x => x.GetProjectById(1)).Returns(new Project(1, "Library"));
            _mockDataStore.Setup(x => x.UserProjects(1)).Returns(new List<Project> { new Project(1, "Test Project") });
            _mockDataStore.Setup(x => x.Add(It.IsAny<TimesheetEntry>())).Returns(1);
            // Act
            var result = _timesheetServices.AddEntry(entry, currentUserId);
            // Assert
            Assert.True(result.IsSuccess);
        }
        [Theory]
        [InlineData(0.05)]  // Minimum valid hours
        [InlineData(24.0)]  // Maximum valid hours
        [InlineData(1.0)]
        [InlineData(8.5)]
        [InlineData(12.25)]
        public void AddEntry_ValidHoursBoundary_ReturnsSuccess(decimal hours)
        {
            // Arrange
            var currentUserId = 1;
            var entry = new TimesheetEntry
            {
                UserId = 1,
                ProjectId = 1,
                Date = DateTime.Today,
                Hours = hours // Valid boundary hours
            };
            _mockUserServices.Setup(x => x.GetUserById(1)).Returns(new User(1, "Test User"));
            _mockProjectServices.Setup(x => x.GetProjectById(1)).Returns(new Project(1, "Library"));
            _mockDataStore.Setup(x => x.UserProjects(1)).Returns(new List<Project> { new Project(1, "Test Project") });
            _mockDataStore.Setup(x => x.Add(It.IsAny<TimesheetEntry>())).Returns(1);
            // Act
            var result = _timesheetServices.AddEntry(entry, currentUserId);
            // Assert
            Assert.True(result.IsSuccess);
        }
        [Fact]
        public void AddEntry_DescriptionTooLong_ReturnsFailure()
        {
            // Arrange
            var currentUserId = 1;
            var entry = new TimesheetEntry
            {
                UserId = 1,
                ProjectId = 1,
                Date = DateTime.Today,
                Hours = 8.0m,
                Description = new string('A', 256) // 256 characters, exceeds 255 limit
            };
            _mockUserServices.Setup(x => x.GetUserById(1)).Returns(new User(1, "Test User"));
            _mockProjectServices.Setup(x => x.GetProjectById(1)).Returns(new Project(1, "Library"));
            _mockDataStore.Setup(x => x.UserProjects(1)).Returns(new List<Project> { new Project(1, "Test Project") });
            // Act
            var result = _timesheetServices.AddEntry(entry, currentUserId);
            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.DescriptionExceedsMaximumLength, result.ErrorMessage);
        }
        [Fact]
        public void AddEntry_EmptyDescription_ReturnsSuccess()
        {
            // Arrange
            var currentUserId = 1;
            var entry = new TimesheetEntry
            {
                UserId = 1,
                ProjectId = 1,
                Date = DateTime.Today,
                Hours = 8.0m,
                Description = null // Empty/null description is valid
            };
            _mockUserServices.Setup(x => x.GetUserById(1)).Returns(new User(1, "Test User"));
            _mockProjectServices.Setup(x => x.GetProjectById(1)).Returns(new Project(1, "Library"));
            _mockDataStore.Setup(x => x.UserProjects(1)).Returns(new List<Project> { new Project(1, "Test Project") });
            _mockDataStore.Setup(x => x.Add(It.IsAny<TimesheetEntry>())).Returns(1);
            // Act
            var result = _timesheetServices.AddEntry(entry, currentUserId);
            // Assert
            Assert.True(result.IsSuccess);
        }
        [Fact]
        public void AddEntry_LongDescription_ReturnsSuccess()
        {
            // Arrange
            var currentUserId = 1;
            var entry = new TimesheetEntry
            {
                UserId = 1,
                ProjectId = 1,
                Date = DateTime.Today,
                Hours = 8.0m,
                Description = new string('A', 255) // max limit
            };
            _mockUserServices.Setup(x => x.GetUserById(1)).Returns(new User(1, "Test User"));
            _mockProjectServices.Setup(x => x.GetProjectById(1)).Returns(new Project(1, "Library"));
            _mockDataStore.Setup(x => x.UserProjects(1)).Returns(new List<Project> { new Project(1, "Test Project") });
            _mockDataStore.Setup(x => x.Add(It.IsAny<TimesheetEntry>())).Returns(1);
            // Act
            var result = _timesheetServices.AddEntry(entry, currentUserId);
            // Assert
            Assert.True(result.IsSuccess);
        }
        [Theory]
        [InlineData(0.001)]  // Less than 0.01 but greater than 0
        [InlineData(0.009)]
        public void AddEntry_HoursBelowMinimumButAboveZero_ReturnsFailure(decimal hours)
        {
            // Arrange
            var currentUserId = 1;
            var entry = new TimesheetEntry
            {
                UserId = 1,
                ProjectId = 1,
                Date = DateTime.Today,
                Hours = hours  // Below minimum threshold
            };
            _mockUserServices.Setup(x => x.GetUserById(1)).Returns(new User(1, "Test User"));
            _mockProjectServices.Setup(x => x.GetProjectById(1)).Returns(new Project(1, "Library"));
            _mockDataStore.Setup(x => x.UserProjects(1)).Returns(new List<Project> { new Project(1, "Test Project") });
            // Act
            var result = _timesheetServices.AddEntry(entry, currentUserId);
            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.InvalidHours, result.ErrorMessage);
        }
        [Fact]
        public void AddEntry_NegativeProjectId_ReturnsFailure()
        {
            // Arrange
            var currentUserId = 1;
            var entry = new TimesheetEntry
            {
                UserId = 1,
                ProjectId = -1,  
                Date = DateTime.Today,
                Hours = 8.0m
            };
            _mockUserServices.Setup(x => x.GetUserById(1)).Returns(new User(1, "Test User"));
            _mockProjectServices.Setup(x => x.GetProjectById(-1)).Returns((Project?)null);  // Negative IDs won't exist!
            // Act
            var result = _timesheetServices.AddEntry(entry, currentUserId);
            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.ProjectNotFound, result.ErrorMessage);
        }
        [Fact]
        public void AddEntry_NullEntry_ReturnsFailure()
        {
            // Arrange
            var currentUserId = 1;
            TimesheetEntry? entry = null;
            // Act
            var result = _timesheetServices.AddEntry(entry!, currentUserId);
            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(ErrorMessages.ErrorAddingTimesheetEntry, result.ErrorMessage); //probably object reference message
        }
        #endregion
        #region Edit Entry Tests
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
            Assert.Equal(ErrorMessages.UnauthorisedEdit, result.ErrorMessage);
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
            Assert.Equal(ErrorMessages.UnauthorisedEdit, result.ErrorMessage);
        }
        [Fact]
        public void EditEntry_ValidEdit_ReturnsSuccess()
        {
            // Arrange
            var currentUserId = 1;
            var existingEntry = new TimesheetEntry
            {
                Id = 1,
                UserId = 1,
                ProjectId = 1,
                Date = DateTime.Today.AddDays(-1),
                Hours = 8.0m,
                Description = "Original work"
            };
            var editedEntry = new TimesheetEntry
            {
                Id = 1,
                UserId = 1,
                ProjectId = 1,
                Date = DateTime.Today.AddDays(-1),
                Hours = 10.0m,  // Changed hours
                Description = "Updated work"
            };
            // Mock the existing entry and validation requirements
            _mockDataStore.Setup(x => x.GetById(1)).Returns(existingEntry);
            _mockUserServices.Setup(x => x.GetUserById(1)).Returns(new User(1, "Test User"));
            _mockProjectServices.Setup(x => x.GetProjectById(1)).Returns(new Project(1, "Test Project"));
            _mockDataStore.Setup(x => x.UserProjects(1)).Returns(new List<Project> { new Project(1, "Test Project") });
            _mockDataStore.Setup(x => x.Update(It.IsAny<TimesheetEntry>())).Returns(true);
            // Act
            var result = _timesheetServices.EditEntry(editedEntry, currentUserId);
            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Value);
        }
        [Fact]
        public void EditEntry_EntryDoesNotExist_ReturnsFailure()
        {
            // Arrange
            var currentUserId = 1;
            var editedEntry = new TimesheetEntry
            {
                Id = 999,  // Non-existent entry
                UserId = 1,
                ProjectId = 1,
                Date = DateTime.Today.AddDays(-1),
                Hours = 8.0m
            };
            // Mock entry as not found
            _mockDataStore.Setup(x => x.GetById(999)).Returns((TimesheetEntry?)null);
            // Act
            var result = _timesheetServices.EditEntry(editedEntry, currentUserId);
            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.TimesheetEntryNotFound, result.ErrorMessage);
        }
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(24.01)]
        [InlineData(100)]
        public void EditEntry_InvalidHours_ReturnsFailure(decimal hours)
        {
            // Arrange
            var currentUserId = 1;
            var existingEntry = new TimesheetEntry
            {
                Id = 1,
                UserId = 1,
                ProjectId = 1,
                Date = DateTime.Today.AddDays(-1),
                Hours = 8.0m
            };
            var editedEntry = new TimesheetEntry
            {
                Id = 1,
                UserId = 1,
                ProjectId = 1,
                Date = DateTime.Today.AddDays(-1),
                Hours = hours  // Invalid hours!
            };
            _mockDataStore.Setup(x => x.GetById(1)).Returns(existingEntry);
            _mockUserServices.Setup(x => x.GetUserById(1)).Returns(new User(1, "Test User"));
            _mockProjectServices.Setup(x => x.GetProjectById(1)).Returns(new Project(1, "Test Project"));
            _mockDataStore.Setup(x => x.UserProjects(1)).Returns(new List<Project> { new Project(1, "Test Project") });
            // Act
            var result = _timesheetServices.EditEntry(editedEntry, currentUserId);
            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.InvalidHours, result.ErrorMessage);
        }
        [Fact]
        public void EditEntry_UserDoesNotExist_ReturnsFailure()
        {
            // Arrange
            var currentUserId = 1;
            var existingEntry = new TimesheetEntry
            {
                Id = 1,
                UserId = 1,
                ProjectId = 1,
                Date = DateTime.Today.AddDays(-1),
                Hours = 8.0m
            };
            var editedEntry = new TimesheetEntry
            {
                Id = 1,
                UserId = 1,
                ProjectId = 1,
                Date = DateTime.Today.AddDays(-1),
                Hours = 10.0m
            };
            _mockDataStore.Setup(x => x.GetById(1)).Returns(existingEntry);
            _mockUserServices.Setup(x => x.GetUserById(1)).Returns((User?)null);  // User not found
            // Act
            var result = _timesheetServices.EditEntry(editedEntry, currentUserId);
            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.UserNotFound, result.ErrorMessage);
        }
        [Fact]
        public void EditEntry_ProjectDoesNotExist_ReturnsFailure()
        {
            // Arrange
            var currentUserId = 1;
            var existingEntry = new TimesheetEntry
            {
                Id = 1,
                UserId = 1,
                ProjectId = 1,
                Date = DateTime.Today.AddDays(-1),
                Hours = 8.0m
            };
            var editedEntry = new TimesheetEntry
            {
                Id = 1,
                UserId = 1,
                ProjectId = 999,  // Non-existent project
                Date = DateTime.Today.AddDays(-1),
                Hours = 8.0m
            };
            _mockDataStore.Setup(x => x.GetById(1)).Returns(existingEntry);
            _mockUserServices.Setup(x => x.GetUserById(1)).Returns(new User(1, "Test User"));
            _mockProjectServices.Setup(x => x.GetProjectById(999)).Returns((Project?)null);  // Project not found
            // Act
            var result = _timesheetServices.EditEntry(editedEntry, currentUserId);
            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.ProjectNotFound, result.ErrorMessage);
        }
        [Fact]
        public void EditEntry_ProjectNotAssignedToUser_ReturnsFailure()
        {
            // Arrange
            var currentUserId = 1;
            var existingEntry = new TimesheetEntry
            {
                Id = 1,
                UserId = 1,
                ProjectId = 1,
                Date = DateTime.Today.AddDays(-1),
                Hours = 8.0m
            };
            var editedEntry = new TimesheetEntry
            {
                Id = 1,
                UserId = 1,
                ProjectId = 2,  // Trying to change to a project user doesn't have access to
                Date = DateTime.Today.AddDays(-1),
                Hours = 8.0m
            };
            _mockDataStore.Setup(x => x.GetById(1)).Returns(existingEntry);
            _mockUserServices.Setup(x => x.GetUserById(1)).Returns(new User(1, "Test User"));
            _mockProjectServices.Setup(x => x.GetProjectById(2)).Returns(new Project(2, "Other Project"));
            _mockDataStore.Setup(x => x.UserProjects(1)).Returns(new List<Project>
            {
                new Project(1, "Assigned Project")  // User only has access to project 1, not 2
            });
            // Act
            var result = _timesheetServices.EditEntry(editedEntry, currentUserId);
            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.ProjectNotAssignedToUser, result.ErrorMessage);
        }
        [Fact]
        public void EditEntry_FutureDate_ReturnsFailure()
        {
            // Arrange
            var currentUserId = 1;
            var existingEntry = new TimesheetEntry
            {
                Id = 1,
                UserId = 1,
                ProjectId = 1,
                Date = DateTime.Today.AddDays(-1),
                Hours = 8.0m
            };
            var editedEntry = new TimesheetEntry
            {
                Id = 1,
                UserId = 1,
                ProjectId = 1,
                Date = DateTime.Today.AddDays(1),  // Future date!
                Hours = 8.0m
            };
            _mockDataStore.Setup(x => x.GetById(1)).Returns(existingEntry);
            _mockUserServices.Setup(x => x.GetUserById(1)).Returns(new User(1, "Test User"));
            _mockProjectServices.Setup(x => x.GetProjectById(1)).Returns(new Project(1, "Test Project"));
            _mockDataStore.Setup(x => x.UserProjects(1)).Returns(new List<Project> { new Project(1, "Test Project") });
            // Act
            var result = _timesheetServices.EditEntry(editedEntry, currentUserId);
            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.DateMustNotBeInTheFuture, result.ErrorMessage);
        }
        [Fact]
        public void EditEntry_ChangeToValidProject_ReturnsSuccess()
        {
            // Arrange
            var currentUserId = 1;
            var existingEntry = new TimesheetEntry
            {
                Id = 1,
                UserId = 1,
                ProjectId = 1,
                Date = DateTime.Today.AddDays(-1),
                Hours = 8.0m
            };
            var editedEntry = new TimesheetEntry
            {
                Id = 1,
                UserId = 1,
                ProjectId = 2,  // Changing to a different project user has access to
                Date = DateTime.Today.AddDays(-1),
                Hours = 8.0m
            };
            _mockDataStore.Setup(x => x.GetById(1)).Returns(existingEntry);
            _mockUserServices.Setup(x => x.GetUserById(1)).Returns(new User(1, "Test User"));
            _mockProjectServices.Setup(x => x.GetProjectById(2)).Returns(new Project(2, "Second Project"));
            _mockDataStore.Setup(x => x.UserProjects(1)).Returns(new List<Project>
            {
                new Project(1, "First Project"),
                new Project(2, "Second Project")  // User has access to both projects
            });
            _mockDataStore.Setup(x => x.Update(It.IsAny<TimesheetEntry>())).Returns(true);
            // Act
            var result = _timesheetServices.EditEntry(editedEntry, currentUserId);
            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Value);
        }
        [Fact]
        public void EditEntry_NullEntry_ReturnsFailure()
        {
            // Arrange
            var currentUserId = 1;
            TimesheetEntry? entry = null;
            // Act
            var result = _timesheetServices.EditEntry(entry!, currentUserId);
            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(ErrorMessages.ErrorUpdatingTimesheetEntry, result.ErrorMessage);
        }
        #endregion
        #region Delete Entry Tests
        [Fact]
        public void DeleteEntry_ValidDelete_ReturnsSuccess()
        {
            // Arrange
            var currentUserId = 1;
            var existingEntry = new TimesheetEntry
            {
                Id = 1,
                UserId = 1,
                ProjectId = 1,
                Date = DateTime.Today.AddDays(-1),
                Hours = 8.0m
            };
            _mockDataStore.Setup(x => x.GetById(1)).Returns(existingEntry);
            _mockDataStore.Setup(x => x.Delete(1)).Returns(true);
            // Act
            var result = _timesheetServices.DeleteEntry(1, currentUserId);
            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Value);
        }
        [Fact]
        public void DeleteEntry_EntryDoesNotExist_ReturnsFailure()
        {
            // Arrange
            var currentUserId = 1;
            _mockDataStore.Setup(x => x.GetById(999)).Returns((TimesheetEntry?)null);
            // Act
            var result = _timesheetServices.DeleteEntry(999, currentUserId);
            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.TimesheetEntryNotFound, result.ErrorMessage);
        }
        [Fact]
        public void DeleteEntry_UserTriesToDeleteAnotherUsersEntry_ReturnsFailure()
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
            _mockDataStore.Setup(x => x.GetById(1)).Returns(existingEntry);
            // Act
            var result = _timesheetServices.DeleteEntry(1, currentUserId);
            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.UnauthorisedDelete, result.ErrorMessage);
        }
        #endregion
        #region Get Entry Tests
        [Fact]
        public void GetEntryById_EntryExists_ReturnsEntry()
        {
            // Arrange
            var expectedEntry = new TimesheetEntry
            {
                Id = 1,
                UserId = 1,
                ProjectId = 1,
                Date = DateTime.Today.AddDays(-1),
                Hours = 8.0m,
                Description = "Test work"
            };
            _mockDataStore.Setup(x => x.GetById(1)).Returns(expectedEntry);
            // Act
            var result = _timesheetServices.GetEntryById(1);
            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(1, result.UserId);
            Assert.Equal(8.0m, result.Hours);
        }
        [Fact]
        public void GetEntryById_EntryDoesNotExist_ReturnsNull()
        {
            // Arrange
            _mockDataStore.Setup(x => x.GetById(999)).Returns((TimesheetEntry?)null);
            // Act
            var result = _timesheetServices.GetEntryById(999);
            // Assert
            Assert.Null(result);
        }
        #endregion
        #region Get Entries for User and Week Tests
        [Fact]
        public void GetEntriesForUserAndWeek_ValidUser_ReturnsEntries()
        {
            // Arrange
            var userId = 1;
            var weekStart = new DateTime(2024, 1, 1);  // Monday
            var expectedEntries = new List<TimesheetEntry>
            {
                new TimesheetEntry { Id = 1, UserId = 1, ProjectId = 1, Date = weekStart, Hours = 8.0m },
                new TimesheetEntry { Id = 2, UserId = 1, ProjectId = 1, Date = weekStart.AddDays(1), Hours = 7.5m }
            };
            _mockUserServices.Setup(x => x.GetUserById(1)).Returns(new User(1, "Test User"));
            _mockDataStore.Setup(x => x.GetByUserAndWeek(userId, weekStart)).Returns(expectedEntries);
            // Act
            var result = _timesheetServices.GetEntriesForUserAndWeek(userId, weekStart);
            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, entry => Assert.Equal(userId, entry.UserId));
        }
        [Fact]
        public void GetEntriesForUserAndWeek_UserDoesNotExist_ReturnsEmptyList()
        {
            // Arrange
            var userId = 999;
            var weekStart = new DateTime(2024, 1, 1);
            _mockUserServices.Setup(x => x.GetUserById(999)).Returns((User?)null);
            // Act
            var result = _timesheetServices.GetEntriesForUserAndWeek(userId, weekStart);
            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
        [Fact]
        public void GetEntriesForUserAndWeek_NoEntriesInWeek_ReturnsEmptyList()
        {
            // Arrange
            var userId = 1;
            var weekStart = new DateTime(2024, 1, 1);
            _mockUserServices.Setup(x => x.GetUserById(1)).Returns(new User(1, "Test User"));
            _mockDataStore.Setup(x => x.GetByUserAndWeek(userId, weekStart)).Returns(new List<TimesheetEntry>());
            // Act
            var result = _timesheetServices.GetEntriesForUserAndWeek(userId, weekStart);
            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
        [Fact]
        public void GetEntriesForUserAndWeek_MultipleEntries_ReturnsOnlyWeekEntries()
        {
            // Arrange
            var userId = 1;
            var weekStart = new DateTime(2024, 1, 1);  // Monday
            var entriesInWeek = new List<TimesheetEntry>
            {
                new TimesheetEntry { Id = 1, UserId = 1, ProjectId = 1, Date = weekStart, Hours = 8.0m },
                new TimesheetEntry { Id = 2, UserId = 1, ProjectId = 1, Date = weekStart.AddDays(2), Hours = 7.5m },
                new TimesheetEntry { Id = 3, UserId = 1, ProjectId = 1, Date = weekStart.AddDays(6), Hours = 6.0m }
            };
            _mockUserServices.Setup(x => x.GetUserById(1)).Returns(new User(1, "Test User"));
            _mockDataStore.Setup(x => x.GetByUserAndWeek(userId, weekStart)).Returns(entriesInWeek);
            // Act
            var result = _timesheetServices.GetEntriesForUserAndWeek(userId, weekStart);
            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
            Assert.All(result, entry =>
            {
                Assert.True(entry.Date >= weekStart);
                Assert.True(entry.Date < weekStart.AddDays(7));
            });
        }
        [Fact]
        public void GetEntriesForUserAndWeek_EntriesOrderedByDate_ReturnsCorrectOrder()
        {
            // Arrange
            var userId = 1;
            var weekStart = new DateTime(2024, 1, 1);
            var orderedEntries = new List<TimesheetEntry>
            {
                new TimesheetEntry { Id = 1, UserId = 1, ProjectId = 1, Date = weekStart, Hours = 8.0m },
                new TimesheetEntry { Id = 2, UserId = 1, ProjectId = 1, Date = weekStart.AddDays(1), Hours = 7.5m },
                new TimesheetEntry { Id = 3, UserId = 1, ProjectId = 1, Date = weekStart.AddDays(4), Hours = 6.0m }
            };
            _mockUserServices.Setup(x => x.GetUserById(1)).Returns(new User(1, "Test User"));
            _mockDataStore.Setup(x => x.GetByUserAndWeek(userId, weekStart)).Returns(orderedEntries);
            // Act
            var result = _timesheetServices.GetEntriesForUserAndWeek(userId, weekStart).ToList();
            // Assert
            Assert.Equal(3, result.Count);
            Assert.True(result[0].Date <= result[1].Date);
            Assert.True(result[1].Date <= result[2].Date);
        }     
        #endregion
        #region Get Total Hours Per Project Tests
        [Fact]
        public void GetTotalHoursPerProject_SingleProject_ReturnsCorrectTotal()
        {
            // Arrange
            var userId = 1;
            var weekStart = new DateTime(2024, 1, 1);
            var entries = new List<TimesheetEntry>
            {
                new TimesheetEntry { Id = 1, UserId = 1, ProjectId = 1, Date = weekStart, Hours = 8.0m },
                new TimesheetEntry { Id = 2, UserId = 1, ProjectId = 1, Date = weekStart.AddDays(1), Hours = 7.5m },
                new TimesheetEntry { Id = 3, UserId = 1, ProjectId = 1, Date = weekStart.AddDays(2), Hours = 6.0m }
            };
            _mockUserServices.Setup(x => x.GetUserById(1)).Returns(new User(1, "Test User"));
            _mockDataStore.Setup(x => x.GetByUserAndWeek(userId, weekStart)).Returns(entries);
            // Act
            var result = _timesheetServices.GetTotalHoursPerProject(userId, weekStart);
            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(21.5m, result[1]);  // 8.0 + 7.5 + 6.0 = 21.5
        }
        [Fact]
        public void GetTotalHoursPerProject_MultipleProjects_ReturnsCorrectTotals()
        {
            // Arrange
            var userId = 1;
            var weekStart = new DateTime(2024, 1, 1);
            var entries = new List<TimesheetEntry>
            {
                new TimesheetEntry { Id = 1, UserId = 1, ProjectId = 1, Date = weekStart, Hours = 8.0m },
                new TimesheetEntry { Id = 2, UserId = 1, ProjectId = 1, Date = weekStart.AddDays(1), Hours = 4.0m },
                new TimesheetEntry { Id = 3, UserId = 1, ProjectId = 2, Date = weekStart.AddDays(1), Hours = 4.0m },
                new TimesheetEntry { Id = 4, UserId = 1, ProjectId = 2, Date = weekStart.AddDays(2), Hours = 8.0m },
                new TimesheetEntry { Id = 5, UserId = 1, ProjectId = 3, Date = weekStart.AddDays(3), Hours = 5.5m }
            };
            _mockUserServices.Setup(x => x.GetUserById(1)).Returns(new User(1, "Test User"));
            _mockDataStore.Setup(x => x.GetByUserAndWeek(userId, weekStart)).Returns(entries);
            // Act
            var result = _timesheetServices.GetTotalHoursPerProject(userId, weekStart);
            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal(12.0m, result[1]);  // Project 1: 8.0 + 4.0
            Assert.Equal(12.0m, result[2]);  // Project 2: 4.0 + 8.0
            Assert.Equal(5.5m, result[3]);   // Project 3: 5.5
        }
        [Fact]
        public void GetTotalHoursPerProject_NoEntries_ReturnsEmptyDictionary()
        {
            // Arrange
            var userId = 1;
            var weekStart = new DateTime(2024, 1, 1);
            _mockUserServices.Setup(x => x.GetUserById(1)).Returns(new User(1, "Test User"));
            _mockDataStore.Setup(x => x.GetByUserAndWeek(userId, weekStart)).Returns(new List<TimesheetEntry>());
            // Act
            var result = _timesheetServices.GetTotalHoursPerProject(userId, weekStart);
            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
        [Fact]
        public void GetTotalHoursPerProject_UserDoesNotExist_ReturnsEmptyDictionary()
        {
            // Arrange
            var userId = 999;
            var weekStart = new DateTime(2024, 1, 1);
            _mockUserServices.Setup(x => x.GetUserById(999)).Returns((User?)null);
            // Act
            var result = _timesheetServices.GetTotalHoursPerProject(userId, weekStart);
            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
        [Theory]
        [InlineData(7.333)]
        [InlineData(8.666)]
        [InlineData(5.999)]
        public void GetTotalHoursPerProject_DecimalRounding_MaintainsPrecision(decimal hours)
        {
            // Arrange
            var userId = 1;
            var weekStart = new DateTime(2024, 1, 1);
            var entries = new List<TimesheetEntry>
            {
                new TimesheetEntry { Id = 1, UserId = 1, ProjectId = 1, Date = weekStart, Hours = hours },
                new TimesheetEntry { Id = 2, UserId = 1, ProjectId = 1, Date = weekStart.AddDays(1), Hours = hours },
                new TimesheetEntry { Id = 3, UserId = 1, ProjectId = 1, Date = weekStart.AddDays(2), Hours = hours }
            };
            _mockUserServices.Setup(x => x.GetUserById(1)).Returns(new User(1, "Test User"));
            _mockDataStore.Setup(x => x.GetByUserAndWeek(userId, weekStart)).Returns(entries);
            // Act
            var result = _timesheetServices.GetTotalHoursPerProject(userId, weekStart);
            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            // Calculate expected total
            var expectedTotal = hours + hours + hours;  // or hours * 3
            // Verify precision is maintained (decimal type should handle this correctly)
            Assert.Equal(expectedTotal, result[1]);
            Assert.Equal(hours * 3, result[1]);
        }
        #endregion
    }
}