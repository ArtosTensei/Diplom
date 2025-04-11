using Microsoft.AspNetCore.Mvc;
using WebApplication3.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication3.Controllers
{
    public class UsersController : Controller
    {
        // Replace with your actual data store or database context
        private static List<User> _users = new List<User>();
        private static int _nextId = 1;


        // GET: Users/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Users/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(UserRegistrationViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.IsParent)
                {
                    // Register parent
                    var parent = new User
                    {
                        Id = _nextId++,
                        Email = model.Email,
                        Password = model.Password, // In real app, hash the password!
                        IsParent = true,
                        Children = new List<User>()
                    };
                    _users.Add(parent);

                    //Register children
                    if (model.Children != null && model.Children.Any())
                    {
                        foreach (var childModel in model.Children)
                        {
                            if (!string.IsNullOrEmpty(childModel.Email) && !string.IsNullOrEmpty(childModel.Password))
                            {
                                var child = new User
                                {
                                    Id = _nextId++,
                                    Email = childModel.Email,
                                    Password = childModel.Password, // In real app, hash the password!
                                    IsParent = false,
                                    ParentId = parent.Id
                                };
                                _users.Add(child);
                                parent.Children.Add(child);
                            }
                        }
                    }

                    // Redirect to a success page or login
                    return RedirectToAction("RegistrationSuccess");
                }
                else
                {
                    //Handle case where someone tries to register a child directly (optional)
                    ModelState.AddModelError("", "Child registration must be done through a parent.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public IActionResult RegistrationSuccess()
        {
            return View(); // Create a view for successful registration
        }


        //Helper View Model
        public class UserRegistrationViewModel
        {
            public string Email { get; set; }
            public string Password { get; set; }
            public bool IsParent { get; set; }
            public List<ChildRegistrationViewModel> Children { get; set; }

        }

        public class ChildRegistrationViewModel
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

    }
}