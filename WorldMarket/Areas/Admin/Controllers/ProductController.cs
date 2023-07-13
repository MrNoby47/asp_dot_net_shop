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
                productVM.Product = _unitOfWork.Products.GetFirstOrDefault(u => u.Id == id);
                return View(productVM);
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
                    var upload = Path.Combine(rootPath, @"images\products");
                    var extension = Path.GetExtension(file.FileName);

                    if(obj.Product.ImgUrl != null)
                    {
                        var oldpath = Path.Combine(rootPath, obj.Product.ImgUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldpath))
                        {
                            System.IO.File.Delete(oldpath);
                        }
                    }

                    using(var filestream = new FileStream(Path.Combine(upload, fileName+extension), FileMode.Create))
                    {
                        file.CopyTo(filestream);
                    }

                    obj.Product.ImgUrl = Path.Combine(@"\images\product", fileName+extension);
                }
                if(obj.Product.Id == 0)
                {
                    _unitOfWork.Products.Add(obj.Product);
                    TempData["success"] = "Product Created Successfully";
                }
                else
                {
                    _unitOfWork.Products.Update(obj.Product);
                    TempData["success"] = "Product Updated Successfully";
                }
               
                _unitOfWork.Save();
                return RedirectToAction("Index");
                

            }
            return View(obj);
        }



        #region API CALLS
        public IActionResult GetAll()
        {
            var productsList = _unitOfWork.Products.GetAll(includeProperties:"Category,CoverType");
            return Json(new { data = productsList});
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var deleteProd = _unitOfWork.Products.GetFirstOrDefault(u => u.Id == id);
            if(deleteProd == null)
            {
                return Json(new { success = false, message = "Something Wrong Happened !" });
            }
           
           var oldpath = Path.Combine(_hosEnvironmeent.WebRootPath, deleteProd.ImgUrl.TrimStart('\\'));
           if (System.IO.File.Exists(oldpath))
           {
               System.IO.File.Delete(oldpath);
           }

            _unitOfWork.Products.Remove(deleteProd);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Deleted Successfully" });
        }
        #endregion

    }
}
