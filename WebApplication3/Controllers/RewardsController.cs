using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WebApplication3.Data;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    public class RewardsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RewardsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> ManageRewards()
        {
            //  ChildRewards
            var rewards = await _context.Rewards
                .Include(r => r.ChildRewards)
                .ThenInclude(cr => cr.Child)
                .ToListAsync();

            return View(rewards);
        }

        // GET: Rewards/Create
        public async Task<IActionResult> AddReward()
        {
            ViewBag.Children = await _context.Children.ToListAsync(); // Populate ViewBag.Children
            return View();
        }

        // POST: Rewards/Create
        [HttpPost]
        public async Task<IActionResult> AddReward([Bind("Id,Name,Points,ChildId")] Reward reward)
        {
            if (_context == null)
            {
                Console.WriteLine("_context is null in AddReward POST");
            }
            if (ModelState.IsValid)
            {
                _context.Add(reward);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception during SaveChanges: {ex.Message}");
                    // Optionally, log the full exception details for debugging
                }
                return RedirectToAction(nameof(ManageRewards));
            }

            // Repopulate ViewBag.Children if there are validation errors!
            ViewBag.Children = await _context.Children.ToListAsync();
            return View(reward);
        }
        private void PrintModelStateErrors()
        {
            foreach (var key in ModelState.Keys)
            {
                var state = ModelState[key];
                if (state.Errors.Any())
                    Console.WriteLine($"Key: {key}, Errors: {string.Join(", ", state.Errors.Select(e => e.ErrorMessage))}");
            }
        }
    }
}
