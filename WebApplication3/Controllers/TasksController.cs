using Microsoft.AspNetCore.Mvc;
using Task = WebApplication3.Models.Task;

namespace WebApplication3.Controllers
{
    public class TasksController : Controller
    {
        private static List<Task> tasks = new List<Task>
        {
            new Task { Id = 1, Title = "Пропылесосить комнату", Description = "Убрать пыль с ковра", Deadline = "20.12.2024", Reward = 10, Status = "Не выполнено" },
            new Task { Id = 2, Title = "Почистить обувь", Description = "Подготовить обувь к зиме", Deadline = "22.12.2024", Reward = 15, Status = "Выполнено" }
        };

        public IActionResult Index()
        {
            return View(tasks);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Task newTask)
        {
            if (ModelState.IsValid)
            {
                newTask.Id = tasks.Count > 0 ? tasks.Max(t => t.Id) + 1 : 1;
                newTask.Status = "Не выполнено";
                tasks.Add(newTask);
                return RedirectToAction("Index");
            }
            return View(newTask); 
        }

        public IActionResult ChangeStatus(int id)
        {
            var task = tasks.FirstOrDefault(t => t.Id == id);
            if (task != null)
            {
                task.Status = task.Status == "Не выполнено" ? "Выполнено" : "Не выполнено";
            }
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var task = tasks.FirstOrDefault(t => t.Id == id);
            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        [HttpPost]
        public IActionResult Edit(Task updatedTask)
        {
            if (ModelState.IsValid)
            {
                var task = tasks.FirstOrDefault(t => t.Id == updatedTask.Id);
                if (task != null)
                {
                    task.Title = updatedTask.Title;
                    task.Description = updatedTask.Description;
                    task.Deadline = updatedTask.Deadline;
                    task.Reward = updatedTask.Reward;
                    task.Status = updatedTask.Status;

                    return RedirectToAction("Index");
                }
            }
            return View(updatedTask);
        }

        public IActionResult Delete(int id)
        {
            var task = tasks.FirstOrDefault(t => t.Id == id);
            if (task != null)
            {
                tasks.Remove(task);
            }
            return RedirectToAction("Index");
        }
    }
}
