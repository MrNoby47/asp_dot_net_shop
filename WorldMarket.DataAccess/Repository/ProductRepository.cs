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
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db):base(db)
        {
            _db = db;
        }

        public void Update(Product obj)
        {
            var prodFromDb = _db.Products.FirstOrDefault(u => u.Id == obj.Id);
            if(prodFromDb != null)
            {
                prodFromDb.Name = obj.Name;
                prodFromDb.Description = obj.Description;
                prodFromDb.Price = obj.Price;
                prodFromDb.ListPrice = obj.ListPrice;
                prodFromDb.Price100 = obj.Price100;
                prodFromDb.Price50 = obj.Price50;
                prodFromDb.CategoryId = obj.CategoryId;
                prodFromDb.CoverTypeId = obj.CoverTypeId;
                if(obj.ImgUrl != null)
                {
                    prodFromDb.ImgUrl = obj.ImgUrl;
                }
            }
        }
    }
}
