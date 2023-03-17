using BulkyBook.DataAccess;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }
        
        //Get
        public IActionResult Upsert(int? id)
        {
			ProductVM productVM = new()
			{
				Product = new(),
				CategoryList = _unitOfWork.categoryRepository.GetAll().Select(i => new SelectListItem
				{
					Text = i.Name,
					Value = i.id.ToString()
				}),
				CoverTypeList = _unitOfWork.coverTypeRepository.GetAll().Select(i => new SelectListItem
				{
					Text = i.Name,
					Value = i.Id.ToString()
				}),
			};

			if (id == null || id == 0)
            {
                // create product
                //ViewBag.CategoryList = CategoryList;
                //ViewData["CoverTypeList"] = CoverTypeList;
                return View(productVM);
            }
            else
            {
				// update product
				productVM.Product = _unitOfWork.productRepository.GetFirstOrDefault(u => u.Id == id);
				return View(productVM);
			}
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM obj,IFormFile? file)
        {
           
            if (ModelState.IsValid)
            {
                string wwwRootPath = _hostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"images\products");
                    var extension = Path.GetExtension(file.FileName);

                    if (obj.Product.ImageUrl != null)
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, obj.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        file.CopyTo(fileStreams);
                    }
                    obj.Product.ImageUrl = @"\images\products\" + fileName + extension;

                }

                if (obj.Product.Id == 0)
                {
                    _unitOfWork.productRepository.Add(obj.Product);
                }
                else
                {
                    _unitOfWork.productRepository.Update(obj.Product);
                }
                _unitOfWork.productRepository.Add(obj.Product);
                _unitOfWork.Save();
                TempData["success"] = "Product Created successfully";
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

        // api calls 

        [HttpGet]
        public IActionResult GetAll()
        {
            var product = _unitOfWork.productRepository.GetAll(includeProperties:"Category,CoverType");
            return Json(new { data = product });
        }

    }
}
