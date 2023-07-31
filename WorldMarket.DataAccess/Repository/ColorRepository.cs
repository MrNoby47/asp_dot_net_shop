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
    public class ColorRepository : Repository<Color>, IColorRepository
    {
        private  ApplicationDbContext _db;
        public ColorRepository(ApplicationDbContext db): base(db)
        {
            _db = db;
        }
        public void Update(Color obj)
        {
            _db.Update(obj);
        }
    }
}
