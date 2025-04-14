using Microsoft.AspNetCore.Mvc;
using WebApplication3.Data;

namespace WebApplication3.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomeController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Index()
        {
            var role = _httpContextAccessor.HttpContext.Session.GetString("Role");
            ViewBag.UserRole = role;
            if (string.IsNullOrEmpty(role))
            {
                return RedirectToAction("Login", "Account");
            }
            if (role == "Parent")
            {
                var parentId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId");
                if (parentId.HasValue)
                {
                    var children = _context.Children.Where(c => c.ParentId == parentId.Value).ToList();
                    ViewBag.Children = children;
                }
            }
            return View();
        }

        // Other actions like Privacy and Error can remain as they are,
        // or be adjusted based on your application's needs.  For brevity,
        // I'm leaving them out of this diff as the primary focus is Index.
    }
}
