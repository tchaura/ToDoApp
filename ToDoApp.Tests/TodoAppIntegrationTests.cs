using System.Net;
using AngleSharp.Html.Parser;

namespace ToDoApp.Tests
{
    public class TodoAppIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public TodoAppIntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetHomePage_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CreateTask_ValidDescription_ReturnsTaskInList()
        {
            // Arrange
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("description", "Test Task")
            });

            // Act
            var response = await _client.PostAsync("/Todo/Create", formContent);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("Test Task", responseString);  // Task should appear in the active list
        }

        [Fact]
        public async Task CreateTask_EmptyDescription_ReturnsBadRequest()
        {
            // Arrange
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("description", string.Empty)
            });

            // Act
            var response = await _client.PostAsync("/Todo/Create", formContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);  // Should return 400 Bad Request
        }

        [Fact]
        public async Task EditTask_ValidIdAndDescription_UpdatesTask()
        {
            // Arrange
            // Create a task to edit
            var createFormContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("description", "Original Task")
            });
            await _client.PostAsync("/Todo/Create", createFormContent);
            var response = await _client.GetAsync("/Todo/GetActiveTasks");
            var responseString = await response.Content.ReadAsStringAsync();
            var taskId = ExtractTaskIdFromResponse(responseString);

            // Act - Edit the task
            var editFormContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("id", taskId.ToString()),
                new KeyValuePair<string, string>("description", "Updated Task")
            });
            var editResponse = await _client.PostAsync("/Todo/Edit", editFormContent);

            // Assert
            editResponse.EnsureSuccessStatusCode();
            var updatedResponseString = await editResponse.Content.ReadAsStringAsync();
            Assert.Contains("Updated Task", updatedResponseString);
        }

        [Fact]
        public async Task EditTask_NonExistentId_ReturnsNotFound()
        {
            // Arrange
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("id", "999"),  // Non-existent ID
                new KeyValuePair<string, string>("description", "Updated Task")
            });

            // Act
            var response = await _client.PostAsync("/Todo/Edit", formContent);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CompleteTask_ValidId_TogglesCompletionStatus()
        {
            // Arrange
            // Create a task to complete
            var createFormContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("description", "Incomplete Task")
            });
            await _client.PostAsync("/Todo/Create", createFormContent);

            // Get the task ID (assuming it's the first task created)
            var response = await _client.GetAsync($"/Todo/GetActiveTasks/");
            var responseString = await response.Content.ReadAsStringAsync();
            var taskId = ExtractTaskIdFromResponse(responseString);

            // Act - Mark the task as completed
            var completeFormContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("id", taskId.ToString())
            });
            var completeResponse = await _client.PostAsync("/Todo/Complete", completeFormContent);

            // Assert
            completeResponse.EnsureSuccessStatusCode();
            var completedResponseString = await completeResponse.Content.ReadAsStringAsync();
            Assert.Contains("Incomplete Task", completedResponseString);
        }

        [Fact]
        public async Task CompleteTask_NonExistentId_ReturnsNotFound()
        {
            // Arrange
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("id", "999")
            });

            // Act
            var response = await _client.PostAsync("/Todo/Complete", formContent);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteTask_ValidId_RemovesTaskFromList()
        {
            // Arrange
            // Create a task to delete
            var createFormContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("description", "Task to Delete")
            });
            await _client.PostAsync("/Todo/Create", createFormContent);
            
            var response = await _client.GetAsync("/Todo/GetActiveTasks");
            var responseString = await response.Content.ReadAsStringAsync();
            var taskId = ExtractTaskIdFromResponse(responseString);

            // Act - Delete the task
            var deleteFormContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("id", taskId.ToString())
            });
            var deleteResponse = await _client.PostAsync("/Todo/Delete", deleteFormContent);

            // Assert
            deleteResponse.EnsureSuccessStatusCode();
            var updatedResponseString = await deleteResponse.Content.ReadAsStringAsync();
            Assert.DoesNotContain("Task to Delete", updatedResponseString);
        }

        [Fact]
        public async Task DeleteTask_NonExistentId_ReturnsNotFound()
        {
            // Arrange
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("id", "999") 
            });

            // Act
            var response = await _client.PostAsync("/Todo/Delete", formContent);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // Helper method to extract task ID from the response
        private int ExtractTaskIdFromResponse(string responseString)
        {
            var parser = new HtmlParser();
            var document = parser.ParseDocument(responseString);
            var idString = document.QuerySelector("li.list-group-item:last-child input[name=id]")?.Attributes.GetNamedItem("value")?.Value;
            return int.TryParse(idString, out var id) ? id : -1;
        }
    }
}
