namespace LanchesMac.Services
{
    public interface ISeedUserRoleInitial
    {
        Task SeedRolesAsync();
        Task SeedUsersAsync();
    }
}