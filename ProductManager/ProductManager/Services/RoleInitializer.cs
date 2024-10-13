using Microsoft.AspNetCore.Identity;

namespace ProductManager.Services
{
    public static class RoleInitializer
    {
        public static async Task CreateRoles(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            string[] roleNames = { "Admin", "User" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var user = await userManager.FindByEmailAsync("test@test.com");
            if (user != null)
            {
                await userManager.AddToRoleAsync(user, "Admin");
            }
        }
    }
}