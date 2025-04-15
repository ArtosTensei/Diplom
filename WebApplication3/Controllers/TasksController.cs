using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication3.Models;
using WebApplication3.Data;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Microsoft.EntityFrameworkCore;


namespace WebApplication3.Controllers
{
    public class TasksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TasksController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }


        public IActionResult Index(int? childId)
        {
            var role = _httpContextAccessor.HttpContext.Session.GetString("Role");
            ViewBag.UserRole = role;
            var userId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId");

            if (childId.HasValue)
            {
                // If childId is provided, get tasks for that specific child
                var tasks = _context.Tasks.Include(t => t.Child).Where(t => t.ChildId == childId.Value).ToList();
                return View(tasks);
            }
            else if (role == "Parent")
            {
                // If no childId, but user is a parent, get tasks for all children
                var tasks = _context.Tasks
                .Include(t => t.Child)
                .Where(t => t.Child.ParentId == userId)
                .ToList();
                return View(tasks);
            }
            else if (role == "Child")
            {
                // If no childId, but user is a child, get tasks for that child
                var tasks = _context.Tasks.Where(t => t.ChildId == userId).ToList();
                return View(tasks);
            }
            else
            {
                // Handle unauthorized access
                ViewBag.ErrorMessage = "Unauthorized access.";
                return View(new List<WebApplication3.Models.Task>());
            }
        }

        // Parents can create tasks for their children
        public IActionResult Create()
        {
            var role = _httpContextAccessor.HttpContext.Session.GetString("Role");
            var userId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId");

            if (role != "Parent")
            {
                return RedirectToAction("Index"); // Or show an error message
            }

            // Get children of the current parent to populate a dropdown
            var children = _context.Children.Where(c => c.ParentId == userId).Select(c => new { c.Id, c.Name }).ToList();
            ViewBag.Children = children;

            return View();
        }

        [HttpPost]
        public IActionResult Create(string description, int points, int childId)
        {
            var role = _httpContextAccessor.HttpContext.Session.GetString("Role");
            var userId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId");
            if (role != "Parent")
            {
                return RedirectToAction("Index"); // Or show an error message
            }
            if (string.IsNullOrEmpty(description) || points <= 0)
            {
                ViewBag.ErrorMessage = "Please provide a valid description and points.";
                return View();
            }
            var child = _context.Children.Find(childId);
            if (child == null || child.ParentId != userId)
            {
                ViewBag.ErrorMessage = "Invalid Child selected.";
                return View();
            }
            var newTask = new WebApplication3.Models.Task { Description = description, Points = points, ChildId = childId, IsCompleted = false };
            _context.Tasks.Add(newTask);
            _context.SaveChanges();
            return RedirectToAction("Index");

            // The following line was causing an error because 'task' is not defined. It's likely a leftover from a previous modification.
            // I'm removing it as it's not needed after a successful task creation.
            // return View(task); 
        }

        // Only the parent of the child who owns the task can edit it
        public IActionResult Edit(int id)
        {
            var role = _httpContextAccessor.HttpContext.Session.GetString("Role");
            var userId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId");

            var task = _context.Tasks.Include(t => t.Child).FirstOrDefault(t => t.Id == id);
            if (task == null)
            {
                return NotFound();
            }

            if (role != "Parent" || task.Child.ParentId != userId)
            {
                return RedirectToAction("Index"); // Or show an error message
            }

            // Get children of the current parent to populate a dropdown
            var children = _context.Children.Where(c => c.ParentId == userId).ToList();
            ViewBag.Children = children;

            return View(task);
        }

        [HttpPost]
        public IActionResult Edit(WebApplication3.Models.Task updatedTask)
        {
            var role = _httpContextAccessor.HttpContext.Session.GetString("Role");
            var userId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId");

            if (role != "Parent")
            {
                return RedirectToAction("Index"); // Or show an error message
            }

            var children = _context.Children.Where(c => c.ParentId == userId).ToList();
            ViewBag.Children = children;

            if (ModelState.IsValid)
            {
                var task = _context.Tasks.Include(t => t.Child).FirstOrDefault(t => t.Id == updatedTask.Id);
                if (task != null && task.Child.ParentId == userId)
                {
                    task.Description = updatedTask.Description;
                    task.Points = updatedTask.Points;
                    task.IsCompleted = updatedTask.IsCompleted;
                    task.ChildId = updatedTask.ChildId; // Allow changing the child for the task

                    _context.SaveChanges();
                    return RedirectToAction("Index");
                }
                return NotFound();
            }

            return View(updatedTask);
        }


        // Only the parent of the child who owns the task can delete it
        public IActionResult Delete(int id)
        {
            var role = _httpContextAccessor.HttpContext.Session.GetString("Role");
            var userId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId");

            var task = _context.Tasks.Include(t => t.Child).FirstOrDefault(t => t.Id == id);
            if (task == null)
            {
                return NotFound();
            }

            if (role != "Parent" || task.Child.ParentId != userId)
            {
                return RedirectToAction("Index"); // Or show an error message
            }

            _context.Tasks.Remove(task);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }


        public IActionResult MarkComplete(int id)
        {
            var task = _context.Tasks.Include(t => t.Child).FirstOrDefault(t => t.Id == id); // Include Child
            if (task == null)
            {
                return NotFound();
            }

            var role = _httpContextAccessor.HttpContext.Session.GetString("Role");
            if (role == "Child")
            {
                var userId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId");
                if (userId != task.ChildId)
                {
                    // The child is trying to mark complete a task that does not belong to them.
                    // Redirect to the Index action or show an appropriate error message.
                    return RedirectToAction("Index");
                }
            }


            var child = task.Child; // Get the child associated with the task
            if (child == null)
            {
                return NotFound(); // Or handle the case where the child is not found
            }

            task.IsCompleted = !task.IsCompleted; // Toggle the completion status

            if (task.IsCompleted)
            {
                child.Points += task.Points; // Add points if task is now complete
            }
            else
            {
                child.Points -= task.Points; // Subtract points if task is now incomplete
            }
            _context.SaveChanges();


            return RedirectToAction("Index", new { childId = task.ChildId });
        }
    }
}