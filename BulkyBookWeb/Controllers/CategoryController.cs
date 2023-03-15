using BulkyBook.DataAccess;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBook.Controllers
{
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Category> objCategoryList = unitOfWork.categoryRepository.GetAll();
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
                ModelState.AddModelError("CustomError","The Display Order can't excatly match the Name");
            }
            if (ModelState.IsValid) 
            {
                unitOfWork.categoryRepository.Add(obj);
                unitOfWork.Save();
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
            var categoryFromDbFirst = unitOfWork.categoryRepository.GetFirstOrDefault(u => u.id == id);
           // var categoryFromDbSingle = _db.Categories.SingleOrDefault(u=>u.id == id);
           if(categoryFromDbFirst == null) 
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
                unitOfWork.categoryRepository.Update(obj);
                unitOfWork.Save();
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
             var categoryFromDbFirst = unitOfWork.categoryRepository.GetFirstOrDefault(u => u.id == id);
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
            var obj = unitOfWork.categoryRepository.GetFirstOrDefault(u => u.id == id);

            if (obj == null)
            {
                return NotFound();
            }
            unitOfWork.categoryRepository.Remove(obj);
            unitOfWork.Save();

            return RedirectToAction("Index");
         
        }

    }
}
