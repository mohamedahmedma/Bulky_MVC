using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize]
	public class OrderController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		[BindProperty]
		public OrderVM orderVM { get; set; }


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
			orderVM = new()
			{
				OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
				OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product")
			};
			return View(orderVM);
		}

		[HttpPost]
		[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
		public IActionResult UpdateOrderDetail()
		{
			var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id /*, includeProperties: ""*/);
			orderHeaderFromDb.Name = orderVM.OrderHeader.Name;
			orderHeaderFromDb.PhoneNumber = orderVM.OrderHeader.PhoneNumber;
			orderHeaderFromDb.StreetAddress = orderVM.OrderHeader.StreetAddress;
			orderHeaderFromDb.City = orderVM.OrderHeader.City;
			orderHeaderFromDb.State = orderVM.OrderHeader.State;
			orderHeaderFromDb.Postal = orderVM.OrderHeader.Postal;

			if (!string.IsNullOrEmpty(orderVM.OrderHeader.Carrier))
			{
				orderHeaderFromDb.Carrier = orderVM.OrderHeader.Carrier;
			}
			if (!string.IsNullOrEmpty(orderVM.OrderHeader.TrackingNumber))
			{
				orderHeaderFromDb.Carrier = orderVM.OrderHeader.TrackingNumber;
			}
			_unitOfWork.OrderHeader.Update(orderHeaderFromDb);
			_unitOfWork.Save();

			TempData["Sucess"] = "Order Details Updated Successfully";
			return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb.Id });
		}



		[HttpPost]
		[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
		public IActionResult Pending()
		{
			if (orderVM.OrderHeader.PaymentStatus == SD.StatusPending)
			{
				_unitOfWork.OrderHeader.UpdateStatus(orderVM.OrderHeader.Id, SD.StatusApproved);
				_unitOfWork.Save();
				TempData["Sucess"] = "Order Details Updated Successfully";
			}
			return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id });
		}

		[HttpPost]
		[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
		public IActionResult StartProcessing()
		{
			_unitOfWork.OrderHeader.UpdateStatus(orderVM.OrderHeader.Id, SD.StatusInProcess);
			_unitOfWork.Save();
			TempData["Sucess"] = "Order Details Updated Successfully";
			return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id });
		}
		[HttpPost]
		[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
		public IActionResult ShipOrder()
		{
			var orderheader = _unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id);

			orderheader.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
			orderheader.Carrier = orderVM.OrderHeader.Carrier;
			orderheader.OrderStatus = SD.StatusShipped;
			orderheader.ShippingDate = DateTime.Now;
			if (orderheader.PaymentStatus == SD.PaymentStatusDelayedPayment)
			{
				orderheader.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
			}


			_unitOfWork.OrderHeader.UpdateStatus(orderVM.OrderHeader.Id, SD.StatusShipped);
			_unitOfWork.Save();
			TempData["Sucess"] = "Order Shipped Successfully";
			return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id });
		}
		[HttpPost]
		[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
		public IActionResult CancelOrder()
		{

			var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id);

			if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
			{
				_unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
			}
			else
			{
				_unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
			}
			_unitOfWork.Save();
			TempData["Sucess"] = "Order Cancelled Successfully";
			return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id });
		}

		[ActionName("Details")]
		[HttpPost]
		public IActionResult Details_PAY_NOW()
		{
			orderVM.OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
			orderVM.OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderVM.OrderHeader.Id , includeProperties: "Product");
            return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id });
        }




        #region API CALLS

        [HttpGet]
		public IActionResult GetAll(string status)
		{
			IEnumerable<OrderHeader> objHeader;

			if(User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
			{
				objHeader = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
            }
			else
			{
				var claimsIdentity = (ClaimsIdentity)User.Identity;
				var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
				objHeader = _unitOfWork.OrderHeader.GetAll(u => u.ApplicationUserId == claim , includeProperties: "ApplicationUser") ;
			}

			switch (status)
			{
				case "pending":
					objHeader = objHeader.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
					break;
				case "inprocess":
					objHeader = objHeader.Where(u => u.OrderStatus == SD.StatusInProcess);
					break;
				case "completed":
					objHeader = objHeader.Where(u => u.OrderStatus == SD.StatusShipped); 
					break;
				case "approved":
					objHeader = objHeader.Where(u => u.OrderStatus == SD.StatusApproved);
					break;
				default:
					break;
			}

			return Json(new { data = objHeader });
		}
		#endregion
	}
}
