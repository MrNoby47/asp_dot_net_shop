using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Plugins;
using WorldMarket.DataAccess.Repository.IRepository;
using WorldMarket.Models;
using WorldMarket.Utility;

namespace WorldMarket.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.SD_Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            IEnumerable<Company> CompanyList = _unitOfWork.Companies.GetAll();
            return View(CompanyList);
        }

        public IActionResult Create(Company obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Companies.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Company Created Successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }
        //GET
        public IActionResult Edit(int id)
        {
            if(id == 0 || id == null)
            {
                return NotFound();
            }
            var CompanyObj = _unitOfWork.Companies.GetFirstOrDefault(u => u.Id == id);
            if(CompanyObj == null)
            {
                return NotFound();
            }
            return View(CompanyObj);
          
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Company obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Companies.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Company Updated Successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        #region API CALLS
        public IActionResult GetAll()
        {
            var CompanyList = _unitOfWork.Companies.GetAll();
            return Json(new { data = CompanyList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
           
            var CompanyDel = _unitOfWork.Companies.GetFirstOrDefault(u => u.Id == id);
            if(CompanyDel == null)
            {
                return Json(new { sucess = false, message = "The Selected Company Is Not Available" });
            }
            _unitOfWork.Companies.Remove(CompanyDel);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Company Deleted Successfully" });
        }


        #endregion


    }
}
