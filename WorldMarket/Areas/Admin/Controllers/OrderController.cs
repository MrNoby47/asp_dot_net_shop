using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using Stripe;
using System.Diagnostics;
using System.Numerics;
using System.Security.Claims;
using WorldMarket.DataAccess.Repository.IRepository;
using WorldMarket.Models;
using WorldMarket.Models.View_Models;
using WorldMarket.Utility;

namespace WorldMarket.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderVM OrderVM { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int orderId)
        {
            OrderVM = new OrderVM
            {
                OrderHeader = _unitOfWork.OrderHeaders.GetFirstOrDefault(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = _unitOfWork.OrderDetails.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product")
            };
            return View(OrderVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrderDetail()
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeaders.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, tracked: false);
            orderHeaderFromDb.StreetAdress = OrderVM.OrderHeader.StreetAdress;
            orderHeaderFromDb.City = OrderVM.OrderHeader.City;
            orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.Name = OrderVM.OrderHeader.Name;
            orderHeaderFromDb.postalCode = OrderVM.OrderHeader.postalCode;
            if (OrderVM.OrderHeader.Carrier != null)
            {
                orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            }
            if (OrderVM.OrderHeader.TrackingNumber != null)
            {
                orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            }
            _unitOfWork.OrderHeaders.Update(orderHeaderFromDb);
            _unitOfWork.Save();
            TempData["success"] = "Order Details update successfuly";
            return RedirectToAction("Details", "Order", new { orderId = orderHeaderFromDb.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartProcessing()
        {
            _unitOfWork.OrderHeaders.UpdateStatus(OrderVM.OrderHeader.Id, SD.OrderStatusInProcess);
            _unitOfWork.Save();
            TempData["success"] = "Order Status update successfuly";
            return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ShipOrder()
        {
            var orderHeader = _unitOfWork.OrderHeaders.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, tracked: false);
            orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeader.OrderStatus = SD.OrderStatusShiped;
            orderHeader.ShippingDate = DateTime.Now;
            _unitOfWork.OrderHeaders.Update(orderHeader);
            _unitOfWork.Save();
            TempData["success"] = "Order Status update successfuly";
            return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
        }



        [ActionName("Details")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Details_PAY_NOW(int orderId)
        {
            OrderVM.OrderHeader = _unitOfWork.OrderHeaders.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
            OrderVM.OrderDetail = _unitOfWork.OrderDetails.GetAll(u => u.OrderHeaderId == OrderVM.OrderHeader.Id, includeProperties: "Product");


            //Stripe Payment Session
            var domain = "https://localhost:44304";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"/Admin/Order/PaymentConfirmation?orderHeaderId={OrderVM.OrderHeader.Id}",
                CancelUrl = domain + $"/Admin/Order/Details?orderId={OrderVM.OrderHeader.Id}"
            };

            foreach (var item in OrderVM.OrderDetail)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Name
                        },
                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(sessionLineItem);
            }
            var service = new SessionService();
            Session session = service.Create(options);

            _unitOfWork.OrderHeaders.UpdateStripeSessionID(OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);

        }



        public ActionResult PaymentConfirmation(int orderHeaderId)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeaders.GetFirstOrDefault(x => x.Id == orderHeaderId);

            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeaders.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, SD.PymentStatusApproved);
                    _unitOfWork.Save();
                }
            }
            return View(orderHeaderId);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = SD.SD_Role_Admin + "," + SD.SD_Role_Emp)]
        public IActionResult CancelOrder()
        {
            var orderHeader = _unitOfWork.OrderHeaders.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, tracked: false);

            if (orderHeader.PaymentStatus == SD.OrderStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };
                var service = new RefundService();
                Refund refund = service.Create(options);
                _unitOfWork.OrderHeaders.UpdateStatus(orderHeader.Id, SD.OrderStatusCancelled, SD.OrderStausRefunded);
            }
            else
            {
                _unitOfWork.OrderHeaders.UpdateStatus(orderHeader.Id, SD.OrderStatusCancelled, SD.OrderStatusCancelled);
            }
            _unitOfWork.Save();
            TempData["success"] = "Order Cancelled successfuly";
            return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
        }

        #region API CALLS 
        [HttpGet]
        public ActionResult GetAll(string? status)
        {
            IEnumerable<OrderHeader> orderHeaders;
            if (User.IsInRole(SD.SD_Role_Admin) || User.IsInRole(SD.SD_Role_Emp))
            {
                orderHeaders = _unitOfWork.OrderHeaders.GetAll(includeProperties: "ApplicationUser");
            }
            else
            {
                var claimIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
                orderHeaders = _unitOfWork.OrderHeaders.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "ApplicationUser");

            }


            switch (status)
            {
                case "pending":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.PaymentStatusPending);
                    break;
                case "inprocess":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.OrderStatusInProcess);
                    break;
                case "completed":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.OrderStatusShiped);
                    break;
                case "approved":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.OrderStatusApproved);
                    break;
                default:
                    break;
            }

            return Json(new { data = orderHeaders });
        }


        #endregion

    }
}
