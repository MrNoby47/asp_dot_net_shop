using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorldMarket.Data;
using WorldMarket.Models;
using WorldMarket.Utility;

namespace WorldMarket.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;
        public DbInitializer(UserManager<IdentityUser> userManager,
                                 RoleManager<IdentityRole> roleManager,
                                 ApplicationDbContext db)
        {
                _userManager = userManager;
                _roleManager = roleManager;
                 _db = db;
        }
        public void Initializer()
        {
            //Migration if they are nit applied
            try
            {
                if(_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch(Exception ex)
            {

            }

            //create roles if they are not created 

            if(!_roleManager.RoleExistsAsync(SD.SD_Role_Admin).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.SD_Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.SD_Role_Emp)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.SD_Role_Ind)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.SD_Role_Comp)).GetAwaiter().GetResult();


                // create Admin user

                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "adminpegaz97",
                    Email = "koopergezoudeur@gmail.com",
                    Name = "Claude Lysias",
                    PhoneNumber = "1234567890",
                    StreetAdress = "test 123 ave",
                    State = "Al",
                    ZipCode = "12356",
                    City = "Los Angels",
                }, "Admin123*").GetAwaiter().GetResult();
                ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(u => u.Email == "koopergezoudeur@gmail.com");
                _userManager.AddToRoleAsync(user, SD.SD_Role_Admin).GetAwaiter().GetResult();
            }


            return;
        }
    }
}
