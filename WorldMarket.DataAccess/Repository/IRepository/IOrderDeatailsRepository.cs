using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorldMarket.Models;

namespace WorldMarket.DataAccess.Repository.IRepository
{
    public interface IOrderDeatailsRepository: IRepository<OrderDetail>
    {
        public void Update(OrderDetail obj);
    }
}
