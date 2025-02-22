using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId
                , includeProperties: "product"),
                OrderHeader = new()
            };

            IEnumerable<ProductImage> productImages = _unitOfWork.ProductImage.GetAll();

            foreach (var item in ShoppingCartVM.ListCart)
            {
                item.product.Images = productImages.Where(x => x.ProductId == item.ProductId).ToList();
                item.Price = GetCartPriceBasedOnQuantity(item);
                ShoppingCartVM.OrderHeader.OrderTotal += (item.Price * item.Count);
            }
            return View(ShoppingCartVM);
        }


        public IActionResult plus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            cartFromDb.Count += 1;
            _unitOfWork.ShoppingCart.Update(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult minus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId , tracked:true);
            if (cartFromDb.Count <= 1)
            {
                HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count()-1);

                _unitOfWork.ShoppingCart.Remove(cartFromDb);

            }
            else
            {
                cartFromDb.Count -= 1;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
            }

            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Remove(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId , tracked:true);
            _unitOfWork.ShoppingCart.Remove(cartFromDb);
            HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count()-1);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId
                , includeProperties: "product"),
                OrderHeader = new()
            };

            foreach (var item in ShoppingCartVM.ListCart)
            {
                var price = GetCartPriceBasedOnQuantity(item);
                item.Price = price;
                ShoppingCartVM.OrderHeader.OrderTotal += (price * item.Count);
            }

            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id ==  userId);

            ShoppingCartVM.OrderHeader.Name  = ShoppingCartVM.OrderHeader.ApplicationUser.Name ;
            ShoppingCartVM.OrderHeader.PhoneNumber  = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber ;
            ShoppingCartVM.OrderHeader.StreetAddress  = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress ;
            ShoppingCartVM.OrderHeader.City  = ShoppingCartVM.OrderHeader.ApplicationUser.City ;
            ShoppingCartVM.OrderHeader.State  = ShoppingCartVM.OrderHeader.ApplicationUser.State ;
            ShoppingCartVM.OrderHeader.Postal  = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode ;

            return View(ShoppingCartVM);
        }
        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPost()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM.ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId
                , includeProperties: "product");


            ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;

			ApplicationUser ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);


			foreach (var item in ShoppingCartVM.ListCart)
            {
                var price = GetCartPriceBasedOnQuantity(item);
                item.Price = price;
                ShoppingCartVM.OrderHeader.OrderTotal += (price * item.Count);
            }

            if(ApplicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            }
            else
            {
				ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
				ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
			}
            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();
            foreach (var item in ShoppingCartVM.ListCart)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = item.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Price = item.Price,
                    Count = item.Count
                };
				_unitOfWork.OrderDetail.Add(orderDetail);
				_unitOfWork.Save();
			}

			if (ApplicationUser.CompanyId.GetValueOrDefault() == 0)
			{
                // it is regular customer account and we need to capture payment
                //stripe logic
			}

			return RedirectToAction(nameof(OrderConfirmation) , new {id = ShoppingCartVM.OrderHeader.Id});
        }
        public IActionResult OrderConfirmation(int id)
        {
            return View(id);
        }
        private double GetCartPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
            {
                return shoppingCart.product.Price;
            }
            else if (shoppingCart.Count <= 100)
            {
                return shoppingCart.product.Price50;
            }
            else if (shoppingCart.Count < 100)
            {
                return shoppingCart.product.Price100;
            }
            else
            {
                return 0;
            }
        }
    }
}
