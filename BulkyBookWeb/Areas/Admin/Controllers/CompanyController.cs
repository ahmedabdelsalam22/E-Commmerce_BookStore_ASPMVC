using BulkyBook.DataAccess;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }
        
        //Get
        public IActionResult Upsert(int? id)
        {
			Company company = new Company();

			if (id == null || id == 0)
            {
              
                return View(company);
            }
            else
            {
                // update product
                company = _unitOfWork.companyRepository.GetFirstOrDefault(u => u.Id == id);
				return View(company);
			}
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Company obj)
        {
           
            if (ModelState.IsValid)
            {
               

                if (obj.Id == 0)
                {
                    _unitOfWork.companyRepository.Add(obj);
                    TempData["success"] = "Company Created successfully";

                }
                else
                {
                    _unitOfWork.companyRepository.Update(obj);
                    TempData["success"] = "Company Updated successfully";

                }
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            return View(obj);
        }



        // api calls 

        [HttpGet]
        public IActionResult GetAll()
        {
            var company = _unitOfWork.companyRepository.GetAll();
            return Json(new { data = company });
        }


        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var obj = _unitOfWork.companyRepository.GetFirstOrDefault(u => u.Id == id);

            if (obj == null)
            {
                return Json(new { success = false , message = "Error while deleting"});
            }
           
            _unitOfWork.companyRepository.Remove(obj);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Deleted successfully" });

        }

    }
}
