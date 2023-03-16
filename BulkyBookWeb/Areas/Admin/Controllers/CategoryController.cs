using BulkyBook.DataAccess;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Category> objCategoryList = _unitOfWork.categoryRepository.GetAll();
            return View(objCategoryList);
        }
        //Get
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("CustomError", "The Display Order can't excatly match the Name");
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.categoryRepository.Add(obj);
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            // var categoryFromDb = _db.Categories.Find(id);
            var categoryFromDbFirst = _unitOfWork.categoryRepository.GetFirstOrDefault(u => u.id == id);
            // var categoryFromDbSingle = _db.Categories.SingleOrDefault(u=>u.id == id);
            if (categoryFromDbFirst == null)
            {
                return NotFound();
            }
            return View(categoryFromDbFirst);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("CustomError", "The Display Order can't excatly match the Name");
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.categoryRepository.Update(obj);
                _unitOfWork.Save();
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
            var categoryFromDbFirst = _unitOfWork.categoryRepository.GetFirstOrDefault(u => u.id == id);
            // var categoryFromDbSingle = _db.Categories.SingleOrDefault(u=>u.id == id);
            if (categoryFromDbFirst == null)
            {
                return NotFound();
            }
            return View(categoryFromDbFirst);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePOST(int? id)
        {
            var obj = _unitOfWork.categoryRepository.GetFirstOrDefault(u => u.id == id);

            if (obj == null)
            {
                return NotFound();
            }
            _unitOfWork.categoryRepository.Remove(obj);
            _unitOfWork.Save();

            return RedirectToAction("Index");

        }

    }
}
