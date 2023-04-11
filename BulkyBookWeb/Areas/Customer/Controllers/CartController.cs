using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
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
		public ShoppingCartVM shoppingCartVM { get; set; }
		public int OrderTotal { get; set; }
		public CartController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IActionResult Index()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

			shoppingCartVM = new ShoppingCartVM()
			{
				ListCart = _unitOfWork.ShoppingCartRepository.GetAll(u => u.ApplicationUserId == claim.Value
			  , includeProperties: "product"),
				OrderHeader = new()

			};

			foreach (var cart in shoppingCartVM.ListCart)
			{
				cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.product.Price,
					cart.product.Price50, cart.product.Price100);
				shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}
			return View(shoppingCartVM);
		}


		public IActionResult Summary()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

			shoppingCartVM = new ShoppingCartVM()
			{
				ListCart = _unitOfWork.ShoppingCartRepository.GetAll(u => u.ApplicationUserId == claim.Value,
				includeProperties: "product"),
				OrderHeader = new()
			};

			shoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.applicationUserRepository.GetFirstOrDefault(
	        u => u.Id == claim.Value);

			shoppingCartVM.OrderHeader.Name = shoppingCartVM.OrderHeader.ApplicationUser.Name;
			shoppingCartVM.OrderHeader.PhoneNumber = shoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
			shoppingCartVM.OrderHeader.StreetAddress = shoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
			shoppingCartVM.OrderHeader.City = shoppingCartVM.OrderHeader.ApplicationUser.City;
			shoppingCartVM.OrderHeader.State = shoppingCartVM.OrderHeader.ApplicationUser.State;
			shoppingCartVM.OrderHeader.PostalCode = shoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

			foreach (var cart in shoppingCartVM.ListCart)
			{
				cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.product.Price,
					cart.product.Price50, cart.product.Price100);
				shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}
			return View(shoppingCartVM);
		}

		[HttpPost]
		[ActionName("Summary")]
		[ValidateAntiForgeryToken]
		public IActionResult SummaryPOST()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

			shoppingCartVM.ListCart = _unitOfWork.ShoppingCartRepository.GetAll(u => u.ApplicationUserId == claim.Value,
			   includeProperties: "product");

			shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
			shoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
			shoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
			shoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;

			foreach (var cart in shoppingCartVM.ListCart)
			{
				cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.product.Price,
					cart.product.Price50, cart.product.Price100);
				shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}

			_unitOfWork.orderHeaderRepository.Add(shoppingCartVM.OrderHeader);
			_unitOfWork.Save();

			foreach (var cart in shoppingCartVM.ListCart)
			{
				OrderDetail orderDetail = new()
				{
					ProductId = cart.ProductId,
					OrderId = shoppingCartVM.OrderHeader.Id,
					Price = cart.Price,
					Count = cart.Count
				};
				_unitOfWork.orderDetailRepository.Add(orderDetail);
				_unitOfWork.Save();
			}
			_unitOfWork.ShoppingCartRepository.RemoveRange(shoppingCartVM.ListCart);
			_unitOfWork.Save();

			return RedirectToAction("Index","Home");
		}

		private double GetPriceBasedOnQuantity(double quantity, double price, double price50, double price100)
		{
			if (quantity <= 50)
			{
				return price;
			}
			else
			{
				if (quantity <= 100)
				{
					return price50;
				}
				return price100;
			}
		}

		public IActionResult Plus(int cartId)
		{
			var cart = _unitOfWork.ShoppingCartRepository.GetFirstOrDefault(u => u.Id == cartId);
			_unitOfWork.ShoppingCartRepository.IncrementCount(cart, 1);
			_unitOfWork.Save();
			return RedirectToAction(nameof(Index));
		}

		public IActionResult Minus(int cartId)
		{
			var cart = _unitOfWork.ShoppingCartRepository.GetFirstOrDefault(u => u.Id == cartId);
			if (cart.Count <= 1)
			{
				_unitOfWork.ShoppingCartRepository.Remove(cart);
			}
			else
			{
				_unitOfWork.ShoppingCartRepository.DecrementCount(cart, 1);
			}
			_unitOfWork.Save();
			return RedirectToAction(nameof(Index));
		}


		public IActionResult Remove(int cartId)
		{
			var cart = _unitOfWork.ShoppingCartRepository.GetFirstOrDefault(u => u.Id == cartId);
			_unitOfWork.ShoppingCartRepository.Remove(cart);
			_unitOfWork.Save();
			return RedirectToAction(nameof(Index));
		}



	}
}
