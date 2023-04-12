using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
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

			shoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
			shoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;

			foreach (var cart in shoppingCartVM.ListCart)
			{
				cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.product.Price,
					cart.product.Price50, cart.product.Price100);
				shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}

			ApplicationUser applicationUser = _unitOfWork.applicationUserRepository.GetFirstOrDefault(u => u.Id == claim.Value);

			if (applicationUser.CompanyId.GetValueOrDefault() == 0)
			{
				shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
				shoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
			}
			else
			{
				shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
				shoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
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


			if (applicationUser.CompanyId.GetValueOrDefault() == 0)
			{
				// STRIPE 
				var domain = "https://localhost:44325/";
				var options = new SessionCreateOptions
				{
					PaymentMethodTypes = new List<string>
				{
				  "card",
				},
					LineItems = new List<SessionLineItemOptions>(),
					Mode = "payment",
					SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={shoppingCartVM.OrderHeader.Id}",
					CancelUrl = domain + $"customer/cart/index",
				};

				foreach (var item in shoppingCartVM.ListCart)
				{

					var sessionLineItem = new SessionLineItemOptions
					{
						PriceData = new SessionLineItemPriceDataOptions
						{
							UnitAmount = (long)(item.Price * 100),//20.00 -> 2000
							Currency = "usd",
							ProductData = new SessionLineItemPriceDataProductDataOptions
							{
								Name = item.product.Title
							},

						},
						Quantity = item.Count,
					};
					options.LineItems.Add(sessionLineItem);

				}

				var service = new SessionService();
				Session session = service.Create(options);
				_unitOfWork.orderHeaderRepository.UpdateStripePaymentID(shoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
				_unitOfWork.Save();
				Response.Headers.Add("Location", session.Url);
				return new StatusCodeResult(303);
			}
			else
			{
				return RedirectToAction("OrderConfirmation", "Cart", new { id = shoppingCartVM.OrderHeader.Id });
			}


		}

		public IActionResult OrderConfirmation(int id)
		{
			OrderHeader orderHeader = _unitOfWork.orderHeaderRepository.GetFirstOrDefault(u => u.Id == id, includeProperties: "ApplicationUser");

			if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
			{
				var service = new SessionService();
				Session session = service.Get(orderHeader.SessionId);
				//check the stripe status
				if (session.PaymentStatus.ToLower() == "paid")
				{
					//_unitOfWork.orderHeaderRepository.UpdateStripePaymentID(id, orderHeader.SessionId, session.PaymentIntentId);
					_unitOfWork.orderHeaderRepository.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
					_unitOfWork.Save();
				}
			}


			List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCartRepository.GetAll(u => u.ApplicationUserId ==
			orderHeader.ApplicationUserId).ToList();
			_unitOfWork.ShoppingCartRepository.RemoveRange(shoppingCarts);
			_unitOfWork.Save();

			return View(id);
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
