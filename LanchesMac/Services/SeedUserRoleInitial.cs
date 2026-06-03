using LanchesMac.Services;
using Microsoft.AspNetCore.Identity;

public class SeedUserRoleInitial : ISeedUserRoleInitial
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    public SeedUserRoleInitial(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    public async Task SeedRolesAsync()
    {
        if (!await _roleManager.RoleExistsAsync("Admin"))
            await _roleManager.CreateAsync(new IdentityRole("Admin"));

        if (!await _roleManager.RoleExistsAsync("Member"))
            await _roleManager.CreateAsync(new IdentityRole("Member"));
    }

    public async Task SeedUsersAsync()
    {
        var email = _configuration["AdminSettings:Email"];
        var username = _configuration["AdminSettings:UserName"];
        var password = _configuration["AdminSettings:Password"];

        var user = await _userManager.FindByEmailAsync(email);

        if (user != null)
            return;

        user = new IdentityUser
        {
            UserName = username,
            Email = email,
            EmailConfirmed = true,
            LockoutEnabled = true
        };

        var result = await _userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "Admin");
        }
    }
}