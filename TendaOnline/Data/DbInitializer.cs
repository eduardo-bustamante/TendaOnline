using Microsoft.AspNetCore.Identity;

namespace TendaOnline.Data;

public static class DbInitializer
{
    public static async Task SeedRolesAndUsersAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

        string[] roles = { "Admin", "Operador" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Criar Admin padrão se não existir
        var adminEmail = "admin@tendaonline.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // Criar Operador de Caixa padrão se não existir
        var operadorEmail = "caixa@tendaonline.com";
        if (await userManager.FindByEmailAsync(operadorEmail) == null)
        {
            var operadorUser = new IdentityUser
            {
                UserName = operadorEmail,
                Email = operadorEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(operadorUser, "Caixa@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(operadorUser, "Operador");
            }
        }
    }
}