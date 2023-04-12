using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Admin.Controllers
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
            OrderVM = new OrderVM()
            {
                OrderHeader = _unitOfWork.orderHeaderRepository.GetFirstOrDefault(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = _unitOfWork.orderDetailRepository.GetAll(u => u.OrderId == orderId, includeProperties: "Product"),
            };
            return View(OrderVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
		public IActionResult UpdateOrderDetail()
		{
			var orderHEaderFromDb = _unitOfWork.orderHeaderRepository
                .GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id,tracked:false);

			orderHEaderFromDb.Name = OrderVM.OrderHeader.Name;
			orderHEaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
			orderHEaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
			orderHEaderFromDb.City = OrderVM.OrderHeader.City;
			orderHEaderFromDb.State = OrderVM.OrderHeader.State;
			orderHEaderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;
			if (OrderVM.OrderHeader.Carrier != null)
			{
				orderHEaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
			}
			if (OrderVM.OrderHeader.TrackingNumber != null)
			{
				orderHEaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
			}
			_unitOfWork.orderHeaderRepository.Update(orderHEaderFromDb);
			_unitOfWork.Save();
			TempData["Success"] = "Order Details Updated Successfully.";
			return RedirectToAction("Details", "Order", new { orderId = orderHEaderFromDb.Id });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult StartProcessing()
		{
            _unitOfWork.orderHeaderRepository.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusInProcess);
			_unitOfWork.Save();
			TempData["Success"] = "Order Status Updated Successfully.";
			return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id});
		}


		#region API CALLS

		[HttpGet]
        public IActionResult GetAll(String status)
        {
            IEnumerable<OrderHeader> orderHeaders;
            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                orderHeaders = _unitOfWork.orderHeaderRepository.GetAll(includeProperties: "ApplicationUser");
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                orderHeaders = _unitOfWork.orderHeaderRepository.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "ApplicationUser");
            }

            switch (status)
            {
                case "pending":
                    orderHeaders = orderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;
                case "inprocess":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;
                case "completed":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                default:
                    break;
            }

            return Json(new { data = orderHeaders });
        }
        #endregion
    }
}
