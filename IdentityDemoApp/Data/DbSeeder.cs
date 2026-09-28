using IdentityDemoApp.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityDemoApp.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdmin(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
           

            string[] roles =
            {
                "Admin",
                "Vendor",
                "Customer"
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var roleResult =
                        await roleManager.CreateAsync(
                            new IdentityRole(role));

                    if (!roleResult.Succeeded)
                    {
                        throw new Exception(
                            "Unable to create role: " + role);
                    }
                }
            }


           
            string adminEmail = "admin@marketlink.com";
            string adminPassword = "Admin@123";

            var adminUser =
                await userManager.FindByEmailAsync(adminEmail);


          
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,

                    FullName = "MarketLink Admin",
                    UserType = "Admin",

                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var createResult =
                    await userManager.CreateAsync(
                        adminUser,
                        adminPassword);

                if (!createResult.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        createResult.Errors.Select(
                            e => e.Description));

                    throw new Exception(
                        "Unable to create admin: " + errors);
                }
            }
            else
            {
               
                adminUser.IsActive = true;
                adminUser.EmailConfirmed = true;
                adminUser.UserType = "Admin";

                await userManager.UpdateAsync(adminUser);
            }


          

            if (!await userManager.IsInRoleAsync(
                adminUser,
                "Admin"))
            {
                var roleResult =
                    await userManager.AddToRoleAsync(
                        adminUser,
                        "Admin");

                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        roleResult.Errors.Select(
                            e => e.Description));

                    throw new Exception(
                        "Unable to assign Admin role: " + errors);
                }
            }
        }
    }
}