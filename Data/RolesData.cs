using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;

namespace HackerRank.Data
{
    public class RolesData
    {
        private static readonly string[] roles = new[] {
            "Administrator"
        };

        public static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {

            foreach (var role in roles)
            {

                if (!await roleManager.RoleExistsAsync(role))
                {
                    var create = await roleManager.CreateAsync(new IdentityRole(role));

                    if (!create.Succeeded)
                    {

                        throw new Exception("Failed to create role");

                    }
                }

            }

        }
    }
}
