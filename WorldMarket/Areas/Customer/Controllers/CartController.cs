using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc; using Microsoft.Extensions.Options; using Stripe.Checkout; using System.Collections.Generic; using System.Security.Claims; using WorldMarket.DataAccess.Repository.IRepository; using WorldMarket.Models; using WorldMarket.Models.View_Models; using WorldMarket.Utility;  namespace WorldMarket.Areas.Customer.Controllers {     [Area("Customer")]     [Authorize]     public class CartController : Controller     {         private readonly IUnitOfWork _unitOfWork;         private readonly IEmailSender _emailSender;         [BindProperty]         public CartVM ShoppingCartVM { get; set; }         public CartController(IUnitOfWork unitOfWork, IEmailSender emailSender)         {             _unitOfWork = unitOfWork;             _emailSender = emailSender;         }         public IActionResult Index()         {             var identityClaims = (ClaimsIdentity)User.Identity;             var claim = identityClaims.FindFirst(ClaimTypes.NameIdentifier);              ShoppingCartVM = new CartVM()             {                 ListCart = _unitOfWork.ShoppingCarts.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "Product"),                 OrderHeader = new()             };                         foreach (var item in ShoppingCartVM.ListCart)             {                 item.Price = GetPriceByQuantity(item.Count, item.Product.Price, item.Product.Price50, item.Product.Price100);                 ShoppingCartVM.OrderHeader.OrderTotal += (item.Price * item.Count);             }                          return View(ShoppingCartVM);         }          public IActionResult Plus(int cartId)         {             var cartFromDb = _unitOfWork.ShoppingCarts.GetFirstOrDefault(u => u.Id == cartId);              _unitOfWork.ShoppingCarts.Incrementation(cartFromDb, 1);             _unitOfWork.Save();             return RedirectToAction(nameof(Index));         }         public IActionResult Minus(int cartId)         {                        var cartFromDb = _unitOfWork.ShoppingCarts.GetFirstOrDefault(u => u.Id == cartId);             if(cartFromDb.Count <= 1)             {                 _unitOfWork.ShoppingCarts.Remove(cartFromDb);                 var count = _unitOfWork.ShoppingCarts.GetAll(x => x.ApplicationUserId == cartFromDb.ApplicationUserId).ToList().Count - 1;
                HttpContext.Session.SetInt32(SD.SessionCart, count);             }             else             {                 _unitOfWork.ShoppingCarts.Decrementation(cartFromDb, 1);             }             _unitOfWork.Save();             return RedirectToAction(nameof(Index));         }         public IActionResult Remove(int cartId)         {             var cartFromDb = _unitOfWork.ShoppingCarts.GetFirstOrDefault(u => u.Id == cartId);                          _unitOfWork.ShoppingCarts.Remove(cartFromDb);             _unitOfWork.Save();             var count = _unitOfWork.ShoppingCarts.GetAll(x => x.ApplicationUserId == cartFromDb.ApplicationUserId).ToList().Count - 1;
            HttpContext.Session.SetInt32(SD.SessionCart, count);
                                      return RedirectToAction(nameof(Index));         }          //GET         public IActionResult Summary()         {             var identityUser = (ClaimsIdentity)User.Identity;             var claim = identityUser.FindFirst(ClaimTypes.NameIdentifier);             ShoppingCartVM = new CartVM()             {                 ListCart = _unitOfWork.ShoppingCarts.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "Product"),                 OrderHeader = new()             };             ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUsers.GetFirstOrDefault(u => u.Id == claim.Value);              ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;             ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;             ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;             ShoppingCartVM.OrderHeader.StreetAdress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAdress;             ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;             ShoppingCartVM.OrderHeader.postalCode = ShoppingCartVM.OrderHeader.ApplicationUser.ZipCode;             foreach (var item in ShoppingCartVM.ListCart){                 item.Price = GetPriceByQuantity(item.Count, item.Product.Price, item.Product.Price50, item.Product.Price100);                 ShoppingCartVM.OrderHeader.OrderTotal += (item.Price * item.Count);             }             return View(ShoppingCartVM);         }          //POST          [HttpPost]         [ValidateAntiForgeryToken]         [ActionName("Summary")]         public IActionResult SummaryPOST()         {             var claimIdentity = (ClaimsIdentity)User.Identity;             var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);              ShoppingCartVM.ListCart = _unitOfWork.ShoppingCarts.GetAll(x => x.ApplicationUserId == claim.Value, includeProperties: "Product");                           ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;             ShoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;              foreach(var cart in ShoppingCartVM.ListCart)             {                 cart.Price= GetPriceByQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);                 ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);             }             ApplicationUser applicationUser = _unitOfWork.ApplicationUsers.GetFirstOrDefault(u => u.Id == claim.Value);             if(applicationUser.CompanyId.GetValueOrDefault() == 0)              {                 ShoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusPending;
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            }             else
            {
                ShoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusApproved;
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;              }              _unitOfWork.OrderHeaders.Add(ShoppingCartVM.OrderHeader);             _unitOfWork.Save();               foreach(var cart in  ShoppingCartVM.ListCart) {                 OrderDetail orderDetail = new()                 {                     ProductId = cart.ProductId,                     Price = cart.Price,                     Count = cart.Count,                     OrderHeaderId = ShoppingCartVM.OrderHeader.Id,                 };                 _unitOfWork.OrderDetails.Add(orderDetail);                 _unitOfWork.Save();                          }              if(applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                //stripe settings
                var domain = "https://localhost:44304/";

                var options = new SessionCreateOptions
                {
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                    SuccessUrl = domain + $"Customer/Cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                    CancelUrl = domain + $"Customer/Cart/Index"
                };

                foreach (var item in ShoppingCartVM.ListCart)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Name,
                                Description = item.Product.Description
                            },
                        },
                        Quantity = item.Count
                    };
                    options.LineItems.Add(sessionLineItem);
                }
                var service = new SessionService();
                Session session = service.Create(options);

                _unitOfWork.OrderHeaders.UpdateStripeSessionID(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);

            }             else
            {
                return RedirectToAction("OrderConfirmation", "Cart", new { id = ShoppingCartVM.OrderHeader.Id });
            }                        }           public IActionResult OrderConfirmation(int id)         {             var identityUser = (ClaimsIdentity)User.Identity;             var claim = identityUser.FindFirst(ClaimTypes.NameIdentifier);              OrderHeader orderHeader = _unitOfWork.OrderHeaders.GetFirstOrDefault(x => x.Id == id, includeProperties: "ApplicationUser");
            List<ShoppingCart> ShoppingCartList = _unitOfWork.ShoppingCarts.GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();             OrderConfirmationVM  orderConfirmationVM= new()             {                 ListCartConfirmation = _unitOfWork.ShoppingCarts.GetAll(x => x.ApplicationUserId == claim.Value, includeProperties: "Product"),                 OrderHeader = new()             };
            orderConfirmationVM.ListCartConfirmation = ShoppingCartList;

            foreach(var item in orderConfirmationVM.ListCartConfirmation)
            {                 item.Price = GetPriceByQuantity(item.Count, item.Product.Price, item.Product.Price50, item.Product.Price100);                 orderConfirmationVM.OrderHeader.OrderTotal += (item.Price * item.Count);             }

            orderConfirmationVM.OrderHeader.Id = orderHeader.Id;
            orderConfirmationVM.OrderHeader.Name = orderHeader.Name;
            orderConfirmationVM.OrderHeader.StreetAdress = orderHeader.StreetAdress;
            orderConfirmationVM.OrderHeader.City = orderHeader.City;
            orderConfirmationVM.OrderHeader.OrderDate = orderHeader.OrderDate;
            orderConfirmationVM.OrderHeader.State = orderHeader.State;

            if (orderHeader.SessionId != null)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeaders.UpdateStatus(orderHeader.Id, SD.OrderStatusApproved, SD.PymentStatusApproved);
                    _unitOfWork.Save();
                }
            }

            _emailSender.SendEmailAsync(orderHeader.ApplicationUser.Email, "New Order - WorldMarket", "<p></New Oorder Created<p>");
            _unitOfWork.ShoppingCarts.RemoveRange(ShoppingCartList);
            HttpContext.Session.Clear();
            _unitOfWork.Save();
            return View(orderConfirmationVM);
        }          private double GetPriceByQuantity(double quantity, double price, double price50, double price100) {                          if(quantity <= 50)                 {                  return price;             }             else             {                 if(quantity <= 100)                 {                     return price50;                 }                 return price100;             }         }     } } 