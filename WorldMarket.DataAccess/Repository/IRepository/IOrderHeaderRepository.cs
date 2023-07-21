using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorldMarket.Models;

namespace WorldMarket.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository: IRepository<OrderHeader>
    {
        public void Update(OrderHeader obj);
        void  UpdateStatus(int id,string orderStatus, string? paymentStatus= null );
        void UpdateStripeSessionID(int id, string SessionId, string PaymentIntentId);
    }
}
