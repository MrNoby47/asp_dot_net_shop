using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorldMarket.Data;
using WorldMarket.DataAccess.Repository.IRepository;
using WorldMarket.Models;

namespace WorldMarket.DataAccess.Repository
{
    public class ApplicationUserRepositotry: Repository<ApplicationUser>, IApplicationUserRepository
    {
        private ApplicationDbContext _db;
        public ApplicationUserRepositotry(ApplicationDbContext db):base(db)
        {
            _db = db;    
        }
    }
}
