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
                var tasks = _context.Tasks.Where(t => t.ChildId == childId.Value).ToList();
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
            var children = _context.Children.Where(c => c.ParentId == userId).ToList();
            ViewBag.Children = children;

            return View();
        }

        [HttpPost]
        public IActionResult Create(WebApplication3.Models.Task task)
        {
            var role = _httpContextAccessor.HttpContext.Session.GetString("Role");
            var userId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId");

            if (role != "Parent")
            {
                return RedirectToAction("Index"); // Or show an error message
            }

            if (ModelState.IsValid)
            {
                // Ensure the task is for a child of the current parent
                var child = _context.Children.Find(task.ChildId);
                if (child != null && child.ParentId == userId)
                {
                    _context.Tasks.Add(task);
                    _context.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.ErrorMessage = "Invalid Child selected.";
                }
            }

            // Repopulate children dropdown if there's an error
            var children = _context.Children.Where(c => c.ParentId == userId).ToList();
            ViewBag.Children = children;

            return View(task);
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
            }

            // Repopulate children dropdown if there's an error
            var children = _context.Children.Where(c => c.ParentId == userId).ToList();
            ViewBag.Children = children;

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


        // Both parent and child can mark a task as complete
        public IActionResult MarkComplete(int id)
        {
            var role = _httpContextAccessor.HttpContext.Session.GetString("Role");
            var userId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId");

            var task = _context.Tasks.Include(t => t.Child).FirstOrDefault(t => t.Id == id);
            if (task == null)
            {
                return NotFound();
            }


            if (role == "Parent")
            {
                // Check if the task belongs to a child of this parent
                if (task.Child.ParentId == userId)
                {
                    task.IsCompleted = !task.IsCompleted;
                    _context.SaveChanges();
                }
            }
            else if (role == "Child" && task.ChildId == userId)
            {
                // Child can only mark their own tasks as complete
                task.IsCompleted = !task.IsCompleted;
                _context.SaveChanges();

                // Update child's points if task is completed
                if (task.IsCompleted)
                {
                    var child = _context.Children.Find(userId);
                    if (child != null)
                    {
                        child.Points += task.Points;
                        _context.SaveChanges();
                    }
                }
            }


            return RedirectToAction("Index");
        }
    }
}
