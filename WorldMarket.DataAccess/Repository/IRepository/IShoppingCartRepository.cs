using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorldMarket.Models;

namespace WorldMarket.DataAccess.Repository.IRepository
{
    public interface IShoppingCartRepository: IRepository<ShoppingCart>
    {
        int Incrementation(ShoppingCart shoppingCart, int count);
        int Decrementation(ShoppingCart shoppingCart, int count);
    }
}
