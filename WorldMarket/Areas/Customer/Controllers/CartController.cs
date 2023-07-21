using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using WorldMarket.DataAccess.Repository.IRepository;
using WorldMarket.Models;
using WorldMarket.Models.View_Models;
using WorldMarket.Utility;

namespace WorldMarket.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public CartVM ShoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var identityClaims = (ClaimsIdentity)User.Identity;
            var claim = identityClaims.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new CartVM()
            {
                ListCart = _unitOfWork.ShoppingCarts.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "Product"),
                OrderHeader = new()
            };
           
            foreach (var item in ShoppingCartVM.ListCart)
            {
                item.Price = GetPriceByQuantity(item.Count, item.Product.Price, item.Product.Price50, item.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (item.Price * item.Count);
            }
            
            return View(ShoppingCartVM);
        }

        public IActionResult Plus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCarts.GetFirstOrDefault(u => u.Id == cartId);

            _unitOfWork.ShoppingCarts.Incrementation(cartFromDb, 1);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCarts.GetFirstOrDefault(u => u.Id == cartId);
            if(cartFromDb.Count <= 1)
            {
                _unitOfWork.ShoppingCarts.Remove(cartFromDb);
            }
            else
            {
                _unitOfWork.ShoppingCarts.Decrementation(cartFromDb, 1);
            }
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Remove(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCarts.GetFirstOrDefault(u => u.Id == cartId);

            _unitOfWork.ShoppingCarts.Remove(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        //GET
        public IActionResult Summary()
        {
            var identityUser = (ClaimsIdentity)User.Identity;
            var claim = identityUser.FindFirst(ClaimTypes.NameIdentifier);
            ShoppingCartVM = new CartVM()
            {
                ListCart = _unitOfWork.ShoppingCarts.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "Product"),
                OrderHeader = new()
            };
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUsers.GetFirstOrDefault(u => u.Id == claim.Value);

            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.StreetAdress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAdress;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.postalCode = ShoppingCartVM.OrderHeader.ApplicationUser.ZipCode;
            foreach (var item in ShoppingCartVM.ListCart){
                item.Price = GetPriceByQuantity(item.Count, item.Product.Price, item.Product.Price50, item.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (item.Price * item.Count);
            }
            return View(ShoppingCartVM);
        }

        //POST 
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public IActionResult SummaryPOST()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM.ListCart = _unitOfWork.ShoppingCarts.GetAll(x => x.ApplicationUserId == claim.Value, includeProperties: "Product");

            ShoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusPending;
            ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;

            foreach(var cart in ShoppingCartVM.ListCart)
            {
                cart.Price= GetPriceByQuantity(cart.Count, cart.Price, cart.Product.Price50, cart.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            _unitOfWork.OrderHeaders.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();


            foreach(var cart in  ShoppingCartVM.ListCart) {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    Price = cart.Price,
                    Count = cart.Count,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                };
                _unitOfWork.OrderDetails.Add(orderDetail);
                _unitOfWork.Save();
            
            }
            _unitOfWork.ShoppingCarts.RemoveRange(ShoppingCartVM.ListCart);
            _unitOfWork.Save();
            return View(ShoppingCartVM);

        }

        private double GetPriceByQuantity(double quantity, double price, double price50, double price100) {
        
                if(quantity <= 50)
                {
                 return price;
            }
            else
            {
                if(quantity <= 100)
                {
                    return price50;
                }
                return price100;
            }
        }
    }
}
