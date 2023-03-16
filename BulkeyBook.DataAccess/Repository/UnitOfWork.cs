using BulkyBook.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            categoryRepository = new CategoryRepository(_db);
            coverTypeRepository = new CoverTypeRepository(_db);
            productRepository = new ProductRepository(_db);
        }

        public ICategoryRepository categoryRepository { get; private set; }

        public ICoverTypeRepository coverTypeRepository { get; private set; }

        public IProductRepository productRepository { get; private set; }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
