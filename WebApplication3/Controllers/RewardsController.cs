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
            // Загружаем награды вместе с информацией о детях через ChildRewards
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReward([Bind("Id,Name,Points")] Reward reward)
        {
            if (ModelState.IsValid)
            {
                _context.Add(reward);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ManageRewards));
            }

            // Repopulate ViewBag.Children if there are validation errors!
            ViewBag.Children = await _context.Children.ToListAsync();
            return View(reward);
        }
    }
}