using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using WorldMarket.Data;
using WorldMarket.DataAccess.Repository;
using WorldMarket.DataAccess.Repository.IRepository;
using WorldMarket.Models;

namespace WorldMarket.Controllers
{
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Category> CategoryList = _unitOfWork.Categories.GetAll();
            return View(CategoryList);
        }
        public IActionResult Create(Category obj)
        {
            if(obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "Name should be different to Display Order");
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.Categories.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index");
               
            }
            return View(obj);
        }
        
        //GET 
        public IActionResult Edit(int? id)
        {
            if(id == 0 || id == null)
            {
                return NotFound();
            }
            var catFromDb = _unitOfWork.Categories.GetFirstOrDefault(u => u.Id == id);
            if(catFromDb == null)
            {
                return NotFound();
            }
            return View(catFromDb);
        }

        //POST 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category obj)
        {
            if(obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "Name must be different to Display Order");
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.Categories.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Category updated successfully";
                return RedirectToAction("Index");
                
            }
            return View(obj);
        }
        //GET
        public IActionResult Delete(int? id)
        {
            if(id == 0 || id == null)
            {
                return NotFound();
            }
            var catFromDb = _unitOfWork.Categories.GetFirstOrDefault(u => u.Id == id);
            if (catFromDb == null)
            {
                return NotFound();
            }
            return View(catFromDb);
        }
        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var catFromDb = _unitOfWork.Categories.GetFirstOrDefault(u => u.Id == id);
            if (catFromDb == null)
            {
                return NotFound();
            }
            _unitOfWork.Categories.Remove(catFromDb);
            _unitOfWork.Save();
            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
