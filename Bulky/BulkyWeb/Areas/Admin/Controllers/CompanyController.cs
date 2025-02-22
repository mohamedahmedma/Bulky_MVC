using BulkyBook.DataAccess.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using BulkyBook.Models;
using Microsoft.AspNetCore.Authorization;
using BulkyBook.Utility;
using BulkyBook.DataAccess.Repository.IRepository;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _uniteOfWork;

        public CompanyController(IUnitOfWork unitOfWork )
        {
            _uniteOfWork = unitOfWork;

        }
        public IActionResult Index()
        {
            var Companys = _uniteOfWork.Company.GetAll().ToList();
            return View(Companys);
        }

        public IActionResult Upsert(int? id)
        {
            if(id == null || id == 0)
            {
                return View(new Company());
            }
            else
            {
                Company company = _uniteOfWork.Company.Get(u => u.Id == id);
                return View(company);
            }
        }
        [HttpPost]
        public IActionResult Upsert(Company company  )
        {
            if (ModelState.IsValid)
            {
                if(company.Id == 0)
                {
                    _uniteOfWork.Company.Add(company);
                }
                else
                {
                    _uniteOfWork.Company.Update(company);
                }
                _uniteOfWork.Save();
                TempData["success"] = "Company created successfully";
                return RedirectToAction("Index");
            }
            else
            {
                return View(company);
            }
            
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> obj = _uniteOfWork.Company.GetAll().ToList();
            return Json(new { data = obj });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var obj = _uniteOfWork.Company.Get(u => u.Id == id);

            if (obj == null)
            {
                return Json(new { successs = false, message = "Error while deleting" });
            }

            _uniteOfWork.Company.Remove(obj);
            _uniteOfWork.Save();
            return Json(new { success = true, message = "Delete Successful" });
        }
        #endregion
    }
}
