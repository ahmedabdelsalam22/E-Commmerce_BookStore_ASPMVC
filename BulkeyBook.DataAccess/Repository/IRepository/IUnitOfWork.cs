using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        ICategoryRepository categoryRepository { get; }
        ICoverTypeRepository coverTypeRepository { get; }
        IProductRepository productRepository { get; }
        ICompanyRepository companyRepository { get; }
        IShoppingCartRepository ShoppingCartRepository { get; }
        IApplicationUserRepository applicationUserRepository { get; }
        IOrderHeaderRepository orderHeaderRepository { get; }
        IOrderDetailRepository orderDetailRepository { get; }
        void Save();
    }
}
