using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Models.ViewModels
{
	public class ShoppingCartVM
	{
		public IEnumerable<ShoppingCart> ListCart { get; set; }
		// i comment CartTotal after add OrderHeader because OrderHeader include CartTotal so i will use it from OrderHeader 
		// public double CartTotal { get; set; }
		public OrderHeader OrderHeader { get; set; }
	}
}
