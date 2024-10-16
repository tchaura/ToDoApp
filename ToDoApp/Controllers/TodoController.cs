using Microsoft.AspNetCore.Mvc;
using ToDoApp.Data;
using Microsoft.EntityFrameworkCore;
using ToDoApp.Models;

namespace ToDoApp.Controllers;

public class TodoController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var todoItems = await context.TodoItems.ToListAsync();
        return View(todoItems);
    }
    
    [HttpPost]
    public async Task<IActionResult> Create(string description)
    {
        if (!string.IsNullOrEmpty(description))
        {
            var newTodo = new TodoItem { Description = description };
            context.Add(newTodo);
            await context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost]
    public async Task<IActionResult> Edit(int id, string description)
    {
        var todoItem = await context.TodoItems.FindAsync(id);
        if (todoItem != null && !string.IsNullOrEmpty(description))
        {
            todoItem.Description = description;
            await context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost]
    public async Task<IActionResult> Complete(int id)
    {
        var todoItem = await context.TodoItems.FindAsync(id);
        if (todoItem != null)
        {
            todoItem.IsCompleted = !todoItem.IsCompleted;
            await context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var todoItem = await context.TodoItems.FindAsync(id);
        if (todoItem != null)
        {
            context.TodoItems.Remove(todoItem);
            await context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}