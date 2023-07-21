using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldMarket.Models.View_Models
{
    public class OrderConfirmationVM
    {
        public IEnumerable<ShoppingCart> ListCartConfirmation { get; set; }
        public OrderHeader OrderHeader { get; set; }    

    }
}
