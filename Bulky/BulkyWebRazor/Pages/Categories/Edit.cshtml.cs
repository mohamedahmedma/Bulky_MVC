using BulkyWebRazor.Data;
using BulkyWebRazor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazor.Pages.Categories
{
	[BindProperties]
    public class EditModel : PageModel
    {
		private readonly AppDbContext _db;
		[BindProperty]
		public Category Category { get; set; }
		public EditModel(AppDbContext db)
		{
			_db = db;
		}
		public void OnGet(int? id )
		{
			if(id != null && id != 0) {
				Category = _db.Categories.FirstOrDefault(x => x.Id == id);
			}
		}

		public IActionResult OnPost()
		{
			if(ModelState.IsValid)
			{
				_db.Categories.Update(Category);
				_db.SaveChanges();
				return RedirectToPage("/Categories/index");
			}
			return Page();
		}
	}
}
