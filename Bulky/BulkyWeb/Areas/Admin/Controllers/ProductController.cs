using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
	[Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]

    public class ProductController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork , IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            var objProductList = _unitOfWork.Product.GetAll(includeProperties:"Category").ToList();
            
            return View(objProductList);
        }

        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new()
            {
                CatgoryList = _unitOfWork.Category
				.GetAll().Select(u => new SelectListItem
				{
					Text = u.Name,
					Value = u.Id.ToString()
				}),
                Product = new Product()
            };
            if(id == null || id == 0)
            {
                return View(productVM);
            }
            else
            {
                //Update
                productVM.Product = _unitOfWork.Product.Get(u => u.Id == id , includeProperties: "Images");
				return View(productVM);
			}
		}
        [HttpPost]
        public IActionResult Upsert(ProductVM obj , List<IFormFile>? files )
        {
            if (ModelState.IsValid)
            {
				if (obj.Product.Id == 0 || obj.Product.Id == null)
				{
					_unitOfWork.Product.Add(obj.Product);
				}
				else
				{
					_unitOfWork.Product.Update(obj.Product);
				}

				_unitOfWork.Save();


				string wwwRootPath = _webHostEnvironment.WebRootPath;
                if(files != null)
                {
                    foreach (var item in files)
                    {
						string filename = Guid.NewGuid().ToString() + Path.GetExtension(item.FileName);
                        string productPath = @"images\products\product-" + obj.Product.Id;
						string finalPath = Path.Combine(wwwRootPath, productPath);
                        if(!Directory.Exists(finalPath))
                        {
                            Directory.CreateDirectory(finalPath);
                        }
                        using (var fileStream = new FileStream(Path.Combine(finalPath, filename), FileMode.Create))
                        {
                            item.CopyTo(fileStream);
                        }

                        ProductImage productImage = new()
                        {
                            ImageUrl = @"\" + productPath + @"\" + filename,
                            ProductId = obj.Product.Id
                        };

                        if(obj.Product.Images == null)
                        {
                            obj.Product.Images = new List<ProductImage>();
                        }

                        obj.Product.Images.Add(productImage);
                    }


                    _unitOfWork.Product.Update(obj.Product);
                    _unitOfWork.Save();
					
				}

                
                TempData["Sucess"] = "Product Created / Updated Successfully";
                return RedirectToAction("Index");
            }
            else
            {
                obj.CatgoryList = _unitOfWork.Category
                .GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
				return View(obj);
			}
        }

        public IActionResult DeleteImage(int imageId)
        {
            var imageToBeDeleted = _unitOfWork.ProductImage.Get(u => u.Id == imageId);
            int productid = imageToBeDeleted.ProductId;
            if(imageToBeDeleted != null)
            {
                if (!string.IsNullOrEmpty(imageToBeDeleted.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, imageToBeDeleted.ImageUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
                _unitOfWork.ProductImage.Remove(imageToBeDeleted);
                _unitOfWork.Save();

                TempData["success"] = "Delted successfully";
			}
            return RedirectToAction(nameof(Upsert) , new { id = productid});
        }

		#region API CALLS

		[HttpGet]
        public IActionResult GetAll()
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new { data = objProductList });
        }
        
		[HttpDelete]
        public IActionResult Delete(int? id)
        {
            var obj = _unitOfWork.Product.Get(u => u.Id == id);

            if(obj == null)
            {
                return Json(new { successs = false, message = "Error while deleting"  });
            }

			//string productPath = @"images\products\product\" + id;

			//var  oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, obj.ImageUrl.TrimStart('\\'));
			//if (System.IO.File.Exists(oldImagePath))
			//{
			//	System.IO.File.Delete(oldImagePath);
			//}

			string productPath = @"images\products\product-" + id;
			string finalPath = Path.Combine(_webHostEnvironment.WebRootPath, productPath);
			if (Directory.Exists(finalPath))
			{
                string[] filePaths = Directory.GetFiles(finalPath);
                foreach (string filePath in filePaths)
                {
                    System.IO.File.Delete(filePath);
				}
                Directory.Delete(finalPath);
			}
			_unitOfWork.Product.Remove(obj);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successful" });
        }
		#endregion
	}
}
