using BulkyBook.DataAccess;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> objProductList = _unitOfWork.productRepository.GetAll();
            return View(objProductList);
        }
        
        //Get
        public IActionResult Upsert(int? id)
        {
            Product product = new();

            IEnumerable<SelectListItem> CategoryList = _unitOfWork.categoryRepository.GetAll().Select(
                u=> new SelectListItem 
                {
                  Text = u.Name,
                  Value = u.id.ToString(),
                });
            IEnumerable<SelectListItem> CoverTypeList = _unitOfWork.coverTypeRepository.GetAll().Select(
               u => new SelectListItem
               {
                   Text = u.Name,
                   Value = u.Id.ToString(),
               });

            if (id == null || id == 0)
            {
                // create product
                ViewBag.CategoryList = CategoryList;
                ViewData["CoverTypeList"] = CoverTypeList;
                return View(product);
            }
            else
            {
              // update product       
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Product obj)
        {
           
            if (ModelState.IsValid)
            {
                _unitOfWork.productRepository.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "CoverType Edit successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            //   var categoryFromDb = _db.Categories.Find(id);
            var coverTypeFromDbFirst = _unitOfWork.coverTypeRepository.GetFirstOrDefault(u => u.Id == id);
            // var categoryFromDbSingle = _db.Categories.SingleOrDefault(u=>u.id == id);
            if (coverTypeFromDbFirst == null)
            {
                return NotFound();
            }
            return View(coverTypeFromDbFirst);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePOST(int? id)
        {
            var obj = _unitOfWork.coverTypeRepository.GetFirstOrDefault(u => u.Id == id);

            if (obj == null)
            {
                return NotFound();
            }

            _unitOfWork.coverTypeRepository.Remove(obj);
            _unitOfWork.Save();
            TempData["success"] = "CoverType Edit successfully";
            return RedirectToAction("Index");

        }

    }
}
