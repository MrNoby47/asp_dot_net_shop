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
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDeatailsRepository
    {
        private ApplicationDbContext _db;
        public OrderDetailRepository(ApplicationDbContext db):base(db)
        {
            _db = db;
        }
        public void Update(OrderDetail obj)
        {
            _db.OrderDetails.Update(obj);
        }
    }
}
