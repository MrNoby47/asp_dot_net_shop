using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Numerics;
using System.Security.Claims;
using WorldMarket.DataAccess.Repository.IRepository;
using WorldMarket.Models;
using WorldMarket.Utility;

namespace WorldMarket.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        #region API CALLS
        public IActionResult GetAll(string status)
        {

            IEnumerable<OrderHeader> orderHeaders;
            if(User.IsInRole(SD.SD_Role_Admin) || User.IsInRole(SD.SD_Role_Emp))
            {
                orderHeaders = _unitOfWork.OrderHeaders.GetAll(includeProperties: "ApplicationUser");
            }
           else {
                var identityUser = (ClaimsIdentity)User.Identity;
                var claim = identityUser.FindFirst(ClaimTypes.NameIdentifier);
                orderHeaders = _unitOfWork.OrderHeaders.GetAll(x => x.ApplicationUserId == claim.Value, includeProperties: "ApplicationUser");
            }
           

            switch (status)
            {
                case "pending":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.OrderStatusPending);
                    break;
                case "complete":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.OrderStatusShiped);
                    break;
                case "approved":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.OrderStatusApproved);
                    break;
                case "inprocess":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.OrderStatusInProcess);
                    break;
                default:
                    break;
            }



            return Json(new { data = orderHeaders });

        }
        #endregion
    }
}
