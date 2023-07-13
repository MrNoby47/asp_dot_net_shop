using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NuGet.Packaging.Signing;
using WorldMarket.DataAccess.Repository.IRepository;
using WorldMarket.Models;
using WorldMarket.Models.View_Models;

namespace WorldMarket.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hosEnvironmeent;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment hosEnvironmeent)
        {
            _unitOfWork = unitOfWork;
            _hosEnvironmeent = hosEnvironmeent;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> ProdFromDb = _unitOfWork.Products.GetAll();
            return View(ProdFromDb);
        }
        //GET
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new()
            {
                Product = new(),
                CategoryList = _unitOfWork.Categories.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                CoverTypeList = _unitOfWork.CoverTypes.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                })
            };
            if (id == null || id == 0)
            {
                //CREATE NEW PRODUCT

                return View(productVM);
            }
            else
            {
                //UPDATE
            }
            return View(productVM);
        }
        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                var rootPath = _hosEnvironmeent.WebRootPath;
                if(file != null)
                {
                    var fileName = Guid.NewGuid().ToString();
                    var upload = Path.Combine(rootPath, @"images\product");
                    var extension = Path.GetExtension(file.FileName);

                    using(var filestream = new FileStream(Path.Combine(upload, fileName+extension), FileMode.Create))
                    {
                        file.CopyTo(filestream);
                    }

                    obj.Product.ImgUrl = Path.Combine(@"\images\product", fileName+extension);
                }
                _unitOfWork.Products.Add(obj.Product);
                _unitOfWork.Save();
                TempData["success"] = "Product Created Successfully";
                return RedirectToAction("Index");
                

            }
            return View(obj);
        }
    }
}
