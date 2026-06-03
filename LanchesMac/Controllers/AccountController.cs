using LanchesMac.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LanchesMac.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            return View(new LoginViewModel() 
            {
                ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel LoginVM)
        {
            if (!ModelState.IsValid)
                return View(LoginVM);

            var user = await _userManager.FindByNameAsync(LoginVM.UserName);

            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user, LoginVM.Password, false, true);

                if (result.Succeeded)
                {
                    if (string.IsNullOrEmpty(LoginVM.ReturnUrl))
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    return Redirect(LoginVM.ReturnUrl);
                }
            }
            ModelState.AddModelError("", "Usuário ou senha inválidos.");
            return View(LoginVM);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel registerVM)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = registerVM.UserName, Email = registerVM.Email, PhoneNumber = registerVM.Telefone };
                var result = await _userManager.CreateAsync(user, registerVM.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Member");
                    return RedirectToAction("Login", "Account");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(registerVM);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied() 
        {
            return View();
        }
    }
}
