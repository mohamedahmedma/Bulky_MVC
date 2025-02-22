using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;   
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
		public UserController(IUnitOfWork db, ApplicationDbContext context , UserManager<IdentityUser> userManager)
        {
            _unitOfWork = db;
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }
        
        public IActionResult RoleManagment(string userId)
        {
            RoleManagmentVM obj = new RoleManagmentVM()
            {
                 User =  _unitOfWork.ApplicationUser.Get(x => x.Id == userId , includeProperties:"Company"),
                RoleList = _roleManager.Roles.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Name
                }),
                CompanyList = _context.Company.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                })
            };
            obj.User.Role = _userManager.GetRolesAsync(_unitOfWork.ApplicationUser.Get(u => u.Id == userId)).GetAwaiter().GetResult().FirstOrDefault();
            return View(obj);
        }

        [HttpPost]
        public IActionResult RoleManagment(RoleManagmentVM roleManagmentVM)
        {
            string RoleID = _context.UserRoles.FirstOrDefault(u => u.UserId == roleManagmentVM.User.Id).RoleId;
            string oldRole = _context.Roles.FirstOrDefault(u => u.Id == RoleID).Name;

            if(! (roleManagmentVM.User.Role == oldRole))
            {
                ApplicationUser app = _context.ApplicationUsers.FirstOrDefault(u => u.Id == roleManagmentVM.User.Id);
                if(roleManagmentVM.User.Role == SD.Role_User_Comp)
                {
                    app.CompanyId = roleManagmentVM.User.CompanyId;
                }
                if(oldRole == SD.Role_User_Comp)
                {
                    app.CompanyId = null;
                }
                _context.SaveChanges();

				_userManager.RemoveFromRoleAsync(app, RoleID).GetAwaiter().GetResult();
				_userManager.AddToRoleAsync(app, roleManagmentVM.User.Role).GetAwaiter().GetResult();
			}

            
            return RedirectToAction(nameof(Index));
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            List<ApplicationUser> obj = _context.ApplicationUsers.Include( u => u.Company).ToList();

            var userRoles = _context.UserRoles.ToList();
            var roles = _context.Roles.ToList();

            foreach(var user in obj)
            {

                var roleId = userRoles.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                user.Role = roles.FirstOrDefault(u => u.Id == roleId).Name;
                if(user.Company == null)
                {
                    user.Company = new Company()
                    {
                        Name = ""
                    };
                }
            }
            return Json(new { data = obj });
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody]string id)
        {
            var obj = _context.ApplicationUsers.FirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return Json(new { success = false, message = "Error while Locking/Unlocking" });
            }

            if (obj.LockoutEnd != null && obj.LockoutEnd > DateTime.Now)
            {
                obj.LockoutEnd = DateTime.Now;
            }
            else
            {
                obj.LockoutEnd = DateTime.Now.AddDays(30);
            }
            _context.ApplicationUsers.Update(obj);
            _context.SaveChanges();
            return Json(new { success = true, message = "Delete Successful" });
        }
        #endregion

    }
}
