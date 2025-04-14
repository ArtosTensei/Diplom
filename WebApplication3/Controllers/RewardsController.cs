using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication3.Data;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    public class RewardsController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly IHttpContextAccessor _httpContextAccessor;


        public RewardsController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
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
            //Pass only needed properties to ViewBag
            ViewBag.Children = await _context.Children.Select(c => new { c.Id, c.Name }).ToListAsync();
            return View();
        }

        // POST: Rewards/Create
        [HttpPost]
        public async Task<IActionResult> AddReward(string name, int points, int childId)
        {
            if (string.IsNullOrEmpty(name))
            {
                ViewBag.ErrorMessage = "Please provide a reward name.";
                ViewBag.Children = await _context.Children.Select(c => new { c.Id, c.Name }).ToListAsync();
                return View();
            }

            if (points <= 0)
            {
                ViewBag.ErrorMessage = "Points must be greater than zero.";
                ViewBag.Children = await _context.Children.Select(c => new { c.Id, c.Name }).ToListAsync();
                return View();
            }

            var child = await _context.Children.FindAsync(childId);
            if (child == null)
            {
                ViewBag.ErrorMessage = "Invalid child selected.";
                ViewBag.Children = await _context.Children.Select(c => new { c.Id, c.Name }).ToListAsync();
                return View();
            }

            var newReward = new Reward { Name = name, Points = points, ChildId = childId };
            _context.Rewards.Add(newReward);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ManageRewards));
        }

        // GET: Rewards/Edit/5
        public async Task<IActionResult> EditReward(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reward = await _context.Rewards.FindAsync(id);
            if (reward == null)
            {
                return NotFound();
            }

            // Pass only needed properties to ViewBag
            ViewBag.Children = await _context.Children.Select(c => new { c.Id, c.Name }).ToListAsync();

            return View(reward);
        }

        // POST: Rewards/Edit/5
        [HttpPost]
        public async Task<IActionResult> EditReward(int id, string name, int points, int childId)
        {
            if (string.IsNullOrEmpty(name))
            {
                ViewBag.ErrorMessage = "Please provide a reward name.";
                ViewBag.Children = await _context.Children.Select(c => new { c.Id, c.Name }).ToListAsync();
                var reward = await _context.Rewards.FindAsync(id);
                return View(reward);
            }

            if (points <= 0)
            {
                ViewBag.ErrorMessage = "Points must be greater than zero.";
                ViewBag.Children = await _context.Children.Select(c => new { c.Id, c.Name }).ToListAsync();
                var reward = await _context.Rewards.FindAsync(id);
                return View(reward);
            }

            var child = await _context.Children.FindAsync(childId);
            if (child == null)
            {
                ViewBag.ErrorMessage = "Invalid child selected.";
                ViewBag.Children = await _context.Children.Select(c => new { c.Id, c.Name }).ToListAsync();
                var reward = await _context.Rewards.FindAsync(id);
                return View(reward);
            }

            var existingReward = await _context.Rewards.FindAsync(id);
            if (existingReward == null)
            {
                return NotFound();
            }

            existingReward.Name = name;
            existingReward.Points = points;
            existingReward.ChildId = childId;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ManageRewards));
        }
        public IActionResult ChildRewards()
        {
            var role = _httpContextAccessor.HttpContext.Session.GetString("Role");
            if (role != "Child")
            {
                return RedirectToAction("Index", "Home"); // Redirect if not a child
            }

            var childId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId");
            if (!childId.HasValue)
            {
                return RedirectToAction("Index", "Home"); // Redirect if no child ID in session
            }

            var child = _context.Children.Find(childId.Value);
            if (child == null)
            {
                return NotFound(); // Child not found
            }

            var rewards = _context.Rewards.Where(r => r.ChildId == childId.Value).ToList();

            ViewBag.ChildPoints = child.Points;
            ViewBag.Rewards = rewards;
            return View(rewards);
        }

        [HttpPost]
        public IActionResult BuyReward(int rewardId)
        {
            var role = _httpContextAccessor.HttpContext.Session.GetString("Role");
            if (role != "Child")
            {
                return RedirectToAction("Index", "Home"); // Redirect if not a child
            }

            var childId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId");
            if (!childId.HasValue)
            {
                return RedirectToAction("Index", "Home"); // Redirect if no child ID in session
            }

            var child = _context.Children.Find(childId.Value);
            if (child == null)
            {
                return NotFound(); // Child not found
            }

            var reward = _context.Rewards.Find(rewardId);
            if (reward == null)
            {
                ViewBag.ErrorMessage = "Reward not found.";
                return RedirectToAction(nameof(ChildRewards));
            }

            if (child.Points >= reward.Points)
            {
                child.Points -= reward.Points;

                //If you are tracking ownership, you would update or create a ChildReward record here.

                _context.SaveChanges();
                return RedirectToAction(nameof(ChildRewards));
            }
            else
            {
                ViewBag.ErrorMessage = "Not enough points to purchase this reward.";
                return RedirectToAction(nameof(ChildRewards));
            }
        }



        private bool RewardExists(int id)
        {
            return _context.Rewards.Any(e => e.Id == id);
        }


    }
}
