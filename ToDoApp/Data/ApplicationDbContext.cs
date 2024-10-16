using Microsoft.EntityFrameworkCore;
using ToDoApp.Models;

namespace ToDoApp.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<TodoItem> TodoItems { get; set; }
}