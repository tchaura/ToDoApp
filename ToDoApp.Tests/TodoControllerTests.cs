using Microsoft.EntityFrameworkCore;
using ToDoApp.Controllers;
using ToDoApp.Data;
using ToDoApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace ToDoApp.Tests
{
    public class TodoControllerTests
    {
        private readonly ApplicationDbContext _context;
        private readonly TodoController _controller;

        public TodoControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TodoTestDb")
                .Options;
            _context = new ApplicationDbContext(options);
            _controller = new TodoController(_context);
        }

        // Reset database for each test
        private void ResetDatabase()
        {
            _context.TodoItems.RemoveRange(_context.TodoItems);
            _context.SaveChanges();
        }

        [Fact]
        public async Task Create_EmptyDescription_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.Create(string.Empty) as BadRequestResult;

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task Create_ValidDescription_ReturnsActiveTasks()
        {
            // Arrange
            ResetDatabase();
            var description = "New Task";

            // Act
            var result = await _controller.Create(description) as PartialViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<IEnumerable<TodoItem>>(result.Model);
            Assert.Single(model); // There should be one item in the list
            Assert.Equal(description, model.First().Description);
        }

        [Fact]
        public async Task Edit_NonExistentTask_ReturnsNotFound()
        {
            // Act
            var result = await _controller.Edit(999, "New Description") as NotFoundObjectResult;

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Task to edit not found", result.Value);
        }

        [Fact]
        public async Task Edit_EmptyDescription_ReturnsNotFound()
        {
            // Arrange
            var item = new TodoItem { Id = 1, Description = "Old Task" };
            _context.TodoItems.Add(item);
            _context.SaveChanges();

            // Act
            var result = await _controller.Edit(item.Id, string.Empty) as NotFoundObjectResult;

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Task to edit not found", result.Value);
        }

        [Fact]
        public async Task Complete_NonExistentTask_ReturnsNotFound()
        {
            // Act
            var result = await _controller.Complete(999) as NotFoundObjectResult;

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Task to complete not found", result.Value);
        }

        [Fact]
        public async Task Complete_TogglesTaskCompletion()
        {
            // Arrange
            var item = new TodoItem { Id = 1, Description = "Incomplete Task", IsCompleted = false };
            _context.TodoItems.Add(item);
            _context.SaveChanges();

            // Act
            var result = await _controller.Complete(item.Id) as PartialViewResult;

            // Assert
            var updatedItem = _context.TodoItems.FirstOrDefault(t => t.Id == item.Id);
            Assert.NotNull(updatedItem);
            Assert.True(updatedItem.IsCompleted);  // Task should now be marked as complete
        }

        [Fact]
        public async Task Delete_NonExistentTask_ReturnsNotFound()
        {
            // Act
            var result = await _controller.Delete(999) as NotFoundObjectResult;

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Task to delete not found", result.Value);
        }

        [Fact]
        public async Task Delete_ValidTask_RemovesFromDatabase()
        {
            // Arrange
            var item = new TodoItem { Description = "Task to Delete" };
            _context.TodoItems.Add(item);
            _context.SaveChanges();

            // Act
            var result = await _controller.Delete(item.Id) as PartialViewResult;

            // Assert
            var deletedItem = _context.TodoItems.FirstOrDefault(t => t.Id == item.Id);
            Assert.Null(deletedItem);  // Task should be removed from the database
        }
    }
}
