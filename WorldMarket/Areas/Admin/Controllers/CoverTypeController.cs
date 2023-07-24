using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorldMarket.DataAccess.Repository.IRepository;
using WorldMarket.Models;
using WorldMarket.Utility;

namespace WorldMarket.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.SD_Role_Admin)]
    public class CoverTypeController : Controller
    {
        private readonly IUnitOfWork _unitOfwork;
        public CoverTypeController(IUnitOfWork unitOfwork)
        {
            _unitOfwork = unitOfwork;
        }

        public IActionResult Index()
        {
            IEnumerable<CoverType> coverList = _unitOfwork.CoverTypes.GetAll();
            return View(coverList);
        }
      
        public IActionResult Create(CoverType obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfwork.CoverTypes.Add(obj);
                _unitOfwork.Save();
                TempData["success"] = "Cover Type Created Successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }
        //GET
        public IActionResult Edit(int? id)
        {
            if (id == 0 || id == null)
            {
                return NotFound();
            }
            var coverFromDb = _unitOfwork.CoverTypes.GetFirstOrDefault(u => u.Id == id);
            if (coverFromDb == null)
            {
                return NotFound();
            }
            return View(coverFromDb);

        }
        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CoverType obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfwork.CoverTypes.Update(obj);
                _unitOfwork.Save();
                TempData["success"] = "Cover Type Updated Successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }
        //GET
        public IActionResult Delete(int? id)
        {
            if (id == 0 || id == null)
            {
                return NotFound();
            }
            var coverFromDb = _unitOfwork.CoverTypes.GetFirstOrDefault(u => u.Id == id);
            if (coverFromDb == null)
            {
                return NotFound();
            }
            return View(coverFromDb);

        }
        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePOST(int? id)
        {
            if (id == 0 || id == null)
            {
                return NotFound();
            }
            var coverFromDb = _unitOfwork.CoverTypes.GetFirstOrDefault(u => u.Id == id);
            if (coverFromDb == null)
            {
                return NotFound();
            }
            _unitOfwork.CoverTypes.Remove(coverFromDb);
            _unitOfwork.Save();
            TempData["success"] = "Cover Type Deleted Successfully";
            return RedirectToAction("Index");
        }
    }
}
