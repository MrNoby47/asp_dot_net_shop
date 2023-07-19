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
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _db;
        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            Categories = new CategoryRepository(db);
            CoverTypes = new CoverTypeRepository(db);
            Products = new ProductRepository(db);
            Companies = new CompanyRepository(db);
            ApplicationUsers = new ApplicationUserRepositotry(db);
            ShoppingCarts = new ShoppingCartRepository(db);
        }

        public ICategoryRepository Categories { get; private set; }
        public ICoverTypeRepository CoverTypes { get; private set; }
        public IProductRepository Products { get; private set; }    
        public ICompanyRepository Companies { get; private set; }
        public IApplicationUserRepository ApplicationUsers { get; private set; }
        public IShoppingCartRepository ShoppingCarts { get; set; }

       
        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
