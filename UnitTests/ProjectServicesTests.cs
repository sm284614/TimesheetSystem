using Moq;
using TimesheetSystem.Data;
using TimesheetSystem.Models;
using TimesheetSystem.Services;
namespace TimesheetSystem.ProjectServiceTests
{
    public class ProjectServicesTests
    {
        [Fact]
        public void GetProjectById_WhenProjectDoesNotExist_ReturnsNull()
        {
            // Arrange
            var mockDataStore = new Mock<ITimesheetDataStore>();
            mockDataStore.Setup(ds => ds.Projects).Returns(new List<Project>());
            var service = new ProjectServices(mockDataStore.Object);
            // Act
            var result = service.GetProjectById(99);
            // Assert
            Assert.Null(result);
        }
        [Fact]
        public void GetProjectsByUserId_WhenUserExistsButHasNoProjects_ReturnsEmptyCollection()
        {
            // Arrange
            var mockDataStore = new Mock<ITimesheetDataStore>();
            mockDataStore.Setup(ds => ds.Users).Returns(new List<User>{new User(1,"Test User" )});
            mockDataStore.Setup(ds => ds.UserProjects(1)).Returns(new List<Project>());
            var service = new ProjectServices(mockDataStore.Object);
            // Act
            var result = service.GetProjectsByUserId(1);
            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}
