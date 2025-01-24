using Microsoft.AspNetCore.Identity;

public static class RoleInitializer
{
    public static async Task InitializeAsync(RoleManager<IdentityRole> roleManager)
    {
        if (!await roleManager.RoleExistsAsync("USER"))
        {
            await roleManager.CreateAsync(new IdentityRole("USER"));
        }

        if (!await roleManager.RoleExistsAsync("ADMIN"))
        {
            await roleManager.CreateAsync(new IdentityRole("ADMIN"));
        }

        if (!await roleManager.RoleExistsAsync("COMPUTER"))
        {
            await roleManager.CreateAsync(new IdentityRole("COMPUTER"));
        }
    }
}
