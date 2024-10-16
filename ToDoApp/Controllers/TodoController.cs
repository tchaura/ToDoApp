using System.Net;

namespace ToDoApp.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoApp.Data;
using ToDoApp.Models;

public class TodoController : Controller
{
    private readonly ApplicationDbContext _context;

    public TodoController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IActionResult> Index()
    {
        var todoItems = await _context.TodoItems.ToListAsync();
        return View(todoItems);
    }
    
    public async Task<IActionResult> GetActiveTasks()
    {
        var activeTasks = await _context.TodoItems.Where(t => !t.IsCompleted).OrderBy(t => t.CreatedAt).ToListAsync();
        return PartialView("_TodoListPartial", activeTasks);
    }
    
    public async Task<IActionResult> GetCompletedTasks()
    {
        var completedTasks = await _context.TodoItems.Where(t => t.IsCompleted).OrderBy(t => t.CreatedAt).ToListAsync();
        return PartialView("_TodoListPartial", completedTasks);
    }

    [HttpPost]
    public async Task<IActionResult> Create(string description)
    {
        if (string.IsNullOrEmpty(description)) return BadRequest();
        var newTodo = new TodoItem { Description = description };
        _context.Add(newTodo);
        await _context.SaveChangesAsync();

        return await GetActiveTasks();
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, string description)
    {
        var todoItem = await _context.TodoItems.FindAsync(id);
        if (todoItem == null || string.IsNullOrEmpty(description)) return NotFound("Task to edit not found");
        todoItem.Description = description;
        await _context.SaveChangesAsync();

        return await GetActiveTasks();
    }

    [HttpPost]
    public async Task<IActionResult> Complete(int id)
    {
        var todoItem = await _context.TodoItems.FindAsync(id);
        if (todoItem == null) return NotFound("Task to complete not found");
        todoItem.IsCompleted = !todoItem.IsCompleted;
        await _context.SaveChangesAsync();

        return todoItem.IsCompleted ? await GetCompletedTasks() : await GetActiveTasks();
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var todoItem = await _context.TodoItems.FindAsync(id);
        if (todoItem == null) return NotFound("Task to delete not found");
        _context.TodoItems.Remove(todoItem);
        await _context.SaveChangesAsync();

        return todoItem.IsCompleted ? await GetCompletedTasks() : await GetActiveTasks();
    }
}
