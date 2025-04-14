using Microsoft.AspNetCore.Mvc;
using WebApplication3.Models;
using Microsoft.AspNetCore.Identity;
using WebApplication3.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace WebApplication3.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<Parent> _parentHasher;
        private readonly IPasswordHasher<Child> _childHasher;

        public AccountController(ApplicationDbContext context, IPasswordHasher<Parent> parentHasher, IPasswordHasher<Child> childHasher)
        {
            _context = context;
            _parentHasher = parentHasher;
            _childHasher = childHasher;
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            // Authentication logic
            var parent = _context.Parents.FirstOrDefault(p => p.Email == email);
            var child = _context.Children.FirstOrDefault(c => c.Email == email);

            if (parent != null)
            {
                var result = _parentHasher.VerifyHashedPassword(parent, parent.Password, password);
                if (result == PasswordVerificationResult.Success)
                {
                    // Authentication successful, store role and ID in session
                    HttpContext.Session.SetString("Role", "Parent");
                    HttpContext.Session.SetInt32("UserId", parent.Id);
                    return RedirectToAction("Index", "Home");
                }
            }
            else if (child != null)
            {
                var result = _childHasher.VerifyHashedPassword(child, child.Password, password);
                if (result == PasswordVerificationResult.Success)
                {
                    // Authentication successful, store role and ID in session
                    HttpContext.Session.SetString("Role", "Child");
                    HttpContext.Session.SetInt32("UserId", child.Id);
                    return RedirectToAction("Index", "Home");
                }
            }

            // Authentication failed, display error message
            ViewBag.ErrorMessage = "Invalid email or password.";
            return View();
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public IActionResult Register(string name, string email, string password, string role, int? age = null, string parentEmail = null)
        {
            if (_context.Parents.Any(p => p.Email == email) || _context.Children.Any(c => c.Email == email))
            {
                ViewBag.ErrorMessage = "User with this email already exists.";
                return View();
            }

            if (role == "Parent")
            {
                var parent = new Parent { Name = name, Email = email };
                parent.Password = _parentHasher.HashPassword(parent, password);
                _context.Parents.Add(parent);
            }
            else if (role == "Child")
            {
                if (age == null || string.IsNullOrEmpty(parentEmail))
                {
                    ViewBag.ErrorMessage = "Age and Parent's Email are required for child registration.";
                    return View();
                }

                // Find the parent by email
                var parent = _context.Parents.FirstOrDefault(p => p.Email == parentEmail);
                if (parent == null)
                {
                    ViewBag.ErrorMessage = "Parent with this email not found.";
                    return View();
                }

                var child = new Child { Name = name, Email = email, Age = age.Value, ParentId = parent.Id, Points = 0 };
                child.Password = _childHasher.HashPassword(child, password);
                _context.Children.Add(child);
            }

            _context.SaveChanges();
            return RedirectToAction("Login");
        }

        //  Действие Logout вставлено ЗДЕСЬ
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // GET: /Account/Edit
        public async Task<IActionResult> Edit()
        {
            string role = HttpContext.Session.GetString("Role");
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (!userId.HasValue)
            {
                return RedirectToAction("Login");
            }

            EditProfileViewModel viewModel = new();

            if (role == "Parent")
            {
                Parent? parent = await _context.Parents.FindAsync(userId.Value);
                if (parent == null)
                {
                    return NotFound();
                }

                viewModel.Id = parent.Id;
                viewModel.Name = parent.Name;
                viewModel.Email = parent.Email;
                viewModel.Role = "Parent";
            }
            else if (role == "Child")
            {
                Child? child = await _context.Children.FindAsync(userId.Value);
                if (child == null)
                {
                    return NotFound();
                }

                viewModel.Id = child.Id;
                viewModel.Name = child.Name;
                viewModel.Email = child.Email;
                viewModel.Role = "Child";
                viewModel.Age = child.Age;
            }

            return View(viewModel);
        }

        // POST: /Account/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            var role = HttpContext.Session.GetString("Role");
            var userId = HttpContext.Session.GetInt32("UserId");

            if (!userId.HasValue)
            {
                return RedirectToAction("Login");
            }

            if (role == "Parent")
            {
                var parent = await _context.Parents.FindAsync(userId.Value);
                if (parent == null)
                {
                    return NotFound();
                }

                if (!string.IsNullOrEmpty(model.CurrentPassword) || !string.IsNullOrEmpty(model.NewPassword) || !string.IsNullOrEmpty(model.ConfirmPassword))
                {
                    // Attempting to change password
                    var result = _parentHasher.VerifyHashedPassword(parent, parent.Password, model.CurrentPassword);
                    if (result == PasswordVerificationResult.Success)
                    {
                        if (model.NewPassword == model.ConfirmPassword)
                        {
                            // Change the password
                            parent.Password = _parentHasher.HashPassword(parent, model.NewPassword);
                            _context.Update(parent); // Mark entity as modified
                        }
                        else
                        {
                            ModelState.AddModelError("ConfirmPassword", "The new password and confirmation password do not match.");
                            return View(model);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("CurrentPassword", "The current password entered is incorrect.");
                        return View(model);
                    }
                }
                else
                {
                    // Not attempting to change password, just update name/email
                    parent.Name = model.Name;
                    parent.Email = model.Email;
                    _context.Update(parent);
                }
            }
            else if (role == "Child")
            {
                var child = await _context.Children.FindAsync(userId.Value);
                if (child == null)
                {
                    return NotFound();
                }

                child.Name = model.Name;
                // Do not update email for children

                _context.Update(child);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }

        /* [HttpPost]
         public IActionResult Edit(Parent parentModel) // Overload for Parent
         {
             var role = HttpContext.Session.GetString("Role");
             var userId = HttpContext.Session.GetInt32("UserId");

             if (userId == null || role != "Parent")
             {
                 return RedirectToAction("Login");
             }

             var parent = _context.Parents.Find(userId);
             if (parent == null)
             {
                 return NotFound();
             }

             // Update properties, exclude password
             parent.Name = parentModel.Name;
             parent.Email = parentModel.Email;

             _context.SaveChanges();
             return RedirectToAction("Index", "Home"); // Or a profile page
         }*/
    }
}
